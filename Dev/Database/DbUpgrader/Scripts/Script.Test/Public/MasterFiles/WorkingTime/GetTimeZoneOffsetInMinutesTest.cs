using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.WorkingTime;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.WorkingTime.Testing
{
	[TestedType(typeof(GetTimeZoneOffsetInMinutes))]
	sealed class GetTimeZoneOffsetInMinutesTest : DbCreateScriptTest
	{
	}

	class GetTimeZoneOffsetInMinutesFunctionTest : TimeZoneInMinutesFunctionTests
	{
		protected override void SetUp()
		{
			base.SetUp();

			StoreTimeZoneOffsets();
		}

		void StoreTimeZoneOffsets()
		{
			var sql = $@"
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2013-10-05', '2014-04-06', 660);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2014-04-06', '2014-10-05', 600);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUADL', '2014-04-06', '2014-10-05', 570);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2014-10-05', '2015-04-06', 660);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'GBLON', '2014-01-01', '2014-12-31', 60);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'DEDUS', '2014-01-01', '2014-12-31', 120);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'FRPAR', '2014-01-01', '2014-12-31', 120);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'ITMIL', '2014-01-01', '2014-12-31', 120);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'NZAKL', '2014-01-01', '2014-12-31', 720);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'USLAX', '2014-01-01', '2014-12-31', -420);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUBNE', '2014-01-01', '2014-12-31', 600);
";
			Db.Connection.Command(sql).ExecuteNonQuery();
		}

		protected override string FunctionName => "GetTimeZoneOffsetInMinutes";
	}
}

