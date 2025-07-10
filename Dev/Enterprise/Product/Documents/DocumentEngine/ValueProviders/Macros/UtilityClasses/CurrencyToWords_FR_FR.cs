
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_FR_FR : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_FR_FR()
			: base(new NumberToString_FR_FR(), "un", "{0} {1}", "{0}", "{0} et {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			return currency + "s";
		}
	}
}

#endregion
