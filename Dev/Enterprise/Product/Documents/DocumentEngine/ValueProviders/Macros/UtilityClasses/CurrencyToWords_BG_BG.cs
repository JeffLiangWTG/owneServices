#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_BG_BG : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_BG_BG()
			: base(new NumberToString_BG_BG(), "едно", "{0} {1}", "{0}", "{0} и {1} {2}")
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
