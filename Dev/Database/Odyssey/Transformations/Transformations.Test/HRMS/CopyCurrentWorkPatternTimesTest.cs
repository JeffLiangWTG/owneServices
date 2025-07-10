using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS.Testing
{
	[TestedType(typeof(CopyCurrentWorkPatternTimes))]
	internal class CopyCurrentWorkPatternTimesTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var now = DateTime.UtcNow;
			var selectSQL = $@"
SELECT GS_Code + '|' + GW_DayOfWeek + '|' + CONVERT(VARCHAR(8), GW_StartTime, 108) + '|' + CONVERT(VARCHAR(8), GW_EndTime, 108)
FROM dbo.GlbWorkTime
JOIN dbo.GlbStaff ON GW_ParentID = GS_PK
WHERE GS_Code in ('WP1', 'WP2', 'WP3', 'WP4') AND GW_ParentTableCode = 'GS'
ORDER BY GS_Code, GW_DayOfWeek, GW_StartTime";

			var actual = new StringBuilder();
			Db.Connection.ExecuteReader(selectSQL, reader => actual.AppendLine(reader.GetString(0)));

			var expected = @"
WP1|MON|07:00:00|13:00:00
WP2|WED|08:00:00|13:00:00
WP2|WED|14:00:00|17:00:00
WP3|THU|07:00:00|14:00:00
WP4|SAT|08:00:00|17:00:00
";
			AssertEquals("Should be copied from current work pattern", expected.Trim(), actual.ToString().Trim());
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CopyCurrentWorkPatternTimes();
		}

		protected override void PrepareTestData()
		{
			var noWorkPattern = Guid.NewGuid();
			var hasCurrentWorkPattern = Guid.NewGuid();
			var hasCurrentWorkPattern2 = Guid.NewGuid();
			var currentWorkPattern = Guid.NewGuid();
			var previousWorkPattern = Guid.NewGuid();
			var currentWorkPattern2 = Guid.NewGuid();
			var unapprovedWorkPattern = Guid.NewGuid();
			var noCurrentWorkPattern = Guid.NewGuid();
			var futureWorkPattern = Guid.NewGuid();

			Db.Connection.ExecuteNonQuery($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{noWorkPattern}', 'WP1', 'WP1', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{noWorkPattern}', 'GS', 'MON', '1900-1-1 07:00:00', '1900-1-1 13:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{hasCurrentWorkPattern}', 'WP2', 'WP2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{hasCurrentWorkPattern}', 'GS', 'TUE', '1900-1-1 07:00:00', '1900-1-1 13:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkPattern (GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES ('{previousWorkPattern}', '1900-1-1 20:00:00', '{hasCurrentWorkPattern}', '2019-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{previousWorkPattern}', 'GWP', 'THU', '1900-1-1 06:00:00', '1900-1-1 17:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkPattern (GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES ('{currentWorkPattern}', '1900-1-1 20:00:00', '{hasCurrentWorkPattern}', '2020-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{currentWorkPattern}', 'GWP', 'WED', '1900-1-1 08:00:00', '1900-1-1 13:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{currentWorkPattern}', 'GWP', 'WED', '1900-1-1 14:00:00', '1900-1-1 17:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{noCurrentWorkPattern}', 'WP3', 'WP3', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{noCurrentWorkPattern}', 'GS', 'THU', '1900-1-1 07:00:00', '1900-1-1 14:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkPattern (GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES ('{futureWorkPattern}', '1900-1-1 20:00:00', '{noCurrentWorkPattern}', DATEADD(d, 10, GETUTCDATE()), GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES (NEWID(), '{futureWorkPattern}', 'GS', 'FRI', '1900-1-1 09:00:00', '1900-1-1 10:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbStaff(GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES('{hasCurrentWorkPattern2}', 'WP4', 'WP4', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.GlbWorkPattern(GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES('{currentWorkPattern2}', '1900-1-1 20:00:00', '{hasCurrentWorkPattern2}', '2020-1-1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbWorkTime(GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES(NEWID(), '{currentWorkPattern2}', 'GWP', 'SAT', '1900-1-1 08:00:00', '1900-1-1 17:00:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbWorkPattern(GWP_PK, GWP_StandardDuration, GWP_GS_Staff, GWP_EffectiveDate, GWP_IsApproved, GWP_SystemCreateTimeUtc, GWP_SystemCreateUser, GWP_SystemLastEditTimeUtc, GWP_SystemLastEditUser)
VALUES('{unapprovedWorkPattern}', '1900-1-1 20:00:00', '{hasCurrentWorkPattern2}', '2020-1-2', 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbWorkTime(GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) VALUES(NEWID(), '{unapprovedWorkPattern}', 'GWP', 'SAT', '1900-1-1 08:23:00', '1900-1-1 17:23:00', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}
	}
}
