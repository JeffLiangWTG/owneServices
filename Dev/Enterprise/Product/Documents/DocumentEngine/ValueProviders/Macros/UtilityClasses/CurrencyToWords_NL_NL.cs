
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_NL_NL : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_NL_NL()
			: base(new NumberToString_NL_NL(), "één", "{0} {1}", "{0}", "{0} en {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			return currency;
		}
	}
}

#endregion
