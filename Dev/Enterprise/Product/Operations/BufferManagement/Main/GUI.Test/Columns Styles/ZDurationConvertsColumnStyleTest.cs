using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class ZDurationConvertsColumnStyleTest : TestCaseWithDummy
	{
		public void TestZDurationConvertsColumnStyle()
		{
			using (var style = new ZDurationConvertsColumnStyleForTest(new ZDurationConvertsColumnStyleInfo()))
			{
				AssertEquals("8:30", style.GetFormatValueObjectForTest(Dummy, new ZDecimal(8.5)));
			}
		}
	}

	public class ZDurationConvertsColumnStyleForTest : ZDurationConvertsColumnStyle
	{
		public ZDurationConvertsColumnStyleForTest(ZDurationConvertsColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}
		public string GetFormatValueObjectForTest(object source, object propertyValue)
		{
			return FormatValueObjectCore(source, propertyValue);
		}
	}
}
