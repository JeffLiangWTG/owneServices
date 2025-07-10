using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_FR_FRTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "FR-FR", "RX", "centimes");
			Factory.Save();
			var frn = new CurrencyToWords_FR_FR();
			AssertEquals("cinq cent quarante-trois euros et 30 centimes", frn.ConvertToWords(543.30, "EUR"));
			AssertEquals("mille sept cent quatre-vingt-douze euros", frn.ConvertToWords(1792, "EUR"));
		}
	}
}
