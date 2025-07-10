using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.PAVE.MENT.Business.ServiceTasks.Test
{
	// This is a required test case
	[TestedType(typeof(AgedScoresServiceTask))]
	public class AgedScoresServiceTaskTestCaseTest : ServiceTaskTestCase<AgedScoresServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}

	// Unable to inherit from ServiceTaskTestCase because I want snapshot protection
	[UseSnapshotProtection]
	public class AgedScoresServiceTaskTest : TestCase
	{
		public void TestHostedServiceAttribute()
		{
			var attributes = typeof(AgedScoresServiceTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(AgedScoresServiceTask).FullName);
			AssertEquals("Minimum Period should be 15 minutes", "15minutes", attribute?.MinimumPeriod);
			AssertEquals("Should be able to run in any branch since any uses of CurrentBranch and CurrentDepartment will cause inconsistent behaviour, so should be carefully analysed.", true, attribute?.CanRunInAnyBranch);
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunServiceTaskSqlWithInvalidCast()
		{
			var sql = @"select 1, 1, 1, GETDATE() as attributeValue from dbo.MENTAgedScoreMetric"; // Will throw a sql exception
			var ancientExpiredQuery = MENTTestHelper.CreateQuery(Factory, "QUEERY", ZDateTime.Now.AddDays(-1), sql);
			AssertEquals(0, ancientExpiredQuery.Notes.DatabaseCount);

			//These won't be run but the first query will attemp to give us details about them
			MENTTestHelper.CreateQuery(Factory, "ANOTHER", ZDateTime.Now.AddDays(20), sql);
			MENTTestHelper.CreateQuery(Factory, "CAAAAAR", ZDateTime.Now.AddDays(20), sql);

			Factory?.Save();

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(0, GetMentAgedScoreMetricTotalRowCount());

			var newFactory = new BusinessObjectFactory();
			var loadedQuery = newFactory.Load<MENTAgedScoreQuery>(ancientExpiredQuery.PK);
			AssertEquals(true, loadedQuery.MAQ_IsFaulty);
			AssertEquals(false, loadedQuery.MAQ_IsActive);
			AssertEquals(3, loadedQuery.Notes.DatabaseCount);

			AssertMultilineASCIIEquals("Service task log", @"Error - All queries run. A query was marked faulty after the maximum number of attempts. Log Messages: 
On attempt 1: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
No column name was specified for column 1 of 'AgedScoreMetricToInsert'.
No column name was specified for column 2 of 'AgedScoreMetricToInsert'.
No column name was specified for column 3 of 'AgedScoreMetricToInsert'.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 1, 1, 1, GETDATE() as attributeValue from dbo.MENTAgedScoreMetric

On attempt 2: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
No column name was specified for column 1 of 'AgedScoreMetricToInsert'.
No column name was specified for column 2 of 'AgedScoreMetricToInsert'.
No column name was specified for column 3 of 'AgedScoreMetricToInsert'.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 1, 1, 1, GETDATE() as attributeValue from dbo.MENTAgedScoreMetric

On attempt 3: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
User Defined Statement is Invalid.
No column name was specified for column 1 of 'AgedScoreMetricToInsert'.
No column name was specified for column 2 of 'AgedScoreMetricToInsert'.
No column name was specified for column 3 of 'AgedScoreMetricToInsert'.
Error Inserting QUEERY into MENTAgedScoreMetric.
The Sql being run was: select 1, 1, 1, GETDATE() as attributeValue from dbo.MENTAgedScoreMetric", task.ServiceLogger.ToString());
		}

		[TestDate(2019, 08, 26, 10, 0, 0)]
		public void TestRunServiceTaskSqlWithInvalidCastInRelatedAcceptabilityBandQuery()
		{
			var band = Factory!.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "Jazz Band";
			band.BAB_Type = "SQL";
			band.BAB_SqlText = "select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component";

			var ancientExpiredQuery = MENTTestHelper.CreateQuery(Factory, "QUEERY", ZDateTime.Now.AddDays(-1));
			ancientExpiredQuery.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			AssertEquals(0, ancientExpiredQuery.Notes.DatabaseCount);

			//These won't be run but the first query will attemp to give us details about them
			MENTTestHelper.CreateQuery(Factory, "ANOTHER", ZDateTime.Now.AddDays(20));
			MENTTestHelper.CreateQuery(Factory, "CAAAAAR", ZDateTime.Now.AddDays(20));

			Factory?.Save();

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(0, GetMentAgedScoreMetricTotalRowCount());

			var newFactory = new BusinessObjectFactory();
			var loadedQuery = newFactory.Load<MENTAgedScoreQuery>(ancientExpiredQuery.PK);
			AssertEquals(true, loadedQuery.MAQ_IsFaulty);
			AssertEquals(false, loadedQuery.MAQ_IsActive);
			AssertEquals(0, loadedQuery.Notes.DatabaseCount);

			AssertMultilineASCIIEquals("Service task log", @"Error - All queries run. A query was marked faulty after the maximum number of attempts. Log Messages: 
On attempt 1: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component

On attempt 2: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component

On attempt 3: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
Insert of BAB data into temp table failed
Error Inserting QUEERY into MENTAgedScoreMetric.
Related Acceptability Band: Jazz Band.
The Acceptability Band Sql being run was: select 1 as Value, 'AAAAA' as ReleaseGroup, null as Component", task.ServiceLogger.ToString());
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunServiceTaskNoRowsInserted()
		{
			var sql = MENTTestHelper.CreateInsertSql(attributeValue: MENTAgedScoreMetricSchema.Constants.MAS_MAQ_NKCode, table: MENTAgedScoreMetricSchema.Constants.TableName);
			var queryToRun = MENTTestHelper.CreateQuery(Factory, "QUEERY", ZDateTime.Now.AddDays(-1), sql);

			//These won't be run but will be reported on.
			var query1 = MENTTestHelper.CreateQuery(Factory, "ANOTHER", ZDateTime.Now.AddDays(20), sql);
			var query2 = MENTTestHelper.CreateQuery(Factory, "CAAAAAR", ZDateTime.Now.AddDays(20), sql);

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(0, GetMentAgedScoreMetricTotalRowCount());

			AssertMultilineASCIIEquals("Service task log", @"All queries run successfully. Log Messages: 
On attempt 1: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", task.ServiceLogger.ToString());
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunServiceTask()
		{
			var sql = MENTTestHelper.CreateInsertSql(score: "69", attributeValue: MENTAgedScoreQuerySchema.Constants.MAQ_Code);
			var ancientExpiredQuery = MENTTestHelper.CreateQuery(Factory, "QUEERY", ZDateTime.Now.AddDays(-1), sql);

			//These won't be run but will be reported on.
			var query1 = MENTTestHelper.CreateQuery(Factory, "ANOTHER", ZDateTime.Now.AddDays(20), sql);
			var query2 = MENTTestHelper.CreateQuery(Factory, "CAAAAAR", ZDateTime.Now.AddDays(20), sql);

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(3, GetMentAgedScoreMetricTotalRowCount());
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, ancientExpiredQuery.MAQ_Code);
			AssertEquals(3, (int)Db.Connection.ExecuteScalar(rowCountSql));

			var sumOfScoresSql = string.Format(MENTTestHelper.MENTSumForNameSql, ancientExpiredQuery.MAQ_Code);
			AssertEquals(207.0m, (decimal)Db.Connection.ExecuteScalar(sumOfScoresSql));

			AssertMultilineASCIIEquals("Service task log", @"All queries run successfully. Log Messages: 
On attempt 1: 
Starting Insert of QUEERY into MENTAgedScoreMetric.
Finished Insert of QUEERY into MENTAgedScoreMetric.", task.ServiceLogger.ToString());
		}

		[TestDate(2019, 5, 15, 10, 0, 0)]
		public void TestRunServiceTask_CorrectlyDropsTempTables_BeforeAndAfterEachRun()
		{
			using (var extraConnectionToPreventTempTableAutoDrop = Db.NewExtraConnectionToMainDbWithReaderCredentials())
			{
				var tempTableSql = "CREATE Table ##tempmentdataQUEERY (myfield int)";
				extraConnectionToPreventTempTableAutoDrop.ExecuteNonQuery(tempTableSql);

				var sql = MENTTestHelper.CreateInsertSql(score: "69", attributeValue: MENTAgedScoreQuerySchema.Constants.MAQ_Code);
				var ancientExpiredQuery = MENTTestHelper.CreateQuery(Factory, "QUEERY", ZDateTime.Now.AddDays(-1), sql);
				var task = new AgedScoresServiceTaskForTest();
				task.Run();

				sql = "select count(*) from tempdb.sys.objects where name like '##tempmentscore%'";
				var tempTablesCount = extraConnectionToPreventTempTableAutoDrop.ExecuteScalar(sql);
				AssertEquals(0, tempTablesCount);
			}
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestGetAndOrderActiveAgedScoreQueries()
		{
			var sql = MENTTestHelper.CreateInsertSql(score: "count(*)", attributeValue: "'hi'", table: GlbStaffSchema.Constants.TableName);
			var ancientExpiredQuery = MENTTestHelper.CreateQuery(Factory, "ANCEXP", ZDateTime.Now.AddDays(-200));
			ancientExpiredQuery.MAQ_SqlText = sql;
			var soonToExpireQuery = MENTTestHelper.CreateQuery(Factory, "EXP", ZDateTime.Now.AddMinutes(10), sql);
			var futureQuery = MENTTestHelper.CreateQuery(Factory, "FUT", ZDateTime.Now.AddDays(20), sql);

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(1, GetMentAgedScoreMetricTotalRowCount());
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, ancientExpiredQuery.MAQ_Code);
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(rowCountSql));

			AssertMultilineASCIIEquals("Service task log", @"All queries run successfully. Log Messages: 
On attempt 1: 
Starting Insert of ANCEXP into MENTAgedScoreMetric.
Finished Insert of ANCEXP into MENTAgedScoreMetric.", task.ServiceLogger.ToString());
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestGetAndOrderActiveQueryScheduleAgedScoreQueries()
		{
			var sql = MENTTestHelper.CreateInsertSql(score: "count(*)", attributeValue: "'hi'", table: GlbStaffSchema.Constants.TableName);
			var query1 = MENTTestHelper.CreateQuery(Factory, "ANCEXP", ZDateTime.Now.AddDays(-200), sql);
			var query2 = MENTTestHelper.CreateQuery(Factory, "EXP", ZDateTime.Now.AddDays(-200), sql);
			query2.QuerySchedule.S5_IsActive = false;

			Factory?.Save();

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(1, GetMentAgedScoreMetricTotalRowCount());
			var rowCountSql = string.Format(MENTTestHelper.MENTRowCountForNameSql, query1.MAQ_Code);
			AssertEquals(1, (int)Db.Connection.ExecuteScalar(rowCountSql));

			AssertMultilineASCIIEquals("Service task log", @"All queries run successfully. Log Messages: 
On attempt 1: 
Starting Insert of ANCEXP into MENTAgedScoreMetric.
Finished Insert of ANCEXP into MENTAgedScoreMetric.", task.ServiceLogger.ToString());
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestRunMultipleQueries()
		{
			var query1 = MENTTestHelper.CreateQuery(Factory, "ONE", nextRunTime: ZDateTime.Now.AddDays(-200));
			query1.MAQ_SqlText = MENTTestHelper.CreateInsertSql(attributeValue: string.Format("'{0}' + MAQ_Code", query1.PK.ToString().Substring(0, 3)));
			var query2 = MENTTestHelper.CreateQuery(Factory, "TWO", nextRunTime: ZDateTime.Now.AddDays(-200));
			query2.MAQ_SqlText = MENTTestHelper.CreateInsertSql(attributeValue: string.Format("'{0}' + MAQ_Code", query2.PK.ToString().Substring(0, 3)));
			var query3 = MENTTestHelper.CreateQuery(Factory, "THREE", nextRunTime: ZDateTime.Now.AddDays(-200));
			query3.MAQ_SqlText = MENTTestHelper.CreateInsertSql(attributeValue: string.Format("'{0}' + MAQ_Code", query3.PK.ToString().Substring(0, 3)));

			Factory?.Save();

			var task = new AgedScoresServiceTaskForTest();
			task.Run();

			AssertEquals(9, GetMentAgedScoreMetricTotalRowCount());

			AssertContainsExactLinesInAnyOrder("Service task log", @"All queries run successfully. Log Messages: 

On attempt 1: 
Starting Insert of ONE into MENTAgedScoreMetric.
Finished Insert of ONE into MENTAgedScoreMetric.


On attempt 1: 
Starting Insert of TWO into MENTAgedScoreMetric.
Finished Insert of TWO into MENTAgedScoreMetric.

On attempt 1: 
Starting Insert of THREE into MENTAgedScoreMetric.
Finished Insert of THREE into MENTAgedScoreMetric.", task.ServiceLogger.ToString());
		}

		[TestDate(2014, 1, 20, 10, 0, 0)]
		public void TestInserterUsesUTC()
		{
			var query1 = MENTTestHelper.CreateQuery(Factory, "ONE", nextRunTime: ZDateTime.Now.AddDays(-200));
			query1.MAQ_SqlText = MENTTestHelper.CreateInsertSql(attributeValue: string.Format("'{0}'", query1.PK.ToString().Substring(0, 3)));

			Factory?.Save();

			new AgedScoresServiceTaskForTest().Run();

			var sb = new StringBuilder(MENTTestHelper.MENTTotalRowCountSql);
			sb.Append(" WHERE MAS_TimeRecordedUtc > GETUTCDATE() - (1.0/48.0) AND MAS_TimeRecordedUtc < GETUTCDATE() + (1.0/48.0)");

			AssertEquals(1, (int)Db.Connection.ExecuteScalar(sb.ToString()));
		}

		[TestDate(2024, 7, 3)]
		public void TestShouldExecuteQueriesWhichCanBeExecutedOnly()
		{
			AssertEquals("Precondition", 0, GetMentAgedScoreMetricTotalRowCount());

			BMSTestHelper.CreateWorkflowAndTask(Factory, "Some workflow to generate metrics data");

			var queryThatAllowsExecution1 = CreateQueryWithBand("GoodBoy1", AcceptabilityBandTypes.Codes.Count);
			Factory?.Save();

			new AgedScoresServiceTaskForTest().Run();
			var intialRowCount = GetMentAgedScoreMetricTotalRowCount();
			Assert("Should add some rows", intialRowCount != 0);

			new AgedScoresServiceTaskForTest().Run();
			AssertEquals("Should still contain results of the previous execution - should not execute again for the query which does not allow execution", intialRowCount, GetMentAgedScoreMetricTotalRowCount());

			var queryThatAllowsExecution2 = CreateQueryWithBand("GoodBoy2", AcceptabilityBandTypes.Codes.Count);
			Factory?.Save();

			new AgedScoresServiceTaskForTest().Run();
			Assert("Should increase the number of rows", GetMentAgedScoreMetricTotalRowCount() > intialRowCount);
		}

		[TestDate(2019, 5, 14, 10, 0, 0)]
		public void TestAcceptabilityBandUsesFixedTableName_DoesNotThrow()
		{
			var buffer = BMSTestHelper.CreateBuffer(BMSTestHelper.CreateSystem(Factory, "ORG"));

			var queryToRun = Factory!.NewWithValidTestData<MENTAgedScoreQuery>();
			var queryToRun2 = Factory.NewWithValidTestData<MENTAgedScoreQuery>();

			queryToRun.MAQ_Code = "Q1";
			queryToRun.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun.MAQ_SqlText = "select 10 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";
			queryToRun2.MAQ_Code = "Q2";
			queryToRun2.QuerySchedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddDays(-1);
			queryToRun2.MAQ_SqlText = "select 10 as score, null releaseGroup, null component, MAQ_Code as attributeValue, 'XAL' as staff from dbo.MENTAgedScoreQuery";

			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();
			band.BAB_Type = AcceptabilityBandTypes.Codes.SQL;
			band.BAB_SqlText = $"select 100 as value, null releaseGroup, '{buffer.PK.ToString()}' component from dbo.MENTAgedScoreQuery";
			band.BAB_FC_Component = buffer.PK;

			queryToRun.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			queryToRun2.MAQ_BAB_RelatedAcceptabilityBand = band.PK;

			Factory?.Save();

			AssertNoExceptionThrown(() => new AgedScoresServiceTaskForTest().Run());
		}

		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, AgedScoresServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, AgedScoresServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", AgedScoresServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", AgedScoresServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
			Factory = new BusinessObjectFactory();
			BMSTestHelper.EnableBMSInRegistry();
			Factory?.Save();
		}

		BusinessObjectFactory? Factory { get; set; }

		static int GetMentAgedScoreMetricTotalRowCount() => (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql);

		MENTAgedScoreQuery CreateQueryWithBand(string queryName, string bandType)
		{
			var query = MENTTestHelper.CreateQuery(Factory, queryName, nextRunTime: ZDateTime.UtcNow);
			var band = Factory!.NewWithValidTestData<BMComponentAcceptabilityBand>();
			query.MAQ_BAB_RelatedAcceptabilityBand = band.PK;
			band.BAB_Type = bandType;
			return query;
		}

		#endregion
	}
}
