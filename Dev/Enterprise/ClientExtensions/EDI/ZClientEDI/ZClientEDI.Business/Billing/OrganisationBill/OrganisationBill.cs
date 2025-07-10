using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class OrganisationBill : BillRecipient, IDocumentSupportable
	{
		/// <summary>
		/// Current environment branch should be invoice branch and the factory should be created for that branch
		/// </summary>
		public OrganisationBill(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dateForExchange, BillingRunContext billingRunContext = null)
			: base(factory, branchPK, orgPK, currencyCode, dateForExchange)
		{
			RunContext = billingRunContext;
		}

		public readonly BillingRunContext RunContext;

		public ZDateTime DateTo { get; set; }

		public ZDateTime PeriodStart
		{
			get
			{
				var d = DateTo;
				return new ZDateTime(d.Year, d.Month, 1);
			}
		}

		public ZBool CanInvoice
		{
			get { return canInvoice ?? (canInvoice = (InvoicePkForThisMonth.IsEmpty && !IsTooSmallToBill && !HasErrors && IsBilled)).Value; }
		}
		ZBool? canInvoice;

		void InvalidateCanInvoice()
		{
			canInvoice = null;
		}

		public ZBool IsBilled
		{
			get { return systemBills.Count > 0 ? systemBills[0].IsBilled : ZBool.True; }
		}

		public ZBool IsBillable
		{
			get { return systemBills.Count > 0 ? systemBills[0].IsBillable : ZBool.True; }
		}

		#region Status

		public static class StatusMessages
		{
			public const string Error = "1 - Error";
			public const string Warning = "2 - Warning";
			public const string NotBilled = "3 - Not Billable Delivery";
			public const string NotBillable = "3 - Not Billable Database";
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
				ZString result = "";

				if (!InvoicePkForThisMonth.IsEmpty)
				{
					result = StatusMessages.Invoiced;
				}
				else if (!IsBilled)
				{
					result = StatusMessages.NotBilled;
				}
				else if (!IsBillable)
				{
					result = StatusMessages.NotBillable;
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
				else if (IsInvoicedByPartner)
				{
					result = StatusMessages.ReadyPartner;
				}
				else if (InvoiceForLastMonth == null)
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

		#region LicenceMode

		public ZString LicenceMode
		{
			get
			{
				if (!isLicenceModeCalculated)
				{
					isLicenceModeCalculated = true;
					licenceMode = CalculateLicenceMode();
				}
				return licenceMode;
			}
		}

		ZString licenceMode;
		bool isLicenceModeCalculated;

		string CalculateLicenceMode()
		{
			string mode = null;
			foreach (SystemUsage usage in SystemUsages)
			{
				if (string.IsNullOrEmpty(mode))
				{
					mode = usage.LicenceMode;
				}
				else if (mode != usage.LicenceMode && usage.LicenceMode != "")
				{
					mode = MonthlyUsageBilling.LicenceModeConstants.Codes.ALL;
					break;
				}
			}

			return mode;
		}

		#endregion

		#region ex View Model properties

		public ZString InvoicingBranchCode
		{
			get
			{
				if (invoicingBranchCode.IsDefault)
				{
					var branch = InvoicingBranch;
					invoicingBranchCode = branch != null ? branch.GB_Code : ZString.Empty;
				}

				return invoicingBranchCode;
			}
		}
		ZString invoicingBranchCode;

		public ZString InvoiceNumber
		{
			get; private set;
		}

		public ZString LastMonthInvoiceNumber
		{
			get { return InvoiceForLastMonth != null ? InvoiceForLastMonth.AH_TransactionNum : ZString.Empty; }
		}

		public ZDecimal LastMonthInvoiceAmount
		{
			get { return lastMonthInvoiceAmount ?? (lastMonthInvoiceAmount = InvoiceForLastMonth != null ? InvoiceForLastMonth.AH_OSExTaxAmount : ZDecimal.Zero).Value; }
		}
		ZDecimal? lastMonthInvoiceAmount;

		#endregion

		#region System Bills

		public SystemBillCollection SystemBills
		{
			get { return systemBills ?? (systemBills = new SystemBillCollection(Factory)); }
		}
		SystemBillCollection systemBills;

		public void AddSystemBill(SystemBill systemBill)
		{
			SystemBills.Add(systemBill);
			SystemUsages.AddRange(systemBill.SystemUsages);
		}

		#endregion

		#region System Usage List

		public SystemUsageCollection SystemUsages
		{
			get { return systemUsages ?? (systemUsages = new SystemUsageCollection(Factory)); }
		}

		SystemUsageCollection systemUsages;

		#endregion

		#region Amount Properties

		/// <summary>
		/// Amount before discount - only for display purposes
		/// 
		/// </summary>
		public ZDecimal Amount { get; private set; }

		/// <summary>
		/// Total discount - only for display purposes
		/// </summary>
		public ZDecimal DiscountAmount { get; private set; }

		public ZDecimal SurchargeAmount { get; private set; }

		/// <summary>
		/// TotalAmount = Amount - DiscountAmount
		/// </summary>
		public ZDecimal TotalAmount { get; private set; }

		public ZDecimal AmountForProcessingFee { get; private set; }
		public ZDecimal ProcessingFeeAmount { get; private set; }
		public ZDecimal ProcessingFeePercent { get; private set; }
		public ZString ProcessingFeeCode { get; private set; }
		public ZDecimal TotalDue { get; private set; }

		public ZDecimal SystemMinimumFeeAmount { get; private set; }
		public List<SystemMinimumFee> SystemMinimumFees
		{
			get { return systemMinimumFees ?? (systemMinimumFees = new List<SystemMinimumFee>()); }
		}
		List<SystemMinimumFee> systemMinimumFees;

		void CalculateAmounts()
		{
			Amount = 0;
			DiscountAmount = 0;
			SurchargeAmount = 0;
			TotalAmount = 0;
			ZDecimal amountExemptProcessingFee = 0;

			var processingFeeExemptBillingSystems = EDIDataRegistry.Instance.ProcessingFeeExemptBillingSystems.Value;

			foreach (SystemBill systemBill in SystemBills)
			{
				ZString currencyCode = systemBill.CurrencyCode;
				var billAmountInInvoiceCurrency = AmountInInvoiceCurrency(systemBill.Amount, currencyCode);
				Amount += billAmountInInvoiceCurrency;
				if (processingFeeExemptBillingSystems.ContainsCode(systemBill.SystemCode))
				{
					amountExemptProcessingFee += billAmountInInvoiceCurrency;
				}
				else
				{
					amountExemptProcessingFee += AmountInInvoiceCurrency(systemBill.AmountExemptProcessingFee, currencyCode);
				}
				DiscountAmount += AmountInInvoiceCurrency(systemBill.DiscountAmount, currencyCode);
				SurchargeAmount += AmountInInvoiceCurrency(systemBill.SurchargeAmount, currencyCode);
				TotalAmount += AmountInInvoiceCurrency(systemBill.TotalAmount, currencyCode);
			}

			CalculateSystemMinimumFees();
			SystemMinimumFeeAmount = SystemMinimumFees.Sum(x => x.Amount);
			TotalAmount += SystemMinimumFeeAmount;

			// Processing discount is a percentage of the amount before the deposit is deducted.
			// Processing fee is a percentage of the amount after the deposit is deducted.
			AmountForProcessingFee = TotalAmount - amountExemptProcessingFee;
			if (!IsInvoicedByPartner && SelfBilling != null)
			{
				ProcessingFeeCode = SelfBilling.L4_ProcessingFee;
				ProcessingFeePercent = SelfBilling.L4_ProcessingFeePercent;

				if (ProcessingFeePercent != 0m && ProcessingFeeCode.EqualsIgnoringCase(BillingConstants.DiscountType.Prepayment) && !ShouldApplyPrepaymentDiscount)
				{
					ProcessingFeePercent = 0m;
				}

				if (EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.GetBoolFromCode(ProcessingFeeCode))
				{
					ProcessingFeePercent = -ProcessingFeePercent;
				}
			}
			var workingProcessingFeeAmount = !IsInvoicedByPartner ? CalculateProcessingFeeAmount(AmountForProcessingFee) : ZDecimal.Zero;
			ZDecimal amountBeforeDepositDeducted = TotalAmount;
			if (workingProcessingFeeAmount < 0)
			{
				amountBeforeDepositDeducted += workingProcessingFeeAmount;
			}

			var depositInInvoiceCurrency = AmountInInvoiceCurrency(Deposit, DepositCurrencyCode);
			DepositDeductedInInvoiceCurrency = Math.Min(depositInInvoiceCurrency, amountBeforeDepositDeducted);
			if (DepositCurrencyCode != InvoiceCurrencyCode)
			{
				DepositDeducted = Math.Min(Deposit, BillingInvoicingHelper.GetAmountInInvoiceCurrency(DepositDeductedInInvoiceCurrency, DateForExchangeRate, InvoiceCurrencyCode, DepositCurrencyCode, InvoicingBranch, Factory));
			}
			else
			{
				DepositDeducted = DepositDeductedInInvoiceCurrency;
			}

			if (workingProcessingFeeAmount > 0)
			{
				AmountForProcessingFee = Math.Max(0m, AmountForProcessingFee - DepositDeductedInInvoiceCurrency);
				workingProcessingFeeAmount = CalculateProcessingFeeAmount(AmountForProcessingFee);
			}

			ProcessingFeeAmount = workingProcessingFeeAmount;
			TotalDue = TotalAmount - DepositDeductedInInvoiceCurrency + ProcessingFeeAmount;
		}

		ClientInvoiceDelivery InvoiceDeliveryForMinimumFee { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculateSystemMinimumFees()
		{
			SystemMinimumFees.Clear();

			var minFeeSystemUsages = SystemBills.Cast<SystemBill>()
				.SelectMany(x => x.SystemUsages)
				.Where(x => x.IsMinimumFeeOwner)
				.ToList();

			var databaseMinimumFeeContribution = minFeeSystemUsages.Any() ? DatabaseMinimumFeeContribution() : null;

			foreach (var minFeeOwnerUsage in minFeeSystemUsages)
			{
				if (InvoiceDeliveryForMinimumFee == null)
				{
					InvoiceDeliveryForMinimumFee = minFeeOwnerUsage.InvoiceDelivery;
				}
				var priceHeader = minFeeOwnerUsage.PriceHeader;
				var dbPk = minFeeOwnerUsage.User.DatabasePK;
				var db = Factory.Load<LicenceDatabase>(dbPk);

				var minimumFeePriceItem = priceHeader.LocalOrStandardItems.FindByCode("#MF");
				var nonProductionFeePriceItem = !priceHeader.L6_TestDbPriceCode.IsEmpty ? priceHeader.LocalOrStandardItems.FindByCode(priceHeader.L6_TestDbPriceCode) : null;
				var nonProductionSystemBilling = new SystemLicenceBilling();

				var billPeriodStart = minFeeOwnerUsage.PeriodStart;

				var contributionGroup = databaseMinimumFeeContribution
					.Where(x => x.PeriodStart == billPeriodStart
						&& x.DatabasePk == minFeeOwnerUsage.User.DatabasePK);

				if (minimumFeePriceItem != null)
				{
					var minimumFeeCurrencyCode = minimumFeePriceItem.L7_RX_NKCurrency.IsEmpty ? priceHeader.L6_RX_NKCurrency : minimumFeePriceItem.L7_RX_NKCurrency;
					var minimumFeeInInvoiceCurrency = AmountInInvoiceCurrency(minimumFeePriceItem.L7_Price, minimumFeeCurrencyCode);

					var minimumFees = RunContext?.SystemMinimumFeesService?.GetSystemMinimumFeesByDatabase(dbPk, billPeriodStart) ?? contributionGroup;
					var dbUsageAmountInInvoiceCurrency = minimumFees.Sum(x => AmountInInvoiceCurrency(x.Amount, x.Currency));

					if (minimumFeeInInvoiceCurrency > 0 && minimumFeeInInvoiceCurrency > dbUsageAmountInInvoiceCurrency)
					{
						var systemMinimumFeeAdjustment = new SystemMinimumFee(dbPk, billPeriodStart, minimumFeeInInvoiceCurrency - dbUsageAmountInInvoiceCurrency, InvoiceCurrencyCode, minimumFeePriceItem.L7_ChargeCode, minimumFeePriceItem);
						SystemMinimumFees.Add(systemMinimumFeeAdjustment);
					}
				}

				if (nonProductionFeePriceItem != null)
				{
					var dbOwnerPk = DatabaseOwnerHelper.GetDatabaseOwner(dbPk.ToGuid());
					var dbOwner = Factory.Load<LicenceCompany>(dbOwnerPk);
					if (dbOwner != null)
					{
						var delivery = dbOwner.InvoiceDeliveries.FindByServerAndSystem(db.LD_ServerCode, BillingConstants.BillingSystem.ODM);
						ZGuid payer = dbOwner.LC_OH;
						if (delivery != null && !delivery.L9_OH_InvoiceTo.IsEmpty)
						{
							payer = delivery.L9_OH_InvoiceTo;
						}

						if (payer == OrganisationPK)
						{
							var testDbPks = nonProductionSystemBilling.CalculateMinimumFees(Factory, db, priceHeader, billPeriodStart);
							foreach (var testDbPk in testDbPks)
							{
								var nonProductionSystemFeeCurrencyCode = nonProductionFeePriceItem.L7_RX_NKCurrency.IsEmpty ? priceHeader.L6_RX_NKCurrency : nonProductionFeePriceItem.L7_RX_NKCurrency;
								var nonProductionSystemFeeInInvoiceCurrency = AmountInInvoiceCurrency(nonProductionFeePriceItem.L7_Price, nonProductionSystemFeeCurrencyCode);

								var nonProductionSystemFee = new SystemMinimumFee(testDbPk, billPeriodStart, nonProductionSystemFeeInInvoiceCurrency, InvoiceCurrencyCode, nonProductionFeePriceItem.L7_ChargeCode, nonProductionFeePriceItem, isNonProductionSystemFee: true);
								SystemMinimumFees.Add(nonProductionSystemFee);
							}
						}
					}
				}
			}
		}

		List<SystemMinimumFee> DatabaseMinimumFeeContribution()
		{
			var databaseMinimumFeeContribution = new List<SystemMinimumFee>(SystemBills.Count);
			foreach (SystemBill systemBill in SystemBills)
			{
				var systemMinimumFeeContributionBill = systemBill as ISystemMinimumFeeContributionBill;
				if (systemMinimumFeeContributionBill != null)
				{
					var systemMinimumFeeContribution = systemMinimumFeeContributionBill.CalculateMinimumFeeContribution();
					databaseMinimumFeeContribution.AddRange(systemMinimumFeeContribution);
				}
			}
			return databaseMinimumFeeContribution;
		}

		public void CalculateAll(ZDecimal minimumAmountToBill)
		{
			CalculateExchangeRates();
			CalculateDeposit();
			CalculateAmounts();
			CheckExistingInvoice();

			ClearAllNotifications();
			base.AddRowNotifications(this);
			ValidateAll();
			ApplyMinimumAmountToBill(minimumAmountToBill);
		}

		#endregion

		#region Deposit

		void CalculateDeposit()
		{
			IsDepositValid = true;
			IsDepositExchangeRatesValid = true;
			if (IsInvoicedByPartner
				|| LicCompany == null)
			{
				Deposit = 0m;
			}
			else
			{
				Deposit = Math.Max(LicCompany.MonthlyUsageDepositBalance, 0m);
				DepositCurrencyCode = LicCompany.MonthlyUsageDepositCurrency;
				IsDepositValid = LicCompany.IsMonthlyUsageDepositValid;
				if (Deposit > 0 && string.IsNullOrEmpty(DepositCurrencyCode))
				{
					if (OdplBill != null)
					{
						DepositCurrencyCode = OdplBill.CurrencyCode;
					}
					else if (systemBills.Count > 0)
					{
						DepositCurrencyCode = systemBills[0].CurrencyCode;
					}
				}

				if (Deposit != 0 && AmountInInvoiceCurrency(Deposit, DepositCurrencyCode) == 0m)
				{
					IsDepositExchangeRatesValid = false;
				}
			}
		}

		public ZDecimal Deposit { get; private set; }
		public ZDecimal DepositDeducted { get; private set; }
		public ZDecimal DepositDeductedInInvoiceCurrency { get; private set; }
		public ZString DepositCurrencyCode { get; private set; }
		ZBool IsDepositValid { get; set; }
		ZBool IsDepositExchangeRatesValid { get; set; }

		public ZBool OdplCommitmentOnly
		{
			get
			{
				var bill = OdplBill as OdplSystemBill;
				return bill != null && bill.OdplCommitmentOnly;
			}
		}

		SystemBill OdplBill
		{
			get { return SystemBills.Cast<SystemBill>().FirstOrDefault(x => x.SystemCode == BillingConstants.BillingSystem.ODM); }
		}

		#endregion

		#region Minumum Amount To Bill

		void ApplyMinimumAmountToBill(ZDecimal minAmount)
		{
			IsTooSmallToBill = !IsInvoicedByPartner
				&& !HasErrors
				&& TotalDue < minAmount
				&& TotalDue > 0
				&& DepositDeducted < minAmount;

			if (IsTooSmallToBill)
			{
				// If there are 100% discounts, see if we can drop some small amounts to make a zero amount invoice
				List<SystemBill> zeroBills = new List<SystemBill>();
				foreach (SystemBill bill in SystemBills)
				{
					if (bill.TotalAmount == 0)
					{
						zeroBills.Add(bill);
					}
				}

				if (zeroBills.Count > 0 && zeroBills.Sum(x => x.Amount) >= minAmount)
				{
					SystemBills.RemoveAll();
					SystemUsages.RemoveAll();
					foreach (SystemBill bill in zeroBills)
					{
						AddSystemBill(bill);
					}
					CalculateAmounts();
					if (TotalDue == 0m)
					{
						IsTooSmallToBill = false;
					}
				}
			}
		}

		public ZBool IsTooSmallToBill { get; private set; }

		#endregion

		#region Existing Invoices

		public void SetEnabledSystemCodes(IEnumerable<string> codes)
		{
			enabledSystemCodes = codes;
		}

		IEnumerable<string> enabledSystemCodes;

		public ARInvoice InvoiceForLastMonth
		{
			get
			{
				if (!isInvoiceForLastMonthCalculated)
				{
					isInvoiceForLastMonthCalculated = true;
					invoiceForLastMonth = InvoiceForMonth(new ZDateTime(DateTo.Year, DateTo.Month, 1).AddMonths(-1));
				}
				return invoiceForLastMonth;
			}
		}
		ARInvoice invoiceForLastMonth;
		bool isInvoiceForLastMonthCalculated;

		public ZBool IsNewUser
		{
			get { return InvoiceForLastMonth == null && !IsInvoicedByPartner; }
		}

		ARInvoice InvoiceForMonth(ZDateTime date)
		{
			ARInvoice result = null;
			var start = new ZDateTime(date.Year, date.Month, 1);
			ClientChargeableUsage lastUsage = ClientChargeableUsage.LastGroupUsageInDateRange(Factory, Organisation, start, start.AddMonths(1), enabledSystemCodes);
			if (lastUsage != null)
			{
				var invoice = lastUsage.Invoice;
				if (invoice != null && !invoice.IsCancelled)
				{
					result = invoice;
				}
			}

			return result;
		}

		#endregion

		#region Exchange Rates/Currency Calculation

		void CalculateExchangeRates()
		{
			ExchangeRatesFound = true;

			if (InvoicingBranch != null)
			{
				IEnumerable<ZString> allCurrenciesToExchange = SystemUsages.Cast<SystemUsage>()
					.Where(x => x.Amount != 0 & 0 != string.Compare(x.CurrencyCode, InvoiceCurrencyCode, StringComparison.OrdinalIgnoreCase))
					.Select(x => x.CurrencyCode.ToUpper())
					.Distinct()
					.OrderBy(s => s);

				foreach (ZString currencyToExchange in allCurrenciesToExchange)
				{
					ZDecimal exchangeRate = BillingInvoicingHelper.GetExchangeRate(DateForExchangeRate, currencyToExchange, InvoiceCurrencyCode, InvoicingBranch);
					ExchangeRates.Add(currencyToExchange, exchangeRate);
					if (exchangeRate == 0m)
					{
						ExchangeRatesFound = false;
					}
				}
			}
		}

		bool ExchangeRatesFound { get; set; }

		Dictionary<ZString, ZDecimal> ExchangeRates
		{
			get { return exchangeRates ?? (exchangeRates = new Dictionary<ZString, ZDecimal>()); }
		}
		Dictionary<ZString, ZDecimal> exchangeRates;

		void AddExchangeRatesNotes(ARInvoice invoice)
		{
			foreach (var currencyExchangeRate in ExchangeRates)
			{
				string exchangeNote = string.Format(CultureInfo.InvariantCulture, "Exchange rate used: 1 {0} = {1} {2}",
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
			get { return "Organisation Bill"; }
		}

		public new static class ValidationMessages
		{
			public const string SubsidiaryHaveDeposit = "Subsidiary organisation have a deposit balance. It should be transferred to paying organisation.";
		}

		public void ValidateAll()
		{
			InvalidateCanInvoice();

			if (!IsInvoicedByPartner && OdplBill != null)
			{
				ValidateSubsidiaryDeposits();
			}

			ValidateExchangeRates();
			ValidateDeposit();

			foreach (SystemBill systemBill in SystemBills)
			{
				systemBill.ValidateAll(this);
			}
		}

		List<ZGuid> ChargeableUsagePKs
		{
			get
			{
				var chargeableUsagePKs = new List<ZGuid>();
				foreach (SystemBill systemBill in SystemBills)
				{
					foreach (var usage in systemBill.SystemUsages)
					{
						chargeableUsagePKs.AddRange(usage.ChargeableUsagePKs);
					}
				}
				return chargeableUsagePKs;
			}
		}

		List<ClientChargeableUsage> ChargeableUsages
		{
			get
			{
				List<ClientChargeableUsage> result = new List<ClientChargeableUsage>();

				List<ZGuid> chargeableUsagePKs = ChargeableUsagePKs;
				foreach (ZGuid chargeableUsagePk in chargeableUsagePKs)
				{
					Factory.AddFetchHint(typeof(ClientChargeableUsage), chargeableUsagePk);
				}

				foreach (ZGuid chargeableUsagePk in chargeableUsagePKs)
				{
					result.Add(Factory.Load<ClientChargeableUsage>(chargeableUsagePk));
				}

				return result;
			}
		}

		ARInvoice[] GetNonCancelledInvoices(List<ClientChargeableUsage> chargeableUsages)
		{
			return chargeableUsages.Select(s => s.Invoice).Where(s => s != null && !s.AH_IsCancelled).ToArray();
		}

		void CheckExistingInvoice()
		{
			if (!IsInvoicedByPartner)
			{
				var invoices = GetNonCancelledInvoices(ChargeableUsages);
				var invoice = (invoices.Length > 0) ? invoices[0] : null;
				InvoicePkForThisMonth = invoice != null ? invoice.PK : ZGuid.Empty;
				InvoiceNumber = invoice != null ? invoice.AH_TransactionNum : ZString.Empty;
			}
		}

		void ValidateSubsidiaryDeposits()
		{
			List<ZString> subsidiariesWithDepositBalance = new List<ZString>();
			foreach (SystemUsage systemUsage in SystemUsages.Cast<SystemUsage>().Where(x => x.OrganisationPK != this.OrganisationPK && x.Billing != null))
			{
				if (systemUsage.LicCompany.MonthlyUsageDepositBalance > 0 && !subsidiariesWithDepositBalance.Contains(systemUsage.Organisation.OH_Code))
				{
					subsidiariesWithDepositBalance.Add(systemUsage.Organisation.OH_Code);
				}
			}

			foreach (ZString subsidiaryWithDepositBalance in subsidiariesWithDepositBalance)
			{
				AddRowError(subsidiaryWithDepositBalance + " " + ValidationMessages.SubsidiaryHaveDeposit);
			}
		}

		void ValidateExchangeRates()
		{
			if (!ExchangeRatesFound)
			{
				string missingCurrencies = string.Join(", ", ExchangeRates.Where(s => s.Value == 0 && !s.Key.IsEmpty).Select(s => s.Key.ToString()).ToArray());
				if (!string.IsNullOrEmpty(missingCurrencies))
				{
					AddRowError("Exchange rate for " + DateForExchangeRate.ToShortDateString() + " not found for " + missingCurrencies + " to " + InvoiceCurrencyCode);
				}
			}
		}

		void ValidateDeposit()
		{
			if (!IsDepositExchangeRatesValid)
			{
				AddRowError("Deposit Exchange Rate for " + DateForExchangeRate.ToShortDateString() + " not found for " + DepositCurrencyCode + " to " + InvoiceCurrencyCode);
			}

			if (!IsDepositValid)
			{
				AddRowError("The Deposit is invalid. Please refer to [Organisation -> License -> Invoicing -> Deposits] for more details.");
			}
		}

		#endregion

		#region Create Invoice

		public ZGuid InvoicePkForThisMonth
		{
			get { return invoicePkForThisMonth; }
			private set
			{
				invoicePkForThisMonth = value;
				InvalidateCanInvoice();
			}
		}
		ZGuid invoicePkForThisMonth;

		public ARInvoice CreateInvoice(ZDateTime postAndInvoiceDateOverride, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			ARInvoice invoice = base.CreateInvoice(postAndInvoiceDateOverride, PopulateInvoice, usSalesTaxCalculator: usSalesTaxCalculator);

			if (invoice != null)
			{
				InvoicePkForThisMonth = invoice.PK;
				InvoiceNumber = invoice.AH_TransactionNum;
			}
			else
			{
				InvoicePkForThisMonth = ZGuid.Empty;
				InvoiceNumber = ZString.Empty;
			}

			return invoice;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void PopulateInvoice(ARInvoice invoice)
		{
			string invoiceDescription = EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription.Value;
			invoice.AH_Desc = invoiceDescription + " - " + DateTo.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

			BillingInvoicingHelper.AddCommentLine(invoice, invoice.AH_Desc);
			BillingInvoicingHelper.AddCommentLine(invoice, ZString.Empty);

			List<SystemBill.BillLine> allLines = new List<SystemBill.BillLine>();

			foreach (SystemBill systemBill in SystemBills)
			{
				var systemLines = new List<SystemBill.BillLine>();
				systemBill.CreateInvoiceLines(systemLines, DateForExchangeRate, invoice);

				foreach (var line in systemLines)
				{
					if (line.AmountInInvoiceCurrency == ZDecimal.Zero)
					{
						line.AmountInInvoiceCurrency = BillingInvoicingHelper.GetAmountInInvoiceCurrency(line.Amount, DateForExchangeRate, line.CurrencyCode, invoice.TransactionCurrency.RX_Code, invoice.Branch, invoice.Branch.Factory);
					}
				}

				allLines.AddRange(systemLines);
			}

			if (SystemMinimumFees.Any() && InvoiceDeliveryForMinimumFee != null)
			{
				foreach (var minimumFeeGroupedByChargeCode in SystemMinimumFees.GroupBy(x => x.ChargeCode))
				{
					bool minimumFeeOnly = !minimumFeeGroupedByChargeCode.Any(x => x.IsNonProductionSystemFee);
					bool nonProductionSystemFeeOnly = !minimumFeeGroupedByChargeCode.Any(x => !x.IsNonProductionSystemFee);

					string minimumFeeInvoiceDescription = "";
					if (minimumFeeOnly)
					{ minimumFeeInvoiceDescription = "System Minimum Fee"; }
					else if (nonProductionSystemFeeOnly)
					{ minimumFeeInvoiceDescription = "Non-Production System Fee"; }
					else
					{ minimumFeeInvoiceDescription = "System Minimum Fee / Non-Production System Fee"; }

					var systemMinimumFeeLine = new SystemBill.BillLine(
						minimumFeeGroupedByChargeCode.Sum(x => x.Amount),
						new SystemBill.TaxGroup(InvoiceDeliveryForMinimumFee),
						InvoiceCurrencyCode,
						minimumFeeGroupedByChargeCode.Key,
						minimumFeeInvoiceDescription,
						allLines.Count, "", ZGuid.Empty);
					systemMinimumFeeLine.AmountInInvoiceCurrency = systemMinimumFeeLine.Amount;
					allLines.Add(systemMinimumFeeLine);
				}
			}

			var taxGroups = allLines.GroupBy(s => s.Tax).OrderBy(s => s.Key.Code);
			bool hasMultipleTaxes = taxGroups.Count() > 1;
			bool hasDeposit = DepositDeductedInInvoiceCurrency > 0m;
			var depositRemaining = DepositDeductedInInvoiceCurrency;
			bool isDepositBeforeProcessingFee = hasDeposit && ProcessingFeeAmount >= 0;
			bool addDepositPerLine = TotalDue == 0 && hasDeposit;

			foreach (var taxGroup in taxGroups)
			{
				ZDecimal taxGroupAmount = 0;
				ZDecimal depositDeducted = 0;

				// Do the group of lines that are eligible for a processing fee first, then the lines that are exempt fees.
				// If the fee is negative then it should reduce the total first, then any deposit can reduce the new total.
				//    So clients get the benefit of the discount even if they are under deposit.
				// If fee is positive, the deposit should reduce the total first, then the fee applies to new total.
				//     So clients pay no extra charge if the deposit fully covers the total.
				// If the deposit fully covers the total then add a deposit line of opposite sign for each amount line
				//     to avoid rounding differences which cause totals to not be exactly zero.
				//  
				foreach (var processingFeeGroup in taxGroup.GroupBy(x => x.IsProcessingFeeExempt).OrderBy(x => x.Key ? 1 : 0))
				{
					ZDecimal feeGroupAmount = 0;

					foreach (var line in processingFeeGroup.OrderBy(s => s.SystemCode.IsEmpty ? 1 : 0).ThenBy(s => s.SystemCode).ThenBy(s => s.Sequence))
					{
						ZDecimal amountInInvoiceCurrency = line.AmountInInvoiceCurrency;

						var amountLine = BillingInvoicingHelper.AddAmountLine(invoice, amountInInvoiceCurrency, line.ChargeCodeName, line.Tax.TaxId,
							LineDescription(line.Description, line.Tax, hasMultipleTaxes));
						if (!line.TaxDate.IsEmpty)
						{
							amountLine.AL_TaxDate = line.TaxDate.Date;
						}
						if (line.Description.IsEmpty)
						{
							amountLine.AL_Desc = OrganisationBill.LineDescription(amountLine.GenericChargeBizO.VC_Description, line.Tax, hasMultipleTaxes);
						}

						if (!line.DepartmentPK.IsEmpty)
						{
							amountLine.AL_GE = line.DepartmentPK;
						}
						feeGroupAmount += amountInInvoiceCurrency;

						if (addDepositPerLine)
						{
							var depositLine = BillingInvoicingHelper.AddAmountLine(invoice, -amountInInvoiceCurrency, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, taxGroup.Key.TaxId,
								LineDescription("Deposit Deducted", taxGroup.Key, hasMultipleTaxes));
							if (!line.TaxDate.IsEmpty)
							{
								depositLine.AL_TaxDate = line.TaxDate.Date;
							}
						}

						if (line.RequireCommentLineAfter)
						{
							BillingInvoicingHelper.AddCommentLine(invoice, ZString.Empty);
						}
					}

					if (isDepositBeforeProcessingFee && depositRemaining > 0)
					{
						var depositUse = Math.Min(depositRemaining, feeGroupAmount);
						depositDeducted += depositUse;
						depositRemaining -= depositUse;
						feeGroupAmount -= depositUse;
					}

					if (!processingFeeGroup.Key && feeGroupAmount != 0)
					{
						ZDecimal processingFee = CalculateProcessingFeeAmount(feeGroupAmount);

						if (processingFee != 0)
						{
							feeGroupAmount += processingFee;

							string description = ProcessingFeeDescription;
							ZString chargeCode = processingFee > 0
								? EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value
								: EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

							BillingInvoicingHelper.AddAmountLine(invoice, processingFee, chargeCode, taxGroup.Key.TaxId,
								LineDescription(description, taxGroup.Key, hasMultipleTaxes));

							if (addDepositPerLine)
							{
								BillingInvoicingHelper.AddAmountLine(invoice, -processingFee, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, taxGroup.Key.TaxId,
									LineDescription("Deposit Deducted", taxGroup.Key, hasMultipleTaxes));
							}

							BillingInvoicingHelper.AddCommentLine(invoice, ZString.Empty);
						}
					}

					if (!isDepositBeforeProcessingFee && depositRemaining > 0)
					{
						var depositUse = Math.Min(depositRemaining, feeGroupAmount);
						depositDeducted += depositUse;
						depositRemaining -= depositUse;
						feeGroupAmount -= depositUse;
					}

					taxGroupAmount += feeGroupAmount;
				}

				if (depositDeducted != 0 && !addDepositPerLine)
				{
					BillingInvoicingHelper.AddAmountLine(invoice, -depositDeducted, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, taxGroup.Key.TaxId,
						LineDescription("Deposit Deducted", taxGroup.Key, hasMultipleTaxes));
				}

				if (taxGroupAmount != 0 && taxGroup.Key.SalesTax != null)
				{
					decimal rate = taxGroup.Key.SalesTaxPercentage;
					ZDecimal taxAmount = Utilities.Round(taxGroupAmount * rate * 0.01m, BillingConstants.RoundingDecimals);
					if (taxAmount != 0)
					{
						BillingInvoicingHelper.AddAmountLine(invoice, taxAmount, taxGroup.Key.SalesTax.AC_Code, null,
							string.Concat(taxGroup.Key.SalesTax.AC_Desc, " (", taxGroup.Key.SalesTax.AC_Code, ')'));
					}
				}
			}

			AddFreeTrialComments(invoice);
			AddExchangeRatesNotes(invoice);
			AddComments(invoice);
			CalculateNextPrepay(invoice);
			AddAttachments(invoice);

			invoice.CommissionCreatorOverride = new BillingCommissionCreator(invoice, new OdplBilledUsageCommissionGroupsCalculator());

			var processingFeeExemptBillingSystems = EDIDataRegistry.Instance.ProcessingFeeExemptBillingSystems.Value;
			var processingFeePercent = (ProcessingFeePercent != 0m && SelfBilling != null) ? SelfBilling.SignedProcessingFeePercent : ZDecimal.Zero;
			foreach (SystemBill systemBill in SystemBills)
			{
				systemBill.CreateRevenueBreakdown(invoice, processingFeeExemptBillingSystems.ContainsCode(systemBill.SystemCode) ? ZDecimal.Zero : processingFeePercent);
			}

			if (SystemMinimumFees.Any())
			{
				BillingInvoicingHelper.CreateOdplMinimumFeeRevenueBreakdown(SystemMinimumFees, invoice, PeriodStart, processingFeePercent);
			}

			foreach (SystemBill systemBill in SystemBills)
			{
				systemBill.OnInvoiceFactorySaving(invoice);
			}
		}

		internal static ZString LineDescription(ZString description, SystemBill.TaxGroup tax, bool hasMultipleTaxes)
		{
			if (!hasMultipleTaxes || tax.Code.IsEmpty)
			{
				return description;
			}
			else
			{
				int index = description.IndexOf("\r\n", StringComparison.OrdinalIgnoreCase);
				if (index == -1)
				{
					return description + " (" + tax.Code + ")";
				}
				else
				{
					return description.Substring(0, index)
						+ " (" + tax.Code + ")"
						+ description.Substring(index);
				}
			}
		}

		void AddComments(ARInvoice invoice)
		{
			ZString comment = MonthlyUsageInvoiceComment;
			if (!comment.IsEmpty)
			{
				BillingInvoicingHelper.AddCommentLine(invoice, comment);
			}
			BillingInvoicingHelper.AddCommentLine(invoice, EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment.Value);
		}

		void AddFreeTrialComments(ARInvoice invoice)
		{
			foreach (SystemBill systemBill in SystemBills)
			{
				var freeTrials = systemBill.GetFreeTrials();
				if (freeTrials != null && freeTrials.Count > 0)
				{
					foreach (ICodeDescription item in freeTrials)
					{
						BillingInvoicingHelper.AddCommentLine(invoice, "Note: You have activated a free trial for " + item.Description + ". Any usage after the free trial period will be billed as per current pricelist.");
					}
				}
			}
		}

		void CalculateNextPrepay(ARInvoice invoice)
		{
			var branch = InvoicingBranch;
			var localCompany = branch.Company;

			if (PeriodStart >= new ZDateTime(2017, 7, 1))
			{
				var prepaidBalanceTotalAmount = 0m;
				var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
				var prepaidBalance = DepositBalanceCollection.LoadFromDb(OrganisationPK, true).FirstOrDefault(x => x.Amount > 0 && x.ChargeCode == prepaidBalanceChargeCode);
				if (prepaidBalance != null)
				{
					prepaidBalanceTotalAmount = prepaidBalance.Amount + prepaidBalance.Tax;

					if (prepaidBalanceTotalAmount != 0m && prepaidBalance.CurrencyCode != InvoiceCurrencyCode)
					{
						prepaidBalanceTotalAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(prepaidBalanceTotalAmount, DateForExchangeRate, prepaidBalance.CurrencyCode, InvoiceCurrencyCode, branch, Factory);
					}
				}

				PrepayNext.CurrentPrepaymentBalance = prepaidBalanceTotalAmount;
			}
			else
			{
				var outstandingBalance = ARBalance.GetOutstandingBalance(OrganisationPK.ToGuid(), localCompany.PK.ToGuid(), ZDateTime.Today.ToDateTime());

				if (localCompany.GC_RX_NKLocalCurrency != InvoiceCurrencyCode)
				{
					outstandingBalance = BillingInvoicingHelper.GetAmountInInvoiceCurrency(outstandingBalance, DateForExchangeRate, localCompany.GC_RX_NKLocalCurrency, InvoiceCurrencyCode, branch, Factory);
				}

				PrepayNext.CurrentPrepaymentBalance = outstandingBalance;
			}

			var predeterminedPrepaidBalance = Organisation.LicCompany?.SelfBilling?.L4_PredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var predeterminedCurrency = Organisation.LicCompany?.SelfBilling?.L4_RX_NKPredeterminedPrepaidBalanceCurrency ?? ZString.Empty;
			if (predeterminedPrepaidBalance != 0m && predeterminedCurrency != InvoiceCurrencyCode)
			{
				predeterminedPrepaidBalance = BillingInvoicingHelper.GetAmountInInvoiceCurrency(predeterminedPrepaidBalance, DateForExchangeRate, predeterminedCurrency, InvoiceCurrencyCode, branch, Factory);
			}

			var futurePredeterminedPrepaidBalance = Organisation.LicCompany?.SelfBilling?.L4_FuturePredeterminedPrepaidBalance ?? ZDecimal.Zero;
			var futurePredeterminedCurrency = Organisation.LicCompany?.SelfBilling?.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency ?? ZString.Empty;
			if (futurePredeterminedPrepaidBalance != 0m && futurePredeterminedCurrency != InvoiceCurrencyCode)
			{
				futurePredeterminedPrepaidBalance = BillingInvoicingHelper.GetAmountInInvoiceCurrency(futurePredeterminedPrepaidBalance, DateForExchangeRate, futurePredeterminedCurrency, InvoiceCurrencyCode, branch, Factory);
			}

			PrepayNext.PrepaymentBalanceRequired = predeterminedPrepaidBalance;
			PrepayNext.FuturePrepaymentBalanceRequired = futurePredeterminedPrepaidBalance;
			PrepayNext.CurrentInvoiceTotalAmount = invoice.AH_OSTotalAmount;
		}

		#endregion

		#region Partner Summary

		public ZipStream CreateSummaryInExcel()
		{
			DocOrganisationBill docWrapper = DocOrganisationBill.New(this, Factory, OrganisationPK);
			(var data, var fileExtension) = BillingInvoicingHelper.GetRawDocumentInExcel(SummaryDocTemplate, docWrapper);
			return new ZipStream(OrganisationCode + " Billing Summary." + fileExtension, new MemoryStream(data));
		}

		#endregion

		#region Attachments

		protected void AddAttachments(ARInvoice invoice)
		{
			List<ZGuid> allOrganisationPKs = new List<ZGuid>(SystemUsages.Cast<SystemUsage>().Select(x => x.OrganisationPK).Distinct());
			if (!allOrganisationPKs.Contains(this.OrganisationPK))
			{
				allOrganisationPKs.Add(this.OrganisationPK);
			}

			const int MaxAttachmentCount = 3;
			if (allOrganisationPKs.Count > MaxAttachmentCount)
			{
				AddAttachment(invoice, this.OrganisationPK);
				allOrganisationPKs.Remove(this.OrganisationPK);
				ZipAttach(invoice, allOrganisationPKs);
			}
			else
			{
				foreach (ZGuid orgPk in allOrganisationPKs)
				{
					AddAttachment(invoice, orgPk);
				}
			}
		}

		const string ParentSummaryDocType = "OD1";
		const string ChildSummaryDocType = "OD2";

		void ZipAttach(IDocManagerSupport docManagerSupport, List<ZGuid> orgPks)
		{
			List<ZipStream> entriesInPDF = new List<ZipStream>(orgPks.Count);
			List<ZipStream> entriesInExcel = new List<ZipStream>(orgPks.Count);
			string excelExtension = null;
			foreach (ZGuid orgPk in orgPks)
			{
				DocOrganisationBill docWrapper = DocOrganisationBill.New(this, Factory, orgPk);
				if (!docWrapper.IsEmpty)
				{
					EDIOrgHeader org = Factory.Load<EDIOrgHeader>(orgPk);
					ZString orgCode = org != null ? org.OH_Code : ZString.Empty;
					ZString summaryFileName = orgCode + " Billing Summary";

					AddZipStreamToList(entriesInPDF, summaryFileName + '.' + BillingConstants.FileExtensions.Pdf, BillingInvoicingHelper.GetRawDocumentInPdf(SummaryDocTemplate, docWrapper));
					byte[] rawExcel;
					(rawExcel, excelExtension) = BillingInvoicingHelper.GetRawDocumentInExcel(SummaryDocTemplate, docWrapper);
					AddZipStreamToList(entriesInExcel, summaryFileName + '.' + excelExtension, rawExcel);
				}
			}

			AddZipStreamsToDocManager(docManagerSupport, "Billing Summaries (pdf).zip", entriesInPDF);
			AddZipStreamsToDocManager(docManagerSupport, "Billing Summaries (" + excelExtension + ").zip", entriesInExcel);
		}

		void AddZipStreamToList(List<ZipStream> entries, string summaryFileName, byte[] data)
		{
			MemoryStream contentStream = new MemoryStream(data);
			entries.Add(new ZipStream(summaryFileName, contentStream));
		}

		void AddZipStreamsToDocManager(IDocManagerSupport docManagerSupport, string fileName, List<ZipStream> entries)
		{
			if (entries.Count > 0)
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					ZipCreator creator = new ZipCreator();
					creator.ZipStream(entries, outputStream);
					docManagerSupport.DocManagerInfo.AddFileOrDocument(outputStream.ToArray(), fileName, ChildSummaryDocType);
				}
			}
		}

		void AddAttachment(ARInvoice invoice, ZGuid orgPK)
		{
			DocOrganisationBill docWrapper = DocOrganisationBill.New(this, invoice.Factory, orgPK);
			if (!docWrapper.IsEmpty)
			{
				EDIOrgHeader org = Factory.Load<EDIOrgHeader>(orgPK);
				ZString organisationCode = org != null ? org.OH_Code : ZString.Empty;

				ZString summaryDocType = OrganisationPK == orgPK ? ParentSummaryDocType : ChildSummaryDocType;
				ZString summaryFileName = organisationCode + " Billing Summary";

				BillingInvoicingHelper.AddAttachmentInPdf(invoice, SummaryDocTemplate, docWrapper, summaryFileName, summaryDocType);
				BillingInvoicingHelper.AddAttachmentInExcel(invoice, SummaryDocTemplate, docWrapper, summaryFileName, summaryDocType);
			}
			docWrapper.ClearAllSummaryLines();
		}

		public StmTemplate SummaryDocTemplate
		{
			get { return summaryDocTemplate ?? (summaryDocTemplate = LoadStmTemplate(Factory, "Billing Summary")); }
		}
		StmTemplate summaryDocTemplate;

		StmTemplate LoadStmTemplate(BusinessObjectFactory factory, ZString templateName)
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, Enterprise.Core.Constants.DataContext.CargoWiseBilling);

			return factory.LoadTop1<StmTemplate>(query);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new OrganisationBillDocumentSupporter(this); }
		}

		#endregion

		public class PrepayInfo
		{
			public decimal CurrentPrepaymentBalance { get; set; }
			public decimal PrepaymentBalanceRequired { get; set; }
			public decimal FuturePrepaymentBalanceRequired { get; set; }
			public decimal CurrentInvoiceTotalAmount { get; set; }
		}

		public PrepayInfo PrepayNext = new PrepayInfo();

		#region Processing Fee

		bool ShouldApplyPrepaymentDiscount
		{
			get
			{
				var result = false;
				var branch = InvoicingBranch;

				var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
				var prepaidBalance = DepositBalanceCollection.LoadFromDb(OrganisationPK, true).FirstOrDefault(x => x.Amount > 0 && x.ChargeCode == prepaidBalanceChargeCode);
				if (prepaidBalance != null)
				{
					var prepaidBalanceTotalAmount = prepaidBalance.Amount + prepaidBalance.Tax;
					var predeterminedPrepaidBalance = SelfBilling?.L4_PredeterminedPrepaidBalance ?? ZDecimal.Zero;

					if (prepaidBalanceTotalAmount != 0 && predeterminedPrepaidBalance != 0)
					{
						var predeterminedCurrency = SelfBilling?.L4_RX_NKPredeterminedPrepaidBalanceCurrency ?? ZString.Empty;

						if (predeterminedCurrency != prepaidBalance.CurrencyCode)
						{
							if (prepaidBalance.CurrencyCode != InvoiceCurrencyCode)
							{
								prepaidBalanceTotalAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(prepaidBalanceTotalAmount, DateForExchangeRate, prepaidBalance.CurrencyCode, InvoiceCurrencyCode, branch, Factory);
							}

							if (predeterminedCurrency != InvoiceCurrencyCode)
							{
								predeterminedPrepaidBalance = BillingInvoicingHelper.GetAmountInInvoiceCurrency(predeterminedPrepaidBalance, DateForExchangeRate, predeterminedCurrency, InvoiceCurrencyCode, branch, Factory);
							}
						}

						result = (prepaidBalanceTotalAmount >= predeterminedPrepaidBalance);
					}
				}

				return result;
			}
		}

		ZDecimal CalculateProcessingFeeAmount(ZDecimal amount)
		{
			return (ProcessingFeePercent != 0m && SelfBilling != null) ? SelfBilling.CalculateProcessingFeeAmount(amount) : ZDecimal.Zero;
		}

		internal ZDecimal CalculateProcessingFeeAmount_Exposed(ZDecimal amount)
		{
			return CalculateProcessingFeeAmount(amount);
		}

		#endregion
	}
}

