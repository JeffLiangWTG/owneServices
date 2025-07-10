using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWord_BG_BGTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "BGL"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "BG-BG", "RX", "лева");
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "BG-BG", "RX", "стотинки");
			Factory.Save();
			var blg = new CurrencyToWords_BG_BG();
			CombineAssertions(delegate
			{
				AssertEquals("седем лева и петдесет и две стотинки", blg.ConvertToWords(7.52, "BGL"));
				AssertEquals("хиляда седемстотин и пет лева", blg.ConvertToWords(1705.00, "BGL"));
			});
		}
	}
}
