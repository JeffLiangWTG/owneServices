
#region SuppressResourceStringsCheckRegion

using System.Collections.Generic;
using Enterprise.DocumentEngineIntegration;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public sealed class CurrencyToWords_EN : IndoEuropeanCurrencyConverter, ICurrencyToWordsConverter
	{
		public CurrencyToWords_EN()
			: base(new NumberToString_EN(), "one", "{0} {1}", "{0} only", "{0} and {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			if (refCurrency != null)
			{
				if (CurrenciesMajorUnitsWithoutPlurality.Contains(refCurrency.Code))
				{
					return currency;
				}

				if (CurrenciesMajorUnitsWithIrregularPlurality.ContainsKey(refCurrency.Code))
				{
					return CurrenciesMajorUnitsWithIrregularPlurality[refCurrency.Code];
				}
			}

			return currency + "s";
		}

		static List<string> CurrenciesMajorUnitsWithoutPlurality => new List<string>
		{
			CurrencyCodes.Bangladesh,
			CurrencyCodes.China,
			CurrencyCodes.Japan,
			CurrencyCodes.KoreaRepublicOf,
			CurrencyCodes.KoreaDemocraticPeoplesRepublic,
			CurrencyCodes.Madagascar
		};

		static Dictionary<string, string> CurrenciesMajorUnitsWithIrregularPlurality => new Dictionary<string, string>
		{
			{ CurrencyCodes.Brazil, "reais" },
			{ CurrencyCodes.Sweden, "kronor" },
			{ CurrencyCodes.Venezuela, "bolívares" }
		};

		string ICurrencyToWordsConverter.Convert(double amount, string currencyCode) => ConvertToWords(amount, currencyCode);
	}
}

#endregion
