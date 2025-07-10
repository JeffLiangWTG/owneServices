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
	/// <summary>
	/// TransactionalSystemBill extended with default SummarySections
	/// and Invoice Lines.
	/// </summary>
	public class TransactionSystemBillEx : TransactionalSystemBill
	{
		public TransactionSystemBillEx(ZString systemCode, BusinessObjectFactory factory)
			: base(systemCode, factory)
		{
		}

		#region General Summary

		public override SummarySection[] GetGeneralSummarySections(ZGuid organisationPK)
		{
			var organisationUsages = GetGeneralSummarySectionsUsages(organisationPK);
			if (organisationUsages.Length == 0)
			{
				return Array.Empty<SummarySection>();
			}

			var summarySectionList = new List<SummarySection>();
			foreach (var usageFilter in GetGeneralSummarySectionsUsageFilters())
			{
				AddSummarySection(summarySectionList, organisationUsages, usageFilter);
			}
			return summarySectionList.ToArray();
		}

		protected virtual PriceItemUsage[] GetGeneralSummarySectionsUsages(ZGuid organisationPK)
		{
			return SystemUsages.Cast<PriceItemUsage>().Where(x => x.OrganisationPK == organisationPK).ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<Predicate<PriceItemUsage>> GetGeneralSummarySectionsUsageFilters()
		{
			yield return (u => true);
		}

		void AddSummarySection(List<SummarySection> sections, PriceItemUsage[] organisationUsages, Predicate<PriceItemUsage> usageFilter)
		{
			var filteredUsages = organisationUsages.Where(x => usageFilter(x)).ToList();

			if (filteredUsages.Count > 0)
			{
				bool hasDifferentPeriods = filteredUsages.Select(x => x.PeriodStart).Distinct().Count() > 1;
				SummarySection firstSummary = null;

				SummarySection section = new SummarySection(Factory);
				foreach (var usage in filteredUsages)
				{
					SummarySection currentSummary = usage.GetGeneralSummarySections().First();
					if (firstSummary == null)
					{
						firstSummary = currentSummary;
					}
					foreach (SummaryLine line in currentSummary.Lines.Cast<SummaryLine>())
					{
						if (hasDifferentPeriods)
						{
							line.MainDescription += " " + usage.PeriodStartAsText;
						}
						section.Lines.Add(line);
					}
				}

				section.Header.MainDescription = firstSummary.Header.MainDescription;
				section.Header.AdditionalDescription = firstSummary.Header.AdditionalDescription;
				section.Header.PurchasedCount = firstSummary.Header.PurchasedCount;
				section.Header.UnitPrice = firstSummary.Header.UnitPrice;
				section.Header.UnitCount = firstSummary.Header.UnitCount;
				section.Header.TotalUnitCount = firstSummary.Header.TotalUnitCount;
				section.Header.Amount = firstSummary.Header.Amount;
				section.Header.TotalAmount = filteredUsages.Sum(x => x.Amount).ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				section.Header.ClientCompanyDescription = firstSummary.Header.ClientCompanyDescription;

				sections.Add(section);
			}
		}

		#endregion

		#region Group Summary

		public override SummarySection[] GetGroupSummarySections()
		{
			SummarySection result = BuildGroupSummarySectionOneLinePerOrg();

			return new SummarySection[] { result };
		}

		#endregion

		#region Invoice

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<PriceItemUsage>();
			var groupWeights = BuildTaxGroupWeights(SystemUsages);

			bool hasSinglePriceForAllPeriods = !usages.GroupBy(u => new { u.CurrencyCode, u.UnitPrice }).Skip(1).Any();
			if (hasSinglePriceForAllPeriods)
			{
				var usage = usages.First();
				var groupUnitCount = DiscountCalculations.Sum(x => x.Value.UnitCount);
				var groupAmount = DiscountCalculations.Sum(x => x.Value.Amount);

				if (groupAmount != 0)
				{
					CreateChargeProRataLines(lines, groupWeights, groupAmount, groupUnitCount, usage);
				}
			}
			else
			{
				foreach (var usagePeriodGroup in usages.GroupBy(u => u.PeriodStart))
				{
					var usage = usagePeriodGroup.First();
					var discountCalculation = DiscountCalculations[usage.PeriodStart];
					var groupUnitCount = discountCalculation.UnitCount;
					var groupAmount = discountCalculation.Amount;

					if (groupAmount != 0)
					{
						CreateChargeProRataLines(lines, groupWeights, groupAmount, groupUnitCount, usage);
					}
				}
			}

			CreateDiscountAndSurchargeProRataLines(lines, groupWeights);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void CreateChargeProRataLines(List<BillLine> lines, List<KeyValuePair<TaxGroup, decimal>> groupWeights, decimal amount, int unitCount, PriceItemUsage firstUsage)
		{
			if (amount != 0)
			{
				decimal totalWeight = groupWeights.Sum(x => x.Value);
				decimal amountRemaining = amount;
				int unitCountRemaining = unitCount;

				var nonZeroGroupWeights = groupWeights.FindAll(x => x.Value != 0);
				for (int i = 0; i < nonZeroGroupWeights.Count; ++i)
				{
					bool isLastGroup = (i == nonZeroGroupWeights.Count - 1);

					var groupWeight = nonZeroGroupWeights[i];
					decimal groupAmount = isLastGroup
										? amountRemaining
										: ZArchitecture.Core.Utilities.Round(amount * groupWeight.Value / totalWeight, BillingConstants.RoundingDecimals);

					int groupUnitCount = isLastGroup
										? unitCountRemaining
										: (int)Math.Ceiling(unitCount * groupWeight.Value / totalWeight);

					if (groupAmount != 0)
					{
						amountRemaining -= groupAmount;
						unitCountRemaining -= groupUnitCount;
						lines.Add(new BillLine(groupAmount, groupWeight.Key, CurrencyCode, GetAmountChargeCodeName(firstUsage), GetInvoiceDescription(firstUsage, groupUnitCount), lines.Count, SystemCode, ZGuid.Empty));
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual void CreateDiscountAndSurchargeProRataLines(List<BillLine> lines, List<KeyValuePair<TaxGroup, decimal>> groupWeights)
		{
			decimal totalWeight = groupWeights.Sum(x => x.Value);
			CreateProRataLines(lines, groupWeights, totalWeight, -DiscountAmount, GetDiscountChargeCodeName(null), GetInvoiceDiscountDescription());
			CreateProRataLines(lines, groupWeights, totalWeight, SurchargeAmount, GetSurchargeChargeCodeName(), GetInvoiceSurchargeDescription());
		}

		protected virtual ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			return ZString.Format("{0} - {1} Transactions at {2} {3} per Transaction",
				SystemDescription,
				unitCount,
				systemUsage.CurrencyCode,
				systemUsage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		protected virtual ZString GetInvoiceDiscountDescription()
		{
			return SystemDescription + " Discount";
		}

		protected virtual ZString GetInvoiceSurchargeDescription()
		{
			return SystemDescription + " Surcharge";
		}

		#endregion

		#region Validation

		protected override void ValidateUnitPrice(BusinessObject notificationOwner)
		{
			if (SystemUsages.Count > 0)
			{
				foreach (var systemUsage in SystemUsages)
				{
					var itemUsage = systemUsage as PriceItemUsage;
					if (itemUsage != null && itemUsage.SystemCode != BillingConstants.BillingSystem.OceanTracingLegacy && itemUsage.PriceItem == null)
					{
						notificationOwner.AddRowError(string.Format(CultureInfo.InvariantCulture, "{0} [{1}]: No price found for {2}", BillingSystemDescription, itemUsage.SubCode, itemUsage.Organisation.OH_Code));
					}
				}
			}
		}

		#endregion
	}
}

