using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.NetworkVisualisation.ServiceTasks.Test
{
	[TestedType(typeof(BufferPenetrationUpdaterServiceTask))]
	[TestDate(2014, 10, 6)]
	class BufferPenetrationUpdaterServiceTaskTest : BMServiceTaskTestCase<BufferPenetrationUpdaterServiceTask>
	{
		public void TestRun()
		{
			VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-20);

			var workflow1_1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "workflow1_2");
			var workflow2_1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "workflow2_1");
			var workflow2_2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader2, "workflow2_2");
			var workflow3_1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader3, "workflow3_1");
			var workflow3_2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader3, "workflow3_2");

			var task1_1 = VisualBoardsTestHelper.CreateTask(workflow1_1, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3, description: "task1_1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task1_2 = VisualBoardsTestHelper.CreateTask(workflow1_2, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3, description: "task1_2");
			var task2_1 = VisualBoardsTestHelper.CreateTask(workflow2_1, GlbStaff.CurrentUser.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 1.5), description: "task2_1");
			var task2_2 = VisualBoardsTestHelper.CreateTask(workflow2_2, GlbStaff.CurrentUser.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 1.5), description: "task2_2");
			var task3_1 = VisualBoardsTestHelper.CreateTask(workflow3_1, GlbStaff.CurrentUser.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 1.5), description: "task3_1");
			var task3_2 = VisualBoardsTestHelper.CreateTask(workflow3_2, GlbStaff.CurrentUser.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 1.5), description: "task3_2");

			Factory.Save();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var subDiagram1 = NetworkTestCase.CreateShape(jobHeader2, diagram);
			var subDiagram2 = NetworkTestCase.CreateShape(jobHeader3, diagram);

			var shape1_1 = NetworkTestCase.CreateShape(workflow1_1, diagram, "shape1_1");
			var shape1_2 = NetworkTestCase.CreateShape(workflow1_2, diagram, "shape1_2");

			shape1_1.MakeVisiblePrerequisiteOf(shape1_2, diagram);
			shape1_2.MakeVisiblePrerequisiteOf(subDiagram1, diagram);
			subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2, diagram);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, createNodeViewModels: false);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			networkViewModel.GetJobController().TriggerSaveAction(); // Force penetration to recalculate

			CombineAssertions(() =>
			{
				AssertEquals("X: subDiagram1", 800.0, subDiagram1.AsEntity(network).X);
				AssertEquals("X: subDiagram2", 1200.0, subDiagram2.AsEntity(network).X);
				AssertEquals("X: shape1_1", 0.0, shape1_1.AsEntity(network).X);
				AssertEquals("X: shape1_2", 400.0, shape1_2.AsEntity(network).X);

				AssertEquals("Width: subDiagram1", 400.0, subDiagram1.AsEntity(network).Width);
				AssertEquals("Width: subDiagram2", 400.0, subDiagram2.AsEntity(network).Width);
				AssertEquals("Width: shape1_1", 400.0, shape1_1.AsEntity(network).Width);
				AssertEquals("Width: shape1_2", 400.0, shape1_2.AsEntity(network).Width);

				AssertEquals("ImplicitDurationHours: subDiagram1", 36m, subDiagram1.ProcessHeader.ImplicitDurationHours);
				AssertEquals("ImplicitDurationHours: subDiagram2", 36m, subDiagram2.ProcessHeader.ImplicitDurationHours);
				AssertEquals("ImplicitDurationHours: shape1_1", 36m, shape1_1.ProcessHeader.ImplicitDurationHours);
				AssertEquals("ImplicitDurationHours: shape1_2", 36m, shape1_2.ProcessHeader.ImplicitDurationHours);

				AssertEquals("PlannedDurationInMinutes: subDiagram1", 1920, ((IBufferedItem)subDiagram1.ProcessHeader).PlannedDurationInMinutes);
				AssertEquals("PlannedDurationInMinutes: subDiagram2", 1920, ((IBufferedItem)subDiagram2.ProcessHeader).PlannedDurationInMinutes);
				AssertEquals("PlannedDurationInMinutes: shape1_1", 1920, ((IBufferedItem)shape1_1.ProcessHeader).PlannedDurationInMinutes);
				AssertEquals("PlannedDurationInMinutes: shape1_2", 1920, ((IBufferedItem)shape1_2.ProcessHeader).PlannedDurationInMinutes);

				AssertEquals("RemainingEstimateInMinutes: subDiagram1", 2160, ((IBufferedItem)subDiagram1.ProcessHeader).RemainingEstimateInMinutes);
				AssertEquals("RemainingEstimateInMinutes: subDiagram2", 2160, ((IBufferedItem)subDiagram2.ProcessHeader).RemainingEstimateInMinutes);
				AssertEquals("RemainingEstimateInMinutes: shape1_1", 0, ((IBufferedItem)shape1_1.ProcessHeader).RemainingEstimateInMinutes);
				AssertEquals("RemainingEstimateInMinutes: shape1_2", 2160, ((IBufferedItem)shape1_2.ProcessHeader).RemainingEstimateInMinutes);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Closed shape1_1 should not have a buffer penetration", 0m, shape1_1.BufferPenetration);
				AssertEquals("shape1_2", 0m, shape1_2.BufferPenetration);
				AssertEquals("subDiagram1", 0m, subDiagram1.BufferPenetration);
				AssertEquals("subDiagram2", 0m, subDiagram2.BufferPenetration);
				AssertEquals("buffer", 1.3125m, buffer.BufferPenetration);
			});

			var serviceTask = new BufferPenetrationUpdaterServiceTask_ForTest();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			CombineAssertions(() =>
			{
				shape1_1.ScheduleBizo.Reload();
				shape1_2.ScheduleBizo.Reload();
				subDiagram1.ScheduleBizo.Reload();
				subDiagram2.ScheduleBizo.Reload();
				buffer.ScheduleBizo.Reload();
				AssertEquals("Closed shape1_1 should not have a buffer penetration", 0m, shape1_1.BufferPenetration);
				AssertEquals("shape1_2", 1.3125m, shape1_2.BufferPenetration);
				AssertEquals("subDiagram1", 0.8125m, subDiagram1.BufferPenetration);
				AssertEquals("subDiagram2", 0.3125m, subDiagram2.BufferPenetration);
				AssertEquals("buffer", 1.3125m, buffer.BufferPenetration);

				AssertEquals("shape1_1.PenetratingBufferSizeInMinutes", 0, shape1_1.PenetratingBufferSizeInMinutes);
				AssertEquals("shape1_2.PenetratingBufferSizeInMinutes", 3840, shape1_2.PenetratingBufferSizeInMinutes);
				AssertEquals("subDiagram1.PenetratingBufferSizeInMinutes", 3840, subDiagram1.PenetratingBufferSizeInMinutes);
				AssertEquals("subDiagram2.PenetratingBufferSizeInMinutes", 3840, subDiagram2.PenetratingBufferSizeInMinutes);
				AssertEquals("buffer.PenetratingBufferSizeInMinutes", 0, buffer.PenetratingBufferSizeInMinutes);
			});

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);
			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			networkViewModel.GetJobController().TriggerSaveAction();
			serviceTask.RunTask();

			CombineAssertions(() =>
			{
				shape1_1.ScheduleBizo.Reload();
				shape1_2.ScheduleBizo.Reload();
				subDiagram1.ScheduleBizo.Reload();
				subDiagram2.ScheduleBizo.Reload();
				buffer.ScheduleBizo.Reload();
				AssertEquals("Closed shape1_1 should not have a buffer penetration", 0m, shape1_1.BufferPenetration);
				AssertEquals("Closed shape1_2 should not get its buffer penetration updated", 1.3125m, shape1_2.BufferPenetration);
				AssertEquals("subDiagram1", 1.42188m, subDiagram1.BufferPenetration);
				AssertEquals("subDiagram2", 0.92188m, subDiagram2.BufferPenetration);
				AssertEquals("buffer", 1.42188m, buffer.BufferPenetration);
			});

			serviceTask.RunTask();

			CombineAssertions(() =>
			{
				shape1_1.ScheduleBizo.Reload();
				shape1_2.ScheduleBizo.Reload();
				subDiagram1.ScheduleBizo.Reload();
				subDiagram2.ScheduleBizo.Reload();
				buffer.ScheduleBizo.Reload();
				AssertEquals("Closed shape1_1 should not have a buffer penetration", 0m, shape1_1.BufferPenetration);
				AssertEquals("Closed shape1_2 should not get its buffer penetration updated", 1.3125m, shape1_2.BufferPenetration);
				AssertEquals("No changes to subDiagram1", 1.42188m, subDiagram1.BufferPenetration);
				AssertEquals("No changes to subDiagram2", 0.92188m, subDiagram2.BufferPenetration);
				AssertEquals("No changes to buffer", 1.42188m, buffer.BufferPenetration);
			});

			string expectedLogMessages = """
			Information|Processed 1 batches, 4 records in total, 3 records updated

			Information|Processed 1 batches, 3 records in total, 2 records updated

			Information|Processed 1 batches, 3 records in total, 0 records updated
			""";

			AssertMultilineASCIIEquals("Service task log", expectedLogMessages, log.ToString());

			//Run service task on a different login, branch/dep
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = "ATGUK";
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var createUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.PK.ToGuid(), branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				serviceTask.RunTask();

				CombineAssertions(() =>
				{
					shape1_1.ScheduleBizo.Reload();
					shape1_2.ScheduleBizo.Reload();
					subDiagram1.ScheduleBizo.Reload();
					subDiagram2.ScheduleBizo.Reload();
					buffer.ScheduleBizo.Reload();
					AssertEquals("Closed shape1_1 should not have a buffer penetration", 0m, shape1_1.BufferPenetration);
					AssertEquals("Closed shape1_2 should not get its buffer penetration updated", 1.3125m, shape1_2.BufferPenetration);
					AssertEquals("No changes to subDiagram1", 1.42188m, subDiagram1.BufferPenetration);
					AssertEquals("No changes to subDiagram2", 0.92188m, subDiagram2.BufferPenetration);
					AssertEquals("No changes to buffer", 1.42188m, buffer.BufferPenetration);
				});
			}
		}

		public void TestRunSimple()
		{
			AssertNoExceptionThrown(() =>
			{
				VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
				var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

				jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-20);
				var workflow1_1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
				var task1_1 = VisualBoardsTestHelper.CreateTask(workflow1_1, GlbStaff.CurrentUser.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 3, description: "task1_1", taskStatus: ProcessTaskStatusCodeList.Codes.Working);
				Factory.Save();

				var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
				var shape1_1 = NetworkTestCase.CreateShape(workflow1_1, diagram, "shape1_1");
				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram, createNodeViewModels: false);
				var network = networkViewModel.GetJobNetwork();
				network.SwitchToScaled();
				networkViewModel.SuggestAndAcceptAllBuffers();
				networkViewModel.ToggleApproval();

				networkViewModel.GetJobController().TriggerSaveAction(); // Force penetration to recalculate

				var serviceTask = new BufferPenetrationUpdaterServiceTask_ForTest();
				var log = InitialiseTaskSchedule(serviceTask);
				serviceTask.RunTask();
			});
		}

		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, BufferPenetrationUpdaterServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires a value equal to 'PLN'.", BufferPenetrationUpdaterServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires a value equal to 'PLN'.", BufferPenetrationUpdaterServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires a value equal to 'PLN'.", BufferPenetrationUpdaterServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region Implementation

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
		}

		class BufferPenetrationUpdaterServiceTask_ForTest : BufferPenetrationUpdaterServiceTask
		{
			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
		}

		#endregion

	}
}
