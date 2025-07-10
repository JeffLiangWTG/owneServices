using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	class TimeZoneFunctionUseCacheTests : TransactionedTestCase
	{
		public void TestGetTimeZoneOffset_WhenRuleChangeYearToYear()
		{
			// Use America/Sao_Paulo for easier setting up data, but it's actually Pacific/Fiji time zone
			var sql = @"
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2019-01-13 15:00:00','2019-11-10 15:00:00',720);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2019-11-10 15:00:00','2020-01-12 15:00:00',780);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2020-01-12 15:00:00','2020-12-20 15:00:00',720);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2020-12-20 15:00:00','2021-01-17 15:00:00',780);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2021-01-17 15:00:00','2021-11-14 15:00:00',720);
";
			TestConnection.ExecuteScalar(sql);
			AssertTimeZoneOffset("BRSAO", "2019-01-14", 12, new DateTime(2019, 1, 13, 15, 0, 0), new DateTime(2019, 11, 10, 15, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2019-11-11", 13, new DateTime(2019, 11, 10, 15, 0, 0), new DateTime(2020, 1, 12, 15, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2020-01-13", 12, new DateTime(2020, 1, 12, 15, 0, 0), new DateTime(2020, 12, 20, 15, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2020-12-21", 13, new DateTime(2020, 12, 20, 15, 0, 0), new DateTime(2021, 01, 17, 15, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2021-01-18", 12, new DateTime(2021, 1, 17, 15, 0, 0), new DateTime(2021, 11, 14, 15, 0, 0));
		}

		public void TestGetTimeZoneOffset_WhenStartRuleIsTheSameYearAsNow()
		{
			var sql = @"
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2008-10-18 21:00:00','2009-02-14 22:00:00',-120);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2009-02-14 22:00:00','2009-10-17 21:00:00',-180);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2017-10-14 21:00:00','2018-02-17 22:00:00',-120);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2018-02-17 22:00:00','2018-11-03 21:00:00',-180);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2018-11-03 21:00:00','2019-02-16 22:00:00',-120);
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'BRSAO','2019-02-16 22:00:00','2019-11-09 21:00:00',-180);

INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'VNCUV','1900-01-01 00:00:00','2079-06-06 00:00:00',420);
";
			TestConnection.ExecuteScalar(sql);
			AssertTimeZoneOffset("BRSAO", "2008-10-20", -2, new DateTime(2008, 10, 18, 21, 0, 0), new DateTime(2009, 2, 14, 22, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2009-02-16", -3, new DateTime(2009, 2, 14, 22, 0, 0), new DateTime(2009, 10, 17, 21, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-02-17", -2, new DateTime(2017, 10, 14, 21, 0, 0), new DateTime(2018, 2, 17, 22, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-02-19", -3, new DateTime(2018, 2, 17, 22, 0, 0), new DateTime(2018, 11, 3, 21, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2018-11-5", -2, new DateTime(2018, 11, 3, 21, 0, 0), new DateTime(2019, 2, 16, 22, 0, 0));
			AssertTimeZoneOffset("BRSAO", "2019-2-18", -3, new DateTime(2019, 2, 16, 22, 0, 0), new DateTime(2019, 11, 9, 21, 0, 0));

			// No day light saving
			AssertTimeZoneOffset("VNCUV", "2019-2-18", 7, new DateTime(1900, 1, 1), new DateTime(2079, 6, 6));
		}

		void AssertTimeZoneOffset(string unloco, string utctime, decimal expectOffset, DateTime expectStart, DateTime expectEnd)
		{
			var sql = $"SELECT DstStart, DstEnd, Offset FROM CalculateTimeZoneOffsetFromRefUNLOCO('{unloco}', '{utctime}', 1);";
			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				if (reader.Read())
				{
					var dstStart = (DateTime)reader[0];
					AssertEquals(expectStart, dstStart);
					var dstEnd = (DateTime)reader[1];
					AssertEquals(expectEnd, dstEnd);
					var offset = (decimal)reader[2];
					AssertEquals(expectOffset, offset);
				}
			}
		}

		[TestDate(2014, 6, 1)] // Falls outside daylight savings time of year
		public void TestGetTimeZoneOffset()
		{
			var sql = @"
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'AUSYD','2014-04-01','2015-01-01',600);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'GBLON','2014-04-01','2015-01-01',60);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'DEDUS','2014-04-01','2015-01-01',120);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'FRPAR','2014-04-01','2015-01-01',120);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'ITMIL','2014-04-01','2015-01-01',120);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'NZAKL','2014-04-01','2015-01-01',720);
			INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'USLAX','2014-04-01','2015-01-01',-420);
			";
			TestConnection.Command(sql).ExecuteScalar();
			var referenceDate = new DateTime(2014, 6, 1);
			AssertTimeZoneOffset("AUSYD", 10, referenceDate: referenceDate);
			AssertTimeZoneOffset("GBLON", 1, referenceDate: referenceDate);
			AssertTimeZoneOffset("DEDUS", 2, referenceDate: referenceDate);
			AssertTimeZoneOffset("FRPAR", 2, referenceDate: referenceDate);
			AssertTimeZoneOffset("ITMIL", 2, referenceDate: referenceDate);
			AssertTimeZoneOffset("NZAKL", 12, referenceDate: referenceDate);
			AssertTimeZoneOffset("USLAX", -7, referenceDate: referenceDate);
		}

		[TestDate(2014, 6, 1)] // Falls outside daylight savings time of year
		public void TestGetTimeZoneOffsetCanReturnPartHourOffset()
		{
			var sql = @"
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'AUADL','2014-01-01','2015-01-01',570);
";
			TestConnection.Command(sql).ExecuteScalar();
			var referenceDate = new DateTime(2014, 6, 1);
			AssertTimeZoneOffset("AUADL", 9.5m, referenceDate: referenceDate);
		}

		[TestDate(2014, 11, 1)] // Falls within daylight savings time of year
		public void TestGetTimeZoneOffset_ForDaylightSavings()
		{
			var sql = @"
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'AUSYD','2014-02-01','2015-01-01',660);
";
			TestConnection.Command(sql).ExecuteScalar();
			AssertTimeZoneOffset("AUSYD", 11, referenceDate: new DateTime(2014, 11, 1));
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
			var daylightSavingZone = TestConnection.ExecuteScalar(sql);
			AssertEquals(DBNull.Value, daylightSavingZone);

			sql = @"
INSERT INTO [dbo].[RefDatabase_RefUNLOCOUtcOffset] ([RLO_PK],[RLO_RL_NKCode],[RLO_StartTimeUtc],[RLO_EndTimeUtc],[RLO_OffsetMinutesFromUtc]) VALUES (newid(),'AUBNE','2014-02-01','2014-07-02',600);
";

			TestConnection.Command(sql).ExecuteScalar();

			AssertTimeZoneOffset(unloco, 10, referenceDate: new DateTime(2014, 2, 1));
			AssertTimeZoneOffset(unloco, 10, referenceDate: new DateTime(2014, 7, 1));
		}

		void AssertTimeZoneOffset(string unloco, decimal expectedOffset, string expectedMethod = "Calculate", DateTime? referenceDate = null)
		{
			var sql = $"SELECT Offset FROM dbo.{FunctionName}('{unloco}', '{(referenceDate ?? DateTime.Now).ToSqlFormat()}')";
			if (FunctionName == "CalculateTimeZoneOffsetFromRefUNLOCO")
			{
				sql = $"SELECT Offset FROM dbo.{FunctionName}('{unloco}', '{(referenceDate ?? DateTime.Now).ToSqlFormat()}', 1)";
			}
			var offset = TestConnection.ExecuteScalar(sql);

			AssertEquals("Offset for " + unloco, expectedOffset, offset);
		}

		protected virtual string FunctionName => "CalculateTimeZoneOffsetFromRefUNLOCO";

		protected override DbConnection TestConnection => Db.Connection;
	}

	[TestedType(typeof(CalculateTimeZoneOffsetFromRefUNLOCO))]
	sealed class CalculateTimeZoneOffsetFromRefUNLOCOUseCacheTest : DbCreateScriptTest
	{
	}
}
