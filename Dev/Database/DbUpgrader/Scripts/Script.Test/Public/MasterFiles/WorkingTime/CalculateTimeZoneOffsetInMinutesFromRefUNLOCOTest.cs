using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	class TimeZoneInMinutesFunctionTests : TransactionedTestCase
	{
		public void TestGetTimeZoneOffset_WhenRuleChangeYearToYear()
		{
			// Use America/Sao_Paulo for easier setting up data, but it's actually Pacific/Fiji time zone
			var sql = @"
DECLARE @r2pk uniqueidentifier;
DECLARE @stdpk uniqueidentifier;
SELECT @r2pk = R3_R2_DaylightSavingZone, @stdpk = R3_R2_StandardZone FROM dbo.RefTimeZoneSet WHERE R3_TimeZoneSetName = 'America/Sao_Paulo';
UPDATE dbo.RefTimeZone SET R2_OffsetMinutesFromUTC = 720 where R2_PK = @stdpk;
UPDATE dbo.RefTimeZone SET R2_OffsetMinutesFromUTC = 780 where R2_PK = @r2pk;
DELETE dbo.RefTimeZoneRule WHERE R4_R2 = @r2pk;
INSERT dbo.RefTimeZOneRule ([R4_PK]
		   ,[R4_FromYear]
		   ,[R4_ToYear]
		   ,[R4_StartOrEndRule]
		   ,[R4_DaylightSavingDayWeekDate]
		   ,[R4_DaylightSavingDate]
		   ,[R4_DaylightSavingDayCount]
		   ,[R4_DaylightSavingDayName]
		   ,[R4_DaylightSavingMonth]
		   ,[R4_TypeOfTime]
		   ,[R4_R2])
VALUES (newid(), 2019, 2019, 'STA', 'MON', '2007-01-01 03:00:00', 2, 'SUN', 'NOV', 'LOC', @r2pk),
(newid(), 2020, 2020, 'STA', 'MON', '2007-01-01 03:00:00', 3, 'SUN', 'DEC', 'LOC', @r2pk),
(newid(), 2021, 0, 'STA', 'MON', '2007-01-01 03:00:00', 2, 'SUN', 'NOV', 'LOC', @r2pk),
(newid(), 2018, 2020, 'END', 'MON', '2007-01-01 02:00:00', 2, 'SUN', 'JAN', 'LOC', @r2pk),
(newid(), 2021, 2023, 'END', 'MON', '2007-01-01 02:00:00', 3, 'SUN', 'JAN', 'LOC', @r2pk)
";
			TestConnection.Command(sql).ExecuteScalar();
			AssertTimeZoneOffset("BRSAO", "2019-01-14", 720, new DateTime(2019, 1, 12, 14, 0, 0), new DateTime(2019, 11, 9, 14, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2019-11-11", 780, new DateTime(2019, 11, 9, 14, 0, 0), new DateTime(2020, 1, 11, 14, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2020-01-13", 720, new DateTime(2020, 1, 11, 14, 0, 0), new DateTime(2020, 12, 19, 14, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2020-12-21", 780, new DateTime(2020, 12, 19, 14, 0, 0), new DateTime(2021, 01, 16, 14, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2021-01-18", 720, new DateTime(2021, 1, 16, 14, 0, 0), new DateTime(2021, 11, 13, 14, 0, 0));
		}

		public void TestGetTimeZoneOffset_WhenStartRuleIsTheSameYearAsNow()
		{
			var sql = @"
delete from RefDatabase_RefUNLOCOUtcOffset;
DECLARE @r2pk uniqueidentifier;
DECLARE @stdpk uniqueidentifier;
DECLARE @saigonR2Pk uniqueidentifier;
SELECT @r2pk = R3_R2_DaylightSavingZone, @stdpk = R3_R2_StandardZone FROM dbo.RefTimeZoneSet WHERE R3_TimeZoneSetName = 'America/Sao_Paulo';
UPDATE dbo.RefTimeZone SET R2_OffsetMinutesFromUTC = -180 where R2_PK = @stdpk;
UPDATE dbo.RefTimeZone SET R2_OffsetMinutesFromUTC = -120 where R2_PK = @r2pk;
SELECT @saigonR2Pk = R3_R2_StandardZone FROM dbo.ReftimeZoneSet WHERE R3_TimeZoneSetName = 'Asia/Saigon';
UPDATE dbo.RefTimeZone SET R2_OffsetMinutesFromUTC = 420 where R2_PK = @saigonR2Pk;
DELETE dbo.RefTimeZoneRule WHERE R4_R2 = @r2pk;
INSERT dbo.RefTimeZOneRule ([R4_PK]
		   ,[R4_FromYear]
		   ,[R4_ToYear]
		   ,[R4_StartOrEndRule]
		   ,[R4_DaylightSavingDayWeekDate]
		   ,[R4_DaylightSavingDate]
		   ,[R4_DaylightSavingDayCount]
		   ,[R4_DaylightSavingDayName]
		   ,[R4_DaylightSavingMonth]
		   ,[R4_TypeOfTime]
		   ,[R4_R2])
VALUES (newid(), 2008, 2017, 'STA', 'MON', '2007-01-01 00:00:00', 3, 'SUN', 'OCT', 'LOC', @r2pk),
(newid(), 2018, 2018, 'STA', 'MON', '2000-01-01 00:00:00', 1, 'SUN', 'NOV', 'LOC', @r2pk),
(newid(), 2001, 2019, 'END', 'MON', '2000-01-01 00:00:00', 3, 'SUN', 'FEB', 'LOC', @r2pk),
(newid(), 2019, 2019, 'STA', 'MON', '2000-01-01 00:00:00', 2, 'SUN', 'NOV', 'LOC', @r2pk)
";
			TestConnection.Command(sql).ExecuteScalar();
			AssertTimeZoneOffset("BRSAO", "2008-10-20", -120, new DateTime(2008, 10, 19, 2, 0, 0), new DateTime(2009, 2, 15, 3, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2009-02-16", -180, new DateTime(2009, 2, 15, 3, 0, 0), new DateTime(2009, 10, 18, 2, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-02-17", -120, new DateTime(2017, 10, 15, 2, 0, 0), new DateTime(2018, 2, 18, 3, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-02-19", -180, new DateTime(2018, 2, 18, 3, 0, 0), new DateTime(2018, 11, 4, 2, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-11-05", -120, new DateTime(2018, 11, 4, 2, 0, 0), new DateTime(2019, 2, 17, 3, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2019-02-18", -180, new DateTime(2019, 2, 17, 3, 0, 0), new DateTime(2019, 11, 10, 2, 0, 0));

			// No day light saving
			AssertTimeZoneOffset("VNCUV", "2019-2-18", 420, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		}

		void AssertTimeZoneOffset(string unloco, string utctime, short expectOffset, DateTime expectStart, DateTime expectEnd)
		{
			var sql = $"SELECT DstStart, DstEnd, Offset FROM CalculateTimeZoneOffsetInMinutesFromRefUNLOCO('{unloco}', '{utctime}', 0);";
			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				if (reader.Read())
				{
					var dstStart = (DateTime)reader[0];
					AssertEquals(expectStart, dstStart);
					var dstEnd = (DateTime)reader[1];
					AssertEquals(expectEnd, dstEnd);
					var offset = (short)reader[2];
					AssertEquals(expectOffset, offset);
				}
			}
		}

		[TestDate(2014, 6, 1)] // Falls outside daylight savings time of year
		public void TestGetTimeZoneOffset()
		{
			var referenceDate = new DateTime(2014, 6, 1);
			AssertTimeZoneOffset("AUSYD", 600, referenceDate: referenceDate);
			AssertTimeZoneOffset("GBLON", 60, referenceDate: referenceDate);
			AssertTimeZoneOffset("DEDUS", 120, referenceDate: referenceDate);
			AssertTimeZoneOffset("FRPAR", 120, referenceDate: referenceDate);
			AssertTimeZoneOffset("ITMIL", 120, referenceDate: referenceDate);
			AssertTimeZoneOffset("NZAKL", 720, referenceDate: referenceDate);
			AssertTimeZoneOffset("USLAX", -420, referenceDate: referenceDate);
		}

		[TestDate(2014, 6, 1)] // Falls outside daylight savings time of year
		public void TestGetTimeZoneOffsetCanReturnPartHourOffset()
		{
			var referenceDate = new DateTime(2014, 6, 1);
			AssertTimeZoneOffset("AUADL", 570, referenceDate: referenceDate);
		}

		[TestDate(2014, 11, 1)] // Falls within daylight savings time of year
		public void TestGetTimeZoneOffset_ForDaylightSavings()
		{
			AssertTimeZoneOffset("AUSYD", 660, referenceDate: new DateTime(2014, 11, 1));
		}

		public void TestGetTimeZoneOffset_UsesStdOffsetWhenDaylightSavingZoneIsNull()
		{
			string unloco = "AUBNE";

			//precondition
			var sql = string.Format(@"
SELECT R3_R2_DaylightSavingZone
FROM dbo.RefUNLOCO
JOIN dbo.RefTimeZoneSet ON RL_R3 = R3_PK
WHERE RL_Code = '{0}'",
				unloco);
			var daylightSavingZone = TestConnection.Command(sql).ExecuteScalar();
			AssertEquals(DBNull.Value, daylightSavingZone);

			AssertTimeZoneOffset(unloco, 600, referenceDate: new DateTime(2014, 2, 1));
			AssertTimeZoneOffset(unloco, 600, referenceDate: new DateTime(2014, 7, 1));
		}

		void AssertTimeZoneOffset(string unloco, short expectedOffset, string expectedMethod = "Calculate", DateTime? referenceDate = null)
		{
			var sql = $"SELECT Offset FROM dbo.{FunctionName}('{unloco}', '{(referenceDate ?? DateTime.Now).ToSqlFormat()}')"; // test only so we don't care if Datetime.Now is accurate or not
			if (FunctionName == "CalculateTimeZoneOffsetInMinutesFromRefUNLOCO")
			{
				sql = $"SELECT Offset FROM dbo.{FunctionName}('{unloco}', '{(referenceDate ?? DateTime.Now).ToSqlFormat()}', 0)"; // test only so we don't care if Datetime.Now is accurate or not
			}
			var offset = TestConnection.Command(sql).ExecuteScalar();

			AssertEquals("Offset for " + unloco, expectedOffset, offset);
		}

		protected virtual string FunctionName => "CalculateTimeZoneOffsetInMinutesFromRefUNLOCO";

		protected override DbConnection TestConnection => Db.Connection;
	}

	[TestedType(typeof(CalculateTimeZoneOffsetInMinutesFromRefUNLOCO))]
	sealed class CalculateTimeZoneOffsetInMinutesFromRefUNLOCOTest : DbCreateScriptTest
	{
	}
}
