using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_TR_TRTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currencyEUR = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var currencyTRY = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currencyEUR.PK, "TR-TR", "RX", "euro sent");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyTRY.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			var trk = new CurrencyToWords_TR_TR();
			CombineAssertions(delegate
			{
				AssertEquals("yalnizonikieuro", trk.ConvertToWords(12, "EUR"));
				AssertEquals("bireuroellieuro sent", trk.ConvertToWords(1.5, "EUR"));

				AssertEquals("yediliraelliikikuruş", trk.ConvertToWords(7.52, "TRY"));
				AssertEquals("yalnizbinyediyüzbeşlira", trk.ConvertToWords(1705.00, "TRY"));
			});
		}
	}
}
