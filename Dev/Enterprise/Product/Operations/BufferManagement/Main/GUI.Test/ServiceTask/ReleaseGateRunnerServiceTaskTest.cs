using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ReleaseGateRunnerServiceTask))]
	class ReleaseGateRunnerServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<ReleaseGateRunnerServiceTask>
	{
		#region Release Failure Logging

		public void TestHistoricalReleaseGateFailureShouldBeKept_OnceAFilterRuleFails()
		{
			var dan = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel Keogh");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, "DAN", 60 * 200);

			Factory.Save();

			var serviceTask = new ReleaseGateRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			workflow.Reload();
			AssertEquals("Should be blocked due to insufficient capacity", bucket.PK, workflow.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals("", @"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Keogh: required 300 hours, currently has 19 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (300 hours)".StripTaskIds(), workflow.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "We will never get here",
			});

			Factory.Save();

			var serviceTask2 = new ReleaseGateRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask2);
			serviceTask2.RunTask();

			workflow.Reload();
			AssertEquals("Should be blocked by filter rule", bucket.PK, workflow.FH_FC_CurrentComponent);
			AssertMultilineASCIIEquals("",
				@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Keogh: required 300 hours, currently has 19 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001000 (300 hours)".StripTaskIds(), workflow.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());
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

	[TestDate(2015, 3, 30)]
	class ReleaseGateRunnerServiceTaskNonTransactionedTest : SystemSchematicServiceTaskNonTransactionedTestCase<ReleaseGateRunnerServiceTask>
	{
		#region Async Regression

		public void TestRunAsync_ShouldParalleliseSystemsAndBuffers_AndNotReportCurrentBranchNotSetError()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var config1 = TestConfigsHelper.CreateSchematicTestConfig(Factory, "ORG", "System1");
			var config2 = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ", "System2");

			config1.Buffer.FC_DisplaySequence = 1;
			config2.Buffer.FC_DisplaySequence = 1;
			var buffer1_2 = BMSTestHelper.CreateBuffer(config1.System, "buffer1_2", sequence: 2);
			var buffer2_2 = BMSTestHelper.CreateBuffer(config2.System, "buffer2_2", sequence: 2);

			var link1_2 = BMSTestHelper.LinkComponents(config1.Bucket, buffer1_2);
			var link2_2 = BMSTestHelper.LinkComponents(config2.Bucket, buffer2_2);

			FilterStripsTestHelper.AddFilterStrips(config1.ComponentLink.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "workflow1_1",
			});

			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Two BMNCNSchedules, One BMNCNShape",
			});

			FilterStripsTestHelper.AddFilterStrips(config2.ComponentLink.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "workflow2_1",
			});

			FilterStripsTestHelper.AddFilterStrips(link2_2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Two BMNCNSchedules, One BMNCNShape",
			});

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Two BMNCNSchedules, One BMNCNShape");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Two BMNCNSchedules, One BMNCNShape");

			BMSTestHelper.CreateTask(workflow1_1, resource.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow1_2, resource.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow2_1, resource.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow2_2, resource.GS_Code, 60);

			AssertEquals(config1.Bucket, workflow1_1.CurrentComponent);
			AssertEquals(config1.Bucket, workflow1_2.CurrentComponent);
			AssertEquals(config2.Bucket, workflow2_1.CurrentComponent);
			AssertEquals(config2.Bucket, workflow2_2.CurrentComponent);

			Factory.Save();

			var logService = new ReleaseGateFailureLogService_ForTest();
			var task = new ReleaseGateRunnerServiceTask_ForTest(logService);
			var logger = new BufferManagementLogger();

			task.ServiceLogger = logger;
			task.RunTask();

			Factory.ReloadBusinessObjects(ProcessHeaderSchema.Instance, workflow1_1, workflow1_2, workflow2_1, workflow2_2);

			VisualBoardsTestCase.AssertSamePK(config1.Buffer, workflow1_1.CurrentComponent);
			VisualBoardsTestCase.AssertSamePK(buffer1_2, workflow1_2.CurrentComponent);
			VisualBoardsTestCase.AssertSamePK(config2.Buffer, workflow2_1.CurrentComponent);
			VisualBoardsTestCase.AssertSamePK(buffer2_2, workflow2_2.CurrentComponent);

			AssertEquals("Running these service tasks multi-threaded should not report errors", 0, ErrorReporter.TotalErrorCount);
		}

		#endregion

		#region Performance Regression

		public void TestRunTask_ShouldNotRunMultipleBuffersInOneRun()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "ORG", "System1");
			config.Buffer.FC_DisplaySequence = 1;

			var buffer2 = BMSTestHelper.CreateBuffer(config.System, "buffer2", sequence: 2);
			var link2 = BMSTestHelper.LinkComponents(config.Bucket, buffer2);

			FilterStripsTestHelper.AddFilterStrips(config.ComponentLink.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "workflow1_1",
			});

			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Two BMNCNSchedules, One BMNCNShape",
			});

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Two BMNCNSchedules, One BMNCNShape");

			BMSTestHelper.CreateTask(workflow1_1, resource.GS_Code, 60);
			BMSTestHelper.CreateTask(workflow1_2, resource.GS_Code, 60);

			AssertEquals(config.Bucket, workflow1_1.CurrentComponent);
			AssertEquals(config.Bucket, workflow1_2.CurrentComponent);

			Factory.Save();

			var task = new ReleaseGateRunnerServiceTask();
			var logger = new BufferManagementLogger();

			task.ServiceLogger = logger;
			task.RunTask();

			Factory.ReloadBusinessObjects(ProcessHeaderSchema.Instance, workflow1_1, workflow1_2);

			VisualBoardsTestCase.AssertSamePK(config.Buffer, workflow1_1.CurrentComponent);
			VisualBoardsTestCase.AssertSamePK(buffer2, workflow1_2.CurrentComponent);

			var log = logger.ToString();

			Assert(log, log.Contains("System1 - ORG.buffer: Starting Release Gate run with 1 workflows to process."));
			Assert(log, log.Contains("System1 - ORG.buffer2: Starting Release Gate run with 1 workflows to process."));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			BMSTestHelper.EnableBMSInRegistry();

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();
		}

		#endregion
	}
}
