
#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_ES_ES : CurrencyConverterWithNumericFraction
	{
		public CurrencyToWords_ES_ES()
			: base(new NumberToString_ES_ES(), "un", " con ", CurrencyLabelOptions.LocalMajorUnits)
		{ }

		protected override string Pluralize(string currency)
		{
			return currency.EndsWith("s", System.StringComparison.OrdinalIgnoreCase) ? currency : currency + "s";
		}
	}
}

#endregion
