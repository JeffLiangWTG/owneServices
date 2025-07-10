
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_DE_DE : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_DE_DE()
			: base(new NumberToString_DE_DE(), "ein", "{0} {1}", "{0}", "{0} und {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			return currency;
		}
	}
}

#endregion
