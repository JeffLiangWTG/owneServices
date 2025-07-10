using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class CusStatementHeaderExtensions
	{
		public static ZDecimal CalculateInterestOnAdditionalDutyOrTax(this CusStatementHeader header, ZString taxOrFeeType, ZDecimal additionalDutyOrTaxAmount, ZDateTime assessmentDate)
		{
			var result = ZDecimal.Zero;

			if (header?.B2_DueDate.IsValid ?? false)
			{
				var penaltyDueDate = header.B2_DueDate.AddDays(1);
				var taxRates = new RefCusTaxOrFee.Loader(header.Factory).LoadTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, taxOrFeeType, assessmentDate, penaltyDueDate).OrderBy(x => x.ZZF_StartDate);
				result = taxRates.Aggregate(0m, (acc, rate) =>
				{
					var numberOfDays = ZInt.Zero;
					if (taxRates.Count() == 1)
					{
						numberOfDays = (assessmentDate - penaltyDueDate).Days + 1;
					}
					else
					{
						if (rate.ZZF_EndDate < assessmentDate)
						{
							numberOfDays = (rate.ZZF_EndDate - penaltyDueDate).Days + 1;
							penaltyDueDate = rate.ZZF_EndDate.AddDays(1);
						}
						else
						{
							numberOfDays = (assessmentDate - rate.ZZF_StartDate).Days + 1;
						}
					}

					var dailyRate = annumRates.Contains(taxOrFeeType) ? (ZDecimal)(rate.ZZF_Value / Constants.ZZ.NumberOfDaysInYearForPenaltyCalculation) : rate.ZZF_Value;
					return acc + Math.Truncate(Math.Truncate(additionalDutyOrTaxAmount * numberOfDays * dailyRate) / 10) * 10;
				});
			}
			return result;
		}
		readonly static List<ZString> annumRates = new List<ZString> { Constants.ZZ.RefCusTaxOrFeeCodes.LatePaymentInterests.AnnualRateWithinSixMonths };

		public static ZBool IsWithin6MonthsSinceDueDate(this CusStatementHeader header) => header.B2_DueDate.IsValid ? header.B2_DueDate.AddMonths(6) > ZDateTime.Today : ZBool.False;

		public static ZDecimal CalculatePenaltyOnAdditionalDutyOrTax(this CusStatementHeader header, ZString taxOrFeeType, ZDecimal additionalDutyOrTaxAmount, ZDateTime assessmentDate, IEnumerable<KeyValuePair<int, string>> monthsAndReductionRateCodes = null)
		{
			var result = ZDecimal.Zero;
			if (header?.B2_DueDate.IsValid ?? false)
			{
				var taxOrFee = new RefCusTaxOrFee.Loader(header.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, taxOrFeeType, assessmentDate);
				if (taxOrFee != null)
				{
					var rate = taxOrFee.ZZF_Value;
					if (monthsAndReductionRateCodes != null && monthsAndReductionRateCodes.Any())
					{
						rate = rate * (1 - GetReductionRate(header, assessmentDate, monthsAndReductionRateCodes));
					}
					result = Math.Truncate(additionalDutyOrTaxAmount * rate / 10) * 10;
				}
			}
			return result;
		}

		// Follow-up WI00622308: Additional Duty Reduction Rate
		static ZDecimal GetReductionRate(CusStatementHeader header, ZDateTime assessmentDate, IEnumerable<KeyValuePair<int, string>> monthsAndRateCodes)
		{
			var result = ZDecimal.Zero;
			var taxOrFeeType = ZString.Empty;
			var dueDate = header.B2_DueDate.AddDays(1);

			foreach (var ratePair in monthsAndRateCodes)
			{
				if (assessmentDate < dueDate.AddMonths(ratePair.Key))
				{
					taxOrFeeType = ratePair.Value;
					break;
				}
			}

			var taxOrFee = new RefCusTaxOrFee.Loader(header.Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.KoreaSouth, taxOrFeeType, assessmentDate);
			if (taxOrFee != null)
			{
				result = taxOrFee.ZZF_Value;
			}
			return result;
		}

		public static ZBool Is6MonthsAfterDueDate(this CusStatementHeader header) => header.B2_DueDate.IsValid ? header.B2_DueDate.AddMonths(6) < ZDateTime.Today : ZBool.False;
	}
}
