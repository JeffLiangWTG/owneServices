using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// All STL charges to be combined on a single invoice.
	/// May contain charges from multiple production databases if one organisation pays for multiple databases.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class StlBill : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StlBill(BusinessObjectFactory factory, GlbBranch invoicingBranch, EDIOrgHeader org, ZString currencyCode, ZDateTime periodStart, ZDateTime dateForExchangeRate, bool isBilled = true)
			: base(factory)
		{
			InvoicingBranch = invoicingBranch;
			Organisation = org;
			LicCompany = org.LicCompany;
			InvoiceCurrencyCode = currencyCode;
			PeriodStart = periodStart;
			DateForExchangeRate = dateForExchangeRate;
			IsBilled = isBilled;

			var surchargeSetting = LicCompany?.ReadonlySelfBilling;
			if (surchargeSetting != null && surchargeSetting.IsProcessingFeeValid && !surchargeSetting.IsProcessingFeeADiscount)
			{
				SurchargePercent = surchargeSetting.L4_ProcessingFeePercent;
				SurchargeDescription = surchargeSetting.ProcessingFeeDescription;
			}

			InvoiceLanguage = EDIDataRegistry.Instance.StlInvoiceSupportedLanguages.Value.Contains(Organisation.OH_Language.ToString()) ? Organisation.OH_Language : (ZString)Res.DefaultLanguage;
			InvoiceCultureInfo = Culture.GetCultureForLanguage(InvoiceLanguage) ?? Culture.Invariant;
		}

		public ZString InvoiceCurrencyCode { get; private set; }
		public EDIOrgHeader Organisation { get; private set; }
		public LicenceCompany LicCompany { get; private set; }
		public GlbBranch InvoicingBranch { get; private set; }
		public ZDateTime PeriodStart { get; private set; }
		public ZDateTime DateForExchangeRate { get; private set; }

		#region Amount properties

		public ZDecimal InvoicePreDiscountTotal { get; set; }

		/// <summary>
		/// Invoice total pre-tax amount, including discounts and all surcharges (invoice surcharge, GPR un-enhance surcharge, etc).
		/// </summary>
		public ZDecimal InvoicePostDiscountTotal { get; set; }

		public ZDecimal AudInvoicePostDiscountTotal { get; set; }

		#endregion

		#region Invoice Surcharge

		public readonly ZDecimal SurchargePercent;
		public readonly MultilingualString SurchargeDescription;

		/// <summary>
		/// Surcharge applicable to all amounts on the invoice.
		/// Applicable amount includes the Non-Current Version surcharge since that also has to be processed.
		/// </summary>
		public ZDecimal InvoiceSurchargeTotal { get; set; }

		#endregion

		#region Non-Current Version surcharge

		/// <summary>
		/// Non-Current Version surcharge - may come from multiple databases if their are more than one on the same invoice.
		/// </summary>
		public ZDecimal VersionSurchargeTotal { get; set; }

		#endregion

		public ZString SingleDomesticEntityCountries
		{
			get
			{
				return singleDomesticEntityCountries
					?? (singleDomesticEntityCountries = string.Join(", ", MonthlyUsages.Select(x => x.SingleDomesticEntityCountry).Distinct()) ?? "");
			}
		}

		string singleDomesticEntityCountries;

		#region OverallSalesRep

		public ZString OverallSalesRep
		{
			get
			{
				if (!overallSalesRep.HasValue)
				{
					overallSalesRep = Organisation != null ? Organisation.StaffAssignments.OverallSalesRep : ZString.Empty;
				}

				return overallSalesRep.Value;
			}
		}
		ZString? overallSalesRep;

		#endregion

		public ZString InvoicingBranchCode
		{
			get { return InvoicingBranch != null ? InvoicingBranch.GB_Code : ZString.Empty; }
		}

		public ZString InvoicingCountryCode
		{
			get { return InvoicingBranch != null ? InvoicingBranch.Company.GC_RN_NKCountryCode : ZString.Empty; }
		}

		#region Organisation Information

		public ZString OrganisationCode
		{
			get { return Organisation != null ? Organisation.OH_Code : ZString.Empty; }
		}

		public ZString OrganisationName
		{
			get { return Organisation != null ? Organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString OrganisationUNLOCO
		{
			get { return Organisation != null ? Organisation.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public ZString EnterpriseCode
		{
			get { return Organisation != null && Organisation.LicEnterprise != null ? Organisation.LicEnterprise.LE_EnterpriseCode : ZString.Empty; }
		}

		#endregion

		public ZString LicenceTypes
		{
			get
			{
				var lookups = new DatabaseTypes();
				return string.Join(", ", MonthlyUsages
					.Where(x => x.Database != null)
					.Select(x => lookups.GetDescriptionFromCode((string)x.Database.LD_LicenceType))
					.Distinct()
					.OrderBy(y => y));
			}
		}

		#region Site live

		public ZString SiteLiveStatus
		{
			get
			{
				int n = MonthlyUsages.Count();
				if (n == 0)
				{
					return "N/A";
				}
				else
				{
					int countLive = 0;
					int countNotLive = 0;
					int countBlank = 0;
					foreach (var monthlyUsage in MonthlyUsages)
					{
						if (monthlyUsage.SiteLiveDate.IsEmpty)
						{
							++countBlank;
						}
						else if (monthlyUsage.IsSiteLive)
						{
							++countLive;
						}
						else
						{
							++countNotLive;
						}
					}

					return ((countLive > 0) ? (NoResString)"Yes " : "") + (countLive > 1 ? "(" + countLive.ToString(CultureInfo.InvariantCulture) + ")" : "")
						+ ((countNotLive > 0) ? (NoResString)"No " : "") + (countNotLive > 1 ? "(" + countNotLive.ToString(CultureInfo.InvariantCulture) + ")" : "")
						+ ((countBlank > 0) ? (NoResString)"Blank " : "") + (countBlank > 1 ? "(" + countBlank.ToString(CultureInfo.InvariantCulture) + ")" : "");
				}
			}
		}

		#endregion

		#region A/R Contact

		public OrgContact Contact
		{
			get
			{
				if (!contactInitialized)
				{
					contact = ARContactHelper.FindRelatedReceivableContact(Organisation);
					contactInitialized = true;
				}

				return contact;
			}
			set
			{
				contact = value;
				contactInitialized = true;
			}
		}
		OrgContact contact;
		bool contactInitialized;

		public ZString ContactName
		{
			get { return Contact != null ? Contact.OC_ContactName : ZString.Empty; }
		}

		public ZString ContactEmail
		{
			get { return Contact != null ? Contact.OC_Email : ZString.Empty; }
		}

		#endregion

		#region StlMonthlyUsages

		public void AddMonthlyUsages(IEnumerable<StlMonthlyUsage> monthlyUsagesToAdd)
		{
			if (monthlyUsagesToAdd == null)
			{
				return;
			}

			MonthlyUsagesInternal.AddRange(monthlyUsagesToAdd);
			foreach (var monthlyUsage in monthlyUsagesToAdd)
			{
				taxCodesCalculated = false;

				foreach (var e in monthlyUsage.RowNotifications)
				{
					AddRowNotification(e);
				}
				monthlyUsage.Bill = this;

				if (monthlyUsage.HasPrepaid)
				{
					HasPrepaid = true;
				}
			}
		}

		public ZString TaxCodes
		{
			get
			{
				if (!taxCodesCalculated)
				{
					taxCodesCalculated = true;
					var codes = new HashSet<string>();

					foreach (var monthlyUsage in MonthlyUsages)
					{
						AddTaxCodes(codes, monthlyUsage.MainTaxGroup);
						AddTaxCodes(codes, monthlyUsage.SpecialTaxGroup);
					}

					if (fees != null)
					{
						foreach (var fee in fees)
						{
							var delivery = fee.OwnerDelivery?.Delivery;
							if (delivery != null)
							{
								AddTaxCode(codes, delivery.TaxId, delivery.SalesTaxChargeCode);
							}
						}
					}

					taxCodes = string.Join(", ", codes.OrderBy(x => x));
				}
				return taxCodes;
			}
		}
		ZString taxCodes;
		bool taxCodesCalculated;

		static void AddTaxCodes(HashSet<string> codes, SystemBill.TaxGroup taxGroup)
		{
			if (taxGroup != null)
			{
				AddTaxCode(codes, taxGroup.TaxId, taxGroup.SalesTax);
			}
		}

		static void AddTaxCode(HashSet<string> codes, AccTaxRate taxId, AccChargeCode salesTax)
		{
			if (taxId != null)
			{
				codes.Add(taxId.AT_Code);
			}
			else if (salesTax != null)
			{
				codes.Add(salesTax.AC_Code);
			}
			else
			{
				codes.Add((NoResString)"<default>");
			}
		}

		public IEnumerable<StlMonthlyUsage> MonthlyUsages
		{
			get { return MonthlyUsagesInternal.Cast<StlMonthlyUsage>(); }
		}

		public StlMonthlyUsageCollection MonthlyUsagesForBindingOnly
		{
			get { return MonthlyUsagesInternal; }
		}

		StlMonthlyUsageCollection MonthlyUsagesInternal
		{
			get { return monthlyUsages ?? (monthlyUsages = new StlMonthlyUsageCollection(Factory)); }
		}
		StlMonthlyUsageCollection monthlyUsages;

		#endregion

		public ZBool CanInvoice
		{
			get { return canInvoice ?? (canInvoice = (!InvoicePksForThisMonth.Any() && !IsTooSmallToBill && !HasErrors && IsBilled)).Value; }
		}
		ZBool? canInvoice;

		void InvalidateCanInvoice()
		{
			canInvoice = null;
		}

		public ZBool IsBilled { get; private set; }

		public ZBool IsTooSmallToBill { get; private set; }

		#region Status

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal use only, not client visible, ignored.")]
		internal class StatusMessages
		{
			public const string Error = "1 - Error";
			public const string Warning = "2 - Warning";
			public const string NotBilled = "3 - Not Billable";
			public const string TooSmall = "4 - Below Min.";
			public const string Invoiced = "5 - Invoiced";
			public const string ReadyPartner = "6 - Ready Partner";
			public const string ReadyNew = "7 - Ready New";
			public const string Ready = "8 - Ready";
		}

		public ZString Status
		{
			get
			{
				ZString result;

				if (InvoicePksForThisMonth.Any())
				{
					result = StatusMessages.Invoiced;
				}
				else if (MonthlyUsagesInternal.Count != 0 && !IsBilled)
				{
					result = StatusMessages.NotBilled;
				}
				else if (IsTooSmallToBill)
				{
					result = StatusMessages.TooSmall;
				}
				else if (HasErrors)
				{
					var err = RowErrors.FirstOrDefault();
					result = StatusMessages.Error + " - " + err.Message;
				}
				else if (HasWarnings)
				{
					result = StatusMessages.Warning + " - " + RowWarnings.FirstOrDefault().Message;
				}
				else if (!InvoicesForLastMonth.Any())
				{
					result = StatusMessages.ReadyNew;
				}
				else
				{
					result = StatusMessages.Ready;
				}

				return result;
			}
		}

		#endregion

		#region Existing Invoices

		public class InvoiceSummary
		{
			public ZGuid PK { get; set; }
			public ZString AH_TransactionNum { get; set; }
			public ZDecimal AH_OSExTaxAmount { get; set; }
		}

		public IEnumerable<InvoiceSummary> InvoicesForLastMonth { get; private set; } = Enumerable.Empty<InvoiceSummary>();

		public ZBool IsNewUser
		{
			get { return !InvoicesForLastMonth.Any(); }
		}

		public void SetInvoicesForThisMonth(IEnumerable<InvoiceSummary> invoiceSummaries)
		{
			InvoicesForThisMonth = invoiceSummaries?.ToArray() ?? Enumerable.Empty<InvoiceSummary>();
			InvalidateCanInvoice();
		}

		public ZString InvoiceNumbers
		{
			get { return JoinTransactionNums(InvoicesForThisMonth); }
		}

		public ZString LastMonthInvoiceNumbers
		{
			get { return JoinTransactionNums(InvoicesForLastMonth); }
		}

		public ZDecimal LastMonthInvoiceAmount
		{
			get { return lastMonthInvoiceAmount ?? (lastMonthInvoiceAmount = InvoicesForLastMonth.Sum(x => x.AH_OSExTaxAmount)).Value; }
		}
		ZDecimal? lastMonthInvoiceAmount;

		public void SetInvoicesForLastMonth(IEnumerable<InvoiceSummary> invoiceSummaries)
		{
			InvoicesForLastMonth = invoiceSummaries?.ToArray() ?? Enumerable.Empty<InvoiceSummary>();
		}

		IEnumerable<InvoiceSummary> InvoicesForThisMonth { get; set; } = Enumerable.Empty<InvoiceSummary>();

		void SetInvoicesForThisMonth(IEnumerable<InvoicingBase> invoices)
		{
			var invoiceSummaries = invoices?.Select(x => new InvoiceSummary()
			{
				PK = x.PK,
				AH_TransactionNum = x.AH_TransactionNum,
				AH_OSExTaxAmount = x.AH_OSExTaxAmount
			});
			SetInvoicesForThisMonth(invoiceSummaries);
			InvalidateCanInvoice();
		}

		#endregion

		#region Exchange Rates

		/// <summary>
		/// Exchange rate for invoicing branch currency to invoice currency.
		/// Used only to set the invoice rate.
		/// Will be a reciprocal rate if the invoice company uses reciprocal rates.
		/// </summary>
		public ZDecimal LocalExchangeRate { get; set; }

		/// <summary>
		/// Price currencies to invoice currency
		/// </summary>
		public void AddPriceToInvoiceExchangeRate(string priceCurrency, decimal rate)
		{
			if (priceCurrency != InvoiceCurrencyCode)
			{
				ExchangeRates[priceCurrency] = rate;
			}
		}

		Dictionary<string, decimal> ExchangeRates
		{
			get { return exchangeRates ?? (exchangeRates = new Dictionary<string, decimal>()); }
		}
		Dictionary<string, decimal> exchangeRates;

		void AddExchangeRatesNotes(InvoicingBase invoice)
		{
			foreach (var currencyExchangeRate in ExchangeRates)
			{
				string exchangeNote = Res.GetString("4bd36d4f-076d-4214-83fb-a7979deca6ff", "Exchange rate used: 1 {0} = {1} {2}",
					currencyExchangeRate.Key,
					currencyExchangeRate.Value.ToString(BillingConstants.ExchangeRateDecimalFormat, CultureInfo.InvariantCulture),
					invoice.AH_RX_NKTransactionCurrency);
				BillingInvoicingHelper.AddCommentLine(invoice, exchangeNote);
			}
		}

		#endregion

		#region Validation

		protected override ZString HumanReadableNameCore
		{
			get { return (NoResString)"STL Bill"; }
		}

		#endregion

		#region Create Invoice

		public IEnumerable<ZGuid> InvoicePksForThisMonth => InvoicesForThisMonth.Select(x => x.PK);

		public IEnumerable<InvoicingBase> CreateInvoicesWithoutSave()
		{
			return CreateInvoicesWithoutSave(ZDateTime.Empty);
		}

		public IEnumerable<InvoicingBase> CreateInvoicesWithoutSave(ZDateTime postAndInvoiceDateOverride)
		{
			using (TemporarilySwitchToInvoiceLanguage())
			{
				var invoice = CreateInvoiceForBranch(InvoicingBranch, postAndInvoiceDateOverride);
				var invoices = PopulateInvoice(invoice);
				invoice.Factory.Saved += InvoiceSaved;
				invoice.Factory.GetCachedValue(invoiceCacheKey, () => invoices);
				return invoices;
			}
		}
		const string invoiceCacheKey = "StlBill.Invoice";

		void InvoiceSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= InvoiceSaved;
			var invoices = factory.GetCachedValue<IEnumerable<ARInvoice>>(invoiceCacheKey, () => null);
			if (savedSuccessfully && invoices != null)
			{
				SetInvoicesForThisMonth(invoices);
			}
		}

		protected ARInvoice CreateInvoiceForBranch(GlbBranch invoicingBranch, ZDateTime postAndInvoiceDateOverride, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var invoice = factory.New<EDIARInvoice>();
			invoice.DisableExchangeRateValidation = true;
			invoice.AH_OH = Organisation.PK;
			if (!postAndInvoiceDateOverride.IsEmpty)
			{
				invoice.AH_InvoiceDate = postAndInvoiceDateOverride;
				invoice.AH_PostDate = postAndInvoiceDateOverride;
			}
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = InvoiceCurrencyCode;
			invoice.AH_GB = invoicingBranch.PK;
			invoice.AH_GC = invoicingBranch.GB_GC;
			invoice.AH_ExchangeRate = LocalExchangeRate;
			invoice.USSalesTaxCalculator = usSalesTaxCalculator;
			return invoice;
		}

		public IEnumerable<InvoicingBase> CreateInvoices(ZDateTime postAndInvoiceDateOverride, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			ARInvoice invoice;
			IEnumerable<InvoicingBase> invoices;
			using (TemporarilySwitchToInvoiceLanguage())
			using (BillingInvoicingHelper.BranchContext(InvoicingBranch.PK.ToGuid()))
			{
				invoice = CreateInvoiceForBranch(InvoicingBranch, postAndInvoiceDateOverride, usSalesTaxCalculator: usSalesTaxCalculator);
				invoices = PopulateInvoice(invoice);
				BusinessObjectFactory.SaveTogether(invoice.Factory, invoice.DocManagerInfo.MasterFactory);
				OnInvoiceSaved(invoice);
				SetInvoicesForThisMonth(invoices);
			}

			return invoices;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		IEnumerable<InvoicingBase> PopulateInvoice(ARInvoice invoice)
		{
			InvoiceFinder.Clear();
			bool hasBorderWiseUsage = MonthlyUsages.Any(x => x.DatabaseUsage?.Database?.LD_Product.ToString() == ProductTypes.Codes.BorderWise);
			bool hasCWUsage = MonthlyUsages.Any(x => x.DatabaseUsage?.Database?.IsEnterpriseFamilyDatabase ?? false);
			string invoiceDescription = hasCWUsage || !hasBorderWiseUsage
								? Res.GetString("d7928c1a-a843-43b7-a4d2-5cfde5a28b2f", "STL Monthly Usage Invoice")
								: Res.GetString("dd74a3a9-8d24-4926-8298-11f7e67b3f7f", "BorderWise Monthly Usage Invoice");
			invoiceDescription = invoiceDescription + " - " + PeriodStart.ToString(Res.GetString("StlBill|InvoiceDescriptionDateFormat", "MMMM yyyy"), InvoiceCultureInfo);

			if (!hasBorderWiseUsage && !hasCWUsage)
			{
				var invoiceDescriptionTemplate = EDIDataRegistry.Instance.StlMonthlyUsageInvoiceDescription.Value.GetDescriptionFromCode(MonthlyUsages.FirstOrDefault()?.Database?.LD_Product);
				if (!string.IsNullOrWhiteSpace(invoiceDescriptionTemplate))
				{
					invoiceDescription = Regex.Replace(invoiceDescriptionTemplate, @"\{Date:(.*?)\}", match => PeriodStart.ToString(match.Groups[1].Value, InvoiceCultureInfo));
				}
			}

			invoice.AH_Desc = invoiceDescription;
			BillingInvoicingHelper.AddCommentLine(invoice, invoice.AH_Desc);
			BillingInvoicingHelper.AddCommentLine(invoice, ".");

			var lines = new List<SystemBill.BillLine>();
			var depositChargeCodeToPositiveBalance = GetDepositChargeCodeToPositiveBalance(Organisation.PK);
			var depositUseSummary = new DepositUseSummary(depositChargeCodeToPositiveBalance.Values);

			foreach (var monthlyUsage in MonthlyUsages)
			{
				CreateInvoiceLines(monthlyUsage, lines, depositChargeCodeToPositiveBalance);
			}

			var feeLines = new List<FeeBillLineWithSurcharge>();
			if (fees != null)
			{
				CreateFeeInvoiceLines(feeLines, fees, PeriodStart, SurchargePercent, InvoiceCultureInfo, InvoiceFinder);

				foreach (var line in feeLines)
				{
					line.BillLine.IsIncludedInPrepay = true;
					if (line.BillLine.CurrencyCode != InvoiceCurrencyCode)
					{
						var amountInInvoiceCurrency = CurrencyExchange.GetAmount(line.BillLine.CurrencyCode, InvoiceCurrencyCode, InvoicingBranch.Company, line.BillLine.Amount);
						line.BillLine.Amount = amountInInvoiceCurrency;
						line.BillLine.CurrencyCode = InvoiceCurrencyCode;
						if (line.SurchargeAmount != 0m)
						{
							line.SurchargeAmount = CurrencyExchange.GetAmount(line.BillLine.CurrencyCode, InvoiceCurrencyCode, InvoicingBranch.Company, line.SurchargeAmount);
						}
					}
				}

				lines.AddRange(feeLines.Select(x => x.BillLine));
			}

			depositUseSummary.SetClosingBalance(depositChargeCodeToPositiveBalance.Values, InvoicingBranch, CurrencyExchange);

			if (InvoiceSurchargeTotal != 0)
			{
				var surchargeLine = new SystemBill.BillLine(InvoiceSurchargeTotal, MonthlyUsages.First().MainTaxGroup, InvoiceCurrencyCode, EDIDataRegistry.Instance.StlSurchargeChargeCode.Value,
					ZString.Format("{0} {1}% ", (SurchargeDescription?.ToString() ?? ""), SurchargePercent.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture)),
					lines.Count, BillingConstants.BillingSystem.STL);
				lines.Add(surchargeLine);
			}

			if (VersionSurchargeTotal != 0)
			{
				var reg = EDIDataRegistry.Instance;
				var surchargeLine = new SystemBill.BillLine(VersionSurchargeTotal, MonthlyUsages.First().MainTaxGroup, InvoiceCurrencyCode, reg.VersionSurchargeChargeCode.Value,
					reg.VersionSurchargeDescription.Value,
					lines.Count, BillingConstants.BillingSystem.STL);
				lines.Add(surchargeLine);
			}

			var invoices = PopulateInvoiceCore(invoice, lines, out var amountToPrepayIncTaxInvoiceCurrency);
			CalculateNextPrepay(invoices, amountToPrepayIncTaxInvoiceCurrency);

			var result = new List<InvoicingBase>();
			foreach(var item in invoices)
			{
				AddAttachments(item);
				AddExchangeRatesNotes(item);
				AddComments(item);

				if (item is EDIARInvoice arInvoice)
				{
					result.AddRange(ProcessTax(arInvoice));
				}
				else
				{
					result.Add(item);
				}
			}

			InvoiceFinder.BuildInvoiceMap(result);
			SetChargeableUsagesInvoice();
			CreateRevenueBreakdown(feeLines);

			foreach (var item in result)
			{
				item.CommissionCreatorOverride = new BillingCommissionCreator(item, new StlBilledUsageCommissionGroupsCalculator());
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		decimal PopulateInvoiceLines(InvoicingBase invoice, IEnumerable<SystemBill.BillLine> lines)
		{
			var isCreditNote = invoice is EDIARCreditNote;
			decimal amountToPrepayIncTaxInvoiceCurrency = 0;
			var taxGroups = lines.GroupBy(s => s.Tax).OrderBy(s => s.Key?.Code);
			bool hasMultipleTaxes = taxGroups.Count() > 1;
			foreach (var taxGroup in taxGroups)
			{
				ZDecimal taxGroupAmount = 0;
				decimal taxGroupPrepaidAmountExTax = 0;

				foreach (var line in taxGroup.OrderBy(s => s.SystemCode).ThenBy(s => s.Sequence))
				{
					//the total amount on credit note should > 0;
					ZDecimal lineAmount = isCreditNote ? Math.Abs(line.Amount) : line.Amount;

					taxGroupAmount += lineAmount;
					var invoiceLine = BillingInvoicingHelper.AddAmountLine(invoice, lineAmount, line.ChargeCodeName, line.Tax.TaxId,
						OrganisationBill.LineDescription(line.Description, line.Tax, hasMultipleTaxes));
					InvoiceFinder.AddBillLine(line, invoiceLine);
					if (!line.TaxDate.IsEmpty)
					{
						invoiceLine.AL_TaxDate = line.TaxDate.Date;
					}
					if (line.Description.IsEmpty)
					{
						invoiceLine.AL_Desc = OrganisationBill.LineDescription(((EDIGenericCharge)invoiceLine.GenericChargeBizO).VC_DescriptionMultilingual, line.Tax, hasMultipleTaxes);
					}

					if (!line.DepartmentPK.IsEmpty)
					{
						invoiceLine.AL_GE = line.DepartmentPK;
						var glHeader = invoiceLine.GLHeader;

						if (glHeader != null &&
							glHeader.SubAccountTypes.Cast<AccGLHeaderSubAccount>().Any(x =>
								x.ASA_SubClass == OrgHeaderSchema.Constants.Prefix &&
								x.ASA_IsSubClassValidationRuleMandatory))
						{
							var subAccount = invoiceLine.SubAccounts.Cast<AccTransactionLineSubAccount>().FirstOrDefault(x => x.AL1_SubClassParentTableCode == OrgHeaderSchema.Constants.Prefix);
							if (subAccount == null)
							{
								subAccount = invoiceLine.SubAccounts.AddNew();
								subAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
							}
							subAccount.AL1_SubClassParentId = invoice.AH_OH;
						}

						// Validation seems to get out of sync. Clear any spurious errors.
						if (invoiceLine.HasErrors)
						{
							invoiceLine.Validation.ValidateAll();
						}
					}

					if (line.IsIncludedInPrepay)
					{
						amountToPrepayIncTaxInvoiceCurrency += invoiceLine.AL_OSAmount;
						taxGroupPrepaidAmountExTax += lineAmount;
					}
				}

				if (taxGroup.Key.SalesTax != null)
				{
					decimal rate = taxGroup.Key.SalesTaxPercentage;
					ZDecimal taxAmount = Utilities.Round(taxGroupAmount * rate * 0.01m, BillingConstants.RoundingDecimals);
					if (taxAmount != 0)
					{
						BillingInvoicingHelper.AddAmountLine(invoice, taxAmount, taxGroup.Key.SalesTax.AC_Code, null,
							string.Concat(taxGroup.Key.SalesTax.AC_Desc, " (", taxGroup.Key.SalesTax.AC_Code, ')'));
					}

					if (taxGroupPrepaidAmountExTax != 0)
					{
						amountToPrepayIncTaxInvoiceCurrency += Utilities.Round(taxGroupPrepaidAmountExTax * rate * 0.01m, BillingConstants.RoundingDecimals);
					}
				}
			}

			return amountToPrepayIncTaxInvoiceCurrency;
		}

		//keep this method, just in case we need to split the invoice.
		IEnumerable<InvoicingBase> PopulateInvoiceCore(ARInvoice invoice, IEnumerable<SystemBill.BillLine> billLines, out decimal amountToPrepayIncTaxInvoiceCurrency)
		{
			var result = new [] { invoice };
			amountToPrepayIncTaxInvoiceCurrency = PopulateInvoiceLines(invoice, billLines);
			return result;
		}

		static void CreateFeeInvoiceLines(List<FeeBillLineWithSurcharge> lines, IEnumerable<FeeUsage> feeUsages, ZDateTime periodStart, ZDecimal surchargePercent, CultureInfo cultureInfo, StlInvoiceFinder invoiceFinder)
		{
			var feeTypes = EDIDataRegistry.Instance.LicenceFeeTypes.Value;
			var feeDiscountChargeCodes = EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.Value;

			foreach (var feeUsagesByTax in feeUsages.GroupBy(x => new SystemBill.TaxGroup(x.OwnerDelivery.Delivery)))
			{
				var tax = feeUsagesByTax.Key;

				foreach (var feeUsage in feeUsagesByTax.OrderBy(x => x.Fee.L8_Order))
				{
					var fee = feeUsage.Fee;
					ZString description = fee.L8_DescriptionMultilingual;

					var dateFormat = Res.GetString("StlBill|FeeInvoiceLinesDateFormat", "MMM yyyy");
					if (fee.L8_RenewalMonths == 1)
					{
						description += System.Environment.NewLine + periodStart.ToString(dateFormat, cultureInfo);
					}
					else
					{
						description += System.Environment.NewLine + Res.GetString("StlBill|CreateFeeInvoiceLines|FromDateToDate", "{0} to {1}",
							periodStart.ToString(dateFormat, cultureInfo),
							periodStart.AddMonths(fee.L8_RenewalMonths - 1).ToString(dateFormat, cultureInfo));
					}

					var amount = feeUsage.PreDiscountAmount;
					ZDecimal surcharge = Utilities.Round(feeUsage.PostDiscountAmount * surchargePercent / 100m, feeUsage.Fee.Currency.Decimals);
					var taxDate = fee.CalculateTaxDate(periodStart);
					var line = new SystemBill.BillLine(amount, tax, fee.L8_RX_NKCurrency, fee.L8_ChargeCode, description, lines.Count, BillingConstants.BillingSystem.Fee, fee.L8_Type, ZGuid.Empty, taxDate);
					if (feeTypes.GetBoolFromCode(fee.L8_Type))
					{
						line.IsProcessingFeeExempt = true;
					}
					lines.Add(new FeeBillLineWithSurcharge(line, surcharge, feeUsage));
					invoiceFinder.AddBillLine(line, feeUsage);

					decimal discountAmount = feeUsage.DiscountAmount;
					if (discountAmount != 0m)
					{
						line.RequireCommentLineAfter = false;
						var discountChargeCode = feeDiscountChargeCodes.GetDescriptionFromCode(fee.L8_ChargeCode);
						var line2 = new SystemBill.BillLine(discountAmount, tax, fee.L8_RX_NKCurrency, discountChargeCode, "", lines.Count, BillingConstants.BillingSystem.Fee, fee.L8_Type, ZGuid.Empty, taxDate);
						lines.Add(new FeeBillLineWithSurcharge(line2, ZDecimal.Zero, feeUsage));
					}
				}
			}
		}

		public static Dictionary<string, DepositBalance> GetDepositChargeCodeToPositiveBalance(ZGuid orgPk)
		{
			var deposits = DepositBalanceCollection.LoadFromDb(orgPk);
			if (deposits.Any(x => !x.IsCurrent))
			{
				DepositBalance.UpdateSafe(false);
				deposits = DepositBalanceCollection.LoadFromDb(orgPk);
			}
			return deposits.Where(x => x.Amount > 0).ToDictionary(x => (string)x.ChargeCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		void CreateRevenueBreakdown(List<FeeBillLineWithSurcharge> feeLines)
		{
			var factory = InvoiceFinder.InvoiceFactory;
			foreach (var monthlyUsage in MonthlyUsages)
			{
				foreach (var usageLinesByPriceCode in monthlyUsage.UsageLines
					.Where(x => x.LocalPreDiscountAmount != 0 || x.LocalPostDiscountAmount != 0)
					.GroupBy(x => new { x.PriceItem?.L7_L6, x.PriceItemCode }))
				{
					var firstUsageLine = usageLinesByPriceCode.First();
					var hasMultipleUsagesPerLine = 0 < usageLinesByPriceCode.Where(x => x.UsageCount > 1).Sum(x => x.Usages.Sum(y => y.UnitCount));
					var priceItem = firstUsageLine.PriceItem;
					var invoice = InvoiceFinder.FindInvoice(firstUsageLine);

					if (!hasMultipleUsagesPerLine
						|| (priceItem != null && BillingConstants.FeeType.IsPerDatabaseInstance(priceItem.L7_FeeType)))
					{
						// One or less usage per price item
						// or usage is per database.
						// Easy breakdown
						foreach (var usageLine in usageLinesByPriceCode)
						{
							var billed = factory.New<EdiBilledUsage>();
							billed.BU9_AH_Invoice = invoice.PK;
							PopulateBilledUsage(billed, invoice, monthlyUsage, usageLine);
							EdiBilledDiscount.CreateBilledDiscounts(billed, usageLine.Discounts?.ToArray());
						}
					}
					else
					{
						// multiple usages per price and/or multiple prices per usage
						decimal preDiscount = 0;
						decimal postDiscount = 0;
						decimal localPreDiscount = 0;
						decimal localPostDiscount = 0;
						decimal totalUnitCount = 0;
						decimal priceAmount = 0;
						decimal totalDiscountUnits = 0;
						decimal totalAdjustPostDiscount = 0;
						foreach (var usageLine in usageLinesByPriceCode)
						{
							preDiscount += usageLine.PreDiscountAmount;
							postDiscount += usageLine.PostDiscountAmount;
							localPreDiscount += usageLine.LocalPreDiscountAmount;
							localPostDiscount += usageLine.LocalPostDiscountAmount;
							totalUnitCount += usageLine.UnitCount;
							priceAmount += usageLine.UnitCount * usageLine.Price;
							totalDiscountUnits += usageLine.UnitCount * usageLine.LicenceUnits;
							totalAdjustPostDiscount += usageLine.PostDiscountAmount - usageLine.UnadjustedPostDiscountAmount;
						}

						decimal averagePrice = priceAmount / totalUnitCount;

						if (firstUsageLine != null && invoice.AH_RX_NKTransactionCurrency != firstUsageLine.PriceCurrency && firstUsageLine.InvoiceCurrencyRateAsMultiplier != 0m)
						{
							preDiscount = Utilities.Round(preDiscount * firstUsageLine.InvoiceCurrencyRateAsMultiplier, firstUsageLine.InvoiceCurrencyDecimals);
							postDiscount = Utilities.Round(postDiscount * firstUsageLine.InvoiceCurrencyRateAsMultiplier, firstUsageLine.InvoiceCurrencyDecimals);
							totalAdjustPostDiscount = Utilities.Round(totalAdjustPostDiscount * firstUsageLine.InvoiceCurrencyRateAsMultiplier, firstUsageLine.InvoiceCurrencyDecimals);
						}

						var allUsages = usageLinesByPriceCode.SelectMany(x => x.Usages);
						var totalSubUnits = allUsages.Sum(x => x.UnitCount);
						var unitCountMultiplier = GetRevenueBreakdownUnitCountMultiplier(priceItem?.L7_Code, totalUnitCount, totalSubUnits);

						foreach (var groupedUsages in allUsages.GroupBy(x =>
							new
							{
								UsageCode = x.Code + '.' + StlBilling.GetUsageKey(x.Code, x.SubCode),
								ClientOrLicenceCompanyPk = x.Code == BillingConstants.BillingSystem.BorderWise ? x.ChargeableUsage.U1_LC : x.ClientCompanyPk,
								DatabasePk = x.Database?.PK ?? ZGuid.Empty
							}))
						{
							var subUnits = groupedUsages.Sum(x => x.UnitCount);
							if (subUnits != 0)
							{
								var firstSubUsage = groupedUsages.First();
								var bu9UnitCount = subUnits;

								if (unitCountMultiplier != 1m)
								{
									bu9UnitCount = (int)Utilities.Round(unitCountMultiplier * bu9UnitCount, 0);
								}

								var usageKey = StlBilling.GetUsageKey(firstSubUsage.Code, firstSubUsage.SubCode);
								var subCode = usageKey.Code;

								var billed = factory.New<EdiBilledUsage>();
								billed.BU9_AH_Invoice = invoice.PK;
								PopulateBilledUsage(billed, invoice, monthlyUsage, firstUsageLine);
								PopulateBilledUsage(billed, firstSubUsage);
								billed.BU9_UnitCount = bu9UnitCount;
								billed.BU9_TransactionAmountPreDiscount = preDiscount * subUnits / totalSubUnits;
								billed.BU9_TransactionAmountPostDiscount = postDiscount * subUnits / totalSubUnits;
								var surcharge = Utilities.Round(billed.BU9_TransactionAmountPostDiscount * SurchargePercent / 100m, firstUsageLine.InvoiceCurrencyDecimals);
								billed.BU9_TransactionProcessingAmount = surcharge;
								billed.BU9_TransactionAmountPostDiscount += surcharge;
								billed.BU9_LocalAmountPreDiscount = localPreDiscount * subUnits / totalSubUnits;
								billed.BU9_LocalAmountPostDiscount = localPostDiscount * subUnits / totalSubUnits;
								var localSurcharge = Utilities.Round(billed.BU9_LocalAmountPostDiscount * SurchargePercent / 100m, firstUsageLine.InvoiceCurrencyDecimals);
								billed.BU9_LocalProcessingAmount = localSurcharge;
								billed.BU9_LocalAmountPostDiscount += localSurcharge;
								billed.BU9_UnitPrice = averagePrice;
								billed.BU9_UsageSubCode = subCode;
								billed.BU9_TotalDiscountUnits = totalDiscountUnits * subUnits / totalSubUnits;
								billed.BU9_CommitmentAdjustTransactionPostDiscount = totalAdjustPostDiscount * subUnits / totalSubUnits;
								EdiBilledDiscount.CreateBilledDiscounts(billed, firstUsageLine.Discounts?.ToArray());
							}
						}
					}
				}
			}

			ZGuid feeDatabasePk = MonthlyUsages.FirstOrDefault(x => x.Database != null)?.Database.PK ?? ZGuid.Empty;
			CreateFeeRevenueBreakdown(feeLines, factory, feeDatabasePk);
		}

		void CreateFeeRevenueBreakdown(List<FeeBillLineWithSurcharge> feeLines, BusinessObjectFactory factory, ZGuid feeDatabasePk)
		{
			foreach (var feeLineGroup in feeLines.GroupBy(x => new { x.BillLine.SubCode, x.BillLine.ChargeCodeName }))
			{
				var invoice = InvoiceFinder.FindInvoice(feeLineGroup.FirstOrDefault(x => x.FeeUsage != null)?.FeeUsage);
				var billed = factory.New<EdiBilledUsage>();
				billed.BU9_AH_Invoice = invoice.PK;
				billed.BU9_PriceCurrency = InvoiceCurrencyCode;
				billed.BU9_UnitPrice = feeLineGroup.Average(x => x.BillLine.Amount);
				billed.BU9_UsageCode = BillingConstants.BillingSystem.Fee;
				billed.BU9_UsageSubCode = feeLineGroup.Key.SubCode;

				billed.BU9_PeriodStart = PeriodStart.Date;
				billed.BU9_UnitCount = feeLineGroup.Count();
				billed.BU9_TransactionAmountPreDiscount = feeLineGroup.Sum(x => x.BillLine.Amount);
				billed.BU9_TransactionProcessingAmount = feeLineGroup.Sum(x => x.SurchargeAmount);
				billed.BU9_TransactionAmountPostDiscount = billed.BU9_TransactionAmountPreDiscount + billed.BU9_TransactionProcessingAmount;

				billed.BU9_LD = feeDatabasePk;

				var localCurrencyRateAsMultiplier = 1m;
				var localCurrency = invoice.AH_Calc_LocalRXCode;
				if (InvoiceCurrencyCode != localCurrency)
				{
					localCurrencyRateAsMultiplier = CurrencyExchangeService.GetInstance(Factory, DateForExchangeRate).GetRate(InvoiceCurrencyCode, localCurrency, Factory.Load<GlbCompany>(invoice.Company?.PK ?? ZGuid.Empty));
				}

				billed.BU9_LocalAmountPreDiscount = localCurrencyRateAsMultiplier * billed.BU9_TransactionAmountPreDiscount;
				billed.BU9_LocalAmountPostDiscount = localCurrencyRateAsMultiplier * billed.BU9_TransactionAmountPostDiscount;
				billed.BU9_LocalProcessingAmount = localCurrencyRateAsMultiplier * billed.BU9_TransactionProcessingAmount;
				billed.BU9_AC_AmountChargeCode = BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, feeLineGroup.Key.ChargeCodeName);
			}
		}

		static void PopulateBilledUsage(EdiBilledUsage billed, InvoicingBase invoice, StlMonthlyUsage monthlyUsage, UsageLine usageLine)
		{
			if (usageLine.PriceItem != null)
			{
				billed.BU9_L7 = usageLine.PriceItem.PK;

				billed.BU9_AC_AmountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, usageLine.PriceItem.L7_ChargeCode);
				billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, usageLine.PriceItem.L7_DiscountChargeCode);
			}

			if (!usageLine.PriceItemCode.IsEmpty)
			{
				billed.BU9_PriceCode = usageLine.PriceItemCode;
			}

			if (!usageLine.PriceCurrency.IsEmpty)
			{
				billed.BU9_PriceCurrency = usageLine.PriceCurrency;
			}

			if (usageLine.Price != 0)
			{
				billed.BU9_UnitPrice = usageLine.Price;
			}

			var usages = (usageLine.Usages.Any() ? usageLine.Usages : usageLine.ParentUsageLine?.Usages) ?? Enumerable.Empty<Usage>();
			PopulateBilledUsage(billed, usages);

			var database = usageLine.Usages.FirstOrDefault(x => x.Database != null)?.Database
				?? monthlyUsage.Database;
			if (database != null)
			{
				billed.BU9_LD = database.PK;
				if (!database.IsEnterpriseFamilyDatabase)
				{
					billed.BU9_BillingModel = database.LD_Product;
				}
			}

			billed.BU9_PeriodStart = !usageLine.PeriodStart.IsEmpty ? usageLine.PeriodStart.Date : monthlyUsage.PeriodStart.Date;
			billed.BU9_UnitCount = (int)usageLine.UnitCount;
			billed.BU9_TotalDiscountUnits = usageLine.LicenceUnits * usageLine.TotalUnitCount;

			var surcharge = Utilities.Round(usageLine.PostDiscountAmount * monthlyUsage.Bill.SurchargePercent / 100m, usageLine.InvoiceCurrencyDecimals);
			var adjustPostDiscount = usageLine.PostDiscountAmount - usageLine.UnadjustedPostDiscountAmount;
			if (invoice.AH_RX_NKTransactionCurrency != usageLine.PriceCurrency && usageLine.InvoiceCurrencyRateAsMultiplier != 0m)
			{
				billed.BU9_TransactionAmountPostDiscount = Utilities.Round((usageLine.PostDiscountAmount + surcharge) * usageLine.InvoiceCurrencyRateAsMultiplier, usageLine.InvoiceCurrencyDecimals);
				billed.BU9_TransactionAmountPreDiscount = Utilities.Round(usageLine.PreDiscountAmount * usageLine.InvoiceCurrencyRateAsMultiplier, usageLine.InvoiceCurrencyDecimals);
				billed.BU9_TransactionProcessingAmount = Utilities.Round(surcharge * usageLine.InvoiceCurrencyRateAsMultiplier, usageLine.InvoiceCurrencyDecimals);
				billed.BU9_CommitmentAdjustTransactionPostDiscount = Utilities.Round(adjustPostDiscount * usageLine.InvoiceCurrencyRateAsMultiplier, usageLine.InvoiceCurrencyDecimals);
			}
			else
			{
				billed.BU9_TransactionAmountPostDiscount = usageLine.PostDiscountAmount + surcharge;
				billed.BU9_TransactionAmountPreDiscount = usageLine.PreDiscountAmount;
				billed.BU9_TransactionProcessingAmount = surcharge;
				billed.BU9_CommitmentAdjustTransactionPostDiscount = adjustPostDiscount;
			}

			var localSurcharge = Utilities.Round(usageLine.LocalPostDiscountAmount * monthlyUsage.Bill.SurchargePercent / 100m, usageLine.InvoiceCurrencyDecimals);
			billed.BU9_LocalAmountPostDiscount = usageLine.LocalPostDiscountAmount + localSurcharge;
			billed.BU9_LocalAmountPreDiscount = usageLine.LocalPreDiscountAmount;
			billed.BU9_LocalProcessingAmount = localSurcharge;
		}

		static void PopulateBilledUsage(EdiBilledUsage billed, IEnumerable<Usage> usages)
		{
			if (usages.Any())
			{
				var first = usages.First();
				var remaining = usages.Skip(1);

				if (first.BilledUsageCode == BillingConstants.BillingSystem.BorderWise)
				{
					billed.BU9_LC = first.ChargeableUsage.U1_LC;
				}
				else
				{
					if (!remaining.Any(x => x.ClientCompanyPk != first.ClientCompanyPk))
					{
						billed.BU9_LCC = first.ClientCompanyPk;
					}

					if (!remaining.Any(x => x.OwnerDelivery.OwnerCompany.PK != first.OwnerDelivery.OwnerCompany.PK))
					{
						billed.BU9_LC = first.OwnerDelivery.OwnerCompany.PK;
					}
				}

				if (!remaining.Any(x => x.BilledUsageCode != first.BilledUsageCode || x.SubCode != first.SubCode))
				{
					billed.BU9_UsageCode = first.BilledUsageCode;
					billed.BU9_UsageSubCode = first.SubCode;
				}
			}
		}

		static void PopulateBilledUsage(EdiBilledUsage billed, Usage usage)
		{
			if (usage != null)
			{
				billed.BU9_LCC = usage.ClientCompanyPk;
				billed.BU9_UsageCode = usage.BilledUsageCode;
				billed.BU9_UsageSubCode = usage.SubCode;
				billed.BU9_LC = usage.BilledUsageCode == BillingConstants.BillingSystem.BorderWise
					? usage.ChargeableUsage.U1_LC
					: usage.OwnerDelivery.OwnerCompany.PK;

				if (usage.Database != null)
				{
					billed.BU9_LD = usage.Database.PK;
				}
			}
		}

		static decimal GetRevenueBreakdownUnitCountMultiplier(string priceCode, decimal totalUsageLineUnitCount, decimal totalSubUsageUnitCount)
		{
			var multipler = 1m;

			if ((priceCode == BillingConstants.Hosting.DataStorageCode ||
					priceCode == BillingConstants.Hosting.eDocsStorageCode ||
					priceCode == BillingConstants.Hosting.UltraFastStorageCode ||
					priceCode == BillingConstants.Hosting.NonProductionStorageCode) && totalSubUsageUnitCount != 0m)
			{
				multipler = totalUsageLineUnitCount / totalSubUsageUnitCount;
			}

			return multipler;
		}

		public IEnumerable<Usage> Usages
		{
			get
			{
				return MonthlyUsages.SelectMany(x => x.Usages);
			}
		}

		void SetChargeableUsagesInvoice()
		{
			foreach (var monthlyUsage in MonthlyUsages)
			{
				foreach (var usageLine in monthlyUsage.UsageLines)
				{
					var invoice = InvoiceFinder.FindInvoice(usageLine);
					foreach (var usage in usageLine.Usages)
					{
						usage.CreateOrUpdateChargeableUsageForInvoice(invoice,
							monthlyUsage.PeriodStart,
							(int)usage.UnitCount,
							usageLine.Price,
							usage.OwnerDelivery != null ? usage.OwnerDelivery.OwnerCompany.PK : ZGuid.Empty);
					}
				}
			}

			if (fees != null)
			{
				foreach (var feeUsage in fees)
				{
					var invoice = InvoiceFinder.FindInvoice(feeUsage);
					feeUsage.CreateOrUpdateChargeableUsageForInvoice((ARInvoice)invoice, PeriodStart);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void CreateInvoiceLines(StlMonthlyUsage monthlyUsage, List<SystemBill.BillLine> lines,
			Dictionary<string, DepositBalance> depositChargeCodeToPositiveBalance)
		{
			var mainTaxGroup = monthlyUsage.MainTaxGroup;
			var specialTaxGroup = monthlyUsage.SpecialTaxGroup;

			var depositUseSummary = new DepositUseSummary(depositChargeCodeToPositiveBalance.Values);

			// if deposits then group by charge code and deposit code
			// 1) if two items have the same charge code, but different deposit codes then if the charge amounts are combined, then the amount to discount is not shown separately
			// 2) if two items with distinct price currencies have the same charge code then the user will add up the price total from the summary, apply the exchange rate
			//    then add up the results. It is ambiguous if rounding should occur before or after adding.
			// 3) Need amounts fully covered by deposit to add to zero in the invoice currency (and not necessarily in the local currency)

			Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal> chargeCodeToDepositUseAmount = new Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal>();
			Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal> chargeCodeToUndiscountedAmount = new Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal>();
			Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal> chargeCodeToDiscountAmount = new Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal>();
			Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal> conversionCreditDiscountChargeCodeToUseAmount = null;
			var chargeCodeToUndiscountedUsageLines = new Dictionary<Tuple<string, SystemBill.TaxGroup>, List<UsageLine>>();

			var conversionCredit = monthlyUsage.ConversionCredit;
			DepositBalance conversionCreditBalance = null;
			decimal conversionCreditAmountInInvoiceCurrency = 0;
			string conversionCreditChargeCode = "";
			if (conversionCredit != null)
			{
				conversionCreditDiscountChargeCodeToUseAmount = new Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal>();

				if (depositChargeCodeToPositiveBalance.TryGetValue(conversionCredit.CreditChargeCode, out conversionCreditBalance))
				{
					conversionCreditAmountInInvoiceCurrency = ConvertToInvoiceCurrency(conversionCreditBalance);
					conversionCreditChargeCode = conversionCreditBalance.ChargeCode;
				}
			}

			decimal totalPostDiscountInInvoiceCurrency = 0;
			foreach (var priceCurrency in monthlyUsage.PriceCurrencies)
			{
				foreach (var usageLinesByChargeCodes in monthlyUsage.UsageLines.Where(x => x.PriceCurrency == priceCurrency)
					.GroupBy(x => new
					{
						ChargeCode = (string)x.PriceItem.L7_ChargeCode,
						DepositChargeCode = depositChargeCodeToPositiveBalance.ContainsKey(x.PriceItem.L7_DepositChargeCode)
							? (string)x.PriceItem.L7_DepositChargeCode
							: (x.IsBorderWise ? "" : conversionCreditChargeCode),
						DiscountChargeCode = (string)x.PriceItem.L7_DiscountChargeCode,
						UseSpecialTax = (specialTaxGroup != null && x.PriceItem.Parent.L6_SystemCode == BillingConstants.PriceHeaderType.LDaaS)
					}))
				{
					var taxGroup = usageLinesByChargeCodes.Key.UseSpecialTax ? specialTaxGroup : mainTaxGroup;

					decimal preDiscount = usageLinesByChargeCodes.Sum(x => x.PreDiscountAmount);
					decimal postDiscount = usageLinesByChargeCodes.Sum(x => x.PostDiscountAmount);
					var firstLine = usageLinesByChargeCodes.First();
					preDiscount = Utilities.Round(preDiscount * firstLine.InvoiceCurrencyRateAsMultiplier, firstLine.InvoiceCurrencyDecimals);
					postDiscount = Utilities.Round(postDiscount * firstLine.InvoiceCurrencyRateAsMultiplier, firstLine.InvoiceCurrencyDecimals);
					totalPostDiscountInInvoiceCurrency += postDiscount;
					AddChargeCodeAmount(chargeCodeToUndiscountedAmount, usageLinesByChargeCodes.Key.ChargeCode, taxGroup, preDiscount);
					AddChargeCodeUsageLines(chargeCodeToUndiscountedUsageLines, usageLinesByChargeCodes.Key.ChargeCode, taxGroup, usageLinesByChargeCodes);
					AddChargeCodeAmount(chargeCodeToDepositUseAmount, usageLinesByChargeCodes.Key.DepositChargeCode, taxGroup, postDiscount);
					AddChargeCodeAmount(chargeCodeToDiscountAmount, usageLinesByChargeCodes.Key.DiscountChargeCode, taxGroup, postDiscount - preDiscount);

					if (conversionCreditDiscountChargeCodeToUseAmount != null && !firstLine.IsBorderWise)
					{
						if (usageLinesByChargeCodes.Key.DiscountChargeCode.Length != 0)
						{
							AddChargeCodeAmount(conversionCreditDiscountChargeCodeToUseAmount, usageLinesByChargeCodes.Key.DiscountChargeCode, taxGroup, postDiscount);
						}
						else
						{
							throw new InvalidOperationException("Price item has no discount charge code for conversion credit revenue discount: " + firstLine.PriceItem.L7_Code + " (" + firstLine.PriceItem.Parent.L6_PricelistVersion + ")");
						}
					}
				}
			}

			var lineOrder = lines.Any() ? lines.Max(x => x.Sequence) + 1 : 0;
			foreach (var pair in chargeCodeToUndiscountedAmount)
			{
				if (pair.Value != 0)
				{
					var line = new SystemBill.BillLine(pair.Value, pair.Key.Item2, InvoiceCurrencyCode, pair.Key.Item1, "", // use charge code description
						lineOrder++, BillingConstants.BillingSystem.STL);
					line.IsIncludedInPrepay = true;
					lines.Add(line);

					if (chargeCodeToUndiscountedUsageLines.TryGetValue(pair.Key, out var usageLines))
					{
						InvoiceFinder.AddBillLine(line, usageLines);
					}
				}
			}

			foreach (var pair in chargeCodeToDiscountAmount)
			{
				if (pair.Value != 0)
				{
					var line = new SystemBill.BillLine(pair.Value, pair.Key.Item2, InvoiceCurrencyCode, pair.Key.Item1, "", // use charge code description
						lineOrder++, BillingConstants.BillingSystem.STL);
					lines.Add(line);
				}
			}

			foreach (var pair in chargeCodeToDepositUseAmount.Where(x => x.Value > 0 && x.Key.Item1 != conversionCreditChargeCode))
			{
				ConvertToInvoiceCurrency(depositChargeCodeToPositiveBalance[pair.Key.Item1]);
			}

			// Conversion credits
			if (conversionCreditAmountInInvoiceCurrency > 0)
			{
				decimal totalConversionCreditUsed = 0;
				chargeCodeToDepositUseAmount.TryGetValue(new Tuple<string, SystemBill.TaxGroup>(conversionCreditChargeCode, mainTaxGroup), out totalConversionCreditUsed);
				totalConversionCreditUsed = Math.Min(totalConversionCreditUsed, conversionCreditAmountInInvoiceCurrency);
				conversionCreditAmountInInvoiceCurrency -= totalConversionCreditUsed;

				// take the amounts off the conversion credit instead of the normal deposits
				foreach (var pair in chargeCodeToDepositUseAmount.Where(x => x.Value > 0 && x.Key.Item1 != conversionCreditChargeCode)
					.ToArray())
				{
					var creditUsed = Math.Min(pair.Value, conversionCreditAmountInInvoiceCurrency);
					if (creditUsed > 0)
					{
						totalConversionCreditUsed += creditUsed;
						conversionCreditAmountInInvoiceCurrency -= creditUsed;
						chargeCodeToDepositUseAmount[pair.Key] = pair.Value - creditUsed;
					}
				}

				if (totalConversionCreditUsed > 0)
				{
					var line = new SystemBill.BillLine(-totalConversionCreditUsed, mainTaxGroup, InvoiceCurrencyCode, conversionCreditChargeCode, "", // use charge code description
						lineOrder++, BillingConstants.BillingSystem.STL);
					line.DepartmentPK = conversionCredit.LS9_GE_Department1;
					lines.Add(line);
					conversionCreditBalance.Amount -= totalConversionCreditUsed;
				}
			}

			foreach (var pair in chargeCodeToDepositUseAmount.Where(x => x.Value > 0 && x.Key.Item1 != conversionCreditChargeCode))
			{
				var depositBalance = depositChargeCodeToPositiveBalance[pair.Key.Item1];
				decimal balance = ConvertToInvoiceCurrency(depositBalance);

				var amountUsed = Math.Min(balance, pair.Value);
				if (amountUsed > 0)
				{
					var line = new SystemBill.BillLine(-amountUsed, pair.Key.Item2, InvoiceCurrencyCode, pair.Key.Item1, "",
						lineOrder++, BillingConstants.BillingSystem.STL);
					lines.Add(line);
					depositBalance.Amount -= amountUsed;
				}
			}

			if (conversionCredit != null && conversionCredit.LS9_Price > 0)
			{
				decimal revenueDiscountInInvoiceCurrency = conversionCredit.LS9_Price;
				if (conversionCredit.LS9_RX_NKPriceCurrency != InvoiceCurrencyCode)
				{
					revenueDiscountInInvoiceCurrency = CurrencyExchange.GetAmount(conversionCredit.LS9_RX_NKPriceCurrency,
						InvoiceCurrencyCode, InvoicingBranch.Company, revenueDiscountInInvoiceCurrency);
				}

				foreach (var pair in conversionCreditDiscountChargeCodeToUseAmount.Where(x => x.Value > 0))
				{
					var splitAmount = Utilities.Round(revenueDiscountInInvoiceCurrency * pair.Value / totalPostDiscountInInvoiceCurrency, BillingConstants.RoundingDecimals);
					var line = new SystemBill.BillLine(-splitAmount, pair.Key.Item2, InvoiceCurrencyCode, pair.Key.Item1, "",
						lineOrder++, BillingConstants.BillingSystem.STL);
					lines.Add(line);

					line = new SystemBill.BillLine(+splitAmount, pair.Key.Item2, InvoiceCurrencyCode, pair.Key.Item1, "",
						lineOrder++, BillingConstants.BillingSystem.STL);
					line.DepartmentPK = conversionCredit.LS9_GE_Department2;
					lines.Add(line);
				}
			}

			depositUseSummary.SetClosingBalance(depositChargeCodeToPositiveBalance.Values, InvoicingBranch, CurrencyExchange);
			monthlyUsage.DepositSummary = depositUseSummary;
		}

		CurrencyExchangeService CurrencyExchange
		{
			get { return CurrencyExchangeService.GetInstance(Factory, DateForExchangeRate); }
		}

		decimal ConvertToInvoiceCurrency(DepositBalance depositBalance)
		{
			// Ignoring the tax balance since it's not used to create the invoice.
			decimal balance = depositBalance.Amount;
			if (depositBalance.CurrencyCode != InvoiceCurrencyCode)
			{
				balance = CurrencyExchange.GetAmount(depositBalance.CurrencyCode, InvoiceCurrencyCode, InvoicingBranch.Company, balance);
				depositBalance.Amount = balance;
				depositBalance.CurrencyCode = InvoiceCurrencyCode;
			}

			return balance;
		}

		static void AddChargeCodeAmount(Dictionary<Tuple<string, SystemBill.TaxGroup>, decimal> chargeCodeToAmount, string chargeCode, SystemBill.TaxGroup tax, decimal amount)
		{
			if (!string.IsNullOrEmpty(chargeCode))
			{
				var key = new Tuple<string, SystemBill.TaxGroup>(chargeCode, tax);
				decimal total;
				chargeCodeToAmount.TryGetValue(key, out total);
				total += amount;
				chargeCodeToAmount[key] = total;
			}
		}

		static void AddChargeCodeUsageLines(Dictionary<Tuple<string, SystemBill.TaxGroup>, List<UsageLine>> chargeCodeToUsageLines, string chargeCode, SystemBill.TaxGroup tax, IEnumerable<UsageLine> usageLines)
		{
			if (!string.IsNullOrEmpty(chargeCode))
			{
				chargeCodeToUsageLines.GetOrAdd(Tuple.Create(chargeCode, tax), () => new List<UsageLine>()).AddRange(usageLines);
			}
		}

		void AddComments(InvoicingBase invoice)
		{
			ZString comment = MonthlyUsageInvoiceComment;
			if (!comment.IsEmpty)
			{
				BillingInvoicingHelper.AddCommentLine(invoice, comment);
			}
			BillingInvoicingHelper.AddCommentLine(invoice, EDIDataRegistry.Instance.PrepaymentInvoiceComment.Value);
			BillingInvoicingHelper.AddCommentLine(invoice, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);
		}

		public ZString MonthlyUsageInvoiceComment
		{
			get
			{
				var selfBilling = LicCompany != null ? LicCompany.ReadonlySelfBilling : null;
				ZString result = selfBilling != null ? selfBilling.L4_InvoiceCommentMultilingual : ZString.Empty;
				if (result.IsEmpty)
				{
					result = EDIDataRegistry.Instance.MonthlyUsageInvoiceComment.GetFallBackValueAtAllLevels(InvoicingBranch.GB_GC.ToGuid(), InvoicingBranch.PK.ToGuid(), Guid.Empty);
				}
				return result;
			}
		}

		void CalculateNextPrepay(IEnumerable<InvoicingBase> invoices, decimal amountToPrepayIncTaxInvoiceCurrency)
		{
			var currencyExchange = CurrencyExchangeService.GetInstance(Factory, DateForExchangeRate);
			Func<string, decimal, decimal> getAmountInInvoiceCurrency = (sourceCurrency, sourceAmount) =>
			{
				return (sourceAmount == 0m || sourceCurrency == InvoiceCurrencyCode) ?
					sourceAmount :
					currencyExchange.GetAmount(sourceCurrency, InvoiceCurrencyCode, InvoicingBranch.Company, sourceAmount);
			};

			PrepayNext.CurrentInvoiceTotalAmount = invoices.Where(x => !x.IsARCreditNote).Sum(x => x.AH_OSTotalAmount);
			PrepayNext.CurrentInvoiceTotalTaxAmount = invoices.Where(x => !x.IsARCreditNote).Sum(x => x.AH_OSTaxAmount);

			if (Prepaid != null)
			{
				PrepayNext.PrepaymentBalanceRequired = getAmountInInvoiceCurrency(Prepaid.PredeterminedPrepaidBalanceCurrency, Prepaid.PredeterminedPrepaidBalance);

				if (Prepaid.ShouldUseOutstandingBalanceAsPrepaidDepositBalance)
				{
					PrepayNext.CurrentPrepaymentBalance = getAmountInInvoiceCurrency(InvoicingBranch.Company.GC_RX_NKLocalCurrency, OutstandingBalanceExDepositLocal);
				}
				else
				{
					PrepayNext.CurrentPrepaymentBalance = getAmountInInvoiceCurrency(Prepaid.PrepaidDepositBalanceCurrency, Prepaid.PrepaidDepositBalance);
				}
				PrepayNext.FuturePrepaymentBalanceRequired = getAmountInInvoiceCurrency(Prepaid.FuturePredeterminedPrepaidBalanceCurrency, Prepaid.FuturePredeterminedPrepaidBalance);
			}

			if (MonthlyUsages.Any(x => x.IsPrepaymentDiscountAvailable))
			{
				var usageInvoice = invoices.First().Factory.New<EdiUsageInvoice>(); //the first invoice is always the STL invoice
				usageInvoice.EUI_AH_Invoice = invoices.First().PK;
				usageInvoice.EUI_PeriodStart = PeriodStart.Date;
				usageInvoice.EUI_PrepayAmountIncTax = amountToPrepayIncTaxInvoiceCurrency;
				usageInvoice.EUI_AccountBalanceIncTax = PrepayNext.CurrentPrepaymentBalance;
			}
		}

		#endregion

		#region Attachments / Summary Doc

		public const string ParentSummaryDocType = "OD1";

		protected void AddAttachments(InvoicingBase invoice)
		{
			foreach (var monthlyUsage in MonthlyUsages)
			{
				AddAttachment(invoice, monthlyUsage);
			}
		}

		void AddAttachment(InvoicingBase invoice, StlMonthlyUsage monthlyUsage)
		{
			var docWrapper = DocStlSummary.New(monthlyUsage, invoice.Factory);
			if (!docWrapper.IsEmpty)
			{
				ZString summaryDocType = ParentSummaryDocType;
				ZString summaryFileName = monthlyUsage.EnterpriseCode + monthlyUsage.ServerCode + (NoResString)" " + monthlyUsage.PeriodStart.ToString((NoResString)"MMMM yyyy", CultureInfo.InvariantCulture) + (NoResString)" Billing Summary";

				var docTemplate = LoadSummaryTemplate(Factory);
				string language = monthlyUsage.Bill?.InvoiceLanguage;
				BillingInvoicingHelper.AddAttachmentInPdf(invoice, docTemplate, docWrapper, summaryFileName, summaryDocType, language);
				BillingInvoicingHelper.AddAttachmentInExcel(invoice, docTemplate, docWrapper, summaryFileName, summaryDocType, language);
			}
			docWrapper.ItemLines.RemoveAll();
		}

		public static StmTemplate LoadSummaryTemplate(BusinessObjectFactory factory)
		{
			return LoadStmTemplate(factory, (NoResString)"STL Billing Summary");
		}

		static StmTemplate LoadStmTemplate(BusinessObjectFactory factory, ZString templateName)
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, Enterprise.Core.Constants.DataContext.CargoWiseBilling);

			return factory.LoadTop1<StmTemplate>(query);
		}

		void OnInvoiceSaved(ARInvoice invoice)
		{
			if (!Globals.IsTest)
			{
				var unsavedDocs = invoice.DocManagerInfo.Files.OfType<StorageDocsBase>().Where(x => !x.IsInDatabase).ToArray();
				if (unsavedDocs.Any())
				{
					invoice.DocManagerInfo.MasterFactory.Save();
					var unsavedDocsAfterSave = unsavedDocs.Where(x => !x.IsInDatabase);
					var errorMessage = new ZStringBuilder();
					errorMessage.AppendLine($"TX#:{invoice.AH_TransactionNum}");
					errorMessage.AppendLine(",unsavedDocs:" + string.Join("|", unsavedDocs.Select(x => $"{x.PK}@{x.Name}")));
					errorMessage.AppendLine(",unsavedDocsAfterSave:" + string.Join("|", unsavedDocsAfterSave.Select(x => $"{x.PK}@{x.Name}")));
					ExceptionReporter.Instance.ReportDeveloperException("StlBill.OnInvoiceSaved.UnsavedDocs", errorMessage.ToString(), new InvalidOperationException(errorMessage.ToString()));
				}
			}
		}

		#endregion

		#region Fees

		FeeUsage[] fees;

		/// <summary>
		/// All the fees on this bill. Some may also appear on MonthlyUsage if they have a database assigned.
		/// </summary>
		/// <param name="feesToAdd"></param>
		public void AddFees(IEnumerable<FeeUsage> feesToAdd)
		{
			if (feesToAdd != null)
			{
				taxCodesCalculated = false;
				this.fees = feesToAdd.ToArray();
			}
		}

		public IEnumerable<FeeUsage> FeeUsages
		{
			get
			{
				return fees ?? (fees = Array.Empty<FeeUsage>());
			}
		}

		public ZInt FeeCount
		{
			get
			{
				return fees != null ? fees.Length : 0;
			}
		}

		#endregion

		#region Language

		public readonly ZString InvoiceLanguage;
		public readonly CultureInfo InvoiceCultureInfo;

		public IDisposable TemporarilySwitchToInvoiceLanguage()
		{
			return InvoiceLanguage != Res.DefaultLanguage ? Res.TemporarilySwitchLanguage(InvoiceLanguage) : null;
		}

		#endregion

		#region Accounting.TaxFramework 

		IEnumerable<ARInvoice> ProcessTax(ARInvoice invoice)
		{
			var processor = GetInvoiceTaxProcessor();
			return processor.Process(invoice);
		}

		protected virtual EDIARInvoiceTaxProcessor GetInvoiceTaxProcessor() => new EDIARInvoiceTaxProcessor();

		static ZString JoinTransactionNums(IEnumerable<InvoiceSummary> summaries)
			=> summaries == null ? ZString.Empty : ZString.Join(", ", summaries.Select(x => x.AH_TransactionNum).OrderBy(x => x).ToArray());

		readonly StlInvoiceFinder InvoiceFinder = new StlInvoiceFinder();

		#endregion

		public ZBool HasPrepaid { get; set; }
		public ZString PrepayCurrency { get { return Prepaid?.PrepayCurrency ?? string.Empty; } }
		public ZDecimal PrepayBalanceRequiredIncTax { get { return Prepaid?.BalanceRequiredIncTax ?? 0m; } }
		public ZDecimal PrepayBalanceActualIncTax { get { return Prepaid?.BalanceActualIncTax ?? 0m; } }

		public ZDecimal OutstandingBalanceExDepositLocal { get; set; }
		public ZDecimal DepositAvailableLocal { get; set; }

		public class PrepayInfo
		{
			public decimal CurrentPrepaymentBalance { get; set; }
			public decimal PrepaymentBalanceRequired { get; set; }
			public decimal FuturePrepaymentBalanceRequired { get; set; }
			public decimal CurrentInvoiceTotalAmount { get; set; }
			public decimal CurrentInvoiceTotalTaxAmount { get; set; }
		}

		public class PrepaidDetail
		{
			public string PrepayCurrency { get; set; }
			public decimal BalanceRequiredIncTax { get; set; }
			public decimal BalanceActualIncTax { get; set; }

			public string PredeterminedPrepaidBalanceCurrency { get; set; }
			public decimal PredeterminedPrepaidBalance { get; set; }
			public string FuturePredeterminedPrepaidBalanceCurrency { get; set; }
			public decimal FuturePredeterminedPrepaidBalance { get; set; }
			public string PrepaidDepositBalanceCurrency { get; set; }
			public decimal PrepaidDepositBalance { get; set; }
			public bool ShouldUseOutstandingBalanceAsPrepaidDepositBalance { get; set; } = true;
		}

		public PrepayInfo PrepayNext = new PrepayInfo();

		public PrepaidDetail Prepaid { get; set; }

		class FeeBillLineWithSurcharge
		{
			public FeeBillLineWithSurcharge(SystemBill.BillLine billLine, ZDecimal surchargeAmount, FeeUsage feeUsage)
			{
				BillLine = billLine;
				SurchargeAmount = surchargeAmount;
				FeeUsage = feeUsage;
			}

			public readonly SystemBill.BillLine BillLine;
			public ZDecimal SurchargeAmount { get; set; }
			public readonly FeeUsage FeeUsage;
		}
	}
}

