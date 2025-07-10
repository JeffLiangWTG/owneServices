using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoDateTimeTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Date = ZDateTime.Empty;

			AssertEquals(true, Dummy.Z0_DateInfo.SetValueFromString("1972-10-25 23:59:58"));
			AssertEquals(new ZDateTime(1972, 10, 25, 23, 59, 58), Dummy.Z0_Date);

			AssertEquals(false, Dummy.Z0_DateInfo.SetValueFromString("123456789"));
			AssertEquals(new ZDateTime(1972, 10, 25, 23, 59, 58), Dummy.Z0_Date);

			AssertEquals(true, Dummy.Z0_DateInfo.SetValueFromString("25/OCT/1972 23:59:59"));
			AssertEquals(new ZDateTime(1972, 10, 25, 23, 59, 59), Dummy.Z0_Date);
		}
	}
}
