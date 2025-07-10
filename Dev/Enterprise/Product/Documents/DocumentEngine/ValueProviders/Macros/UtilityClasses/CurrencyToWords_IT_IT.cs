
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_IT_IT : IndoEuropeanCurrencyConverter
	{
		public CurrencyToWords_IT_IT()
			: base(new NumberToString_IT_IT(), "un", "{0} {1}", "{0}", "{0} e {1} {2}")
		{ }

		protected override string Pluralize(string currency)
		{
			return currency + "s";
		}
	}
}

#endregion
