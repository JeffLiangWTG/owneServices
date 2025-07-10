using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class ZDateTimeConversionHelperTest : TestCase
	{
		public void TestParseRoundTripFormatString()
		{
			string timeString1 = "2012-07-16T16:36:10.3400000+10:00";
			ZDateTime time1 = ZDateTimeConversionHelper.ParseRoundTripFormatString(timeString1);
			AssertEquals(new ZDateTime(2012, 7, 16, 16, 36, 10).AddTicks(3400000), time1);

			string timeString2 = "2012-07-16T17:00:05.0000000Z";
			ZDateTime time2 = ZDateTimeConversionHelper.ParseRoundTripFormatString(timeString2);
			AssertEquals(new ZDateTime(2012, 7, 16, 17, 0, 5), time2);
			AssertEquals(DateTimeKind.Utc, time2.ToDateTime().Kind);
			AssertEquals(time2.ToString("o"), timeString2);
		}

		public void TestForceParseRoundTripFormatStringAsUtc()
		{
			string timeString1 = "2012-07-16T16:36:10.3400000-10:00";
			ZDateTime time1 = ZDateTimeConversionHelper.ForceParseRoundTripFormatStringAsUtc(timeString1);
			AssertEquals(new ZDateTime(2012, 7, 16, 16, 36, 10).AddTicks(3400000), time1);
			AssertEquals(DateTimeKind.Utc, time1.ToDateTime().Kind);

			string timeString2 = "2012-07-16T16:36:10.3400000+01:00";
			ZDateTime time2 = ZDateTimeConversionHelper.ForceParseRoundTripFormatStringAsUtc(timeString2);
			AssertEquals(new ZDateTime(2012, 7, 16, 16, 36, 10).AddTicks(3400000), time2);
			AssertEquals(DateTimeKind.Utc, time2.ToDateTime().Kind);

			string timeString3 = "2012-07-16T17:00:05.0000000Z";
			ZDateTime time3 = ZDateTimeConversionHelper.ForceParseRoundTripFormatStringAsUtc(timeString3);
			AssertEquals(new ZDateTime(2012, 7, 16, 17, 0, 5), time3);
			AssertEquals(DateTimeKind.Utc, time3.ToDateTime().Kind);
		}
	}
}
