using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing.eAdaptor
{
	public class EAdaptorBill : SystemBill, ISystemMinimumFeeContributionBill
	{
		public EAdaptorBill(BusinessObjectFactory factory)
			: base(factory)
		{
			this.SystemCode = BillingConstants.BillingSystem.eAdaptor;
			this.amountChargeCodeName = EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(SystemCode);
			this.discountChargeCodeName = EDIDataRegistry.Instance.TransactionDiscountChargeCodes.Value.GetDescriptionFromCode(SystemCode);
		}

		public EAdaptorBill(BillingRunContext context)
			: this(context.Factory)
		{
			this.context = context;
		}

		readonly BillingRunContext context;
		readonly ZString amountChargeCodeName;
		readonly ZString discountChargeCodeName;

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return amountChargeCodeName;
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			return discountChargeCodeName;
		}

		#endregion

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			CalculateDiscounts();

			Amount = DiscountCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscounts()
		{
			DiscountCalculations.Clear();

			foreach (var usagesGroupedByMonth in SystemUsages.Cast<EAdaptorUsage>().GroupBy(x => x.PeriodStart))
			{
				var periodStart = usagesGroupedByMonth.Key;
				var firstUsage = usagesGroupedByMonth.First();
				var priceHeader = firstUsage.PriceHeader;
				var discountsForMonth = GetDiscountsForMonth(priceHeader, periodStart);

				decimal amountToDiscount = 0;
				decimal licenceUnits = 0;
				decimal totalNonTransactionalAmount = 0;
				foreach (var usage in usagesGroupedByMonth)
				{
					amountToDiscount += usage.TransactionalAmount;
					licenceUnits += usage.LicenceUnitsAmount;
					totalNonTransactionalAmount += usage.NonTransactionalAmount;
				}

				var discountable = new Discountable(amountToDiscount, licenceUnits, context != null ? context.ModuleUsersService : null, usagesGroupedByMonth.First().LicHeader.Database, periodStart);
				var discountCalculation = OdplDiscountCalculator.Calculate(discountsForMonth, discountable, (priceHeader == null) ? null : priceHeader.L6_LicenceUnitRate);

				discountCalculation.Amount += totalNonTransactionalAmount;
				DiscountCalculations.Add(periodStart, discountCalculation);
			}
		}

		#region Discountable

		public class Discountable : IOdplDiscountable
		{
			public Discountable(ZDecimal amount, ZDecimal licenceUnits, IModuleUsers moduleUsers, LicenceDatabase licDatabase, ZDateTime periodStart)
			{
				AmountToDiscount = amount;
				TransactionLicenceUnits = licenceUnits;
				this.moduleUsers = moduleUsers;
				this.licDatabase = licDatabase;
				this.periodStart = periodStart;
			}

			readonly IModuleUsers moduleUsers;
			readonly LicenceDatabase licDatabase;
			readonly ZDateTime periodStart;

			public ZDecimal AmountToDiscount { get; private set; }
			public ZDecimal TransactionLicenceUnits { get; private set; }
			public ZString SystemCode { get { return BillingConstants.BillingSystem.eAdaptor; } }
			public ZDecimal MixedAmountAsMoney { get { return AmountToDiscount; } }
			public ZDecimal MixedAmountAsLicenceUnits { get { return LicenceUnitsToDiscount; } }

			public decimal UserLicenceUnits
			{
				get
				{
					if (!userLicenceUnits.HasValue)
					{
						userLicenceUnits = moduleUsers.TotalUserLicenceUnits(licDatabase, periodStart);
					}
					return userLicenceUnits.Value;
				}
			}
			decimal? userLicenceUnits;

			public ZDecimal LicenceUnitsToDiscount
			{
				get { return TransactionLicenceUnits + UserLicenceUnits; }
			}

			public ZDecimal AmountToDiscountForModule(string moduleCode, out string moduleName)
			{
				throw new InvalidOperationException("Invalid discount for eAdaptor");
			}

			public int CoreOnDemandUsers
			{
				get { throw new InvalidOperationException("Invalid discount for eAdaptor"); }
			}

			public ZBool IsHostedOnWiseCloud => licDatabase?.IsHostedOnWiseCloud ?? false;
		}

		#endregion

		protected Dictionary<ZDateTime, DiscountCalculation> DiscountCalculations
		{
			get { return discountCalculations ?? (discountCalculations = new Dictionary<ZDateTime, DiscountCalculation>()); }
		}
		Dictionary<ZDateTime, DiscountCalculation> discountCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountCalculations.Values.SelectMany(x => x.Details);

		public IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			var minimumFeeContribution = new Dictionary<Tuple<ZGuid, ZDateTime>, ZDecimal>();
			foreach (var usages in SystemUsages.Cast<EAdaptorUsage>().GroupBy(x => x.PeriodStart))
			{
				var period = usages.Key;
				var periodAmount = usages.Sum(x => x.TransactionalAmount);
				var periodDiscountCalculation = DiscountCalculations[period];
				var amount = periodDiscountCalculation.Amount;
				var discountAmount = periodDiscountCalculation.DiscountAmount;

				if (periodAmount > 0)
				{
					foreach (var usagesGroupedByDatabase in usages.Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => x.User.DatabasePK))
					{
						var ratio = usagesGroupedByDatabase.Sum(x => x.TransactionalAmount) / periodAmount;
						var dbContribution = (amount - discountAmount) * ratio;

						var periodDatabaseKey = new Tuple<ZGuid, ZDateTime>(usagesGroupedByDatabase.Key, period);
						if (minimumFeeContribution.ContainsKey(periodDatabaseKey))
						{
							minimumFeeContribution[periodDatabaseKey] += dbContribution;
						}
						else
						{
							minimumFeeContribution[periodDatabaseKey] = dbContribution;
						}
					}
				}
			}

			foreach (var contribution in minimumFeeContribution)
			{
				var minimumFee = new SystemMinimumFee(contribution.Key.Item1, contribution.Key.Item2, contribution.Value, CurrencyCode, "");
				result.Add(minimumFee);
			}

			return result;
		}

		#endregion

		#region Validation

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			foreach (EAdaptorUsage usage in SystemUsages)
			{
				usage.ValidateUnitPrices(notificationOwner, BillingSystemDescription);
			}
		}

		protected override void ValidateAllCore(BusinessObject notificationOwner)
		{
			base.ValidateAllCore(notificationOwner);

			ClientLicencePriceHeader firstPriceHeader = null;

			foreach (SystemUsage usage in SystemUsages)
			{
				var priceHeader = usage.PriceHeader;

				if (priceHeader == null)
				{
					notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: No pricelist found for {1}", BillingSystemDescription, usage.Organisation.OH_Code));
				}
				else if (firstPriceHeader == null)
				{
					firstPriceHeader = priceHeader;
				}
				else
				{
					if (firstPriceHeader.L6_LicenceUnitRate != priceHeader.L6_LicenceUnitRate)
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.CurrentCulture, "{0}: pricelists for {1} has multiple licence unit rate i.e. {2} and {3}", BillingSystemDescription, usage.Organisation.OH_Code, firstPriceHeader.L6_LicenceUnitRate, priceHeader.L6_LicenceUnitRate));
					}
				}
			}
		}

		#endregion

		#region Create Revenue Breakdown

		public override void CreateRevenueBreakdown(ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			var usages = SystemUsages.Cast<EAdaptorUsage>().Where(x => x.Amount != 0).ToArray();

			CreateRevenueBreakdownForTransactionalUsages(usages, invoice, signedProcessingFeePercentage);
			CreateRevenueBreakdownForNonTransactionalUsages(usages, invoice, signedProcessingFeePercentage);
		}

		void CreateRevenueBreakdownForTransactionalUsages(IEnumerable<EAdaptorUsage> usages, ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			var nonTransactionalAmount = usages.Sum(x => x.NonTransactionalAmount);
			var totalPreDiscount = Amount - nonTransactionalAmount;
			var totalUsageAmount = usages.Sum(x => x.TransactionalAmount);

			if (totalPreDiscount == 0 || totalUsageAmount == 0)
			{
				return;
			}

			var total = TotalAmount - nonTransactionalAmount;
			var totalProcessingFee = signedProcessingFeePercentage / 100m * total;
			var totalPostDiscount = total + totalProcessingFee;

			foreach (var usage in usages.Where(x => x.TransactionalAmount != 0))
			{
				CheckUsageCurrencyCode(usage);

				foreach (var transactionalUsages in usage.TransactionalUsages)
				{
					var billed = invoice.Factory.New<EdiBilledUsage>();
					PopulateBilledUsage(billed, invoice, usage, transactionalUsages, totalUsageAmount, totalPreDiscount, totalPostDiscount, totalProcessingFee);
					EdiBilledDiscount.CreateBilledDiscounts(billed, DiscountDetailedInfos, SurchargeDetailedInfos);
				}
			}
		}

		void CreateRevenueBreakdownForNonTransactionalUsages(IEnumerable<EAdaptorUsage> usages, ARInvoice invoice, ZDecimal signedProcessingFeePercentage)
		{
			var totalNonTransactionalAmount = usages.Sum(x => x.NonTransactionalAmount);

			if (totalNonTransactionalAmount == 0)
			{
				return;
			}

			var totalProcessingFee = signedProcessingFeePercentage / 100m * totalNonTransactionalAmount;
			var totalPostDiscount = totalNonTransactionalAmount + totalProcessingFee;

			foreach (var usage in usages.Where(x => x.NonTransactionalAmount != 0))
			{
				CheckUsageCurrencyCode(usage);

				foreach (var nonTransactionalUsages in usage.NonTransactionalUsages)
				{
					var billed = invoice.Factory.New<EdiBilledUsage>();
					PopulateBilledUsage(billed, invoice, usage, nonTransactionalUsages, totalNonTransactionalAmount, totalNonTransactionalAmount, totalPostDiscount, totalProcessingFee);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void CheckUsageCurrencyCode(EAdaptorUsage usage)
		{
			if (usage.CurrencyCode != CurrencyCode)
			{
				throw new InvalidOperationException($"Revenue breakdown has multiple currencies for {Organisation.OH_Code} {SystemDescription} {usage.CurrencyCode}|{CurrencyCode}");
			}
		}

		void PopulateBilledUsage(EdiBilledUsage billed, ARInvoice invoice,
										EAdaptorUsage parentUsage,
										SystemUsage.SubUsage subUsage,
										decimal totalUsageAmount, decimal totalPreDiscount, decimal totalPostDiscount, decimal totalProcessingFee)
		{
			var weight = subUsage.Amount / totalUsageAmount;
			var preDiscount = weight * totalPreDiscount;
			var postDiscount = weight * totalPostDiscount;
			var processingFee = weight * totalProcessingFee;
			var currency = parentUsage.CurrencyCode;
			var dateForExchangeRate = BillingInvoicingHelper.GetDateForExchangeRate(invoice);

			billed.BU9_AC_AmountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, GetAmountChargeCodeName(parentUsage));
			if (preDiscount != postDiscount)
			{
				billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, GetDiscountChargeCodeName(parentUsage));
				if (billed.BU9_AC_DiscountChargeCode.IsEmpty)
				{
					billed.BU9_AC_DiscountChargeCode = BillingInvoicingHelper.GetChargeCodePKOrEmpty(invoice.Branch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
				}
			}

			var priceItem = subUsage.PriceItem;
			billed.BU9_AH_Invoice = invoice.PK;
			billed.BU9_BillingModel = BillingConstants.PriceHeaderType.ODM;
			billed.BU9_LC = parentUsage.User.LicenceCompanyPK;
			billed.BU9_LCC = parentUsage.User.ClientCompanyPK;
			billed.BU9_LD = parentUsage.User.DatabasePK;
			billed.BU9_LocalAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_LocalAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_LocalProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_PeriodStart = PeriodStart.Date;
			billed.BU9_PriceCurrency = currency;
			billed.BU9_TransactionAmountPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_TransactionAmountPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(preDiscount, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_TransactionProcessingAmount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processingFee, dateForExchangeRate, currency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			billed.BU9_UnitCount = subUsage.UnitCount;
			billed.BU9_UnitPrice = priceItem?.L7_Price ?? 0m;
			billed.BU9_UsageCode = SystemCode;
			billed.BU9_UsageSubCode = subUsage.PriceItemCode;

			if (priceItem != null)
			{
				billed.BU9_L7 = priceItem.PK;
				billed.BU9_PriceCode = priceItem.L7_Code;
			}
		}

		#endregion

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			return BuildGeneralSummarySections(organisationPK, false);
		}

		public override SummarySection[] GetGroupSummarySections()
		{
			return new[] { GetGroupSummarySection(GroupSummarySectionType.Fees), GetGroupSummarySection(GroupSummarySectionType.Usage) };
		}

		SummarySection GetGroupSummarySection(GroupSummarySectionType sectionType)
		{
			SummarySection result = new SummarySection(Factory);
			result.Header.MainDescription = SystemDescription + " Group Summary (" + sectionType.ToString() + ")";

			var options = GetGroupSummaryOptions();

			ZDecimal totalAmount = 0m;
			ZDecimal totalLicenceUnitsAmount = 0;

			if (sectionType == GroupSummarySectionType.Usage)
			{
				foreach (EAdaptorUsage systemUsage in SystemUsages.OrderBy(x => x.Organisation.OH_Code).ThenBy(x => x.PeriodStart))
				{
					ZDecimal usageAmount = systemUsage.TransactionalAmount;
					ZDecimal licenceUnitsAmount = systemUsage.LicenceUnitsAmount;
					if (licenceUnitsAmount != 0m || usageAmount != 0m)
					{
						SummaryLine groupLine = result.Lines.AddNew();
						groupLine.MainDescription = systemUsage.Organisation.OH_Code;
						groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
						BuildSummaryLine(groupLine, options, systemUsage);

						groupLine.TaxCode = TaxCode(systemUsage.InvoiceDelivery);
						groupLine.TotalLicenceUnits = licenceUnitsAmount.ToString();

						totalAmount += usageAmount;
						totalLicenceUnitsAmount += licenceUnitsAmount;
					}
				}
			}
			else //fees
			{
				foreach (EAdaptorUsage systemUsage in SystemUsages.OrderBy(x => x.Organisation.OH_Code).ThenBy(x => x.PeriodStart))
				{
					ZDecimal usageAmount = systemUsage.NonTransactionalAmountWithoutDBUsageFee;
					if (usageAmount != 0m)
					{
						SummaryLine groupLine = result.Lines.AddNew();
						groupLine.MainDescription = systemUsage.Organisation.OH_Code;
						groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
						BuildSummaryLine(groupLine, options, systemUsage);

						groupLine.TaxCode = TaxCode(systemUsage.InvoiceDelivery);

						totalAmount += usageAmount;
					}
				}

				foreach (EAdaptorUsage usage in SystemUsages.OrderBy(x => x.PeriodStart))
				{
					foreach (var subUsage in usage.DatabaseUsages)
					{
						ZDecimal usageAmount = subUsage.Amount;
						SummaryLine groupLine = result.Lines.AddNew();
						groupLine.MainDescription = subUsage.PriceItem.L7_DescriptionLocalized;
						groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
						BuildSummaryLine(groupLine, options, usage);
						totalAmount += usageAmount;
					}
				}
			}

			result.Header.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
			result.Header.AmountDescription = ZString.Format("Total Price ({0})", CurrencyCode);
			result.Header.TotalDescription = HasLicenceUnits ? new ZString("Total") : ZString.Format("Total ({0})", CurrencyCode);
			result.Header.TotalLicenceUnits = totalLicenceUnitsAmount.ToString();
			result.Header.LicenceUnitsAmountDescription = "Total Licence Units";

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		enum GroupSummarySectionType
		{
			Fees,
			Usage,
		}

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetMultiPeriodDiscountSummarySections(DiscountCalculations);
		}

		#endregion
	}
}

