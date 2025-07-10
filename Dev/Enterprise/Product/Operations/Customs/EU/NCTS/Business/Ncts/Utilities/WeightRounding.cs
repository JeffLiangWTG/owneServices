using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class WeightRounding
	{
		public static decimal Round(bool isInPhase5TransitionPeriod, ZDecimal weight)
		{
			return isInPhase5TransitionPeriod ? RoundToElevenAndThree(weight) : decimal.Round(weight, 6, MidpointRounding.AwayFromZero);
		}

		static decimal RoundToElevenAndThree(ZDecimal weight)
		{
			int decimalPlaces;
			if (weight >= 10_000_000_000m)
			{
				decimalPlaces = 0;
			}
			else if (weight < 100_000_000m)
			{
				decimalPlaces = 3;
			}
			else
			{
				var integerPartLength = (int)Math.Log10((long)weight) + 1;
				decimalPlaces = 11 - integerPartLength;
			}

			return decimal.Round(weight, decimalPlaces, MidpointRounding.AwayFromZero);
		}
	}
}
