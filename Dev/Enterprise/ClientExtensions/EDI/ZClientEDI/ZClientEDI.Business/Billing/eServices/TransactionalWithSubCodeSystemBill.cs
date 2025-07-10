using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing
{
	public class TransactionalWithSubCodeSystemBill : TransactionSystemBillEx
	{
		public TransactionalWithSubCodeSystemBill(ZString systemCode, BusinessObjectFactory factory)
			: base(systemCode, factory)
		{
		}

		#region Calculate Group Amounts

		protected override void CalculateGroupAmounts()
		{
			CalculateDiscounts();
			Amount = DiscountWithSubCodeCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountWithSubCodeCalculations.Values.Sum(x => x.DiscountAmount);
			SurchargeAmount = -SurchargeWithSubCodeCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscounts()
		{
			CalculateDiscountsWithSubCode(DiscountWithSubCodeCalculations, SurchargeWithSubCodeCalculations);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void CalculateDiscountsWithSubCode(Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountCalc, Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> surchargeCalc)
		{
			discountCalc.Clear();

			var allDiscounts = new List<ClientLicenceBillingDiscount>();
			var localDiscounts = Discounts;

			foreach (var usagesGroupedByMonth in SystemUsages.GroupBy(x => x.PeriodStart))
			{
				foreach (var usagesGroupedBySubCode in usagesGroupedByMonth.GroupBy(x => x.SubCode))
				{
					var firstDayOfMonth = usagesGroupedByMonth.Key;
					var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
					var firstUsage = usagesGroupedBySubCode.First();
					allDiscounts.Clear();
					AddAllDiscounts(allDiscounts, firstUsage, localDiscounts);

					amountToDiscount = usagesGroupedBySubCode.Sum(x => x.Amount);
					ZInt unitCount = usagesGroupedBySubCode.Sum(x => x.UnitCount);
					ZDecimal unitPrice = firstUsage.UnitPrice;

					DiscountCalculation discountCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(allDiscounts, this, unitCount, unitPrice, firstDayOfMonth, lastDayOfMonth, usagesGroupedBySubCode.Key);
					discountCalc.Add(new Tuple<ZDateTime, ZString>(firstDayOfMonth, firstUsage.SubCode), discountCalculation);
					DiscountCalculationsToSystemUsages.Add(discountCalculation, usagesGroupedBySubCode.ToArray());

					DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(allDiscounts, this, discountCalculation.Amount, firstDayOfMonth, lastDayOfMonth, usagesGroupedBySubCode.Key);
					surchargeCalc.Add(new Tuple<ZDateTime, ZString>(firstDayOfMonth, firstUsage.SubCode), surchargeCalculation);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> DiscountWithSubCodeCalculations
		{
			get { return discountCalculations ?? (discountCalculations = new Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> discountCalculations;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> SurchargeWithSubCodeCalculations
		{
			get { return surchargeCalculations ?? (surchargeCalculations = new Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation>()); }
		}
		Dictionary<Tuple<ZDateTime, ZString>, DiscountCalculation> surchargeCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountWithSubCodeCalculations.Values.SelectMany(x => x.Details);
		protected override IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => SurchargeWithSubCodeCalculations.Values.SelectMany(x => x.Details);

		public override IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			var minimumFeeContribution = new Dictionary<Tuple<ZGuid, ZDateTime>, ZDecimal>();
			foreach (var periodDiscountCalculation in DiscountWithSubCodeCalculations)
			{
				var period = periodDiscountCalculation.Key.Item1;
				SystemUsage[] usages = null;

				if (DiscountCalculationsToSystemUsages.TryGetValue(periodDiscountCalculation.Value, out usages))
				{
					var usageCount = usages.Sum(x => x.UnitCount);
					var amount = periodDiscountCalculation.Value.Amount;
					var discountAmount = periodDiscountCalculation.Value.DiscountAmount;
					var surchargeAmount = -SurchargeWithSubCodeCalculations[periodDiscountCalculation.Key].DiscountAmount;

					if (usageCount > 0)
					{
						foreach (var usagesGroupedByDatabase in usages.Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => x.User.DatabasePK))
						{
							var ratio = usagesGroupedByDatabase.Sum(x => x.UnitCount) / (decimal)usageCount;
							var dbContribution = (amount - discountAmount + surchargeAmount) * ratio;

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
			}

			foreach (var contribution in minimumFeeContribution)
			{
				var minimumFee = new SystemMinimumFee(contribution.Key.Item1, contribution.Key.Item2, contribution.Value, CurrencyCode, "");
				result.Add(minimumFee);
			}

			return result;
		}

		#endregion

		#region Invoice

		protected override void CreateInvoiceLinesCore(List<BillLine> lines, ZDateTime dateForExchangeRate, ARInvoice invoice)
		{
			var usages = SystemUsages.Cast<PriceItemUsage>();

			foreach (var usageSubCodeGroup in usages.GroupBy(y => y.SubCode))
			{
				var subCodeGroupWeights = BuildTaxGroupWeights(usageSubCodeGroup);
				bool hasSinglePriceForAllPeriods = !usageSubCodeGroup.GroupBy(u => new { u.CurrencyCode, u.UnitPrice }).Skip(1).Any();

				if (hasSinglePriceForAllPeriods)
				{
					var usage = usageSubCodeGroup.First();
					var discountCalculationsBySubCode = DiscountWithSubCodeCalculations.Where(x => x.Key.Item2 == usage.SubCode);
					var groupUnitCount = discountCalculationsBySubCode.Sum(x => x.Value.UnitCount);
					var groupAmount = discountCalculationsBySubCode.Sum(x => x.Value.Amount);

					if (groupAmount != 0)
					{
						CreateChargeProRataLines(lines, subCodeGroupWeights, groupAmount, groupUnitCount, usage);
					}
				}
				else
				{
					foreach (var usagePeriodGroup in usageSubCodeGroup.GroupBy(u => u.PeriodStart))
					{
						var usage = usagePeriodGroup.First();
						var discountCalculation = DiscountWithSubCodeCalculations[new Tuple<ZDateTime, ZString>(usage.PeriodStart, usage.SubCode)];
						var groupUnitCount = discountCalculation.UnitCount;
						var groupAmount = discountCalculation.Amount;

						if (groupAmount != 0)
						{
							CreateChargeProRataLines(lines, subCodeGroupWeights, groupAmount, groupUnitCount, usage);
						}
					}
				}
			}

			var groupWeights = BuildTaxGroupWeights(SystemUsages);
			CreateDiscountAndSurchargeProRataLines(lines, groupWeights);
		}

		#endregion

	}
}

