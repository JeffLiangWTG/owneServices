using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneBaseTest : TestCase
	{
		/// <summary>
		/// If the DaylightSaving Zone has no START or END rule for any given year,
		/// it should be interpreted as:
		///   - !START and !END => No DST in the whole year
		///   -  START and !END => DST from START rule till the end of the year
		///   - !START and  END => DST from beginning of the year till END rule
		///   -  START and  END => DST as per normal
		/// </summary>
		public void TestWhenDaylightSavingZoneHasNoStartAndOrEndRuleForGivenYear()
		{
			TimeZoneBaseForTesting timeZone = new TimeZoneBaseForTesting();

			// 1997: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(1997, 01, 01, 0, 0, 0), false, "1997: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(1997, 06, 25, 1, 0, 0), false, "1997: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(1997, 12, 31, 23, 59, 59), false, "1997: No DST Rules (end of the year)");

			// 1998: START and !END => DST from START rule till the end of the year
			AssertIsDst(timeZone, new DateTime(1998, 06, 25, 1, 0, 0), false, "1998: No DST END Rule (< DST Start)");
			AssertIsDst(timeZone, new DateTime(1998, 11, 25, 1, 0, 0), true, "1998: No DST END Rule (> DST Start)");
			AssertIsDst(timeZone, new DateTime(1998, 12, 31, 23, 59, 59), true, "1998: No DST END Rule (end of the year)");

			// 1999: START and END => DST as per normal
			AssertIsDst(timeZone, new DateTime(1999, 02, 25, 1, 0, 0), true, "1999: Both START/END Rules (< DST End)");
			AssertIsDst(timeZone, new DateTime(1999, 06, 25, 1, 0, 0), false, "1999: Both START/END Rules (> DST End, < Start)");
			AssertIsDst(timeZone, new DateTime(1999, 11, 25, 1, 0, 0), true, "1999: Both START/END Rules (> DST Start)");

			// 2000: !START and END => DST from beginning of the year till END rule
			AssertIsDst(timeZone, new DateTime(2000, 01, 01, 0, 0, 0), true, "2000: No DST START Rule (start of the year)");
			AssertIsDst(timeZone, new DateTime(2000, 02, 25, 1, 0, 0), true, "2000: No DST START Rule (< DST End)");
			AssertIsDst(timeZone, new DateTime(2000, 06, 25, 1, 0, 0), false, "2000: No DST START Rule (> DST End)");

			// 2001: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(2001, 01, 01, 0, 0, 0), false, "2001: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(2001, 06, 25, 1, 0, 0), false, "2001: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(2001, 12, 31, 23, 59, 59), false, "2001: No DST Rules (end of the year)");
		}

		void AssertIsDst(ITimeZone calculationTimeZone, DateTime testUtc, bool expectedDst, string messagePrefix)
		{
			string assertMessage = string.Format(
				"{0}. Is [UTC {1}] in Daylight Saving?", messagePrefix, EnvProxy.Instance.Time.FormatDateTime(testUtc));
			AssertEquals(assertMessage, expectedDst, calculationTimeZone.IsDaylightSavingBasedOnUtc(testUtc));
		}
	}
}
