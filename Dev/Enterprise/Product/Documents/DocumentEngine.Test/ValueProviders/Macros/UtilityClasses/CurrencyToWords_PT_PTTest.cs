using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_PT_PTTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currencyEUR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var currencyMZN = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "MZN"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currencyEUR.PK, "PT-PT", "RX", "centavos de euro");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyMZN.PK, "PT-PT", "RX", "centavos");
			Factory.Save();
			AssertEquals("doze euros", (new CurrencyToWords_PT_PT()).ConvertToWords(12, "EUR"));
			AssertEquals("um euro", (new CurrencyToWords_PT_PT()).ConvertToWords(1, "EUR"));
			AssertEquals("um euro e 50 centavos de euro", (new CurrencyToWords_PT_PT()).ConvertToWords(1.5, "EUR"));
			AssertEquals("zero euros", (new CurrencyToWords_PT_PT()).ConvertToWords(0, "EUR"));
			AssertEquals("trinta e dois euros", (new CurrencyToWords_PT_PT()).ConvertToWords(32, "EUR"));
			AssertEquals("doze euros e 20 centavos de euro", (new CurrencyToWords_PT_PT()).ConvertToWords(12.2, "EUR"));
			AssertEquals("doze meticais e 20 centavos", (new CurrencyToWords_PT_PT()).ConvertToWords(12.2, "MZN"));
		}
	}
}
