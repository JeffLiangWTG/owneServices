using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture;

public class UpdateNudgeableServiceTasksFromStmServiceTaskTest : TestCase
{
	[TestedType(typeof(UpdateNudgeableServiceTasksFromStmServiceTask))]
	class TestUpdateNudgeableServiceTaskIntoTableWithInvalidSchedule : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNudgeableServiceTasksFromStmServiceTask();
		}

		protected override void PrepareTestData()
		{
			var sqlCmd = @"
DELETE from dbo.StmServiceTask;
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'TEL',1,'<ScheduleConfig><NextRunTimeCalculatorMinutes Period=""14"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate());
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'CMP',1,'<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""899"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate());";

			TestConnection.ExecuteNonQuery(sqlCmd);
		}

		protected override void AssertTransformationResults()
		{
			var sqlQuery1 = @"
SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'TEL'";
			var sqlQuery2 = @"
SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'CMP'";

			var result1 = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery1);
			var result2 = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery2);

			CombineAssertions(() =>
			{
				AssertEquals("<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" /></ScheduleConfig>", result1.Rows[0]["SST_Configuration"]);
				AssertEquals("<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" /></ScheduleConfig>", result2.Rows[0]["SST_Configuration"]);
			});
		}
	}

	[TestedType(typeof(UpdateNudgeableServiceTasksFromStmServiceTask))]
	class InsertNormalServiceTaskIntoTableAndNotUpdate : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNudgeableServiceTasksFromStmServiceTask();
		}

		protected override void PrepareTestData()
		{
			var sqlCmd = @"
DELETE from dbo.StmServiceTask;
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'~01',1,'<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""1"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())";

			TestConnection.ExecuteNonQuery(sqlCmd);
		}

		protected override void AssertTransformationResults()
		{
			var sqlQuery = @"
SELECT SST_Configuration FROM dbo.StmServiceTask";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery);

			AssertEquals("<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" /></ScheduleConfig>", result.Rows[0]["SST_Configuration"]);
		}
	}

	[TestedType(typeof(UpdateNudgeableServiceTasksFromStmServiceTask))]
	class InsertNudgeableServiceTaskWithValidScheduleIntoTableAndNotUpdate : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNudgeableServiceTasksFromStmServiceTask();
		}

		protected override void PrepareTestData()
		{
			var sqlCmd = @"
DELETE from dbo.StmServiceTask;
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'TEL',0,'<ScheduleConfig><NextRunTimeCalculatorMinutes Period=""15"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate());
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'CMP',0,'<ScheduleConfig><NextRunTimeCalculatorHours Period=""1"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate());
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'EJ0',0,'<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""900"" /></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate());";

			TestConnection.ExecuteNonQuery(sqlCmd);
		}

		protected override void AssertTransformationResults()
		{
			var sqlQuery1 = @"
SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'TEL'";
			var sqlQuery2 = @"
SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'CMP'";
			var sqlQuery3 = @"
SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'EJ0'";
			var result1 = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery1);
			var result2 = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery2);
			var result3 = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery3);

			AssertEquals("<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" /></ScheduleConfig>", result1.Rows[0]["SST_Configuration"]);
			AssertEquals("<ScheduleConfig><NextRunTimeCalculatorHours Period=\"1\" /></ScheduleConfig>", result2.Rows[0]["SST_Configuration"]);
			AssertEquals("<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"900\" /></ScheduleConfig>", result3.Rows[0]["SST_Configuration"]);
		}
	}

	[TestedType(typeof(UpdateNudgeableServiceTasksFromStmServiceTask))]
	class TestOtherInformationIsKeptAfterTransformation : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateNudgeableServiceTasksFromStmServiceTask();
		}

		protected override void PrepareTestData()
		{
			var sqlCmd = @"
DELETE from dbo.StmServiceTask;
INSERT INTO dbo.StmServiceTask (SST_PK,SST_ServiceTaskCode,SST_Active,SST_Configuration,SST_SystemCreateTimeUtc,SST_SystemCreateUser,SST_SystemLastEditTimeUtc,SST_SystemLastEditUser,SST_NextRunTime) VALUES (NEWID(),'TEL',1,'<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""30"" StartTime=""09:00:00"" EndTime=""17:00:00"" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>',GetUtcDate(),'~BP',GetUtcDate(),'~BP',GetUtcDate())";

			TestConnection.ExecuteNonQuery(sqlCmd);
		}

		protected override void AssertTransformationResults()
		{
			var sqlQuery = @"
SELECT SST_Active,SST_Configuration FROM dbo.StmServiceTask";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlQuery);

			CombineAssertions(() =>
			{
				AssertEquals("<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"15\" StartTime=\"09:00:00\" EndTime=\"17:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>", result.Rows[0]["SST_Configuration"]);
				AssertEquals(true, result.Rows[0]["SST_Active"]);
			});
		}
	}
}
