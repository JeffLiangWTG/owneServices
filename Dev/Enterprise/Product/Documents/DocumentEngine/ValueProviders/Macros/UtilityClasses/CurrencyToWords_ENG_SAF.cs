
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_ENG_SAF : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_ENG_SAF()
			: base(new NumberToString_ENG_SAF(), "one", "{0}", "{0} only", "{0} and {2} {1}")
		{ }

		protected override string MinorUnitsAmountToString(long value)
		{
			return numberToWords.GetNumberAsString(value);
		}

		protected override string Pluralize(string currency)
		{
			return currency;
		}
	}
}

#endregion
