using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	class PopulateStmServiceTaskFromStmScheduleTaskTest : TestCase
	{
		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class InsertRowsWhoseS5_ParentTableCodeIsSHIfStmServiceTaskIsEmpty : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				branchPk1 = Guid.NewGuid();
				branchPk2 = Guid.NewGuid();
				nextRunTime1 = new DateTime(2024, 1, 1, 0, 0, 0);
				nextRunTime2 = new DateTime(2024, 2, 2, 0, 0, 0);

				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.GlbCompany
	(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
	(@CompanyPk, 'AU', 'AUD', @CompanyCode, 'AU company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
	(@BranchPk1, @CompanyPk, @BranchCode1, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
	(@BranchPk2, @CompanyPk, @BranchCode2, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 1', @TaskCode1, '', @BranchPk1, 'D', 'SH', NULL, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @nextRunTime1, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 2', @TaskCode2, '', @BranchPk2, 'D', 'SH', NULL, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @nextRunTime2, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'not service task', '~NO', '', @BranchPk2, 'D', 'AC', NEWID(), 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @nextRunTime2, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 3', '~T3', '', @BranchPk2, 'H', 'SH', NULL, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP', NULL, 1
"
					, cmd =>
					{
						cmd.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@CompanyCode", SqlDbType.NVarChar, 3, "CCC");
						cmd.AddParameter("@BranchPk1", SqlDbType.UniqueIdentifier, branchPk1);
						cmd.AddParameter("@BranchPk2", SqlDbType.UniqueIdentifier, branchPk2);
						cmd.AddParameter("@BranchCode1", SqlDbType.NVarChar, 3, "BR1");
						cmd.AddParameter("@BranchCode2", SqlDbType.NVarChar, 3, "BR2");
						cmd.AddParameter("@TaskCode1", SqlDbType.NVarChar, 3, taskCode1);
						cmd.AddParameter("@TaskCode2", SqlDbType.NVarChar, 3, taskCode2);
						cmd.AddParameter("nextRunTime1", SqlDbType.DateTime, nextRunTime1);
						cmd.AddParameter("nextRunTime2", SqlDbType.DateTime, nextRunTime2);
					});
			}

			protected override void AssertTransformationResults()
			{
				var sqlText = "SELECT SST_ServiceTaskCode, SST_NextRunTime, SST_GB_Branch, SST_Active FROM dbo.StmServiceTask;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				var task1 = dataTable.Select($"SST_ServiceTaskCode='{taskCode1}'")[0];
				var task2 = dataTable.Select($"SST_ServiceTaskCode='{taskCode2}'")[0];
				var task3 = dataTable.Select($"SST_ServiceTaskCode='~T3'")[0];

				AssertEquals(3, dataTable.Rows.Count);

				AssertEquals(branchPk1, task1["SST_GB_Branch"]);
				AssertEquals(true, task1["SST_Active"]);
				AssertEquals(new DateTimeOffset(nextRunTime1, TimeSpan.Zero), task1["SST_NextRunTime"]);

				AssertEquals(branchPk2, task2["SST_GB_Branch"]);
				AssertEquals(false, task2["SST_Active"]);
				AssertEquals(new DateTimeOffset(nextRunTime2, TimeSpan.Zero), task2["SST_NextRunTime"]);

				AssertEquals(new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0), TimeSpan.Zero), task3["SST_NextRunTime"]);
			}

			Guid branchPk1;
			Guid branchPk2;
			DateTime nextRunTime1;
			DateTime nextRunTime2;

			const string taskCode1 = "~T1";
			const string taskCode2 = "~T2";
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class DeleteAllRowsFromStmServiceTaskBeforeInsert : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
	DELETE from dbo.StmServiceTask
	DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

	INSERT INTO dbo.GlbCompany
	(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
	VALUES
	(@CompanyPk, 'AU', 'AUD', @CompanyCode, 'AU company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
	VALUES
	(@BranchPk1, @CompanyPk, @BranchCode1, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
	VALUES
	(@BranchPk2, @CompanyPk, @BranchCode2, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

	INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
	SELECT NEWID(), 'dummy task 1', @ScheduleTaskCode, '', @BranchPk1, 'D', 'SH', NULL, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP', GetUtcDate(), 1

	INSERT INTO dbo.StmServiceTask
	(SST_PK, SST_ServiceTaskCode, SST_GB_Branch, SST_Active, SST_NextRunTime, SST_Configuration,
		SST_SystemCreateTimeUtc, SST_SystemCreateUser, SST_SystemLastEditTimeUtc, SST_SystemLastEditUser)
	SELECT NEWID(), @ServiceTaskCode, @BranchPk2, 1, GetUtcDate(), '<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""1"" /></ScheduleConfig>', GetUtcDate(), '~BP', GetUtcDate(), '~BP'

	INSERT INTO dbo.StmServiceTask
	(SST_PK, SST_ServiceTaskCode, SST_GB_Branch, SST_Active, SST_NextRunTime, SST_Configuration,
		SST_SystemCreateTimeUtc, SST_SystemCreateUser, SST_SystemLastEditTimeUtc, SST_SystemLastEditUser)
	SELECT NEWID(), @ServiceTaskCode2, @BranchPk2, 1, GetUtcDate(), '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
	"
					, cmd =>
					{
						cmd.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@CompanyCode", SqlDbType.NVarChar, 3, "CCC");
						cmd.AddParameter("@BranchPk1", SqlDbType.UniqueIdentifier, branchPk1);
						cmd.AddParameter("@BranchPk2", SqlDbType.UniqueIdentifier, branchPk2);
						cmd.AddParameter("@BranchCode1", SqlDbType.NVarChar, 3, "BR1");
						cmd.AddParameter("@BranchCode2", SqlDbType.NVarChar, 3, "BR2");
						cmd.AddParameter("@ScheduleTaskCode", SqlDbType.NVarChar, 3, scheduleTaskCode);
						cmd.AddParameter("@ServiceTaskCode", SqlDbType.NVarChar, 3, serviceTaskCode);
						cmd.AddParameter("@ServiceTaskCode2", SqlDbType.NVarChar, 3, serviceTaskCode2);
					});
			}

			protected override void AssertTransformationResults()
			{
				var sqlText = "SELECT SST_ServiceTaskCode, SST_NextRunTime, SST_GB_Branch, SST_Active FROM dbo.StmServiceTask;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(scheduleTaskCode, dataTable.Rows[0]["SST_ServiceTaskCode"]);
			}

			Guid branchPk1 => Guid.NewGuid();
			Guid branchPk2 => Guid.NewGuid();
			const string scheduleTaskCode = "SCH";
			const string serviceTaskCode = "SER";
			const string serviceTaskCode2 = "SE2";
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class RemoveOnlyServiceTaskDuplicatesInStmScheduleTaskBeforePopulatingStmServiceTask : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				lastEditTime1_1 = new DateTime(2022, 1, 1, 0, 0, 0);
				lastEditTime1_2 = new DateTime(2023, 1, 1, 0, 0, 0);
				lastEditTime1_3 = new DateTime(2024, 1, 1, 0, 0, 0);
				lastEditTime2_1 = new DateTime(2024, 1, 1, 9, 9, 0);
				lastEditTime2_2 = new DateTime(2024, 1, 1, 0, 0, 0);

				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 1', @TaskCode1, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP', @nextRunTime, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 1', @TaskCode1, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_2, '~BP', @nextRunTime, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 1', @TaskCode1, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_3, '~BP', @nextRunTime, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 2', @TaskCode2, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', @lastEditTime2_1, '~BP', @nextRunTime, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 2', @TaskCode2, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', @lastEditTime2_2, '~BP', @nextRunTime, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 3', @TaskCode3, '', NULL, 'D', 'SH', NULL, 1, @createTime, '~BP', GetUtcDate(), '~BP', GetUtcDate(), 1

-- non service task rows with same S5_ScheduleType
INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'not service task', 'AAA', '', NULL, 'D', 'AC', NEWID(), 1, @createTime, '~BP', GetUtcDate(), '~BP', GetUtcDate(), 1
INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'not service task', 'AAA', '', NULL, 'D', 'AC', NEWID(), 1, @createTime, '~BP', GetUtcDate(), '~BP', GetUtcDate(), 1
"
					, cmd =>
					{
						cmd.AddParameter("@TaskCode1", SqlDbType.NVarChar, 3, taskCode1);
						cmd.AddParameter("@TaskCode2", SqlDbType.NVarChar, 3, taskCode2);
						cmd.AddParameter("@TaskCode3", SqlDbType.NVarChar, 3, taskCode3);
						cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
						cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, lastEditTime1_1);
						cmd.AddParameter("@lastEditTime1_2", SqlDbType.SmallDateTime, lastEditTime1_2);
						cmd.AddParameter("@lastEditTime1_3", SqlDbType.SmallDateTime, lastEditTime1_3);
						cmd.AddParameter("@lastEditTime2_1", SqlDbType.SmallDateTime, lastEditTime2_1);
						cmd.AddParameter("@lastEditTime2_2", SqlDbType.SmallDateTime, lastEditTime2_2);
						cmd.AddParameter("@nextRunTime", SqlDbType.DateTime, new DateTime(2024, 1, 1, 0, 0, 0));
					});
			}

			protected override void AssertTransformationResults()
			{
				var stmServiceTaskSqlText = "SELECT SST_ServiceTaskCode, SST_SystemLastEditTimeUtc, SST_Active FROM dbo.StmServiceTask;";
				var stmScheduleTaskSqlText = "SELECT S5_ScheduleType FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH' OR (S5_ScheduleType = 'AAA' AND S5_ParentTableCode = 'AC');";

				var stmServiceTaskDataTable = DataUtils.GetDataTableFromQuery(TestConnection, stmServiceTaskSqlText);
				var stmScheduleTaskDataTable = DataUtils.GetDataTableFromQuery(TestConnection, stmScheduleTaskSqlText);

				var task1 = stmServiceTaskDataTable.Select($"SST_ServiceTaskCode='{taskCode1}'")[0];
				var task2 = stmServiceTaskDataTable.Select($"SST_ServiceTaskCode='{taskCode2}'")[0];
				var task3 = stmServiceTaskDataTable.Select($"SST_ServiceTaskCode='{taskCode3}'")[0];

				var schedules = stmScheduleTaskDataTable.Select($"S5_ScheduleType='AAA'");

				AssertEquals(3, stmServiceTaskDataTable.Rows.Count);
				AssertEquals(lastEditTime1_3, task1["SST_SystemLastEditTimeUtc"]);
				AssertEquals(lastEditTime2_1, task2["SST_SystemLastEditTimeUtc"]);
				AssertNotNull(task3);

				AssertEquals(5, stmScheduleTaskDataTable.Rows.Count);
				AssertEquals(2, schedules.Length);
				AssertEquals("AAA", schedules[0]["S5_ScheduleType"]);
				AssertEquals("AAA", schedules[1]["S5_ScheduleType"]);
			}

			DateTime lastEditTime1_1;
			DateTime lastEditTime1_2;
			DateTime lastEditTime1_3;
			DateTime lastEditTime2_1;
			DateTime lastEditTime2_2;

			const string taskCode1 = "~T1";
			const string taskCode2 = "~T2";
			const string taskCode3 = "~T3";
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class CopySchedules : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				manager = new UpgradeManagerForTestWithOutputBuffer();
				var instance = new PopulateStmServiceTaskFromStmScheduleTask();
				instance.Initialise(manager: manager);
				return instance;
			}

			protected override void PrepareTestData()
			{
				period = 2;
				startTime = new DateTime(2024, 1, 1, 11, 0, 0);
				endTime = new DateTime(2024, 1, 1, 22, 0, 0);
				nextRunTime = new DateTime(2024, 1, 1, 1, 1, 1);
				weekDayOccurrence = 2;
				startDate = new DateTime(2024, 1, 1, 0, 0, 0);
				monthNumber = 2;
				additionalConfigSecondaryProcesses = "<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>2</SecondaryProcessesMaxCount></HostedServiceSerializableSettings>";
				additionalConfigConfigString = "<HostedServiceSerializableSettings><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString>TestConfigString</ConfigString></HostedServiceSerializableSettings>";
				additionalConfigInvalidXml = "<HostedServiceSerializableSettings><xml>/xml</HostedServiceSerializableSettings>";

				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorSeconds, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeSeconds, @Period, @StartTimeNull, @EndTimeNull, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMinutes, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMinutes, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorHours, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeHours, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorDays, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeDays, @Period, @StartTimeNull, @EndTimeNull, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorWorkingDays, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeWeeks, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyTrue, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorWeeks, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeWeeks, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyFalse, @DayListMonWedFri,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorWeeks2, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeWeeks, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyFalse, @DayListTueThuSatSun,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMonthsByDate, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceZero, @StartDate, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMonthsByLastDay, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumber99, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrence, @StartDateNull, @DayNumber1, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek2, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrence, @StartDateNull, @DayNumber4, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek3, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrence, @StartDateNull, @DayNumber7, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorYearsByDate, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeYears, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceZero, @StartDate, @DayNumberNull, @MonthNumber, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeNextRunTimeCalculatorYearsByDayOfMonth, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeYears, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrence, @StartDateNull, @DayNumber1, @MonthNumber, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeSecondaryProcessesMaxCount, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMinutes, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigSecondaryProcesses


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeConfigString, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMinutes, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigConfigString


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeInvalidXml, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMinutes, @Period, @StartTime, @EndTime, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigInvalidXml


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeZeroPeriod, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeSeconds, @PeriodZero, @StartTimeNull, @EndTimeNull, @NextRunTime, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeAllNullRunTimes, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeDays, @Period, @StartTimeNull, @EndTimeNull, @NextRunTimeNull, @WeekDaysOnlyNull, @DayListNull,
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumberNull, @MonthNumberNull, @AdditionalConfigNull
",
					cmd =>
					{
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorSeconds", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorSeconds);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMinutes", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMinutes);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorHours", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorHours);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorDays", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorDays);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorWorkingDays", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorWorkingDays);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorWeeks", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorWeeks);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorWeeks2", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorWeeks2);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMonthsByDate", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMonthsByDate);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMonthsByLastDay", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMonthsByLastDay);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMonthsByDayOfWeek);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek2", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMonthsByDayOfWeek2);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorMonthsByDayOfWeek3", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorMonthsByDayOfWeek3);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorYearsByDate", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorYearsByDate);
						cmd.AddParameter("@TaskCodeNextRunTimeCalculatorYearsByDayOfMonth", SqlDbType.VarChar, taskCodeNextRunTimeCalculatorYearsByDayOfMonth);
						cmd.AddParameter("@TaskCodeSecondaryProcessesMaxCount", SqlDbType.VarChar, taskCodeSecondaryProcessesMaxCount);
						cmd.AddParameter("@TaskCodeConfigString", SqlDbType.VarChar, taskCodeConfigString);
						cmd.AddParameter("@TaskCodeInvalidXml", SqlDbType.VarChar, taskCodeInvalidXml);
						cmd.AddParameter("@TaskCodeZeroPeriod", SqlDbType.VarChar, taskCodeZeroPeriod);
						cmd.AddParameter("@TaskCodeAllNullRunTimes", SqlDbType.VarChar, taskCodeAllNullRunTimes);

						cmd.AddParameter("@PeriodTypeSeconds", SqlDbType.VarChar, periodTypeSeconds);
						cmd.AddParameter("@PeriodTypeMinutes", SqlDbType.VarChar, periodTypeMinutes);
						cmd.AddParameter("@PeriodTypeHours", SqlDbType.VarChar, periodTypeHours);
						cmd.AddParameter("@PeriodTypeDays", SqlDbType.VarChar, periodTypeDays);
						cmd.AddParameter("@PeriodTypeWeeks", SqlDbType.VarChar, periodTypeWeeks);
						cmd.AddParameter("@PeriodTypeMonths", SqlDbType.VarChar, periodTypeMonths);
						cmd.AddParameter("@PeriodTypeYears", SqlDbType.VarChar, periodTypeYears);
						cmd.AddParameter("@Period", SqlDbType.Int, period);
						cmd.AddParameter("@PeriodZero", SqlDbType.Int, periodZero);
						cmd.AddParameter("@StartTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@StartTime", SqlDbType.SmallDateTime, startTime);
						cmd.AddParameter("@EndTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@EndTime", SqlDbType.SmallDateTime, endTime);
						cmd.AddParameter("@NextRunTime", SqlDbType.DateTime, nextRunTime);
						cmd.AddParameter("@NextRunTimeNull", SqlDbType.DateTime, DBNull.Value);
						cmd.AddParameter("@WeekDaysOnlyNull", SqlDbType.Bit, false);
						cmd.AddParameter("@WeekDaysOnlyTrue", SqlDbType.Bit, true);
						cmd.AddParameter("@WeekDaysOnlyFalse", SqlDbType.Bit, false);
						cmd.AddParameter("@DayListNull", SqlDbType.VarChar, string.Empty);
						cmd.AddParameter("@DayListMonWedFri", SqlDbType.VarChar, dayListMonWedFri);
						cmd.AddParameter("@DayListTueThuSatSun", SqlDbType.VarChar, dayListTueThuSatSun);
						cmd.AddParameter("@WeekDayOccurrenceNull", SqlDbType.Int, 0);
						cmd.AddParameter("@WeekDayOccurrenceZero", SqlDbType.Int, 0);
						cmd.AddParameter("@WeekDayOccurrence", SqlDbType.Int, weekDayOccurrence);
						cmd.AddParameter("@StartDateNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@StartDate", SqlDbType.SmallDateTime, startDate);
						cmd.AddParameter("@DayNumberNull", SqlDbType.Int, 0);
						cmd.AddParameter("@DayNumber1", SqlDbType.Int, dayNumber1);
						cmd.AddParameter("@DayNumber4", SqlDbType.Int, dayNumber4);
						cmd.AddParameter("@DayNumber7", SqlDbType.Int, dayNumber7);
						cmd.AddParameter("@DayNumber99", SqlDbType.Int, dayNumber99);
						cmd.AddParameter("@MonthNumberNull", SqlDbType.Int, 0);
						cmd.AddParameter("@MonthNumber", SqlDbType.Int, monthNumber);
						cmd.AddParameter("@AdditionalConfigNull", SqlDbType.VarBinary, DBNull.Value);
						cmd.AddParameter("@AdditionalConfigSecondaryProcesses", SqlDbType.VarBinary, Encoding.ASCII.GetBytes(additionalConfigSecondaryProcesses));
						cmd.AddParameter("@AdditionalConfigConfigString", SqlDbType.VarBinary, Encoding.ASCII.GetBytes(additionalConfigConfigString));
						cmd.AddParameter("@AdditionalConfigInvalidXml", SqlDbType.VarBinary, Encoding.ASCII.GetBytes(additionalConfigInvalidXml));
						cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
						cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, new DateTime(2024, 1, 1, 0, 0, 0));
					});
			}

			protected override void AssertTransformationResults()
			{
				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var endTimeAsUtc = endTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = "SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				var taskSeconds = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorSeconds}'").Single();
				var taskMinutes = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMinutes}'").Single();
				var taskHours = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorHours}'").Single();
				var taskDays = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorDays}'").Single();
				var taskWorkingDays = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorWorkingDays}'").Single();
				var taskWeeks = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorWeeks}'").Single();
				var taskWeeks2 = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorWeeks2}'").Single();
				var taskMonthsByDate = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMonthsByDate}'").Single();
				var taskMonthsByLastDate = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMonthsByLastDay}'").Single();
				var taskMonthsByDayOfWeek = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMonthsByDayOfWeek}'").Single();
				var taskMonthsByDayOfWeek2 = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMonthsByDayOfWeek2}'").Single();
				var taskMonthsByDayOfWeek3 = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorMonthsByDayOfWeek3}'").Single();
				var taskYearsByDate = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorYearsByDate}'").Single();
				var taskYearsByDayOfMonth = dataTable.Select($"SST_ServiceTaskCode='{taskCodeNextRunTimeCalculatorYearsByDayOfMonth}'").Single();
				var taskSecondaryProcessesMaxCount = dataTable.Select($"SST_ServiceTaskCode='{taskCodeSecondaryProcessesMaxCount}'").Single();
				var taskConfigString = dataTable.Select($"SST_ServiceTaskCode='{taskCodeConfigString}'").Single();
				var taskInvalidXml = dataTable.Select($"SST_ServiceTaskCode='{taskCodeInvalidXml}'").Single();
				var taskZeroPeriod = dataTable.Select($"SST_ServiceTaskCode='{taskCodeZeroPeriod}'").Single();
				var taskAllNullRunTimes = dataTable.Select($"SST_ServiceTaskCode='{taskCodeAllNullRunTimes}'").Single();

				AssertEquals(19, dataTable.Rows.Count);

				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"{period}\" /></ScheduleConfig>", (string)taskSeconds["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"{period}\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMinutes["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorHours Period=\"{period}\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskHours["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"{period}\" ScheduledRunTime=\"{TruncateToNearestMinute(nextRunTime):HH:mm:ss}\" /></ScheduleConfig>", (string)taskDays["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskWorkingDays["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"{period}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Friday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>", (string)taskWeeks["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"{period}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence><DayOfWeek>Sunday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Saturday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>", (string)taskWeeks2["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"{period}\" DayOfOccurrence=\"{startDate.Day}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMonthsByDate["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"{period}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMonthsByLastDate["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"{weekDayOccurrence}\" DayOfTheWeek=\"{(DayOfWeek)dayNumber1 - 1}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMonthsByDayOfWeek["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"{weekDayOccurrence}\" DayOfTheWeek=\"{(DayOfWeek)dayNumber4 - 1}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMonthsByDayOfWeek2["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"{weekDayOccurrence}\" DayOfTheWeek=\"{(DayOfWeek)dayNumber7 - 1}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskMonthsByDayOfWeek3["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"{monthNumber}\" Day=\"{startDate.Day}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskYearsByDate["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth DayOfTheWeek=\"{(DayOfWeek)dayNumber1 - 1}\" WeekOfTheMonth=\"{weekDayOccurrence}\" MonthOfTheYear=\"{monthNumber}\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskYearsByDayOfMonth["SST_Configuration"]);

				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"{period}\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" />{GetInnerXml(additionalConfigSecondaryProcesses)}</ScheduleConfig>", (string)taskSecondaryProcessesMaxCount["SST_Configuration"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"{period}\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" />{GetInnerXml(additionalConfigConfigString)}</ScheduleConfig>", (string)taskConfigString["SST_Configuration"]);

				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"{period}\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)taskInvalidXml["SST_Configuration"]);
				AssertContains($"Service Task {taskCodeInvalidXml} has invalid schedule configuration. Using defaults for SecondaryProcessesMaxCount and Additional Configuration", string.Join(Environment.NewLine, ((UpgradeManagerForTestWithOutputBuffer)manager).OutputTextCollection));

				AssertEquals("Zero period should default to 1", "<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" /></ScheduleConfig>", (string)taskZeroPeriod["SST_Configuration"]);
				AssertEquals("When no run time found, should default to 00:00:00", $"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"{period}\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>", (string)taskAllNullRunTimes["SST_Configuration"]);

				string GetInnerXml(string outerXml)
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(outerXml);

					return xmlDoc.FirstChild.InnerXml;
				}

				static DateTime TruncateToNearestMinute(DateTime dt)
				{
					return dt.AddSeconds(-dt.Second).AddMilliseconds(-dt.Millisecond);
				}
			}

			const string taskCodeNextRunTimeCalculatorSeconds = "~T1";
			const string taskCodeNextRunTimeCalculatorMinutes = "~T2";
			const string taskCodeNextRunTimeCalculatorHours = "~T3";
			const string taskCodeNextRunTimeCalculatorDays = "~T4";
			const string taskCodeNextRunTimeCalculatorWorkingDays = "~T5";
			const string taskCodeNextRunTimeCalculatorWeeks = "~T6";
			const string taskCodeNextRunTimeCalculatorWeeks2 = "~T7";
			const string taskCodeNextRunTimeCalculatorMonthsByDate = "~T8";
			const string taskCodeNextRunTimeCalculatorMonthsByLastDay = "~T9";
			const string taskCodeNextRunTimeCalculatorMonthsByDayOfWeek = "~TA";
			const string taskCodeNextRunTimeCalculatorMonthsByDayOfWeek2 = "~TB";
			const string taskCodeNextRunTimeCalculatorMonthsByDayOfWeek3 = "~TC";
			const string taskCodeNextRunTimeCalculatorYearsByDate = "~TD";
			const string taskCodeNextRunTimeCalculatorYearsByDayOfMonth = "~TE";
			const string taskCodeSecondaryProcessesMaxCount = "~TF";
			const string taskCodeConfigString = "~TG";
			const string taskCodeInvalidXml = "~TH";
			const string taskCodeZeroPeriod = "~TI";
			const string taskCodeAllNullRunTimes = "~TJ";

			const string periodTypeSeconds = "S";
			const string periodTypeMinutes = "T";
			const string periodTypeHours = "H";
			const string periodTypeDays = "D";
			const string periodTypeWeeks = "W";
			const string periodTypeMonths = "M";
			const string periodTypeYears = "Y";

			const string dayListMonWedFri = "NYNYNYN";
			const string dayListTueThuSatSun = "YNYNYNY";
			const int dayNumber1 = 1;
			const int dayNumber4 = 4;
			const int dayNumber7 = 7;
			const int dayNumber99 = 99;
			const int periodZero = 0;

			int period;
			DateTime startTime;
			DateTime endTime;
			DateTime nextRunTime;
			int weekDayOccurrence;
			DateTime startDate;
			int monthNumber;
			string additionalConfigSecondaryProcesses;
			string additionalConfigConfigString;
			string additionalConfigInvalidXml;

			IUpgradeManager manager;
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class CopyStartTimeAndEndTimeInConjunction : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCode1, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'S', 1, @StartTime, @EndTime, @NextRunTime, 0, '',
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 2', @TaskCode2, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'S', 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 3', @TaskCode3, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'S', 1, @StartTimeNull, @EndTime, @NextRunTime, 0, '',
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 4', @TaskCode4, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'S', 1, @StartTimeNull, @EndTimeNull, @NextRunTime, 0, '',
	0, null, 0, 0, null
",
				cmd =>
				{
					cmd.AddParameter("@TaskCode1", SqlDbType.VarChar, taskCode1);
					cmd.AddParameter("@TaskCode2", SqlDbType.VarChar, taskCode2);
					cmd.AddParameter("@TaskCode3", SqlDbType.VarChar, taskCode3);
					cmd.AddParameter("@TaskCode4", SqlDbType.VarChar, taskCode4);
					cmd.AddParameter("@StartTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
					cmd.AddParameter("@StartTime", SqlDbType.SmallDateTime, startTime);
					cmd.AddParameter("@EndTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
					cmd.AddParameter("@EndTime", SqlDbType.SmallDateTime, endTime);
					cmd.AddParameter("@NextRunTime", SqlDbType.DateTime, new DateTime(2025, 1, 1, 12, 30, 00));
					cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
					cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, new DateTime(2024, 1, 1, 0, 0, 0));
				});
			}
			protected override void AssertTransformationResults()
			{
				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var endTimeAsUtc = endTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = $"SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {taskCode1};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestStartTimeEndTimeBothSet()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var endTimeAsUtc = endTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = $"SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {taskCode1};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestStartTimeSetButEndTimeNotSet()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {taskCode2};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestEndTimeSetButStartTimeNotSet()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {taskCode3};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestStartTimeNotSetAndEndTimeNotSet()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_ServiceTaskCode, SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {taskCode4};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			const string taskCode1 = "111";
			const string taskCode2 = "222";
			const string taskCode3 = "333";
			const string taskCode4 = "444";
			DateTime startTime = new DateTime(2025, 1, 1, 12, 30, 00);
			DateTime endTime = new DateTime(2025, 1, 1, 16, 45, 00);
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class CopyScheduleTimesAsLocalConvertedToUtc : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCode1, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'S', 1, @StartTime, @EndTime, @NextRunTime, 0, '',
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 2', @TaskCode2, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'D', 1, @StartTime, @EndTime, @NextRunTime, 0, '',
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 3', @TaskCode3, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'D', 1, @StartTimeNull, @EndTimeNull, @NextRunTime, 0, '',
	0, null, 0, 0, null
",
				cmd =>
				{
					cmd.AddParameter("@TaskCode1", SqlDbType.VarChar, TaskCode1);
					cmd.AddParameter("@TaskCode2", SqlDbType.VarChar, TaskCode2);
					cmd.AddParameter("@TaskCode3", SqlDbType.VarChar, TaskCode3);
					cmd.AddParameter("@StartTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
					cmd.AddParameter("@StartTime", SqlDbType.SmallDateTime, startTime);
					cmd.AddParameter("@EndTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
					cmd.AddParameter("@EndTime", SqlDbType.SmallDateTime, endTime);
					cmd.AddParameter("@NextRunTime", SqlDbType.DateTime, nextRunTime);
					cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
					cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, new DateTime(2024, 1, 1, 0, 0, 0));
				});
			}
			protected override void AssertTransformationResults()
			{
				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var endTimeAsUtc = endTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCode1};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestStartTimeAndEndTimeAreConverted()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var endTimeAsUtc = endTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCode1};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"1\" StartTime=\"{startTimeAsUtc:HH:mm:ss}\" EndTime=\"{endTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestScheduledRunTimeIsConverted()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var startTimeAsUtc = startTime.Subtract(DateTimeOffset.Now.Offset);
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCode2};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"{startTimeAsUtc:HH:mm:ss}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			public void TestScheduleRunTimeDefaultsToNextRunTimeIfStartTimeIsMissing()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCode3};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"{nextRunTime:HH:mm:00}\" /></ScheduleConfig>", (string)dataRow["SST_Configuration"]);
				});
			}

			const string TaskCode1 = "111";
			const string TaskCode2 = "222";
			const string TaskCode3 = "333";
			DateTime startTime = new DateTime(2025, 1, 1, 12, 30, 0, DateTimeKind.Local);
			DateTime endTime = new DateTime(2025, 1, 1, 16, 45, 0, DateTimeKind.Local);
			DateTime nextRunTime = new DateTime(2025, 1, 1, 14, 14, 14, 14, DateTimeKind.Utc);
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class InsertRowsWhenS5_NextScheduledPrintRunTimeIsMinMaxDateTime : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				branchPk1 = Guid.NewGuid();
				branchPk2 = Guid.NewGuid();
				nextRunTimeMin = new DateTime(1753, 1, 1, 0, 0, 0);
				nextRunTimeMax = new DateTime(9999, 12, 31, 23, 59, 59);

				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.GlbCompany
	(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES
	(@CompanyPk, 'AU', 'AUD', @CompanyCode, 'AU company', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
	(@BranchPk1, @CompanyPk, @BranchCode1, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.GlbBranch
	(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES
	(@BranchPk2, @CompanyPk, @BranchCode2, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 1', @TaskCode1, '', @BranchPk1, 'D', 'SH', NULL, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @nextRunTime1, 1

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser, S5_NextScheduledPrintRunTimeUtc, S5_TaskPeriodCount)
SELECT NEWID(), 'dummy task 2', @TaskCode2, '', @BranchPk2, 'D', 'SH', NULL, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @nextRunTimeMax, 1
"
					, cmd =>
					{
						cmd.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
						cmd.AddParameter("@CompanyCode", SqlDbType.NVarChar, 3, "CCC");
						cmd.AddParameter("@BranchPk1", SqlDbType.UniqueIdentifier, branchPk1);
						cmd.AddParameter("@BranchPk2", SqlDbType.UniqueIdentifier, branchPk2);
						cmd.AddParameter("@BranchCode1", SqlDbType.NVarChar, 3, "BR1");
						cmd.AddParameter("@BranchCode2", SqlDbType.NVarChar, 3, "BR2");
						cmd.AddParameter("@TaskCode1", SqlDbType.NVarChar, 3, taskCode1);
						cmd.AddParameter("@TaskCode2", SqlDbType.NVarChar, 3, taskCode2);
						cmd.AddParameter("nextRunTime1", SqlDbType.DateTime, nextRunTimeMin);
						cmd.AddParameter("nextRunTimeMax", SqlDbType.DateTime, nextRunTimeMax);
					});
			}

			protected override void AssertTransformationResults()
			{
				const string sqlText = "SELECT SST_ServiceTaskCode, SST_NextRunTime, SST_GB_Branch, SST_Active, SST_Configuration FROM dbo.StmServiceTask;";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				var task1 = dataTable.Select($"SST_ServiceTaskCode='{taskCode1}'")[0];
				var task2 = dataTable.Select($"SST_ServiceTaskCode='{taskCode2}'")[0];

				AssertEquals(2, dataTable.Rows.Count);

				AssertEquals(branchPk1, task1["SST_GB_Branch"]);
				AssertEquals(true, task1["SST_Active"]);
				AssertEquals(new DateTimeOffset(nextRunTimeMin, TimeSpan.Zero), task1["SST_NextRunTime"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"00:00:00\" /></ScheduleConfig>", task1["SST_Configuration"]);

				AssertEquals(branchPk2, task2["SST_GB_Branch"]);
				AssertEquals(false, task2["SST_Active"]);
				AssertEquals(new DateTimeOffset(nextRunTimeMax, TimeSpan.Zero), task2["SST_NextRunTime"]);
				AssertEquals($"<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"23:59:00\" /></ScheduleConfig>", task2["SST_Configuration"]);
			}

			Guid branchPk1;
			Guid branchPk2;
			DateTime nextRunTimeMin;
			DateTime nextRunTimeMax;

			const string taskCode1 = "~T1";
			const string taskCode2 = "~T2";
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class WeeklySchedulePatternIsShiftedToMatchLocalTime : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeWeekly, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'W', 1, @StartTime, @EndTimeNull, @NextRunTime, 0, @DayList,
	0, null, 0, 0, null

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 2', @TaskCodeWeeklyNullStartTime, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	'W', 1, @StartTimeNull, @EndTimeNull, @NextRunTime, 0, @DayList,
	0, null, 0, 0, null
",
					cmd =>
					{
						cmd.AddParameter("@TaskCodeWeekly", SqlDbType.VarChar, TaskCodeWeekly);
						cmd.AddParameter("@TaskCodeWeeklyNullStartTime", SqlDbType.VarChar, TaskCodeWeeklyNullStartTime);
						cmd.AddParameter("@StartTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@StartTime", SqlDbType.SmallDateTime, dailyScheduledRunTime);
						cmd.AddParameter("@EndTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@NextRunTime", SqlDbType.DateTime, nextRunTime);
						cmd.AddParameter("@DayList", SqlDbType.VarChar, dayList);
						cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
						cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, new DateTime(2024, 1, 1, 0, 0, 0));
					});
			}

			protected override void AssertTransformationResults()
			{
				var dayOfWeek = DayOfWeek.Sunday;
				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedDayOfWeek = ((int)dayOfWeek + dateDiff) % 7;
				expectedDayOfWeek = expectedDayOfWeek == -1 ? 6 : expectedDayOfWeek;

				var expectedDayOfWeekText = ((DayOfWeek)expectedDayOfWeek).ToString();
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeekly};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence><DayOfWeek>{expectedDayOfWeekText}</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Sunday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Sunday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Monday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Monday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Tuesday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Tuesday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Wednesday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Wednesday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Thursday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Thursday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Friday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Friday);
			public void TestWeeklySchedule_DayOfWeekIsPreservedInLocalTime_Saturday() => AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek.Saturday);

			void AssertWeeklySchedule_DayOfWeekIsPreservedInLocalTime(DayOfWeek dayOfWeek)
			{
				dayList = dayOfWeek switch
				{
					DayOfWeek.Sunday => "YNNNNNN",
					DayOfWeek.Monday => "NYNNNNN",
					DayOfWeek.Tuesday => "NNYNNNN",
					DayOfWeek.Wednesday => "NNNYNNN",
					DayOfWeek.Thursday => "NNNNYNN",
					DayOfWeek.Friday => "NNNNNYN",
					DayOfWeek.Saturday => "NNNNNNY",
					_ => "YNNNNNN"
				};

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedDayOfWeek = ((int)dayOfWeek + dateDiff) % 7;
				expectedDayOfWeek = expectedDayOfWeek == -1 ? 6 : expectedDayOfWeek;

				var expectedDayOfWeekText = ((DayOfWeek)expectedDayOfWeek).ToString();
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeekly};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence><DayOfWeek>{expectedDayOfWeekText}</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_MultipleWeekDaysArePreservedInLocalTime()
			{
				dayList = "NYYYYYN";

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var daysOfWeekText = dateDiff switch
				{
					-1 => "<DayOfWeek>Sunday</DayOfWeek><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek>",
					0 => "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>",
					1 => "<DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek><DayOfWeek>Saturday</DayOfWeek>",
					_ => "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>"
				};

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeekly};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence>{daysOfWeekText}</DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_MissingScheduleTime_DefaultsToNextRunTime()
			{
				dayList = "NYYYYYN";
				nextRunTime = new DateTime(2025, 1, 1, 17, 0, 0, DateTimeKind.Utc);

				var scheduleRunTimeAsUtc = nextRunTime;
				var dateDiff = (scheduleRunTimeAsUtc.Date - scheduleRunTimeAsUtc.ToLocalTime().Date).Days;
				var daysOfWeekText = dateDiff switch
				{
					-1 => "<DayOfWeek>Sunday</DayOfWeek><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek>",
					0 => "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>",
					1 => "<DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek><DayOfWeek>Saturday</DayOfWeek>",
					_ => "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>"
				};

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_Configuration, SST_NextRunTime FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeeklyNullStartTime};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {nextRunTime.ToLocalTime()}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence>{daysOfWeekText}</DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Sunday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Sunday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Monday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Monday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Tuesday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Tuesday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Wednesday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Wednesday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Thursday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Thursday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Friday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Friday);
			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged_Saturday() => AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek.Saturday);

			void AssertWeeklySchedule_DayBoundaryIsNotCrossed_DayOfWeekIsUnchanged(DayOfWeek dayOfWeek)
			{
				dailyScheduledRunTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Local);
				dayList = dayOfWeek switch
				{
					DayOfWeek.Sunday => "YNNNNNN",
					DayOfWeek.Monday => "NYNNNNN",
					DayOfWeek.Tuesday => "NNYNNNN",
					DayOfWeek.Wednesday => "NNNYNNN",
					DayOfWeek.Thursday => "NNNNYNN",
					DayOfWeek.Friday => "NNNNNYN",
					DayOfWeek.Saturday => "NNNNNNY",
					_ => "YNNNNNN"
				};

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var dayOfWeekText = dayOfWeek.ToString();
				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeekly};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(0, dateDiff);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence><DayOfWeek>{dayOfWeekText}</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_MultipleWeekDaysUnchanged()
			{
				dailyScheduledRunTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Local);
				dayList = "NYYYYYN";

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var daysOfWeekText = "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>";

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeekly};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(0, dateDiff);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence>{daysOfWeekText}</DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestWeeklySchedule_DayBoundaryIsNotCrossed_MissingScheduleTime_DefaultsToNextRunTime()
			{
				nextRunTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
				dayList = "NYYYYYN";

				var scheduleRunTimeAsUtc = nextRunTime;
				var dateDiff = (scheduleRunTimeAsUtc.Date - scheduleRunTimeAsUtc.ToLocalTime().Date).Days;
				var daysOfWeekText = "<DayOfWeek>Monday</DayOfWeek><DayOfWeek>Tuesday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Friday</DayOfWeek>";

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = {TaskCodeWeeklyNullStartTime};";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(0, dateDiff);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {nextRunTime.ToLocalTime()}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"{scheduleRunTimeAsUtc:HH:mm:ss}\"><DaysOfOccurrence>{daysOfWeekText}</DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			const string TaskCodeWeekly = "111";
			const string TaskCodeWeeklyNullStartTime = "222";
			string dayList = "YNNNNNN";
			DateTime dailyScheduledRunTime = new DateTime(2025, 1, 1, 5, 30, 0, DateTimeKind.Local);
			DateTime nextRunTime = new DateTime(2025, 1, 1, 14, 14, 14, 14, DateTimeKind.Utc);
		}

		[TestedType(typeof(PopulateStmServiceTaskFromStmScheduleTask))]
		class MonthlyYearlyScheduledTimesTruncated : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new PopulateStmServiceTaskFromStmScheduleTask();
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask
DELETE from dbo.StmScheduleTask WHERE S5_ParentTableCode = 'SH'

INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeMonthsByDate, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	@WeekDayOccurrenceZero, @StartDate, @DayNumberNull, @MonthNumberNull, null


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeMonthsByLastDay, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	@WeekDayOccurrenceNull, @StartDateNull, @DayNumber99, @MonthNumberNull, null


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeMonthsByDayOfWeek, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeMonths, 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	@WeekDayOccurrence, @StartDateNull, @DayNumber1, @MonthNumberNull, null


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeYearsByDate, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeYears, 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	@WeekDayOccurrenceZero, @StartDate, @DayNumberNull, @MonthNumber, null


INSERT INTO dbo.StmScheduleTask
	(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_ParentTableCode, S5_ParentID,
		S5_IsActive, S5_SystemCreateTimeUtc, S5_SystemCreateUser, S5_SystemLastEditTimeUtc, S5_SystemLastEditUser,
		S5_TaskPeriod, S5_TaskPeriodCount, S5_DailyStartTime, S5_DailyEndTime, S5_NextScheduledPrintRunTimeUtc, S5_WeekDaysOnly, S5_DayList,
		S5_WeekDayOccurrenceNumber, S5_StartDate, S5_DayNumber, S5_MonthNumber, S5_ScheduleState)
SELECT
	NEWID(), 'dummy task 1', @TaskCodeYearsByDayOfMonth, '', NULL, 'SH', NULL, 1, @createTime, '~BP', @lastEditTime1_1, '~BP',
	@PeriodTypeYears, 1, @StartTime, @EndTimeNull, @NextRunTime, 0, '',
	@WeekDayOccurrence, @StartDateNull, @DayNumber1, @MonthNumber, null
