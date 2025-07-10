using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class USCustomsBill : TransactionSystemBillEx
	{
		public USCustomsBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.USCustoms, factory)
		{
		}

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			CalculateDiscountsAndSurcharges();
			Amount = DiscountWithOrgCodeCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountWithOrgCodeCalculations.Values.Sum(x => x.DiscountAmount);
			SurchargeAmount = -SurchargeWithOrgCodeCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscountsAndSurcharges()
		{
			DiscountWithOrgCodeCalculations.Clear();
			SurchargeWithOrgCodeCalculations.Clear();
			List<ClientLicenceBillingDiscount> allDiscounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscountCollection localDiscounts = Discounts;

			foreach (var usagesGroupedByMonth in SystemUsages.GroupBy(x => x.PeriodStart))
			{
				foreach (var usagesGroupedByOrg in usagesGroupedByMonth.GroupBy(x => x.OrganisationPK))
				{
					var firstDayOfMonth = usagesGroupedByMonth.Key;
					var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
					var firstUsage = usagesGroupedByOrg.First();
					var prices = firstUsage.PriceHeader;
					allDiscounts.Clear();
					if (prices != null)
					{
						allDiscounts.AddRange(prices.Discounts);
					}
					if (localDiscounts != null)
					{
						allDiscounts.AddRange(localDiscounts);
					}

					amountToDiscount = usagesGroupedByOrg.Sum(x => x.Amount);
					ZInt unitCount = usagesGroupedByOrg.Sum(x => x.UnitCount);
					ZDecimal unitPrice = firstUsage.UnitPrice;

					DiscountCalculation discountCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(allDiscounts, this, unitCount, unitPrice, firstDayOfMonth, lastDayOfMonth);
					DiscountWithOrgCodeCalculations.Add(new Tuple<ZDateTime, ZString>(firstDayOfMonth, firstUsage.Organisation.OH_Code), discountCalculation);
					DiscountCalculationsToSystemUsages.Add(discountCalculation, usagesGroupedByOrg.ToArray());

					DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(allDiscounts, this, discountCalculation.Amount, firstDayOfMonth, lastDayOfMonth);
					SurchargeWithOrgCodeCalculations.Add(new Tuple<ZDateTime, ZString>(firstDayOfMonth, firstUsage.Organisation.OH_Code), surchargeCalculation);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> DiscountWithOrgCodeCalculations
		{
			get { return discountWithOrgCodeCalculations ?? (discountWithOrgCodeCalculations = new Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountWithOrgCodeCalculations;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> SurchargeWithOrgCodeCalculations
		{
			get { return surchargeWithOrgCodeCalculations ?? (surchargeWithOrgCodeCalculations = new Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> surchargeWithOrgCodeCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountWithOrgCodeCalculations.Values.SelectMany(x => x.Details);
		protected override IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => SurchargeWithOrgCodeCalculations.Values.SelectMany(x => x.Details);

		#endregion

		#region Invoice

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<USCustomsUsage>().ToArray();

			foreach (var usagesGroupedByOrg in usages.GroupBy(u => u.OrganisationPK))
			{
				var usage = usagesGroupedByOrg.First();
				var transactionCount = usagesGroupedByOrg.Sum(u => u.TransactionCount);
				var groupAmount = usagesGroupedByOrg.Sum(u => u.Amount);
				var taxGroup = new TaxGroup(usage.InvoiceDelivery);

				if (groupAmount != 0)
				{
					lines.Add(new BillLine(groupAmount, taxGroup, CurrencyCode, GetAmountChargeCodeName(usage), GetInvoiceDescription(usage, transactionCount, groupAmount), lines.Count, SystemCode));
				}

				var discountCalculation = DiscountWithOrgCodeCalculations[new Tuple<ZDateTime, ZString>(PeriodStart, usage.Organisation.OH_Code)];
				if (discountCalculation != null && discountCalculation.DiscountAmount != 0)
				{
					lines.Add(new BillLine(-discountCalculation.DiscountAmount, taxGroup, CurrencyCode, GetDiscountChargeCodeName(usage), GetInvoiceDiscountDescription(usage, discountCalculation), lines.Count, SystemCode));
				}

				var surchargeCalculation = SurchargeWithOrgCodeCalculations[new Tuple<ZDateTime, ZString>(PeriodStart, usage.Organisation.OH_Code)];
				if (surchargeCalculation != null && surchargeCalculation.DiscountAmount != 0)
				{
					lines.Add(new BillLine(-surchargeCalculation.DiscountAmount, taxGroup, CurrencyCode, GetSurchargeChargeCodeName(), GetInvoiceDiscountDescription(usage, surchargeCalculation), lines.Count, SystemCode));
				}
			}
		}

		ZString GetInvoiceDescription(USCustomsUsage usage, int transactionCount, decimal groupAmount)
		{
			string minimumFeeDescription = string.Empty;
			var priceItem = usage.PriceItem;
			if (usage.HasMinimumFee)
			{
				minimumFeeDescription = string.Format(CultureInfo.InvariantCulture, ". Minimum fee of {0} {1} per month which includes {2} Transactions",
					usage.CurrencyCode,
					groupAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					priceItem.L7_UnitBreak);
			}

			return ZString.Format("{0} - {1} - {2} Transactions at {3} {4} per transaction{5}",
				TransactionCategoryDescription,
				usage.Organisation.OH_Code,
				transactionCount,
				usage.CurrencyCode,
				priceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
				minimumFeeDescription);
		}

		ZString GetInvoiceDiscountDescription(USCustomsUsage usage, DiscountCalculation discountCalculation)
		{
			var discount = string.Join(" and ", discountCalculation.InvoiceDescriptions);
			return ZString.Format("{0} - {1} - {2}",
				TransactionCategoryDescription,
				usage.Organisation.OH_Code,
				discount);
		}

		public const string TransactionCategoryDescription = "USC Service Bureau Usage";

		#endregion

		#region Discount Summary

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(DiscountWithOrgCodeCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(SurchargeWithOrgCodeCalculations);
		}

		SummarySection[] GetDiscountOrSurchargeSummarySections(Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountOrSurchargeCalculations)
		{
			if (discountOrSurchargeCalculations.Count > 0 && discountOrSurchargeCalculations.Values.Any(x => x.DiscountDescriptions.Any()))
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (var org in discountOrSurchargeCalculations.GroupBy(x => x.Key.Item2))
				{
					foreach (var periodOrgCalculation in discountOrSurchargeCalculations.Where(x => x.Key.Item2 == org.Key))
					{
						foreach (ZString discountDescription in periodOrgCalculation.Value.DiscountDescriptions)
						{
							SummaryLine summaryLine = result.Lines.AddNew();
							summaryLine.MainDescription = TransactionCategoryDescription + " - " + org.Key + " - " + discountDescription;

							if (HasUsagesForEarlierPeriods)
							{
								ZString periodAsString = periodOrgCalculation.Key.Item1.ToString("MMM yyyy", CultureInfo.InvariantCulture);
								summaryLine.MainDescription = periodAsString + " " + summaryLine.MainDescription;
							}
						}
					}
				}
				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion
	}
}

