using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.GB;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.GB;

[TestedType(typeof(DeleteChiefServiceTasks))]
class DeleteChiefServiceTasksTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new DeleteChiefServiceTasks();

	protected override void PrepareTestData()
	{
		var sql = @"
				DELETE FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('MDD', 'MUD', 'CDD', 'CUD', 'PNT') AND S5_TypeOfDocument = 'GBC';

				INSERT INTO dbo.StmScheduleTask(S5_PK, S5_ScheduleDescription, S5_TaskPeriod, S5_TaskPeriodCount, S5_DayNumber, S5_DayList, S5_MonthNumber, S5_WeekDayOccurrenceNumber, S5_StartDate, S5_EndAfterCount, S5_ScheduleActualRunCount, S5_AccountingPeriodScheduleFrstRun, S5_ScheduleType, S5_TypeOfDocument, S5_NextScheduledPrintRunTimeUtc, S5_ParentTableCode, S5_RunTimeInMinutes, S5_WeekDaysOnly, S5_IsActive, S5_IsPrivate, S5_GS_NKPrintUser, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_OverdueDurationInSeconds)
				VALUES (NEWID(), 'CNS Compass downloader', 'T', 15, 0, 'NNNNNNN', 0, 0, GETUTCDATE(), 0, 0, 0, 'CDD', 'GBC', GETUTCDATE(), 'SH', 0, 0, 0, 1, '~BP', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 0),
					   (NEWID(), 'CNS Compass uploader and downloader', 'S', 15, 0, 'NNNNNNN', 0, 0, GETUTCDATE(), 0, 0, 0, 'CUD', 'GBC', GETUTCDATE(), 'SH', 0, 0, 0, 1, '~BP', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 0),
					   (NEWID(), 'MCP Destin8 downloader', 'T', 15, 0, 'NNNNNNN', 0, 0, GETUTCDATE(), 0, 0, 0, 'MDD', 'GBC', GETUTCDATE(),'SH', 0, 0, 0, 1, '~BP', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 0),
					   (NEWID(), 'MCP Destin8 uploader and downloader', 'S', 15, 0, 'NNNNNNN', 0, 0, GETUTCDATE(), 0, 0, 0, 'MUD', 'GBC', GETUTCDATE(), 'SH', 0, 0, 0, 1, '~BP', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 0),
					   (NEWID(), 'Pentant uploader and downloader', 'S', 300, 0, 'NNNNNNN', 0, 0, GETUTCDATE(), 0, 0, 0, 'PNT', 'GBC', GETUTCDATE(), 'SH', 0, 0, 0, 1, '~BP', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 0)";
		TestConnection.ExecuteNonQuery(sql);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("No service task schedule with code MDD, MUD, CDD, CUD and PDT are present", 0, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM dbo.StmScheduleTask WHERE S5_ScheduleType IN ('MDD', 'MUD', 'CDD', 'CUD', 'PNT')"));
	}
}
