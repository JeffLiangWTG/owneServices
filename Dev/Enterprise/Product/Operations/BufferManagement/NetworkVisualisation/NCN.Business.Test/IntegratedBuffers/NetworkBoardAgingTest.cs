using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkBoardAgingTest_WithSetupData : BoardAgingTestBase
	{
		#region Board Aging

		public void TestBoardAging_NetworkItem()
		{
			SetupTestData(60);

			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, estVariationFactor: 1);

			RunBufferPenetrationUpdaterServiceTask();
			AssertTasksInCell(new[]
			{
				task
			}, 8, 2, new[]
			{
				task
			}); // No progress and 2 days aging in a 6 day buffer
		}

		public void TestBoardAging_NetworkItem_PartiallyCompleted_AsMuchProgressAsTimeHasElapsed()
		{
			SetupTestData(960 + 480);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 960, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 2 days = the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining

			RunBufferPenetrationUpdaterServiceTask();
			AssertTasksInCell(new[]
			{
				task2
			}, 12, 2, new[]
			{
				task2
			}); // Should be no buffer consumption as the total amount of work done is the same as the time since release
		}

		public void TestBoardAging_NetworkItem_PartiallyCompleted_LessProgressThanTimeHasElapsed()
		{
			SetupTestData(480 + 480);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 1 days = half the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining

			RunBufferPenetrationUpdaterServiceTask();
			AssertTasksInCell(new[]
			{
				task2
			}, 10, 2, new[]
			{
				task2
			}); // Should be 1 day buffer consumption as the total amount of work done is 1 day less than the time since release
		}

		public void TestBoardAging_NetworkItem_PartiallyCompleted_LessProgressThanTimeHasElapsed_ButWithEstToComplete()
		{
			SetupTestData(480 + 480);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task1", estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed); // 8 hrs * 60 mins * 1 days = half the amount of time that has passed
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 480, description: "task2", estVariationFactor: 1); // 8 hrs * 60 mins = the amount of time remaining
			task2.P9_EstimatedTimeToComplete = new ZInt(10).GetDateTimeFromMinutes();

			RunBufferPenetrationUpdaterServiceTask();
			AssertTasksInCell(new[]
			{
				task2
			}, 12, 2, new[]
			{
				task2
			}); // Should be no buffer consumption as the work remaining is negligible, and we're within the original estimate
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		void SetupTestData(int shapeDurationMinutes)
		{
			workflow.JobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-2).AddMinutes(-10);
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow; // Ensures the scheduled time is used rather than release date

			var diagram = NetworkTestCase.CreateDiagram(workflow.JobHeader);
			var shape = NetworkTestCase.CreateShape(workflow, diagram, "shape");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ResolutionIncrement = diagram.Scale = new ZInt(shapeDurationMinutes).GetDateTimeFromMinutes();
			shape.AsEntity(network).Width = 100;

			networkViewModel.SuggestAndAcceptAllBuffers();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			buffer.ExplicitDurationMinutes = 8 * 60 * 6; // 8 hrs * 6 (6 days total)

			networkViewModel.ToggleApproval();

			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(TagProvider.GetCCPMReleaseTagGroup(Factory));
			rtrTag.Definition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			var link = (TagLink)workflow.AddTag(rtrTag).Link;
			link.TGL_SystemCreateTimeUtc = workflow.JobHeader.FH_DoNotStartBeforeDate;

			Factory.Save();
		}

		void RunBufferPenetrationUpdaterServiceTask()
		{
			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1); // To make workflows update their status
			Factory.Save();

			NetworkTestCase.RunBufferPenetrationUpdater();

			Factory.ServiceContainer.AddService(new ApprovedShapeBufferPenetrationService(workflow.JobHeader.ProcessHeaders));
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	[TestDate(2015, 7, 14)]
	class NetworkBoardAgingTest : NetworkTestCase
	{
		public void TestBoardAging_ApprovedCCPMProjectItemWithNoRelatedBuffers_ShouldUseOperationalBuffer()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-30);

			var operationalBufferReleaseTime = ZDateTime.UtcNow.AddDays(-5).AddHours(10);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Another thing", config.Buffer, operationalBufferReleaseTime, config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "How come I can't get no tang round here?", config.Buffer, operationalBufferReleaseTime, config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Hold on a second", config.Buffer, operationalBufferReleaseTime, config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60, description: "task3");

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2);

			UnitTestUserNotification.Instance.AddOKAnswer(); // We need to save the diagram first.
			UnitTestUserNotification.Instance.AddOKAnswer(); // There is a shape that has no connection to the project buffer. We want to approve anyway since we're specifically testing how that impacts buffer penetration.

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			NetworkTestCase.RunCCPMAndNCNTagRules(Factory);

			Factory.Save();

			NetworkTestCase.RunBufferPenetrationUpdater();

			Factory.ServiceContainer.AddService(new ApprovedShapeBufferPenetrationService(jobHeader.ProcessHeaders)); // Simulates how boards ignore CCPM buffers unless this service is present.

			AssertNotEquals(ZDecimal.Zero, shape1.Shape.BufferPenetration);
			AssertNotEquals(ZDecimal.Zero, shape2.Shape.BufferPenetration);
			AssertEquals("This shape has no related buffers, so buffer penetration is still zero", ZDecimal.Zero, shape3.Shape.BufferPenetration);

			AssertTasksInCell("Tasks which have a shape that has a downstream buffer should use that shape's buffer penetration", jobHeader.Tasks.ToArray(), 0, 2, new[] { task1, task2 });
			AssertTasksInCell("Tasks which have a shape that does NOT have a downstream buffer should use operational buffer penetration", jobHeader.Tasks.ToArray(), 10, 2, new[] { task3 });
		}

		public void TestBoardAging_ApprovedNCNProjectItem_ShouldUseOperationalBuffer()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-30);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Another thing", config.Buffer, ZDateTime.UtcNow.AddDays(-5), config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "How come I can't get no tang round here?", config.Buffer, ZDateTime.UtcNow.AddDays(-5));
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Hold on a second", config.Buffer, ZDateTime.UtcNow.AddDays(-5));
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60, description: "task3");

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			NetworkTestCase.RunCCPMAndNCNTagRules(Factory);

			Factory.Save();

			NetworkTestCase.RunBufferPenetrationUpdater();

			AssertNotEquals(ZDecimal.Zero, shape1.Shape.BufferPenetration);
			AssertNotEquals(ZDecimal.Zero, shape2.Shape.BufferPenetration);
			AssertNotEquals(ZDecimal.Zero, shape3.Shape.BufferPenetration);

			networkViewModel.ToggleApproval();
			network["Project Buffer"].Delete();
			networkViewModel.ToggleApproval();
			Factory.Save();

			NetworkTestCase.RunCCPMAndNCNTagRules(Factory);
			Factory.Save();

			Factory.ServiceContainer.AddService(new ApprovedShapeBufferPenetrationService(jobHeader.ProcessHeaders));
			AssertTasksInCell("Tasks for an approved NCN should use operational buffer penetration, no matter what is inside the shape's BufferPenetration property, or its StartableTime and PlannedDuration", jobHeader.Tasks.ToArray(), 9, 2, new[] { task1, task2, task3 });
		}

		#region Implementation

		void AssertTasksInCell(string message, ProcessTask[] allTasks, int row, int col, ProcessTask[] expectedTasksInCell)
		{
			AssertTasksInCell(message, config.BufferSection, BMSTestHelper.CreateViewModel(config.BufferSection), allTasks, Tuple.Create(row, col, expectedTasksInCell));
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		VisualBoardTestConfig config;

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}
}
