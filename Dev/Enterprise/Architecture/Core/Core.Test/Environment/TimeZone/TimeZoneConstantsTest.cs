using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneConstantsTest : TestCase
	{
		public void TestGetMonthAsInt()
		{
			int result = TimeZoneConstants.GetMonthAsInt("");
			AssertEquals("Blank input should return -1", -1, result);

			result = TimeZoneConstants.GetMonthAsInt("JAN");
			AssertEquals("JAN should correspond to 1 in int form", 1, result);

			result = TimeZoneConstants.GetMonthAsInt("FEB");
			AssertEquals("FEB should correspond to 2 in int form", 2, result);

			result = TimeZoneConstants.GetMonthAsInt("MAR");
			AssertEquals("MAR should correspond to 3 in int form", 3, result);

			result = TimeZoneConstants.GetMonthAsInt("APR");
			AssertEquals("APR should correspond to 4 in int form", 4, result);

			result = TimeZoneConstants.GetMonthAsInt("MAY");
			AssertEquals("MAY should correspond to 5 in int form", 5, result);

			result = TimeZoneConstants.GetMonthAsInt("JUN");
			AssertEquals("JUN should correspond to 6 in int form", 6, result);

			result = TimeZoneConstants.GetMonthAsInt("JUL");
			AssertEquals("JUL should correspond to 7 in int form", 7, result);

			result = TimeZoneConstants.GetMonthAsInt("AUG");
			AssertEquals("AUG should correspond to 8 in int form", 8, result);

			result = TimeZoneConstants.GetMonthAsInt("SEP");
			AssertEquals("SEP should correspond to 9 in int form", 9, result);

			result = TimeZoneConstants.GetMonthAsInt("OCT");
			AssertEquals("OCT should correspond to 10 in int form", 10, result);

			result = TimeZoneConstants.GetMonthAsInt("NOV");
			AssertEquals("NOV should correspond to 11 in int form", 11, result);

			result = TimeZoneConstants.GetMonthAsInt("DEC");
			AssertEquals("DEC should correspond to 12 in int form", 12, result);

			result = TimeZoneConstants.GetMonthAsInt("xxx");
			AssertEquals("Invalid input should return -1", -1, result);
		}
	}
}
