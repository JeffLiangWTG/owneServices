using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DecimalHelperTest : TestCaseWithFactory
	{
		public void TestGetESStringDecimalFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Values is parsed correctly", "9", DecimalHelper.GetESStringDecimalFormat((ZDecimal)9.000));
				AssertEquals("Values is parsed correctly", "3,5", DecimalHelper.GetESStringDecimalFormat((ZDecimal)3.5));
				AssertEquals("Values is parsed correctly", "260.200,96", DecimalHelper.GetESStringDecimalFormat((ZDecimal)260200.96));
				AssertEquals("Values is parsed correctly", "1,23", DecimalHelper.GetESStringDecimalFormat((ZDecimal)1.2300));
			});
		}

		public void TestDecimalParseToESFormat()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Values is parsed incorrectly when it has a dot because it is expecting a comma", (ZDecimal)123, DecimalHelper.DecimalParseToESFormat("12.3"));
				AssertEquals("Values is parsed correctly when it has a comma", (ZDecimal)12.3, DecimalHelper.DecimalParseToESFormat("12,3"));
			});
		}
	}
}
