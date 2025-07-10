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
	public class ABMCustomsBill : TransactionalWithSubCodeSystemBill
	{
		public ABMCustomsBill(BusinessObjectFactory factory)
			: base(BillingConstants.BillingSystem.ABMCustoms, factory)
		{
		}

		#region Charge Codes

		public override ZString GetAmountChargeCodeName(SystemUsage usage)
		{
			return GetChargeCodeName((ABMCustomsUsage)usage, false);
		}

		internal ZString GetChargeCodeName(ABMCustomsUsage usage, bool isDiscount)
		{
			if (usage.IsCustoms) { return isDiscount ? CustomsDiscountCodeName : CustomsCodeName; }
			if (usage.IsPortCommunity) { return isDiscount ? PortCommunityDiscountChargeCode : PortCommunityChargeCode; }
			if (usage.IsFiscalRep) { return isDiscount ? FiscalRepDiscountChargeCode : FiscalRepChargeCode; }
			else
			{
				return isDiscount ? base.GetDiscountChargeCodeName(usage) : base.GetAmountChargeCodeName(usage);
			}
		}

		public override ZString GetDiscountChargeCodeName(SystemUsage usage)
		{
			return GetChargeCodeName((ABMCustomsUsage)usage, true);
		}

		ZString CustomsCodeName
		{
			get { return EDIDataRegistry.Instance.ABMCustomsWareMessagingChargeCode.Value; }
		}

		ZString PortCommunityChargeCode
		{
			get { return EDIDataRegistry.Instance.ABMMovementMessagingChargeCode.Value; }
		}

		ZString FiscalRepChargeCode
		{
			get { return EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingChargeCode.Value; }
		}

		ZString CustomsDiscountCodeName
		{
			get { return EDIDataRegistry.Instance.ABMCustomsWareMessagingDiscountChargeCode.Value; }
		}

		ZString PortCommunityDiscountChargeCode
		{
			get { return EDIDataRegistry.Instance.ABMMovementMessagingDiscountChargeCode.Value; }
		}

		ZString FiscalRepDiscountChargeCode
		{
			get { return EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingDiscountChargeCode.Value; }
		}

		#endregion

		#region Invoice

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<ABMCustomsUsage>().ToArray();
			Array.Sort(usages, new ABMCustomsUsageComparer());

			foreach (var usageSubCodeGroup in usages.GroupBy(u => u.SubCode))
			{
				var subCodeGroupWeights = BuildTaxGroupWeights(usageSubCodeGroup);

				var usage = usageSubCodeGroup.First();
				var groupUnitCount = usageSubCodeGroup.Sum(u => u.UnitCount);
				var groupAmount = usageSubCodeGroup.Sum(u => u.Amount);

				if (groupAmount != 0)
				{
					CreateChargeProRataLines(lines, subCodeGroupWeights, groupAmount, groupUnitCount, usage);

					var discountCalculation = DiscountWithSubCodeCalculations[new Tuple<ZDateTime, ZString>(PeriodStart, usage.SubCode)];
					var surchargeCalculation = SurchargeWithSubCodeCalculations[new Tuple<ZDateTime, ZString>(PeriodStart, usage.SubCode)];
					decimal totalWeight = subCodeGroupWeights.Sum(x => x.Value);
					decimal discountAmount = discountCalculation != null ? discountCalculation.DiscountAmount : ZDecimal.Zero;
					decimal surchargeAmount = surchargeCalculation != null ? surchargeCalculation.DiscountAmount : ZDecimal.Zero;
					decimal discountRemaining = discountAmount;
					decimal surchargetRemaining = surchargeAmount;

					var nonZeroGroupWeights = subCodeGroupWeights.FindAll(x => x.Value != 0);
					for (int i = 0; i < nonZeroGroupWeights.Count; ++i)
					{
						bool isLastGroup = (i == nonZeroGroupWeights.Count - 1);
						var groupWeight = nonZeroGroupWeights[i];

						if (discountAmount != 0)
						{
							decimal discountTaxGroupAmount = isLastGroup ? discountRemaining : ZArchitecture.Core.Utilities.Round(discountAmount * groupWeight.Value / totalWeight, BillingConstants.RoundingDecimals);
							if (discountTaxGroupAmount != 0)
							{
								discountRemaining -= discountTaxGroupAmount;
								lines.Add(new BillLine(-discountTaxGroupAmount, groupWeight.Key, CurrencyCode, GetDiscountChargeCodeName(usage), GetInvoiceDiscountDescription(usage, discountCalculation), lines.Count, SystemCode));
							}
						}

						if (surchargeAmount != 0)
						{
							decimal surchargeTaxGroupAmount = isLastGroup ? surchargetRemaining : ZArchitecture.Core.Utilities.Round(surchargeAmount * groupWeight.Value / totalWeight, BillingConstants.RoundingDecimals);
							if (surchargeTaxGroupAmount != 0)
							{
								surchargetRemaining -= surchargeTaxGroupAmount;
								lines.Add(new BillLine(-surchargeTaxGroupAmount, groupWeight.Key, CurrencyCode, GetSurchargeChargeCodeName(), GetInvoiceDiscountDescription(usage, surchargeCalculation), lines.Count, SystemCode));
							}
						}
					}
				}
			}
		}

		protected override ZString GetInvoiceDescription(PriceItemUsage systemUsage, int unitCount)
		{
			return ZString.Format("{0} - {1} Transactions at {2} {3} per transaction",
				GetTransactionDescription(systemUsage.SubCode, true),
				unitCount,
				systemUsage.CurrencyCode,
				systemUsage.PriceItem.L7_Price.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture));
		}

		ZString GetInvoiceDiscountDescription(ABMCustomsUsage usage, DiscountCalculation discountCalculation)
		{
			var discount = string.Join(" and a ", discountCalculation.InvoiceDescriptions);
			return ZString.Format("A {0} to be applied on {1}", discount, GetTransactionDescription(usage.SubCode, true));
		}

		ZString GetTransactionDescription(string code, bool includeABMPrefix)
		{
			ZString result = includeABMPrefix ? "ABM " : string.Empty;
			if (ABMCustomsTransactionTypes == null)
			{
				ABMCustomsTransactionTypes = new ABMCustomsTransactionTypes();
			}

			return result + ABMCustomsTransactionTypes.GetDescriptionFromCode(code);
		}

		ABMCustomsTransactionTypes ABMCustomsTransactionTypes;

		#endregion

		#region Summary Sections

		protected override PriceItemUsage[] GetGeneralSummarySectionsUsages(ZGuid organisationPK)
		{
			var usages = base.GetGeneralSummarySectionsUsages(organisationPK).Cast<ABMCustomsUsage>().ToArray();
			Array.Sort(usages, new ABMCustomsUsageComparer());
			return usages;
		}

		protected override IEnumerable<Predicate<PriceItemUsage>> GetGeneralSummarySectionsUsageFilters()
		{
			yield return (u => ((ABMCustomsUsage)u).IsCustoms);
			yield return (u => ((ABMCustomsUsage)u).IsPortCommunity);
			yield return (u => ((ABMCustomsUsage)u).IsFiscalRep);
		}

		class ABMCustomsUsageComparer : IComparer<ABMCustomsUsage>
		{
			public int Compare(ABMCustomsUsage x, ABMCustomsUsage y)
			{
				if (x.SubCode != y.SubCode)
				{
					return GetSubCodeOrder(x.SubCode).CompareTo(GetSubCodeOrder(y.SubCode));
				}

				if (x.PeriodStart != y.PeriodStart)
				{
					return x.PeriodStart.CompareTo(y.PeriodStart);
				}

				if (x.Jurisdiction != y.Jurisdiction)
				{
					return x.Jurisdiction.CompareTo(y.Jurisdiction);
				}

				if (x.Department != y.Department)
				{
					return x.Department.CompareTo(y.Department);
				}

				if (x.Provider != y.Provider)
				{
					return x.Provider.CompareTo(y.Provider);
				}

				return 0;
			}

			int GetSubCodeOrder(string subCode)
			{
				if (subCode == ABMCustomsTransactionTypes.Codes.Customs) { return 1; }
				else if (subCode == ABMCustomsTransactionTypes.Codes.PortCommunity) { return 2; }
				else if (subCode == ABMCustomsTransactionTypes.Codes.FiscalRep) { return 3; }
				else { return 4; }
			}
		}

		#endregion

		#region Group Summary

		public override SummarySection[] GetGroupSummarySections()
		{
			var result = new List<SummarySection>(1);
			var options = GetGroupSummaryOptions();

			var usages = SystemUsages.Cast<ABMCustomsUsage>().ToArray();
			Array.Sort(usages, new ABMCustomsUsageComparer());

			foreach (var transactionGroup in usages.GroupBy(x => x.SubCode))
			{
				var section = new SummarySection(Factory);
				section.Header.MainDescription = GetTransactionDescription(transactionGroup.Key, true) + " Group Summary";
				ZDecimal totalAmount = 0m;

				var usageGroups = transactionGroup.OrderBy(x => x.Organisation.OH_Code).ThenBy(x => x.PeriodStart).GroupBy(x => new { x.Organisation.OH_Code, x.LicenceNineCode, x.PeriodStartAsText });
				foreach (var group in usageGroups)
				{
					ZDecimal usageAmount = group.Sum(x => x.Amount);
					if (usageAmount != 0m)
					{
						SummaryLine groupLine = section.Lines.AddNew();
						groupLine.MainDescription = group.Key.OH_Code;
						groupLine.Amount = usageAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
						BuildSummaryLine(groupLine, options, group.First());
						totalAmount += usageAmount;
					}
				}

				section.Header.TotalAmount = totalAmount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
				section.Header.AmountDescription = ZString.Format("Total Price ({0})", CurrencyCode);
				section.Header.TotalDescription = ZString.Format("Total {0} ({1})", GetTransactionDescription(transactionGroup.Key, true), CurrencyCode);

				result.Add(section);
			}

			return result.ToArray();
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
						summaryLine.MainDescription = GetTransactionDescription(periodProviderDiscountCalculation.Key.Item2, false) + " - " + discountDescription;

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
	}
}

