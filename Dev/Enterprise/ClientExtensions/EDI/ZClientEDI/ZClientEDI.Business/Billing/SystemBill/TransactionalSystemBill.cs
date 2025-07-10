using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class TransactionalSystemBill : PriceItemBill, IDiscountable, ISystemMinimumFeeContributionBill
	{
		public TransactionalSystemBill(ZString systemCode, BusinessObjectFactory factory)
			: base(systemCode, factory)
		{
			this.SystemCode = systemCode;
			this.amountChargeCodeName = EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(systemCode);
			this.discountChargeCodeName = EDIDataRegistry.Instance.TransactionDiscountChargeCodes.Value.GetDescriptionFromCode(systemCode);
		}

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
			CalculateDiscountsAndSurcharges();

			Amount = DiscountCalculations.Values.Sum(x => x.Amount);
			DiscountAmount = DiscountCalculations.Values.Sum(x => x.DiscountAmount);
			SurchargeAmount = -SurchargeCalculations.Values.Sum(x => x.DiscountAmount);
		}

		void CalculateDiscountsAndSurcharges()
		{
			DiscountCalculations.Clear();
			DiscountCalculationsToSystemUsages.Clear();
			SurchargeCalculations.Clear();
			List<ClientLicenceBillingDiscount> allDiscounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscountCollection localDiscounts = Discounts;

			foreach (var usagesGroupedByMonth in SystemUsages.GroupBy(x => x.PeriodStart))
			{
				var firstDayOfMonth = usagesGroupedByMonth.Key;
				var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
				var firstUsage = usagesGroupedByMonth.First();
				allDiscounts.Clear();
				AddAllDiscounts(allDiscounts, firstUsage, localDiscounts);

				amountToDiscount = usagesGroupedByMonth.Sum(x => x.Amount);
				ZInt unitCount = usagesGroupedByMonth.Sum(x => x.UnitCount);
				ZDecimal unitPrice = firstUsage.UnitPrice;

				DiscountCalculation discountCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalDiscount(allDiscounts, this, unitCount, unitPrice, firstDayOfMonth, lastDayOfMonth);
				DiscountCalculations.Add(firstDayOfMonth, discountCalculation);
				DiscountCalculationsToSystemUsages.Add(discountCalculation, usagesGroupedByMonth.ToArray());

				DiscountCalculation surchargeCalculation = ClientLicenceBillingDiscountCollection.CalculateTransactionalSurcharge(allDiscounts, this, discountCalculation.Amount, firstDayOfMonth, lastDayOfMonth);
				SurchargeCalculations.Add(firstDayOfMonth, surchargeCalculation);
			}
		}

		protected virtual void AddAllDiscounts(List<ClientLicenceBillingDiscount> discountsListToAddTo, SystemUsage firstUsage, ClientLicenceBillingDiscountCollection localDiscounts)
		{
			var prices = firstUsage.PriceHeader;
			if (prices != null)
			{
				discountsListToAddTo.AddRange(prices.Discounts);
			}
			if (localDiscounts != null)
			{
				discountsListToAddTo.AddRange(localDiscounts);
			}
		}

		public Dictionary<DiscountCalculation, SystemUsage[]> DiscountCalculationsToSystemUsages
		{
			get { return discountCalculationsToSystemUsages ?? (discountCalculationsToSystemUsages = new Dictionary<DiscountCalculation, SystemUsage[]>()); }
		}
		Dictionary<DiscountCalculation, SystemUsage[]> discountCalculationsToSystemUsages;

		protected Dictionary<ZDateTime, DiscountCalculation> DiscountCalculations
		{
			get { return discountCalculations ?? (discountCalculations = new Dictionary<ZDateTime, DiscountCalculation>()); }
		}
		Dictionary<ZDateTime, DiscountCalculation> discountCalculations;

		protected Dictionary<ZDateTime, DiscountCalculation> SurchargeCalculations
		{
			get { return surchargeCalculations ?? (surchargeCalculations = new Dictionary<ZDateTime, DiscountCalculation>()); }
		}
		Dictionary<ZDateTime, DiscountCalculation> surchargeCalculations;

		protected override IEnumerable<DiscountDetailedInfo> DiscountDetailedInfos => DiscountCalculations.Values.SelectMany(x => x.Details);
		protected override IEnumerable<DiscountDetailedInfo> SurchargeDetailedInfos => SurchargeCalculations.Values.SelectMany(x => x.Details);

		#endregion

		#region IDiscountable Members

		ZString IDiscountable.SystemCode
		{
			get { return SystemCode; }
		}

		ZDecimal IDiscountable.AmountToDiscount
		{
			get { return amountToDiscount; }
		}

		protected ZDecimal amountToDiscount;

		#endregion

		#region Summary Sections

		public override SummarySection[] GetDiscountSummarySections()
		{
			return GetMultiPeriodDiscountSummarySections(DiscountCalculations);
		}

		public override SummarySection[] GetSurchargeSummarySections()
		{
			return GetMultiPeriodDiscountSummarySections(SurchargeCalculations);
		}

		#endregion

		public virtual IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution()
		{
			var result = new List<SystemMinimumFee>(1);

			var minimumFeeContribution = new Dictionary<Tuple<ZGuid, ZDateTime>, ZDecimal>();
			foreach (var periodDiscountCalculation in DiscountCalculations)
			{
				var period = periodDiscountCalculation.Key;
				SystemUsage[] usages = null;

				if (DiscountCalculationsToSystemUsages.TryGetValue(periodDiscountCalculation.Value, out usages))
				{
					var usageCount = usages.Sum(x => x.UnitCount);
					var amount = periodDiscountCalculation.Value.Amount;
					var discountAmount = periodDiscountCalculation.Value.DiscountAmount;
					var surchargeAmount = -SurchargeCalculations[period].DiscountAmount;

					if (usageCount > 0)
					{
						foreach (var usagesGroupedByDatabase in usages.Where(x => !x.User.DatabasePK.IsEmpty).GroupBy(x => x.User.DatabasePK))
						{
							var ratio = usagesGroupedByDatabase.Sum(x => x.UnitCount) / (decimal)usageCount;
							var dbContribution = (amount - discountAmount + surchargeAmount) * ratio;

							var periodDatabaseKey = new Tuple<ZGuid, ZDateTime>(usagesGroupedByDatabase.Key, period);
							if (minimumFeeContribution.ContainsKey(periodDatabaseKey))
							{
								minimumFeeContribution[periodDatabaseKey] = minimumFeeContribution[periodDatabaseKey] + dbContribution;
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
	}
}

