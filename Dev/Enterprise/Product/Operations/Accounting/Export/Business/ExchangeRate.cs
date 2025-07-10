using System;

namespace Enterprise.Accounting.Export.Business
{
	public static class ExchangeRate
	{
		internal static int Decimals(int subUnitRatio)
			=> (subUnitRatio <= 1) ? 0 : (int)Math.Log10(subUnitRatio);

		public static decimal LocalToForeign(decimal localAmount, decimal exchangeRate, int subUnitRatio, bool isReciprocal)
		{
			return ConvertedAmount(localAmount, exchangeRate, isReciprocal, Decimals(subUnitRatio), true);
		}

		static decimal ConvertedAmount(decimal amount, decimal exchangeRate, bool divide, int decimals, bool shouldRound)
		{
			decimal result = 0;

			if (exchangeRate != 0)
			{
				result = divide ? amount / exchangeRate : amount * exchangeRate;
				result = shouldRound ? Round(result, decimals) : result;
			}

			return result;
		}

		static decimal Round(decimal amount, int roundingScale)
		{
			int originalMultipler = amount > 0 ? 1 : -1;
			decimal bankersRound = Math.Round(Math.Abs(amount), roundingScale);

			if (bankersRound - Math.Abs(amount) == -((decimal)Math.Pow(10, -roundingScale) / 2))
			{
				bankersRound += (decimal)Math.Pow(10, -roundingScale);
			}

			return bankersRound * originalMultipler;
		}
	}
}
