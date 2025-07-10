using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyConvertorTest : TestCase
	{
		public void TestReplacement()
		{
			AssertEquals("twelve dollars only", (new CurrencyToWords_EN()).ConvertToWords(12, "USD"));
			AssertEquals("one dollar only", (new CurrencyToWords_EN()).ConvertToWords(1, "USD"));
			AssertEquals("one dollar and 50 cents", (new CurrencyToWords_EN()).ConvertToWords(1.5, "USD"));
			AssertEquals("zero dollars only", (new CurrencyToWords_EN()).ConvertToWords(0, "USD"));
			AssertEquals("thirty two dollars only", (new CurrencyToWords_EN()).ConvertToWords(32, "USD"));
			AssertEquals("twelve dollars and 20 cents", (new CurrencyToWords_EN()).ConvertToWords(12.2, "USD"));
			AssertEquals("twelve dollars and twenty cents", (new CurrencyToWords_EN()).ConvertToWords(12.2, "USD", CurrencyConvertor.SpecialFormat.LTR));
			AssertEquals("twelve dollars and 20 cents", (new CurrencyToWords_EN()).ConvertToWords(12.2, "USD", CurrencyConvertor.SpecialFormat.None));
			AssertEquals("cent vingt-trois ariarys et cinquante-six ", (new CurrencyToWords_FR_FR()).ConvertToWords(123.56, "MGA", CurrencyConvertor.SpecialFormat.LTR));
		}
	}
}
