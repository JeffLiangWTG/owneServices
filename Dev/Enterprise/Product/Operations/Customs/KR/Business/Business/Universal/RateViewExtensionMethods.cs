using CargoWise.Types;
using Enterprise.Customs.Universal;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class RateViewExtensionMethods
	{
		public static ZDecimal GetRate(this RateView dutyRate)
		{
			var result = ZDecimal.Zero;

			if (dutyRate != null)
			{
				var rateExpresstions = dutyRate.ZZ2_RateFormula.Split(Constants.ZZ.RateFormulaConstants.Multiply);
				if (rateExpresstions.Length == 2)
				{
					var firstRateExpression = rateExpresstions[0].Replace(" ", "");
					if (dutyRate.RateCode == Constants.ZZ.RateCodes.DutyAdValorem || dutyRate.RateCode == Constants.ZZ.RateCodes.DutyReductionRate)
					{
						var rateString = string.Equals(firstRateExpression, Constants.ZZ.RateFormulaConstants.VFD, System.StringComparison.OrdinalIgnoreCase) ? rateExpresstions[1] : rateExpresstions[0];
						if (ZDecimal.TryParse(rateString, out result))
						{
							result = result * 100;
						}
					}
					else
					{
						var rateString = firstRateExpression.StartsWith(Constants.ZZ.RateFormulaConstants.UQPlaceholderOpeningBracket) ? rateExpresstions[1] : rateExpresstions[0];
						ZDecimal.TryParse(rateString, out result);
					}
				}
			}

			return result;
		}
	}
}
