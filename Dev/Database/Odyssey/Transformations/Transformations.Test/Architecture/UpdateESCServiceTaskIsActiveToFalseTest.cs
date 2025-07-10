using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	class UpdateESCServiceTaskIsActiveToFalseTest : TestCase
	{
		[TestedType(typeof(UpdateESCServiceTaskIsActiveToFalse))]
		class TestUpdateRecordsInStmScheduleTaskAndStmServiceTask : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new UpdateESCServiceTaskIsActiveToFalse();
			}

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask"))
				{
					TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmScheduleTask
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmScheduleTask
(
	S5_PK,
	S5_ScheduleDescription,
	S5_ScheduleType,
	S5_TypeOfDocument,
	S5_ParentTableCode,
	S5_ParentID,
	S5_TaskPeriod,
	S5_IsActive
)
SELECT
	NEWID(),
	'Escrow Export Source code',
	'ESC',
	'CSP',
	'SH',
	NULL,
	'D',
	1

INSERT INTO dbo.StmServiceTask
(
	SST_PK,
	SST_ServiceTaskCode,
	SST_Active,
	SST_Configuration,
	SST_SystemCreateTimeUtc,
	SST_SystemCreateUser,
	SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser,
	SST_NextRunTime
)
SELECT
	NEWID(),
	'ESC',
	1,
	'',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP',
	GetUtcDate()
");
				}
			}

			protected override void AssertTransformationResults()
			{
				var sqlText1 = "SELECT S5_IsActive FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH' AND S5_ScheduleType = 'ESC'";
				var dataTable1 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText1);

				AssertEquals(1, dataTable1.Rows.Count);
				AssertEquals(false, dataTable1.Rows[0]["S5_IsActive"]);

				var sqlText2 = "SELECT SST_Active FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'ESC'";
				var dataTable2 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText2);

				AssertEquals(1, dataTable2.Rows.Count);
				AssertEquals(false, dataTable2.Rows[0]["SST_Active"]);
			}
		}

		[TestedType(typeof(UpdateESCServiceTaskIsActiveToFalse))]
		class TestOnlyUpdateRecordInStmScheduleTaskIfStmServiceTaskIsEmpty : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new UpdateESCServiceTaskIsActiveToFalse();
			}

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask"))
				{
					TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmScheduleTask
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmScheduleTask
(
	S5_PK,
	S5_ScheduleDescription,
	S5_ScheduleType,
	S5_TypeOfDocument,
	S5_ParentTableCode,
	S5_ParentID,
	S5_TaskPeriod,
	S5_IsActive
)
SELECT
	NEWID(),
	'Escrow Export Source code',
	'ESC',
	'CSP',
	'SH',
	NULL,
	'D',
	1
");
				}
			}

			protected override void AssertTransformationResults()
			{
				var sqlText1 = "SELECT S5_IsActive FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH' AND S5_ScheduleType = 'ESC'";
				var dataTable1 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText1);

				AssertEquals(1, dataTable1.Rows.Count);
				AssertEquals(false, dataTable1.Rows[0]["S5_IsActive"]);

				var sqlText2 = "SELECT SST_Active FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'ESC'";
				var dataTable2 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText2);

				AssertEquals(0, dataTable2.Rows.Count);
			}
		}

		[TestedType(typeof(UpdateESCServiceTaskIsActiveToFalse))]
		class TestDoNotChangeIsActiveIfAlreadyFalse : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new UpdateESCServiceTaskIsActiveToFalse();
			}

			protected override void PrepareTestData()
			{
				using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask"))
				{
					TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmScheduleTask
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmScheduleTask
(
	S5_PK,
	S5_ScheduleDescription,
	S5_ScheduleType,
	S5_TypeOfDocument,
	S5_ParentTableCode,
	S5_ParentID,
	S5_TaskPeriod,
	S5_IsActive
)
SELECT
	NEWID(),
	'Escrow Export Source code',
	'ESC',
	'CSP',
	'SH',
	NULL,
	'D',
	0

INSERT INTO dbo.StmServiceTask
(
	SST_PK,
	SST_ServiceTaskCode,
	SST_Active,
	SST_Configuration,
	SST_SystemCreateTimeUtc,
	SST_SystemCreateUser,
	SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser,
	SST_NextRunTime
)
SELECT
	NEWID(),
	'ESC',
	0,
	'',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP',
	GetUtcDate()
");
				}
			}

			protected override void AssertTransformationResults()
			{
				var sqlText1 = "SELECT S5_IsActive FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH' AND S5_ScheduleType = 'ESC'";
				var dataTable1 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText1);

				AssertEquals(1, dataTable1.Rows.Count);
				AssertEquals(false, dataTable1.Rows[0]["S5_IsActive"]);

				var sqlText2 = "SELECT SST_Active FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'ESC'";
				var dataTable2 = DataUtils.GetDataTableFromQuery(TestConnection, sqlText2);

				AssertEquals(1, dataTable2.Rows.Count);
				AssertEquals(false, dataTable2.Rows[0]["SST_Active"]);
			}
		}
	}
}
