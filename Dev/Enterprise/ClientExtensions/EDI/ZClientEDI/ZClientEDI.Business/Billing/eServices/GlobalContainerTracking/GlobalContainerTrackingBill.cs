using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class GlobalContainerTrackingBill : TransactionSystemBillEx
	{
		public GlobalContainerTrackingBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.GlobalContainerTracking, factory)
		{
		}

		#region Summary Sections

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			var organisationUsages = GetGeneralSummarySectionsUsages(organisationPK);
			if (organisationUsages == null || organisationUsages.Length == 0)
			{
				return System.Array.Empty<SummarySection>();
			}

			var summarySectionList = new List<SummarySection>();
			AddGlobalContainerTrackingSummarySection(summarySectionList, organisationUsages);
			return summarySectionList.ToArray();
		}

		void AddGlobalContainerTrackingSummarySection(List<SummarySection> sections, PriceItemUsage[] organisationUsages)
		{
			bool hasMultiplePeriods = organisationUsages.Select(x => x.PeriodStart).Distinct().Count() > 1;

			SummarySection section = new SummarySection(Factory);
			foreach (var periodGroup in organisationUsages.GroupBy(x => x.PeriodStart))
			{
				var headerLine = section.Lines.AddNew();
				headerLine.MainDescription = SystemDescription;
				if (hasMultiplePeriods)
				{
					headerLine.MainDescription += " " + periodGroup.First().PeriodStartAsText;
				}

				foreach (var usage in periodGroup)
				{
					SummarySection currentSummary = usage.GetGeneralSummarySections().First();
					foreach (SummaryLine line in currentSummary.Lines.Cast<SummaryLine>())
					{
						section.Lines.Add(line);
					}
				}

				if (hasMultiplePeriods)
				{
					section.Lines.AddNew();
				}
			}

			var firstSummary = organisationUsages.First().GetGeneralSummarySections().First();
			section.Header.MainDescription = firstSummary.Header.MainDescription;
			section.Header.AdditionalDescription = firstSummary.Header.AdditionalDescription;
			section.Header.UnitPrice = firstSummary.Header.UnitPrice;
			section.Header.UnitCount = firstSummary.Header.UnitCount;
			section.Header.TotalUnitCount = firstSummary.Header.TotalUnitCount;
			section.Header.Amount = firstSummary.Header.Amount;
			section.Header.TotalAmount = organisationUsages.Sum(x => x.Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);

			sections.Add(section);
		}

		#endregion

		#region Invoice

		[SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<GlobalContainerTrackingUsage>();

			foreach (var usageTaxGroup in usages.GroupBy(x => new TaxGroup(x.InvoiceDelivery)))
			{
				var firstUsage = usageTaxGroup.First();
				var groupAmount = usageTaxGroup.Sum(u => u.Amount);
				lines.Add(new BillLine(groupAmount, usageTaxGroup.Key, CurrencyCode, GetAmountChargeCodeName(firstUsage), SystemDescription + " Usage", lines.Count, SystemCode));
			}

			var groupWeights = BuildTaxGroupWeights(SystemUsages);
			CreateDiscountAndSurchargeProRataLines(lines, groupWeights);
		}

		#endregion
	}
}

