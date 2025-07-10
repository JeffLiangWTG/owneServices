using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class OceanCarrierMessagingBill : TransactionalWithSubCodeSystemBill
	{
		public OceanCarrierMessagingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.OceanCarrierMessaging, factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(usage.SubCode);
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			return (usage != null ? EDIDataRegistry.Instance.TransactionDiscountChargeCodes.Value.GetDescriptionFromCode(usage.SubCode) : null)
				?? base.GetDiscountChargeCodeName(usage);
		}

		#endregion

		#region Invoice

		protected override void CreateDiscountAndSurchargeProRataLines(List<BillLine> lines, List<KeyValuePair<TaxGroup, decimal>> groupWeights)
		{
			CreateDiscountProRataLines(lines);

			decimal totalWeight = groupWeights.Sum(x => x.Value);
			CreateProRataLines(lines, groupWeights, totalWeight, SurchargeAmount, GetSurchargeChargeCodeName(), GetInvoiceSurchargeDescription());
		}

		void CreateDiscountProRataLines(List<BillLine> lines)
		{
			var usages = SystemUsages.Cast<PriceItemUsage>();

			foreach (var usageTaxGroup in usages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				foreach (var usageSubCodeGroup in usageTaxGroup.GroupBy(y => y.SubCode))
				{
					bool hasSinglePriceForAllPeriods = !usageSubCodeGroup.GroupBy(u => new { u.CurrencyCode, u.UnitPrice }).Skip(1).Any();

					if (hasSinglePriceForAllPeriods)
					{
						var usage = usageSubCodeGroup.First();
						var discountCalculationsBySubCode = DiscountWithSubCodeCalculations.Where(x => x.Key.Item2 == usage.SubCode);
						var discountAmount = discountCalculationsBySubCode.Sum(x => x.Value.DiscountAmount);

						if (discountAmount != 0)
						{
							lines.Add(new BillLine(-discountAmount, usageTaxGroup.Key, CurrencyCode, GetDiscountChargeCodeName(usage), GetDiscountDescription(usage), lines.Count, SystemCode));
						}
					}
					else
					{
						foreach (var usagePeriodGroup in usageSubCodeGroup.GroupBy(u => u.PeriodStart))
						{
							var usage = usagePeriodGroup.First();
							var discountCalculation = DiscountWithSubCodeCalculations[new Tuple<ZDateTime, ZString>(usage.PeriodStart, usage.SubCode)];
							var discountAmount = discountCalculation.Amount;

							if (discountAmount != 0)
							{
								lines.Add(new BillLine(-discountAmount, usageTaxGroup.Key, CurrencyCode, GetDiscountChargeCodeName(usage), GetDiscountDescription(usage), lines.Count, SystemCode));
							}
						}
					}
				}
			}
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			return ZString.Format("{0} - {1} Transactions at {2} {3} per Transaction",
				MessageTypes.GetDescriptionFromCode(systemUsage.SubCode),
				unitCount,
				systemUsage.CurrencyCode,
				systemUsage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		ZString GetDiscountDescription(PriceItemUsage systemUsage)
		{
			return ZString.Format("{0} Discount", MessageTypes.GetDescriptionFromCode(systemUsage.SubCode));
		}

		#endregion

		#region Discount Summary

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(DiscountWithSubCodeCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetDiscountOrSurchargeSummarySections(SurchargeWithSubCodeCalculations);
		}

		SummarySection[] GetDiscountOrSurchargeSummarySections(Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountOrSurchargeCalculations)
		{
			if (discountOrSurchargeCalculations.Count > 0 && discountOrSurchargeCalculations.Values.Any(x => x.DiscountDescriptions.Any()))
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (var calculation in discountOrSurchargeCalculations)
				{
					foreach (ZString discountDescription in calculation.Value.DiscountDescriptions)
					{
						SummaryLine summaryLine = result.Lines.AddNew();
						summaryLine.MainDescription = MessageTypes.GetDescriptionFromCode(calculation.Key.Item2) + " - " + discountDescription;

						if (HasUsagesForEarlierPeriods)
						{
							ZString periodAsString = calculation.Key.Item1.ToString("MMM yyyy", CultureInfo.InvariantCulture);
							summaryLine.MainDescription = periodAsString + " " + summaryLine.MainDescription;
						}
					}
				}

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion

		CodeDescriptionPairList MessageTypes => OceanCarrierMessagingBillingSystem.GetCachedOceanCarrierMessageTypes(Factory);
	}
}

