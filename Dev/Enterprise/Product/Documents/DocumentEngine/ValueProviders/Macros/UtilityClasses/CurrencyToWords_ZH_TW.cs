namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities
{
	public class CurrencyToWords_ZH_TW : ChineseCurrencyToWords
	{
		public CurrencyToWords_ZH_TW()
			: base("ZH-TW", new NumberToString_ZH_TW())
		{ }
	}
}
