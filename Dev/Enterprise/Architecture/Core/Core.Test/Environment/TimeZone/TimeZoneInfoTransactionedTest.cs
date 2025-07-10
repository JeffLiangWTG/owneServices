using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneInfoTransactionedTest : TransactionedTestCase
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
			Guid dstZonePk = Guid.NewGuid();
			string sqlText = string.Format(@"
				DECLARE @ZonePk uniqueidentifier
				SET @ZonePk = '{0}'
				INSERT dbo.RefTimeZone (R2_PK) VALUES (@ZonePk)
				INSERT dbo.RefTimeZoneRule (R4_PK, R4_FromYear, R4_ToYear, R4_StartOrEndRule, R4_DaylightSavingDayWeekDate, R4_DaylightSavingDate, R4_TypeOfTime, R4_R2) 
					VALUES (NEWID(), 2002, 2003, '{1}', '{2}', '1900-09-20 00:00:00', '{3}', @ZonePk)
				INSERT dbo.RefTimeZoneRule (R4_PK, R4_FromYear, R4_ToYear, R4_StartOrEndRule, R4_DaylightSavingDayWeekDate, R4_DaylightSavingDate, R4_TypeOfTime, R4_R2) 
					VALUES (NEWID(), 2003, 2004, '{4}', '{2}', '1900-04-10 00:00:00', '{3}', @ZonePk)
				",
				dstZonePk.ToString(),
				TimeZoneConstants.DstTransitionTypeStart,
				TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc,
				TimeZoneConstants.DstTransitionTypeEnd);
			Db.Connection.ExecuteNonQuery(sqlText);

			TimeZoneInfoForTesting timeZone = new TimeZoneInfoForTesting("", 0m, 0m, dstZonePk);

			// 2001: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(2001, 01, 01, 0, 0, 0), false, "2001: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(2001, 06, 25, 1, 0, 0), false, "2001: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(2001, 12, 31, 23, 59, 59), false, "2001: No DST Rules (end of the year)");

			// 2002: START and !END => DST from START rule till the end of the year
			AssertIsDst(timeZone, new DateTime(2002, 06, 25, 1, 0, 0), false, "2002: No DST END Rule (< DST Start)");
			AssertIsDst(timeZone, new DateTime(2002, 11, 25, 1, 0, 0), true, "2002: No DST END Rule (> DST Start)");
			AssertIsDst(timeZone, new DateTime(2002, 12, 31, 23, 59, 59), true, "2002: No DST END Rule (end of the year)");

			// 2003: START and END => DST as per normal
			AssertIsDst(timeZone, new DateTime(2003, 02, 25, 1, 0, 0), true, "2003: Both START/END Rules (< DST End)");
			AssertIsDst(timeZone, new DateTime(2003, 06, 25, 1, 0, 0), false, "2003: Both START/END Rules (> DST End, < Start)");
			AssertIsDst(timeZone, new DateTime(2003, 11, 25, 1, 0, 0), true, "2003: Both START/END Rules (> DST Start)");

			// 2004: !START and END => DST from beginning of the year till END rule
			AssertIsDst(timeZone, new DateTime(2004, 01, 01, 0, 0, 0), true, "2004: No DST START Rule (start of the year)");
			AssertIsDst(timeZone, new DateTime(2004, 02, 25, 1, 0, 0), true, "2004: No DST START Rule (< DST End)");
			AssertIsDst(timeZone, new DateTime(2004, 06, 25, 1, 0, 0), false, "2004: No DST START Rule (> DST End)");

			// 2005: !START and !END => No DST in the whole year
			AssertIsDst(timeZone, new DateTime(2005, 01, 01, 0, 0, 0), false, "2005: No DST Rules (start of the year)");
			AssertIsDst(timeZone, new DateTime(2005, 06, 25, 1, 0, 0), false, "2005: No DST Rules (any date-time)");
			AssertIsDst(timeZone, new DateTime(2005, 12, 31, 23, 59, 59), false, "2005: No DST Rules (end of the year)");
		}

		void AssertIsDst(ITimeZone calculationTimeZone, DateTime testUtc, bool expectedDst, string messagePrefix)
		{
			string assertMessage = string.Format(
				"{0}. Is [UTC {1}] in Daylight Saving?", messagePrefix, EnvProxy.Instance.Time.FormatDateTime(testUtc));
			AssertEquals(assertMessage, expectedDst, calculationTimeZone.IsDaylightSavingBasedOnUtc(testUtc));
		}
	}
}
