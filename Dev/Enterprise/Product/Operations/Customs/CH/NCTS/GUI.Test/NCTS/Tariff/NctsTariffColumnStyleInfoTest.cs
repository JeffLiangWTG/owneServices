using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(NctsTariffColumnStyleInfo))]
class NctsTariffColumnStyleInfoTest : TestCase
{
	public void TestColumnStyleType()
	{
		var  columnStyleInfo = new NctsTariffColumnStyleInfo();
		AssertEquals(typeof(NctsTariffColumnStyle), columnStyleInfo.ColumnStyleType);
	}
}
