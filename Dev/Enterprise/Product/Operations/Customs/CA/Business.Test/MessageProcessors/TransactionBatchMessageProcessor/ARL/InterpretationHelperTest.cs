using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InterpretationHelperTest : TestCaseWithFactory
	{
		public void TestFormatAmount()
		{
			AssertEquals("135,987.23", InterpretationHelper.FormatAmount(135987.23));
			AssertEquals("(135,987.23)", InterpretationHelper.FormatAmount(-135987.23));
			AssertEquals("0.00", InterpretationHelper.FormatAmount(0));
			AssertEquals("", InterpretationHelper.FormatAmount(0, true));
		}

		public void TestFormatDate()
		{
			AssertEquals("2014-11-17", InterpretationHelper.FormatDate(new ZDateTime(2014, 11, 17)));
			AssertEquals("", InterpretationHelper.FormatDate(ZDateTime.Empty));
		}
	}
}
