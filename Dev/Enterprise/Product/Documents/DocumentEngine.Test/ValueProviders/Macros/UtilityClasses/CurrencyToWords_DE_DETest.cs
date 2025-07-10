using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_DE_DETest : TestCase
	{
		public void TestReplacement()
		{
			AssertEquals("zwölf euro", (new CurrencyToWords_DE_DE()).ConvertToWords(12, "EUR"));
			AssertEquals("ein euro", (new CurrencyToWords_DE_DE()).ConvertToWords(1, "EUR"));
			AssertEquals("ein euro und 50 euro-cents", (new CurrencyToWords_DE_DE()).ConvertToWords(1.5, "EUR"));
			AssertEquals("null euro", (new CurrencyToWords_DE_DE()).ConvertToWords(0, "EUR"));
			AssertEquals("zweiunddreißig euro", (new CurrencyToWords_DE_DE()).ConvertToWords(32, "EUR"));
			AssertEquals("zwölf euro und 20 euro-cents", (new CurrencyToWords_DE_DE()).ConvertToWords(12.2, "EUR"));
		}
	}
}
