using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	abstract public class ChineseCurrencyToWords : CurrencyConvertor
	{
		internal ChineseCurrencyToWords(string language, INumberToWords numberToWords)
		{
			this.language = language;
			this.NumberToWords = numberToWords;
		}

		public override string ConvertToWords(double amount, string currencyCode, SpecialFormat specialFormat = SpecialFormat.None)
		{
			var currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			var result = "";
			var currencyDesc = "";
			var ratio = 0;

			if (currency == null)
			{
				currencyDesc = currencyCode;
			}
			else
			{
				currencyDesc = currency.RX_DescMultilingual.ToString(language);
				ratio = currency.RX_SubUnitRatio;
			}

			var majorUnits = MajorUnitsInAmount(amount);
			var minorUnits = MinorUnitsInAmount(Math.Abs(amount), ratio);
			if (amount < 0)
			{
				majorUnits = 0 - MajorUnitsInAmount(Math.Abs(amount));
			}

			bool needsAnOnlySignAtTheEnd = ((minorUnits - (minorUnits / 10 * 10)) == 0);

			result = currencyDesc + NumberToWords.GetNumberAsString(majorUnits) + majorCurrencyName;

			if (ratio > 10)
			{
				if (minorUnits / 10 != 0)
				{
					result += NumberToWords.GetNumberAsString(minorUnits / 10) + tenthsCurrencyName;
				}

				if (!needsAnOnlySignAtTheEnd)
				{
					result += NumberToWords.GetNumberAsString(minorUnits - (minorUnits / 10 * 10)) + minorCurrencyName;
				}
			}
			else
			{
				if (!needsAnOnlySignAtTheEnd)
				{
					result += NumberToWords.GetNumberAsString(minorUnits) + minorCurrencyName;
				}
			}

			if (needsAnOnlySignAtTheEnd)
			{
				result += onlySign;
			}

			return result;
		}

		readonly INumberToWords NumberToWords;
		readonly string language;

		#region SuppressResourceStringsCheckRegion

		public const string onlySign = "\u6574";
		const string majorCurrencyName = "元";
		const string tenthsCurrencyName = "角";
		const string minorCurrencyName = "分";

		#endregion
	}

	public class CurrencyToWords_ZH_CN : ChineseCurrencyToWords
	{
		public CurrencyToWords_ZH_CN()
			: base("ZH-CN", new NumberToString_ZH_CN())
		{ }
	}
}
