using System;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoOffsetDateTimeTest : TestCaseWithDummy
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetValueFromString()
		{
			Dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;

			AssertEquals(true, Dummy.Z0_DateTimeOffsetInfo.SetValueFromString("1972-10-25 23:59:58 +01:00"));
			AssertEquals(new ZDateTimeOffset(1972, 10, 25, 23, 59, 58, TimeSpan.FromHours(1)), Dummy.Z0_DateTimeOffset);

			AssertEquals(false, Dummy.Z0_DateTimeOffsetInfo.SetValueFromString("123456789"));
			AssertEquals(new ZDateTimeOffset(1972, 10, 25, 23, 59, 58, TimeSpan.FromHours(1)), Dummy.Z0_DateTimeOffset);

			AssertEquals(true, Dummy.Z0_DateTimeOffsetInfo.SetValueFromString("25/OCT/1972 23:59:59 -01:00"));
			AssertEquals(new ZDateTimeOffset(1972, 10, 25, 23, 59, 59, TimeSpan.FromHours(-1)), Dummy.Z0_DateTimeOffset);

			Dummy.Z0_DateTimeOffset = ZDateTimeOffset.Empty;

			AssertEquals(true, Dummy.Z0_DateTimeOffsetInfo.SetValueFromString("25/OCT/1972 23:59:59"));
			AssertEquals(new ZDateTimeOffset(1972, 10, 25, 23, 59, 59, TimeSpan.FromHours(10)), Dummy.Z0_DateTimeOffset);

			Dummy.Z0_DateTimeOffset = new ZDateTimeOffset(1972, 10, 25, 23, 59, 59, TimeSpan.FromHours(-1));

			AssertEquals(true, Dummy.Z0_DateTimeOffsetInfo.SetValueFromString("25/OCT/1972 23:59:59"));
			AssertEquals(new ZDateTimeOffset(1972, 10, 25, 23, 59, 59, TimeSpan.FromHours(-1)), Dummy.Z0_DateTimeOffset);
		}
	}
}
