using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.Client.EDI.Billing.ODPL
{
	public interface IOdplDiscountable : IDiscountable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		ZDecimal AmountToDiscountForModule(string moduleCode, out string moduleName);

		// Amount of on-demand usage as licence units rather than money
		ZDecimal LicenceUnitsToDiscount { get; }

		// Number of on-demand users of Core module.
		int CoreOnDemandUsers { get; }

		// Mixed amount is the total, unadjusted usage amount including both purchased users and on-demand users.
		ZDecimal MixedAmountAsMoney { get; }
		ZDecimal MixedAmountAsLicenceUnits { get; }
		ZBool IsHostedOnWiseCloud { get; }
	}

	/// <summary>
	/// Supported discounts:
	/// Volume
	/// - the amount of usage determines which volume break applies. More usage means a higher discount.
	///    Break units can be Licence Units, in which case the amount in licence units determines the break.
	///    The amount used to determine the break, and the amount that is discounted can be different.
	///    
	/// Minimum fee 
	/// - with user count (L5_Units): amount is number of core users times core price (discount break / discount units). no actual discount, just a simplified price
	/// - without user count: amount is just the fixed discount amount
	/// 
	/// Module specific
	/// - discounts the amount for a single module, e.g., because the client contributed to the development of that module.
	///   Can be tricky to combine with other discounts (see Module + Commitment/Prepayment below).
	/// 
	/// Prepayment
	/// - raises the usage amount to the prepayment break if it is lower. Discount is on amounts above the prepayment break.
	///   Discount break not supported as Licence Units.
	///   Prepayment discounts are rarely granted. All but one are expired as of Mar 2013. Remaining one will expire Sep 2013.
	/// 
	/// Commitment
	/// - raises the usage amount to the commitment break if it is lower. Discount is on entire amount.
	///   Discount break can be in Licence Units.
	///
	/// Special
	/// - a straight percentage discount on the amount
	///
	/// Capped
	/// - a straight percentage discount with a cap on the total discount.
	///   E.g., client is granted a $100 discount for converting to ODM, to be taken off the bill at 10% per month.
	///   So if they use $100 a month it takes 10 months to use up the dicsount.
	///
	/// SPECIAL COMBINATIONS
	/// Minimum fee + Volume
	/// - the volume amount is the total mixed usage not the minimum fee amount, i.e., the amount they will be charged when the minimum fee discount ends.
	/// 
	/// Prepayment + Volume
	/// - the volume amount is the amount above the prepayment break. So the volume and the discount will be small if the usage is only just over the prepayment break.
	///   Combination was never actually used on any client.
	///   Doesn't work properly if volume is in licence units.
	///   
	/// Commitment + Volume
	/// - amount for volume break comparison is not altered by commitment. The discounts apply separately.
	/// 
	/// Module + Commitment/Prepayment
	/// - Commitment discount can wholly or partially eliminate a module discount.
	///   E.g., module discount reduces the amount below the commitment. Commitment discount then raises it back up.
	///   Any discount calculated before commitment could behave like this, but currently only module discounts come earlier.
	/// 
	/// INVALID COMBINATIONS
	/// Prepayment + Commitment don't work together. They do the same thing. If both exist, prepayment will be calculated and commitment ignored.
	/// </summary>
	public static class OdplDiscountCalculator
	{
		static public OdplDiscountCalculation CalculateDiscount(OdplSystemBill bill)
		{
			List<ClientLicenceBillingDiscount> discounts = GetDiscounts(bill);
			ClientLicencePriceHeader priceHeader = bill.SystemUsages.First().PriceHeader;
			return Calculate(discounts, bill, (priceHeader == null) ? null : priceHeader.L6_LicenceUnitRate);
		}

		#region GetDiscounts

		public static List<ClientLicenceBillingDiscount> GetDiscounts(SystemBill bill)
		{
			List<ClientLicenceBillingDiscount> result = new List<ClientLicenceBillingDiscount>();

			var usage = bill.SystemUsages[0];
			var systemCode = usage.SystemCode;
			var firstDayOfMonth = bill.PeriodStart;
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
			ClientLicencePriceHeader priceHeader = usage.PriceHeader;
			if (priceHeader != null)
			{
				result.AddRange(priceHeader.Discounts.Where(x => x.L5_SystemCode == systemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)));
			}

			var orgDiscounts = bill.Discounts;
			if (orgDiscounts != null)
			{
				result.AddRange(orgDiscounts.Where(x => x.L5_SystemCode == systemCode && x.IsDateRangeMatched(firstDayOfMonth, lastDayOfMonth)));
			}

			return result;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static OdplDiscountCalculation Calculate(IList<ClientLicenceBillingDiscount> discounts,
			IOdplDiscountable discountable,
			ZDecimal? licenceUnitRate = null)
		{
			ZDecimal amountToDiscount = discountable.AmountToDiscount;
			OdplDiscountCalculation result = new OdplDiscountCalculation() { Amount = amountToDiscount };

			if (discounts == null || discounts.Count == 0)
			{
				return result;
			}

			if (discounts.Any(x => x.IsVolume) && discounts.Any(x => x.IsCommitment))
			{
				result.ErrorDescriptions = result.ErrorDescriptions.Concat(new ZString[] { "Volume discount is not allowed with a Commitment discount" });
			}

			ZDecimal licenceUnitsAmount = discountable.LicenceUnitsToDiscount;

			// 1. Minimum fee/Module specific
			var minFee = discounts.FirstOrDefault(x => x.L5_Type == BillingConstants.DiscountType.MinimumFee);
			OdplDiscountCalculation moduleCalculation = null;

			if (minFee != null)
			{
				// Minimum fee discount is more like a simple price list than a discount.
				// It completely replaces the amount based on the full pricelist with a new amount
				// based on the core price from the discount and the number of core users.
				var minFeeCalculation = minFee.CalculateOnDemandMinimumFee(discountable.CoreOnDemandUsers);
				result = new OdplDiscountCalculation(minFeeCalculation);
				amountToDiscount = result.Amount;
			}
			else
			{
				moduleCalculation = CalculateModuleSpecificDiscounts(discounts, discountable);
				if (moduleCalculation != null)
				{
					moduleCalculation.Amount = amountToDiscount;
					amountToDiscount -= moduleCalculation.DiscountAmount;
					result.Amount = amountToDiscount;
				}
			}

			ZDecimal amountAfterModuleDiscounts = amountToDiscount;
			ZDecimal amountForVolumeDiscount = amountToDiscount;

			// volume to use for comparing with volume discount break amounts
			ZDecimal volumeAsMoney = discountable.MixedAmountAsMoney;
			ZDecimal volumeAsLicenceUnits = discountable.MixedAmountAsLicenceUnits;

			// 2. Threshold discount - prepayment or commitment
			ClientLicenceBillingDiscount prepaymentDiscount = discounts.FirstOrDefault(x => x.L5_Type == BillingConstants.DiscountType.Prepayment);
			ClientLicenceBillingDiscount commitmentDiscount = discounts.FirstOrDefault(x => x.L5_Type == BillingConstants.DiscountType.Commitment);
			if (prepaymentDiscount != null)
			{
				DiscountCalculation prepaymentDiscountCalculation = prepaymentDiscount.Calculate(result.Amount);
				result.Amount = prepaymentDiscountCalculation.Amount;
				result.MergeDiscountWith(prepaymentDiscountCalculation);
				amountForVolumeDiscount = result.Amount - prepaymentDiscount.L5_BreakAmount;
				volumeAsMoney = amountForVolumeDiscount;
			}
			else if (commitmentDiscount != null)
			{
				DiscountCalculation commitmentDiscountCalculation = commitmentDiscount.Calculate(result.Amount, licenceUnitRate, licenceUnitsAmount);
				result.Amount = commitmentDiscountCalculation.Amount;
				result.MergeDiscountWith(commitmentDiscountCalculation);
			}

			// 3. Volume discounts
			if (amountForVolumeDiscount > 0)
			{
				var volumeDiscount = ClientLicenceBillingDiscountCollection.GetVolumeDiscount(discounts, volumeAsMoney, volumeAsLicenceUnits);
				if (volumeDiscount != null)
				{
					result.MergeDiscountWith(volumeDiscount.Calculate(amountForVolumeDiscount));
				}
			}

			// 4. special
			foreach (ClientLicenceBillingDiscount billingDiscount in discounts.Where(x => x.IsSpecial))
			{
				result.MergeDiscountWith(billingDiscount.Calculate(result.Amount));
			}

			// 5. capped
			ClientLicenceBillingDiscount cappedDiscount = discounts.FirstOrDefault(x => x.L5_Type == BillingConstants.DiscountType.Capped);
			if (cappedDiscount != null)
			{
				result.MergeDiscountWith(cappedDiscount.Calculate(result.TotalAmount));
			}

			// 6. WiseCloud
			// If a WiseCloud discount appears on the pricelist and on the organisation, then the organisation discount is the only one that applies. It suppresses the pricelist one.
			if (discountable.IsHostedOnWiseCloud)
			{
				var wiseCloudDiscount = discounts.Where(x => x.IsWiseCloud).OrderBy(x => x.L5_DiscountCode.IsEmpty ? 0 : 1).FirstOrDefault(x => x.IsWiseCloud);
				if (wiseCloudDiscount != null)
				{
					result.MergeDiscountWith(wiseCloudDiscount.Calculate(result.Amount));
				}
			}

			if (moduleCalculation != null)
			{
				// Module discounts can be reduced by a threshold discount.
				// E.g. if Core had a 100% discount, but there's a commitment minumum, then the amount is raised to the minimum.
				// It won't be completely free.
				ZDecimal amountDelta = result.Amount - amountAfterModuleDiscounts;
				if (amountDelta > 0)
				{
					ZDecimal adjustedModuleDiscount = Math.Max(moduleCalculation.DiscountAmount - amountDelta, 0m);
					moduleCalculation.ModuleDiscountAdjustmentFactor = moduleCalculation.DiscountAmount != 0 ? adjustedModuleDiscount / moduleCalculation.DiscountAmount : 0m;
					var adjustedAmountDiff = moduleCalculation.DiscountAmount - adjustedModuleDiscount;
					moduleCalculation.DiscountAmount = adjustedModuleDiscount;

					ClientLicenceBillingDiscount thresholdDiscount = prepaymentDiscount ?? commitmentDiscount;
					ZString thresholdDescription = thresholdDiscount != null
						? BillingConstants.GetDiscountTypeList().GetDescriptionFromCode(thresholdDiscount.L5_Type)
						: "Minimum spend";

					var newDescription = thresholdDescription + " reduces Module Specific Discount to " + adjustedModuleDiscount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture);
					moduleCalculation.MergeDiscountDetails(newDescription, thresholdDiscount.L5_Type, adjustedAmountDiff, 0);
				}

				moduleCalculation.Amount = result.Amount + moduleCalculation.DiscountAmount;
				moduleCalculation.MergeDiscountWith(result);
				result = moduleCalculation;
			}

			return result;
		}

		static OdplDiscountCalculation CalculateModuleSpecificDiscounts(IList<ClientLicenceBillingDiscount> discounts, IOdplDiscountable discountable)
		{
			OdplDiscountCalculation result = null;
			foreach (ClientLicenceBillingDiscount moduleSpecificDiscount in discounts.Where(x => x.IsModuleSpecific))
			{
				string name;
				ZDecimal amount = discountable.AmountToDiscountForModule(moduleSpecificDiscount.L5_ModuleCode, out name);

				if (amount > 0)
				{
					if (result == null)
					{
						result = new OdplDiscountCalculation();
					}

					var moduleSpecificCalculation = moduleSpecificDiscount.CalculateModuleSpecific(amount, name);
					result.AddModuleDiscountCalculation(moduleSpecificDiscount.L5_ModuleCode, moduleSpecificCalculation);
				}
			}

			return result;
		}

		internal static OdplDiscountCalculation CalculateSurcharge(OdplSystemBill bill)
		{
			ZDecimal amountToAddSurcharge = bill.DiscountCalculation.Amount;
			OdplDiscountCalculation result = new OdplDiscountCalculation() { Amount = amountToAddSurcharge };

			var discounts = GetDiscounts(bill);
			if (discounts != null && discounts.Count > 0)
			{
				foreach (ClientLicenceBillingDiscount surcharge in discounts.Where(x => x.IsSurcharge))
				{
					result.MergeDiscountWith(surcharge.Calculate(result.Amount));
				}
			}

			return result;
		}
	}

	public class OdplDiscountCalculation : DiscountCalculation
	{
		public OdplDiscountCalculation()
			: base()
		{
		}

		public OdplDiscountCalculation(DiscountCalculation discountCalculation)
		{
			Amount = discountCalculation.Amount;
			DiscountAmount = discountCalculation.DiscountAmount;

			foreach (var detail in discountCalculation.Details)
			{
				MergeDiscountDetails(detail);
			}
		}

		public void AddModuleDiscountCalculation(ZString moduleCode, DiscountCalculation moduleDiscountCalculation)
		{
			if (ModuleDiscountCalculations == null)
			{
				ModuleDiscountCalculations = new List<KeyValuePair<ZString, DiscountCalculation>>();
			}
			ModuleDiscountCalculations.Add(new KeyValuePair<ZString, DiscountCalculation>(moduleCode, moduleDiscountCalculation));
			MergeDiscountWith(moduleDiscountCalculation);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<KeyValuePair<ZString, DiscountCalculation>> ModuleDiscountCalculations;
		public decimal ModuleDiscountAdjustmentFactor = 1;
	}
}

