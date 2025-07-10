using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_ENG_SAFTest : TestCase
	{
		public void TestReplacement()
		{
			AssertEquals("twelve only", (new CurrencyToWords_ENG_SAF()).ConvertToWords(12, "INR"));
			AssertEquals("one only", (new CurrencyToWords_ENG_SAF()).ConvertToWords(1, "INR"));
			AssertEquals("one and paise fifty", (new CurrencyToWords_ENG_SAF()).ConvertToWords(1.5, "INR"));
			AssertEquals("zero only", (new CurrencyToWords_ENG_SAF()).ConvertToWords(0, "INR"));
			AssertEquals("thirty two only", (new CurrencyToWords_ENG_SAF()).ConvertToWords(32, "INR"));
			AssertEquals("twelve and paise twenty", (new CurrencyToWords_ENG_SAF()).ConvertToWords(12.2, "INR"));
		}
	}
}
