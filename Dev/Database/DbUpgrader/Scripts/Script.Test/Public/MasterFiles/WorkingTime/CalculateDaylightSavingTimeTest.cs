using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	class CalculateDaylightSavingTimeFuncTest : TransactionedTestCase
	{
		public void TestGetTimeZoneOffset_WhenStartRuleIsTheSameYearAsNow()
		{
			AssertCalculateDaylightSaving(2018, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "LOC", "STA", -180, -120, new DateTime(2018, 11, 17, 22, 0, 0));
			AssertCalculateDaylightSaving(2018, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "LOC", "END", -180, -120, new DateTime(2018, 11, 17, 21, 0, 0));
			AssertCalculateDaylightSaving(2018, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "STD", "STA", -180, -120, new DateTime(2018, 11, 17, 21, 0, 0));
			AssertCalculateDaylightSaving(2018, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "UTC", "STA", -180, -120, new DateTime(2018, 11, 18, 0, 0, 0));
			AssertCalculateDaylightSaving(2017, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "UTC", "STA", -180, -120, new DateTime(2018, 11, 18, 0, 0, 0));
			AssertCalculateDaylightSaving(2020, 2018, 2018, "MON", new DateTime(2000, 01, 01), "SUN", "NOV", 3, "UTC", "STA", -180, -120, new DateTime(2018, 11, 18, 0, 0, 0));
			AssertCalculateDaylightSaving(2018, 2018, 2018, "DAT", new DateTime(2000, 10, 04), "SUN", "NOV", 3, "UTC", "STA", -180, -120, new DateTime(2018, 10, 04, 0, 0, 0));
			AssertCalculateDaylightSaving(2019, 2018, 2018, "DAT", new DateTime(2000, 10, 04), "SUN", "NOV", 3, "UTC", "STA", -180, -120, new DateTime(2018, 10, 04, 0, 0, 0));
		}

		void AssertCalculateDaylightSaving(int year, int toYear, int fromYear, string daylightSavingDayWeekDate,
			DateTime daylightSavingDate, string daylightSavingDayName, string daylightSavingMonth, int daylightSavingDayCount,
			string typeOfTime, string startOrEndRule, short stdOffSet, short dstOffset, DateTime expected)
		{
			var sql = @"SELECT value FROM dbo.CalculateDaylightSavingTime(@year, @ToYear, @FromYear, @DaylightSavingDayWeekDate,
@DaylightSavingDate, @DaylightSavingDayName,@DaylightSavingMonth,@DaylightSavingDayCount,
@TypeOfTime,@StartOrEndRule,@StdOffset,@DstOffset)";
			var result = TestConnection.ExecuteScalar(sql, x =>
				{
					x.AddParameter("@year", SqlDbType.Int, year);
					x.AddParameter("@ToYear", SqlDbType.Int, toYear);
					x.AddParameter("@FromYear", SqlDbType.Int, fromYear);
					x.AddParameter("@DaylightSavingDayWeekDate", SqlDbType.VarChar, daylightSavingDayWeekDate);
					x.AddParameter("@DaylightSavingDate", SqlDbType.SmallDateTime, daylightSavingDate);
					x.AddParameter("@DaylightSavingDayName", SqlDbType.VarChar, daylightSavingDayName);
					x.AddParameter("@DaylightSavingMonth", SqlDbType.VarChar, daylightSavingMonth);
					x.AddParameter("@DaylightSavingDayCount", SqlDbType.Int, daylightSavingDayCount);
					x.AddParameter("@TypeOfTime", SqlDbType.VarChar, typeOfTime);
					x.AddParameter("@StartOrEndRule", SqlDbType.VarChar, startOrEndRule);
					x.AddParameter("@StdOffset", SqlDbType.SmallInt, stdOffSet);
					x.AddParameter("@DstOffset", SqlDbType.SmallInt, dstOffset);
				});
			AssertEquals(expected, result);
		}

		protected virtual string FunctionName => "CalculateDaylightSavingTime";

		protected override DbConnection TestConnection => Db.Connection;
	}

	[TestedType(typeof(CalculateDaylightSavingTime))]
	sealed class CalculateDaylightSavingTimeTest : DbCreateScriptTest
	{
	}
}