",
					cmd =>
					{
						cmd.AddParameter("@TaskCodeWeekly", SqlDbType.VarChar, TaskCodeWeekly);
						cmd.AddParameter("@StartTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@StartTime", SqlDbType.SmallDateTime, dailyScheduledRunTime);
						cmd.AddParameter("@EndTimeNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@NextRunTime", SqlDbType.SmallDateTime, nextRunTime);
						cmd.AddParameter("@DayList", SqlDbType.VarChar, dayList);
						cmd.AddParameter("@createTime", SqlDbType.SmallDateTime, new DateTime(2021, 1, 1, 0, 0, 0));
						cmd.AddParameter("@lastEditTime1_1", SqlDbType.SmallDateTime, new DateTime(2024, 1, 1, 0, 0, 0));
						cmd.AddParameter("@TaskCodeMonthsByDate", SqlDbType.VarChar, taskCodeMonthsByDate);
						cmd.AddParameter("@TaskCodeMonthsByLastDay", SqlDbType.VarChar, taskCodeMonthsByLastDay);
						cmd.AddParameter("@TaskCodeMonthsByDayOfWeek", SqlDbType.VarChar, taskCodeMonthsByDayOfWeek);
						cmd.AddParameter("@TaskCodeYearsByDate", SqlDbType.VarChar, taskCodeYearsByDate);
						cmd.AddParameter("@TaskCodeYearsByDayOfMonth", SqlDbType.VarChar, taskCodeYearsByDayOfMonth);
						cmd.AddParameter("@PeriodTypeMonths", SqlDbType.VarChar, "M");
						cmd.AddParameter("@PeriodTypeYears", SqlDbType.VarChar, "Y");
						cmd.AddParameter("@WeekDayOccurrenceNull", SqlDbType.Int, 0);
						cmd.AddParameter("@WeekDayOccurrenceZero", SqlDbType.Int, 0);
						cmd.AddParameter("@WeekDayOccurrence", SqlDbType.Int, weekDayOccurrence);
						cmd.AddParameter("@StartDateNull", SqlDbType.SmallDateTime, DBNull.Value);
						cmd.AddParameter("@StartDate", SqlDbType.SmallDateTime, startDate);
						cmd.AddParameter("@DayNumberNull", SqlDbType.Int, 0);
						cmd.AddParameter("@DayNumber1", SqlDbType.Int, 1);
						cmd.AddParameter("@DayNumber4", SqlDbType.Int, 4);
						cmd.AddParameter("@DayNumber7", SqlDbType.Int, 7);
						cmd.AddParameter("@DayNumber99", SqlDbType.Int, 99);
						cmd.AddParameter("@MonthNumberNull", SqlDbType.Int, 0);
						cmd.AddParameter("@MonthNumber", SqlDbType.Int, monthNumber);
					});
			}

			protected override void AssertTransformationResults()
			{
			}

			public void TestMonthsByLastDay_UtcTimeIsTruncated()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = dateDiff switch
				{
					-1 => $"{dailyScheduledRunTime:00:00:00}",
					0 => $"{scheduleRunTimeAsUtc:HH:mm:00}",
					1 => $"{dailyScheduledRunTime:23:59:00}",
					_ => $"{scheduleRunTimeAsUtc:HH:mm:00}"
				};

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeMonthsByLastDay}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"1\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestMonthsByDate_UtcTimeIsTruncated()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = dateDiff switch
				{
					-1 => $"{dailyScheduledRunTime:00:00:00}",
					0 => $"{scheduleRunTimeAsUtc:HH:mm:00}",
					1 => $"{dailyScheduledRunTime:23:59:00}",
					_ => $"{scheduleRunTimeAsUtc:HH:mm:00}"
				};

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeMonthsByDate}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"{startDate.Day}\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestMonthsByDayOfWeek_UtcTimeIsTruncated()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = dateDiff switch
				{
					-1 => $"{dailyScheduledRunTime:00:00:00}",
					0 => $"{scheduleRunTimeAsUtc:HH:mm:00}",
					1 => $"{dailyScheduledRunTime:23:59:00}",
					_ => $"{scheduleRunTimeAsUtc:HH:mm:00}"
				};

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeMonthsByDayOfWeek}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=\"{weekDayOccurrence}\" DayOfTheWeek=\"{(DayOfWeek)1 - 1}\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestMonthlySchedule_DayBoundaryIsNotCrossed_UtcTimeIsFullyConverted()
			{
				dailyScheduledRunTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Local);

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = $"{scheduleRunTimeAsUtc:HH:mm:00}";

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeMonthsByLastDay}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(0, dateDiff);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"1\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestYearsByDate_UtcTimeIsTruncated()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = dateDiff switch
				{
					-1 => $"{dailyScheduledRunTime:00:00:00}",
					0 => $"{scheduleRunTimeAsUtc:HH:mm:00}",
					1 => $"{dailyScheduledRunTime:23:59:00}",
					_ => $"{scheduleRunTimeAsUtc:HH:mm:00}"
				};

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeYearsByDate}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"{monthNumber}\" Day=\"{startDate.Day}\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestYearsByDayOfMonth_UtcTimeIsTruncated()
			{
				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = dateDiff switch
				{
					-1 => $"{dailyScheduledRunTime:00:00:00}",
					0 => $"{scheduleRunTimeAsUtc:HH:mm:00}",
					1 => $"{dailyScheduledRunTime:23:59:00}",
					_ => $"{scheduleRunTimeAsUtc:HH:mm:00}"
				};

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeYearsByDayOfMonth}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth DayOfTheWeek=\"{(DayOfWeek)1 - 1}\" WeekOfTheMonth=\"{weekDayOccurrence}\" MonthOfTheYear=\"{monthNumber}\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			public void TestYearlySchedule_DayBoundaryIsNotCrossed_UtcTimeIsFullyConverted()
			{
				dailyScheduledRunTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Local);

				PrepareTestData();

				GetNewTestTransformationInstance().Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);

				var scheduleRunTimeAsUtc = dailyScheduledRunTime.Subtract(DateTimeOffset.Now.Offset);
				var dateDiff = (scheduleRunTimeAsUtc.Date - dailyScheduledRunTime.Date).Days;
				var expectedScheduleTimeText = $"{scheduleRunTimeAsUtc:HH:mm:00}";

				var sqlText = $"SELECT SST_Configuration FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = '{taskCodeYearsByDate}';";

				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
				var dataRow = dataTable.Rows[0];

				CombineAssertions(() =>
				{
					AssertEquals(1, dataTable.Rows.Count);
					AssertEquals(0, dateDiff);
					AssertEquals(
						$"TimeZone: {DateTimeOffset.Now.Offset.Hours}, original time: {dailyScheduledRunTime}, converted time: {scheduleRunTimeAsUtc}, difference: {dateDiff}",
						$"<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"{monthNumber}\" Day=\"{startDate.Day}\" ScheduledRunTime=\"{expectedScheduleTimeText}\" /></ScheduleConfig>",
						(string)dataRow["SST_Configuration"]);
				});
			}

			const string TaskCodeWeekly = "111";
			string dayList = "YNNNNNN";
			DateTime dailyScheduledRunTime = new DateTime(2025, 1, 1, 5, 30, 0, DateTimeKind.Local);
			DateTime nextRunTime = new DateTime(2025, 1, 1, 14, 14, 14, 14, DateTimeKind.Utc);
			int weekDayOccurrence = 2;
			DateTime startDate = new DateTime(2024, 1, 1, 0, 0, 0);
			int monthNumber = 2;

			const string taskCodeMonthsByDate = "~TA";
			const string taskCodeMonthsByLastDay = "~TB";
			const string taskCodeMonthsByDayOfWeek = "~TC";
			const string taskCodeYearsByDate = "~TD";
			const string taskCodeYearsByDayOfMonth = "~TE";
		}
	}
}
