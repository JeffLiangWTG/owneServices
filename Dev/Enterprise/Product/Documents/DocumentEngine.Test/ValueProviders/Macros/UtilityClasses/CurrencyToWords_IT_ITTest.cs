using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_IT_ITTest : TestCase
	{
		public void TestReplacement()
		{
			var itl = new CurrencyToWords_IT_IT();
			AssertEquals("nove euros", itl.ConvertToWords(9, "EUR"));
			AssertEquals("nove euros e 99 euro-cents", itl.ConvertToWords(9.99, "EUR"));
		}
	}
}
