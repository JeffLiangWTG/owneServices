using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class BoardAgingTest : BoardAgingTestBase
	{
		[RequiresSTA]
		public void TestBoardAging_NonNetworkItem()
		{
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "task1", estVariationFactor: 1);
			AssertTasksInCell(new[] { task }, 10, 2, new[] { task });
		}

		public void TestBoardAging_NonNetworkItem_PartiallyCompleted_AsMuchProgressAsTimeHasElapsed()
		{
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 960, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 2 days = the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining

			AssertTasksInCell(new[] { task2 }, 12, 2, new[] { task2 }); // Should be no buffer consumption as the total amount of work done is the same as the time since release
		}

		[RequiresSTA]
		public void TestBoardAging_NonNetworkItem_PartiallyCompleted_LessProgressThanTimeHasElapsed()
		{
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 1 days = half the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining

			AssertTasksInCell(new[] { task2 }, 11, 2, new[] { task2 }); // Should be 1 day buffer consumption as the total amount of work done is 1 day less than the time since release
		}

		[RequiresSTA]
		public void TestBoardAging_NonNetworkItem_PartiallyCompleted_LessProgressThanTimeHasElapsed_ButWithEstToComplete()
		{
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 1 days = half the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			task2.P9_EstimatedTimeToComplete = new ZInt(10).GetDateTimeFromMinutes();

			AssertTasksInCell(new[] { task2 }, 12, 2, new[] { task2 }); // Should be no buffer consumption as the work remaining is negligible, and we're within the original estimate
		}

		[RequiresSTA]
		public void TestBoardAging_BufferPenetrationForJobWorkflow()
		{
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1_1", estVariationFactor: 2, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1_2", estVariationFactor: 3);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 80, description: "task1_3", estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 90, description: "task1_4", estVariationFactor: 1);

			//have workflows in different Zones now
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");
			var workflow_zone3 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow_zone3", config.Buffer, ZDateTime.UtcNow);
			var workflow_zone2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow_zone2", config.Buffer, ZDateTime.UtcNow.AddDays(-9));
			var workflow_zone1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow_zone1", config.Buffer, ZDateTime.UtcNow.AddDays(-13));
			var workflow_zone0 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow_zone0", config.Buffer, ZDateTime.UtcNow.AddDays(-17));

			BMSTestHelper.CreateTask(workflow_zone3, description: "task2_3");
			BMSTestHelper.CreateTask(workflow_zone2, description: "task2_2");
			BMSTestHelper.CreateTask(workflow_zone1, description: "task2_1");
			BMSTestHelper.CreateTask(workflow_zone0, description: "task2_0");

			//another job level workflow with the workflow in Zone 1
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader3");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow_zone 1 for jobHeader3", config.Buffer, ZDateTime.Now.AddDays(-13));
			BMSTestHelper.CreateTask(workflow3, description: "task3");

			//another job level workflow with the workflow in Zone 2
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader4");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow_zone 2 for jobHeader4", config.Buffer, ZDateTime.Now.AddDays(-9));
			BMSTestHelper.CreateTask(workflow4, description: "task4");

			CombineAssertions("Precondition: workflows and their respective buffer zones", () =>
			{
				AssertEquals("workflow1", 3, workflow.BufferZone.Value);
				AssertEquals("workflow2", 3, workflow2.BufferZone.Value);

				AssertEquals("workflow_zone3", 3, workflow_zone3.BufferZone);
				AssertEquals("workflow_zone2", 2, workflow_zone2.BufferZone);
				AssertEquals("workflow_zone1", 1, workflow_zone1.BufferZone);
				AssertEquals("workflow_zone0", 0, workflow_zone0.BufferZone);

				AssertEquals("workflow3", 1, workflow3.BufferZone);
				AssertEquals("workflow4", 2, workflow4.BufferZone);
			});

			Factory.Save();

			var allJobs = new[] { jobHeader, jobHeader2, jobHeader3, jobHeader4 };

			AssertTasksInCell("jobHeader should be in Zone 3 - all of its workflows are in Zone 3.", allJobs, 12, 2, jobHeader);
			AssertTasksInCell("jobHeader4 should be in Zone 2 - all of its workflows are in Zone 2.", allJobs, 5, 2, jobHeader4);
			AssertTasksInCell("jobHeader3 should be in Zone 1 - all of its workflows are in Zone 1.", allJobs, 3, 2, jobHeader3);
			AssertTasksInCell("jobHeader2 should be in Zone 0 - highest penetration from four of its workflows.", allJobs, 0, 2, jobHeader2);
		}
	}

	[TestDate(2014, 7, 16)]
	public abstract class BoardAgingTestBase : BMSTestCaseWithFactory
	{
		protected void AssertTasksInCell<T>(T[] allTasksOrWorkflowsInSection, int row, int col, params T[] expectedTasksOrWorkflowsInCell)
			where T : BusinessObject, ITagable // Because workflows and tasks are both tagable...
		{
			AssertTasksInCell(string.Empty, allTasksOrWorkflowsInSection, row, col, expectedTasksOrWorkflowsInCell);
		}

		protected void AssertTasksInCell<T>(string message, T[] allTasksOrWorkflowsInSection, int row, int col, params T[] expectedTasksOrWorkflowsInCell)
			where T : BusinessObject, ITagable // Because workflows and tasks are both tagable...
		{
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);

			AssertTasksInCell(message, config.BufferSection, viewModel, allTasksOrWorkflowsInSection, Tuple.Create(row, col, expectedTasksOrWorkflowsInCell));
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			jobHeader = CreateJobHeader<OrgHeader>(description: "jobHeader");
			workflow = jobHeader.ProcessHeaders[0];

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
		}

		protected VisualBoardTestConfig config;
		protected ProcessJobHeader jobHeader;
		protected ProcessHeader workflow;
	}
}
