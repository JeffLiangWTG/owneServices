using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.PAVE.MENT.Business.Test;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.PAVE.MENT.Business.ServiceTasks.Test
{
	[TestedType(typeof(MENTDataPurgeServiceTask))]
	public class MENTDataPurgeServiceTaskTestCaseTest : ServiceTaskTestCase<MENTDataPurgeServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, MENTDataPurgeServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, MENTDataPurgeServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", MENTDataPurgeServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", MENTDataPurgeServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion
	}

	[UseSnapshotProtection]
	public class MENTDataPurgeServiceTaskTest : NonTransactionedTestCase
	{
		#region Task Config

		public void TestHostedServiceAttribute()
		{
			var attributes = typeof(MENTDataPurgeServiceTask).Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var attribute = Array.Find((HostedServiceAttribute[])attributes, a => a.TypeName == typeof(MENTDataPurgeServiceTask).FullName);
			AssertEquals("Minimum Period should be 15 minutes", "15minutes", attribute?.MinimumPeriod);
			AssertEquals("Should be able to run in any branch since any uses of CurrentBranch and CurrentDepartment will cause inconsistent behaviour, so should be carefully analysed.", true, attribute?.CanRunInAnyBranch);
		}

		public void TestBatchLimit()
		{
			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			AssertEquals(@"This value was specially chosen as a suitable batch limit! Before changing, please:
 - Profile your proposed change
 - Test it against the current batch limit of 100,000
 - Seriously question your actions
", 100000, task.BatchLimit_ForTest);
		}

		#endregion

		#region Run Task

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunServiceTask_BothPurgeSettings()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "May", save: false);
			query.MAQ_PurgeDays = 1;
			query.MAQ_PurgeAllButLatestQuantity = 3;

			var query2 = MENTTestHelper.CreateQuery(Factory, "July", save: false);
			query2.MAQ_PurgeDays = 365;
			query2.MAQ_PurgeAllButLatestQuantity = 2;

			Factory.Save();

			MENTTestHelper.InsertTenTestMetrics(query);
			MENTTestHelper.InsertTenTestMetrics(query2);

			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			task.Run();

			AssertEquals(12, GetMentAgedScoreMetricTotalRowCount());
			AssertContainsExactLinesInAnyOrder("Service task log", @"BMP|MENT Data Purge: Purged 4 records from July by PurgeAllButLatest Quantity.
BMP|MENT Data Purge: Purged 2 records from May by PurgeAllButLatest Quantity.
BMP|MENT Data Purge: Purged 0 records from July by Purge Days.
BMP|MENT Data Purge: Purged 2 records from May by Purge Days.
", task.ServiceLogger.ToString());
		}

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunServiceTask_PurgeDaysNotSet()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "May", save: false);
			query.MAQ_PurgeDays = 0;
			query.MAQ_PurgeAllButLatestQuantity = 3;

			Factory.Save();

			MENTTestHelper.InsertTenTestMetrics(query);

			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			task.Run();

			AssertEquals(8, GetMentAgedScoreMetricTotalRowCount());
			AssertContainsExactLinesInAnyOrder("Service task log", "BMP|MENT Data Purge: Purged 2 records from May by PurgeAllButLatest Quantity.\r\n", task.ServiceLogger.ToString());
		}

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunServiceTask_PurgeAllButLatestQuantityNotSet()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "May", save: false);
			query.MAQ_PurgeDays = 2;
			query.MAQ_PurgeAllButLatestQuantity = 0;

			Factory.Save();

			MENTTestHelper.InsertTenTestMetrics(query);

			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			task.Run();

			AssertEquals(8, GetMentAgedScoreMetricTotalRowCount());
			AssertContainsExactLinesInAnyOrder("Service task log", "BMP|MENT Data Purge: Purged 2 records from May by Purge Days.\r\n", task.ServiceLogger.ToString());
		}

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunServiceTask_PurgeNotConfigured_ShouldNotRemoveData()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "May", save: false);
			query.MAQ_PurgeDays = 0;
			query.MAQ_PurgeAllButLatestQuantity = 0;

			Factory.Save();

			MENTTestHelper.InsertTenTestMetrics(query);

			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			task.Run();

			AssertEquals(10, GetMentAgedScoreMetricTotalRowCount());
		}

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunServiceTask_NoQueriesToPurge_ShouldNotRemoveData()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "May", save: false);
			query.MAQ_PurgeDays = 0;
			query.MAQ_PurgeAllButLatestQuantity = 0;

			var query2 = MENTTestHelper.CreateQuery(Factory, "July", save: false);
			query2.MAQ_PurgeDays = 0;
			query2.MAQ_PurgeAllButLatestQuantity = 0;

			Factory.Save();

			MENTTestHelper.InsertTenTestMetrics(query);
			MENTTestHelper.InsertTenTestMetrics(query2);

			var task = new MENTTestHelper.MENTDataPurgeServiceTaskForTest();
			task.Run();

			AssertEquals(20, GetMentAgedScoreMetricTotalRowCount());
			AssertContainsExactLinesInAnyOrder("Service task log", "BMP|MENT Data Purge completed run with no purge-ready records, of 0 records found.\r\n", task.ServiceLogger.ToString());
		}

		#endregion

		#region Performance

		[TestDate(2019, 5, 10, 16, 0, 0)]
		public void TestRunTask_ShouldImplementBatching()
		{
			const int queriesToMake = 12;
			const int customBatchSize = 10;
			const string queryName = "May";

			var expectedLogsAndCurrentDayQueries = MENTTestHelper.CreateManyTestMetricsAndGetExpectedLogs(Factory, queryName, queriesToMake, purgeDays: 1, purgeAllBut: 1);

			Factory.Save();

			var batchCount = 1;
			var totalQueries = queriesToMake * customBatchSize;
			var task = new MENTTestHelper.MENTDataPurgeServiceTaskWithPerBatchActionForTest(
				batchLimit: customBatchSize,
				(lastBatchProcessedCount) =>
				{
					var expected = totalQueries - lastBatchProcessedCount;
					AssertNumberOfAgedQueriesToPurge($"We should have {expected} more queries to delete on batch number {batchCount}", expected);
					batchCount++;
					totalQueries -= lastBatchProcessedCount;
				});

			AssertNumberOfAgedQueriesToPurge("We should have no more queries left to purge.", queriesToWantLeftOver: totalQueries);

			task.Run();

			int queriesRemaining = expectedLogsAndCurrentDayQueries.currentDayQueries; // create two never-purge metrics per query
			AssertNumberOfAgedQueriesToPurge("We should have no more queries left to purge.", queriesToWantLeftOver: queriesRemaining);
			AssertContainsExactLinesInAnyOrder("Service task log", expectedLogsAndCurrentDayQueries.expectedLog, task.ServiceLogger.ToString());
		}

		void AssertNumberOfAgedQueriesToPurge(string message, int queriesToWantLeftOver)
		{
			var remainingQueries = GetMentAgedScoreMetricTotalRowCount();
			AssertEquals(message, queriesToWantLeftOver, remainingQueries);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			MENTTestHelper.ClearMENTTables();
			base.SetUp();
		}

		static int GetMentAgedScoreMetricTotalRowCount() => (int)Db.Connection.ExecuteScalar(MENTTestHelper.MENTTotalRowCountSql);

		#endregion
	}
}
