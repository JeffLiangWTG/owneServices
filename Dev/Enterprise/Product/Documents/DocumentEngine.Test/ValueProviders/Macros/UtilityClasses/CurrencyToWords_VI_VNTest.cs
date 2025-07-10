using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_VI_VNTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "VI-VN", "RX", "đô la mỹ");
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "VI-VN", "RX", "xu");
			Factory.Save();
			var converter = new CurrencyToWords_VI_VN();
			AssertEquals("ba mươi lăm đô la mỹ", converter.ConvertToWords(35, "USD"));
			AssertEquals("một trăm hai mươi ba đô la mỹ bốn mươi lăm xu", converter.ConvertToWords(123.45, "USD"));
			AssertEquals("Should not throw for unknown currencies", "ba mươi lăm XXX", converter.ConvertToWords(35, "XXX"));
		}
	}
}
