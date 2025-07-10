using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class ValueAndUnitSelfTotallerTest : TestCaseWithFactory
	{
		public void TestValueAndUnitSelfTotallerToString()
		{
			ValueAndUnitSelfTotaller totaller = new ValueAndUnitSelfTotaller(12.546m, "PKG");
			AssertEquals("Test the Format of To String", "12.54 PKG", totaller.ToString("2"));
			totaller.Value = 348.23m;
			AssertEquals("Test the Format of To String with changed value", "348.2 PKG", totaller.ToString("1"));
			totaller.UnitCode = "ZZZ";
			AssertEquals("Test the Format of To String with changed unit code", "348.23 ZZZ", totaller.ToString("2"));
			totaller.EncounteredInvalidCode = true;
			AssertEquals("Test the Format of To String with Invalid code", ZString.Empty, totaller.ToString("2"));
			AssertEquals("Value is zero", ZDecimal.Zero, totaller.Value);
			AssertEquals("UnitCode is Empty", ZString.Empty, totaller.UnitCode);
		}
	}
}
