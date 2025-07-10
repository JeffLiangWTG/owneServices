using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_NL_NLTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "NL-NL", "RX", "euro");
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "NL-NL", "RX", "euro-cent");
			Factory.Save();
			var dch = new CurrencyToWords_NL_NL();
			AssertEquals("negen euro", dch.ConvertToWords(9, "EUR"));
			AssertEquals("negen euro en 99 euro-cent", dch.ConvertToWords(9.99, "EUR"));
		}
	}
}
