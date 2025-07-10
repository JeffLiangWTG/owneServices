using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ReleaseGateRunnerServiceTask))]
	class ReleaseGateRunnerServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<ReleaseGateRunnerServiceTask>
	{
		#region Active Flag

		[TestDate(2019, 1, 1)]
		public void TestProcess_ShouldBypassInactiveBuffers()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			BMSTestHelper.LinkComponents(bucket, buffer1);
			BMSTestHelper.LinkComponents(bucket, buffer2);

			system.FS_Name = "ส็็็็็็็็็็็_(ツ)_ส้้้้้้้้้้้้";
			buffer2.FC_IsActive = false;

			Factory.Save();

			var serviceTask = new ReleaseGateRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var logger = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertMultilineASCIIEquals("", @"Information|????????????_(?)_?????????????.buffer1: Determining workflows eligible for Release Gate.
Information|????????????_(?)_?????????????.buffer1: Assembling reservation tracker.
Information|????????????_(?)_?????????????.buffer1: There are no workflows eligible for release.", logger.ToString());
		}

		[TestDate(2019, 1, 1)]
		public void TestProcess_ShouldBypassInactiveBuffers_EvenIfMadeInactiveDuringRun()
		{
			var systemHappy = BMSTestHelper.CreateSystem(Factory);
			var systemSad = BMSTestHelper.CreateSystem(Factory);

			var bucketHappy = BMSTestHelper.CreateBucket(systemHappy);
			var bufferHappy = BMSTestHelper.CreateBuffer(systemHappy, "buffer1");

			var bucketSad = BMSTestHelper.CreateBucket(systemSad);
			var bufferSad = BMSTestHelper.CreateBuffer(systemSad, "buffer2");

			BMSTestHelper.LinkComponents(bucketHappy, bufferHappy);
			BMSTestHelper.LinkComponents(bucketSad, bufferSad);

			systemHappy.FS_Name = "Live System";
			systemHappy.FS_IsLive = true;

			systemSad.FS_Name = "NotLive System";
			systemSad.FS_IsLive = true;

			Factory.Save();

			var serviceTask = new ReleaseGateRunnerServiceTask_ForTest(ReleaseLogFailureService);
			serviceTask.OnSystemsLoadedAction = () =>
			{
				systemSad.FS_IsLive = false;
				Factory.Save();
			};

			var logger = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertMultilineASCIIEquals("", @"Information|Live System.buffer1: Determining workflows eligible for Release Gate.
Information|Live System.buffer1: Assembling reservation tracker.
Information|Live System.buffer1: There are no workflows eligible for release.", logger.ToString());
			CombineAssertions("We finished the service task run and our system liveliness changed properly", () =>
			{
				AssertEquals("SystemHappy should still be alive!", true, systemHappy.FS_IsLive);
				AssertEquals("SystemSad should be dead! Sad!", false, systemSad.FS_IsLive);
			});
		}

		#endregion

		#region Hosted Service Requirements

		public void TestServiceTask_WhenCapacityCalculationDisabled_ShouldNotMeetHostedServiceRequirement()
		{
			AssertEquals("Should be enabled by default if buffer management is enabled.", string.Empty, ReleaseGateRunnerServiceTask.CheckCapacityCalculationsNotDisabled());

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations' requires a value other than 'True'.", ReleaseGateRunnerServiceTask.CheckCapacityCalculationsNotDisabled());
		}

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, ReleaseGateRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, ReleaseGateRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", ReleaseGateRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", ReleaseGateRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Release Gate Runner.", log.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Release Gate Runner.", log.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Release Gate Runner.", log.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Release Gate Runner.", log.ToString());
		}
		#endregion

		#region Turned off in registry

		[TestDate(2022, 5, 13, 10, 0, 0)]
		public void TestProcess_WhenDisableCapacityCalculationsRegistryItemEnabled_ShouldNotProcessAnything_AndLogInformation()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron Aardvark Aanensen");
			Assert(staff.IsWorking(ZDate.Today));

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow that would normally be released");
			BMSTestHelper.CreateTask(workflow, staff.GS_Code, 80);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var task = GetNewServiceTask();
			task.ServiceLogger = logger;
			task.RunTask();

			workflow.Reload();

			AssertEquals("Capacity calculations are disabled so the service task should do nothing.", "bucket", workflow.CurrentComponent.FC_Name);
			AssertMultilineASCIIEquals("Debug - Service task not run because the [Disable Capacity Calculations] registry item is enabled.", logger.ToString());
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		protected override void SetUpCore()
		{
			base.SetUpCore();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			disposables = new DisposableList(1) { BMSTestCaseWithFactory.DisableAsyncBehaviour() };

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			disposables.Dispose();
		}

		protected override ReleaseGateRunnerServiceTask GetNewServiceTask()
		{
			return new ReleaseGateRunnerServiceTask_ForTest(ReleaseLogFailureService);
		}

		protected override void InitializeProcessHeaders()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");
			var link2 = BMSTestHelper.LinkComponents(bucket2, buffer2, isReleaseGate: true);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Todd Gurley III", bucket);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Josh Gordon", bucket2);

			var task = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			Factory.Save();
		}

		DisposableList disposables;

		#endregion
	}

	public class ReleaseGateRunnerServiceTask_ForTest : ReleaseGateRunnerServiceTask
	{
		public ReleaseGateRunnerServiceTask_ForTest(ReleaseGateFailureLogService_ForTest service)
		{
			this.service = service;
			this.branchPK = Env.CurrentBranchPK;
		}

		protected override void RunTaskCore(CancellationToken token)
		{
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				base.RunTaskCore(token);
			}
		}

		protected override ReleaseGateFailureLogService GetReleaseGateFailureLogService(BusinessObjectFactory factory)
		{
			return service;
		}

		readonly ReleaseGateFailureLogService_ForTest service;
		readonly Guid branchPK;

		protected override void OnSystemsLoaded()
		{
			base.OnSystemsLoaded();
			using (DisposableEnvironment.ForBranch(branchPK))
			{
				OnSystemsLoadedAction?.Invoke();
			}
		}

		public Action OnSystemsLoadedAction { get; set; }
	}
}
