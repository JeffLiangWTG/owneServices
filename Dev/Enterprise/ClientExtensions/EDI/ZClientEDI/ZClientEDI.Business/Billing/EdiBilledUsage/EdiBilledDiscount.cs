using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBilledDiscount : AutoEdiBilledDiscount
	{
		public EdiBilledDiscount(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static void CreateBilledDiscounts(EdiBilledUsage billedUsage, IEnumerable<DiscountDetailedInfo> discountDetailedInfos, IEnumerable<DiscountDetailedInfo> surchargeDetailedInfos)
		{
			if (billedUsage != null)
			{
				var billedUsageDiscountAmount = billedUsage.BU9_TransactionAmountPreDiscount - billedUsage.BU9_TransactionAmountPostDiscount + billedUsage.BU9_TransactionProcessingAmount;

				if (billedUsageDiscountAmount != 0)
				{
					var detailedInfos = IfNull(discountDetailedInfos).Concat(IfNull(surchargeDetailedInfos)).ToArray();

					if (detailedInfos.Any())
					{
						var discounts = detailedInfos.Where(x => x.DiscountAmount != 0m);
						var totalDiscountAmount = discounts.Sum(x => x.DiscountAmount);

						if (totalDiscountAmount != 0)
						{
							var multiplier = billedUsageDiscountAmount / totalDiscountAmount;

							var discountGroups = discounts.GroupBy(x => x.DiscountType).Select(x =>
								new { DiscountType = x.Key, AmountSum = x.Sum(y => y.DiscountAmount), AveragePercent = x.Average(y => y.DiscountPercent) }).ToArray();

							foreach (var discountGroup in discountGroups)
							{
								var billedDiscount = billedUsage.Factory.New<EdiBilledDiscount>();
								billedDiscount.BD9_BU9_Usage = billedUsage.PK;
								billedDiscount.BD9_Type = discountGroup.DiscountType;
								billedDiscount.BD9_PHD_Discount = ZGuid.Empty;
								billedDiscount.BD9_TransactionAmount = Utilities.Round(discountGroup.AmountSum * multiplier, DiscountTransactionAmountDecimals);
								billedDiscount.BD9_Percent = discountGroup.AveragePercent;
							}
						}
					}
				}
			}
		}

		public static void CreateBilledDiscounts(EdiBilledUsage billedUsage, IEnumerable<IStlDiscount> stlDiscounts)
		{
			if (billedUsage != null && stlDiscounts != null && stlDiscounts.Any())
			{
				var billedUsageDiscountAmount = billedUsage.BU9_TransactionAmountPreDiscount - billedUsage.BU9_TransactionAmountPostDiscount + billedUsage.BU9_TransactionProcessingAmount;

				if (billedUsageDiscountAmount != 0)
				{
					var discounts = stlDiscounts.Where(x => x.HeaderDiscount != null && x.Percentage != 0m);

					if (discounts.Sum(x => x.Percentage) != 0)
					{
						var partialDiscounts = discounts.Where(x => x.Percentage < 100m);
						var fullDiscounts = discounts.Where(x => x.Percentage >= 100m);
						decimal partialDiscountAmount = 0;
						if (partialDiscounts.Any())
						{
							if (fullDiscounts.Any())
							{
								decimal effectiveMultiplier = 1;
								foreach (var discount in partialDiscounts)
								{
									effectiveMultiplier *= 1 - discount.Percentage / 100m;
								}
								effectiveMultiplier = Utilities.Round(effectiveMultiplier, StlBilling.DiscountSet.EffectiveMultiplierDecimals);
								partialDiscountAmount = billedUsageDiscountAmount * (1 - effectiveMultiplier);
							}
							else
							{
								partialDiscountAmount = billedUsageDiscountAmount;
							}

							CreateDiscounts(billedUsage, partialDiscounts, partialDiscountAmount);
						}

						if (fullDiscounts.Any())
						{
							CreateDiscounts(billedUsage, fullDiscounts, billedUsageDiscountAmount - partialDiscountAmount);
						}
					}
				}
			}
		}

		static void CreateDiscounts(EdiBilledUsage billedUsage, IEnumerable<IStlDiscount> stlDiscounts, decimal amountToSplit)
		{
			decimal totalPercentage = stlDiscounts.Sum(x => Math.Min(x.Percentage, 100m));

			foreach (var discount in stlDiscounts)
			{
				var billedDiscount = billedUsage.Factory.New<EdiBilledDiscount>();
				billedDiscount.BD9_BU9_Usage = billedUsage.PK;
				billedDiscount.BD9_Type = ZString.Empty;
				billedDiscount.BD9_PHD_Discount = discount.HeaderDiscount.PK;
				billedDiscount.BD9_TransactionAmount = Utilities.Round(amountToSplit * Math.Min(discount.Percentage, 100m) / totalPercentage, DiscountTransactionAmountDecimals);
				billedDiscount.BD9_Percent = discount.Percentage;
			}
		}

		const int DiscountTransactionAmountDecimals = 4;

		static IEnumerable<DiscountDetailedInfo> IfNull(IEnumerable<DiscountDetailedInfo> infos)
		{
			return infos ?? Enumerable.Empty<DiscountDetailedInfo>();
		}
	}
}


