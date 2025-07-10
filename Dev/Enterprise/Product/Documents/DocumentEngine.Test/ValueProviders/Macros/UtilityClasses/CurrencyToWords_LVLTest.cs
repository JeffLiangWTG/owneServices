using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_LVLTest : TestCase
	{
		public void TestReplacement()
		{
			var lvlCurrencyToWords = new CurrencyToWords_LVL();

			AssertEquals("divpadsmit lat", lvlCurrencyToWords.ConvertToWords(12, "LVL"));
			AssertEquals("viens lat", lvlCurrencyToWords.ConvertToWords(1, "LVL"));
			AssertEquals("viens lat un 50 santims", (new CurrencyToWords_LVL()).ConvertToWords(1.5, "LVL"));
			AssertEquals("nulle lat", (new CurrencyToWords_LVL()).ConvertToWords(0, "LVL"));
			AssertEquals("nulle lat un 1 santims", (new CurrencyToWords_LVL()).ConvertToWords(0.01, "LVL"));
			AssertEquals("nulle lat un 32 santims", (new CurrencyToWords_LVL()).ConvertToWords(0.32, "LVL"));
			AssertEquals("divpadsmit lat un 2 santims", (new CurrencyToWords_LVL()).ConvertToWords(12.02, "LVL"));

			AssertEquals("divpadsmit euro", (new CurrencyToWords_LVL()).ConvertToWords(12, "EUR"));
			AssertEquals("viens euro", (new CurrencyToWords_LVL()).ConvertToWords(1, "EUR"));
			AssertEquals("viens euro un 50 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(1.5, "EUR"));
			AssertEquals("nulle euro", (new CurrencyToWords_LVL()).ConvertToWords(0, "EUR"));
			AssertEquals("nulle euro un 1 euro-cents", (new CurrencyToWords_LVL()).ConvertToWords(0.01, "EUR"));
			AssertEquals("nulle euro un 2 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(0.02, "EUR"));
			AssertEquals("nulle euro un 11 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(0.11, "EUR"));
			AssertEquals("nulle euro un 21 euro-cents", (new CurrencyToWords_LVL()).ConvertToWords(0.21, "EUR"));
			AssertEquals("nulle euro un 22 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(0.22, "EUR"));
			AssertEquals("nulle euro un 32 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(0.32, "EUR"));
			AssertEquals("nulle euro un 91 euro-cents", (new CurrencyToWords_LVL()).ConvertToWords(0.91, "EUR"));
			AssertEquals("divpadsmit euro un 2 euro-centi", (new CurrencyToWords_LVL()).ConvertToWords(12.02, "EUR"));
		}
	}
}
