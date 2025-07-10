using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class CurrencyToWords_ES_ESTest : TestCaseWithFactory
	{
		public void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "PEN"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "ES-ES", "RX", "soles");
			Factory.Save();
			var spn = new CurrencyToWords_ES_ES();
			CombineAssertions(delegate
			{
				AssertEquals("doce con 00/100 euros", spn.ConvertToWords(12, "EUR"));
				AssertEquals("un con 00/100 euros", spn.ConvertToWords(1, "EUR"));
				AssertEquals("un con 50/100 euros", spn.ConvertToWords(1.5, "EUR"));
				AssertEquals("cero con 00/100 euros", spn.ConvertToWords(0, "EUR"));
				AssertEquals("treinta y dos con 00/100 euros", spn.ConvertToWords(32, "EUR"));
				AssertEquals("doce con 20/100 euros", spn.ConvertToWords(12.2, "EUR"));

				AssertEquals("ciento once con 51/100 soles", spn.ConvertToWords(111.51, "PEN"));
				AssertEquals("mil cuatrocientos cuarenta y nueve con 00/100 soles", spn.ConvertToWords(1449, "PEN"));
				AssertEquals("mil doscientos sesenta y dos con 65/100 soles", spn.ConvertToWords(1262.65, "PEN"));
				AssertEquals("veintidós mil cuarenta con 00/100 soles", spn.ConvertToWords(22040, "PEN"));
				AssertEquals("noventa y cinco mil cincuenta con 00/100 pesos", spn.ConvertToWords(95050, "MXN"));
				AssertEquals("noventa y siete mil cincuenta yens", spn.ConvertToWords(97050, "JPY"));
				AssertEquals("noventa y siete mil cincuenta con 000/1000 rials", spn.ConvertToWords(97050, "OMR"));
			});
		}
	}
}
