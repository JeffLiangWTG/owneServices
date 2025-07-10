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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ClientMappingBill : TransactionalWithSubCodeSystemBill
	{
		public ClientMappingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.ClientMapping, factory)
		{
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			var organisationUsages = SystemUsages.Cast<ClientMappingUsage>()
									.Where(x => x.OrganisationPK == organisationPK && x.TransactionCount > 0)
									.OrderBy(x => x.SubCode)
									.ThenBy(x => x.PeriodStart)
									.ThenBy(x => x.ServerCode)
									.ToArray();

			if (!organisationUsages.Any())
			{
				return System.Array.Empty<SummarySection>();
			}

			var section = BuildSingleSummarySection(organisationUsages, false);
			var firstSummary = organisationUsages.First().GetGeneralSummarySections().First();
			section.Header.PurchasedCount = firstSummary.Header.PurchasedCount;
			section.Header.TotalUnitCount = firstSummary.Header.TotalUnitCount;

			return new[] { section };
		}

		#endregion

		#region Discount Summary

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetDiscountOrSurchargeSections(DiscountWithSubCodeCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetDiscountOrSurchargeSections(SurchargeWithSubCodeCalculations);
		}

		SummarySection[] GetDiscountOrSurchargeSections(Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountOrSurchargeCalculations)
		{
			if (discountOrSurchargeCalculations.Count > 0 && discountOrSurchargeCalculations.Values.Any(x => x.DiscountDescriptions.Any()))
			{
				SummarySection result = new SummarySection(Factory);
				result.Header.MainDescription = SystemDescription + " Amount Calculations Applied";

				foreach (var periodProviderDiscountCalculation in discountOrSurchargeCalculations)
				{
					foreach (ZString discountDescription in periodProviderDiscountCalculation.Value.DiscountDescriptions)
					{
						SummaryLine summaryLine = result.Lines.AddNew();
						summaryLine.MainDescription = periodProviderDiscountCalculation.Key.Item2 + " - " + discountDescription;

						if (HasUsagesForEarlierPeriods)
						{
							ZString periodAsString = periodProviderDiscountCalculation.Key.Item1.ToString("MMM yyyy", CultureInfo.InvariantCulture);
							summaryLine.MainDescription = periodAsString + " " + summaryLine.MainDescription;
						}
					}
				}

				return new SummarySection[] { result };
			}

			return Array.Empty<SummarySection>();
		}

		#endregion

		#region Invoice

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<ClientMappingUsage>().OrderBy(x => x.SubCode).ToArray();
			var discountAmountByInterface = DiscountWithSubCodeCalculations.Where(d => d.Value.DiscountAmount != 0).GroupBy(d => d.Key.Item2).ToDictionary(g => g.Key, g => g.Sum(d => d.Value.DiscountAmount));
			var surchargeAmountByInterface = SurchargeWithSubCodeCalculations.Where(d => d.Value.DiscountAmount != 0).GroupBy(d => d.Key.Item2).ToDictionary(g => g.Key, g => g.Sum(d => d.Value.DiscountAmount));

			foreach (var usageTaxGroup in usages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				foreach (var usageGroup in usageTaxGroup.GroupBy(u => u.SubCode))
				{
					var usage = usageGroup.First();
					var groupUnitCount = usageGroup.Sum(u => u.UnitCount);
					var groupTransactionCount = usageGroup.Sum(u => u.TransactionCount);
					var groupIncludedTransactionCount = usageGroup.Sum(u => u.IncludedUnitCount);
					var groupAmount = usageGroup.Sum(u => u.Amount);

					if (groupAmount != 0)
					{
						var line = new BillLine(groupAmount, usageTaxGroup.Key, CurrencyCode, GetAmountChargeCodeName(usage), GetInvoiceLineDescription(usage, groupUnitCount, groupTransactionCount, groupIncludedTransactionCount), lines.Count, SystemCode);
						lines.Add(line);

						BillLine discountLine = null;
						Decimal discountAmount;
						if (discountAmountByInterface.TryGetValue(usageGroup.Key, out discountAmount))
						{
							discountLine = new BillLine(-discountAmount, usageTaxGroup.Key, CurrencyCode, GetDiscountChargeCodeName(usage), GetDiscountLineDescrption(usage), lines.Count, SystemCode);
							lines.Add(discountLine);
							line.RequireCommentLineAfter = false;
						}

						Decimal surchargeAmount;
						if (surchargeAmountByInterface.TryGetValue(usageGroup.Key, out surchargeAmount))
						{
							var surchargeLine = new BillLine(-surchargeAmount, usageTaxGroup.Key, CurrencyCode, GetSurchargeChargeCodeName(), GetSurchargeLineDescrption(usage), lines.Count, SystemCode);
							lines.Add(surchargeLine);
							line.RequireCommentLineAfter = false;
							if (discountLine != null)
							{
								discountLine.RequireCommentLineAfter = false;
							}
						}
					}
				}
			}
		}

		string GetInvoiceLineDescription(ClientMappingUsage usage, int groupUnitCount, int groupTransactionCount, int groupIncludedTransactionCount)
		{
			var descriptionBuilder = new ZStringBuilder();
			descriptionBuilder.AppendLine(SystemDescription);
			descriptionBuilder.AppendLine(usage.PriceItem.L7_DescriptionLocalized);

			var chargeUnit = usage.Reference3.ReplaceIgnoringCase("Per ", "").SplitCamelCase();
			var chargeAmountLine = groupUnitCount.ToString(CultureInfo.InvariantCulture) + " " + chargeUnit + " x " + usage.CurrencyCode + " " + usage.PriceItem.L7_Price.ToString(BillingConstants.AmountFourDecimalFormat, CultureInfo.InvariantCulture);
			if (groupIncludedTransactionCount > 0)
			{
				descriptionBuilder.AppendLine("Included " + groupIncludedTransactionCount.ToString(CultureInfo.InvariantCulture) + " " + chargeUnit);
				descriptionBuilder.AppendLine("Actual " + groupTransactionCount.ToString(CultureInfo.InvariantCulture) + " " + chargeUnit);
				descriptionBuilder.Append("Excess fee " + chargeAmountLine);
			}
			else
			{
				descriptionBuilder.Append(chargeAmountLine);
			}

			return descriptionBuilder.ToString();
		}

		string GetDiscountLineDescrption(ClientMappingUsage usage)
		{
			var descriptionBuilder = new ZStringBuilder();
			descriptionBuilder.AppendLine(SystemDescription + " Discount");
			descriptionBuilder.Append(usage.PriceItem.L7_DescriptionLocalized);
			return descriptionBuilder.ToString();
		}

		string GetSurchargeLineDescrption(ClientMappingUsage usage)
		{
			var descriptionBuilder = new ZStringBuilder();
			descriptionBuilder.AppendLine(SystemDescription + " Surcharge");
			descriptionBuilder.Append(usage.PriceItem.L7_DescriptionLocalized);
			return descriptionBuilder.ToString();
		}

		#endregion
	}
}

