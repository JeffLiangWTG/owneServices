using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(TransferRuleRunnerServiceTask))]
	class TransferRuleRunnerServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<TransferRuleRunnerServiceTask>
	{
		#region Hosted Service Requirements

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, TransferRuleRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, TransferRuleRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertEquals("The service task should meet the requirements because EWF is enabled.", string.Empty, TransferRuleRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'EWF', 'BUF', 'PLN'.", TransferRuleRunnerServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Buffer Management Schematic Transfer Runner.", log.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Buffer Management Schematic Transfer Runner.", log.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Buffer Management Schematic Transfer Runner.", log.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Buffer Management Schematic Transfer Runner.", log.ToString());
		}
		#endregion

		#region Transfer

		public void TestProcess_LinkHasEmptyFilters_ShouldDeactivateLinkAndReportToAdministrator()
		{
			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "test@test.com";

			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var bucket4 = BMSTestHelper.CreateBucket(system, "Bucket 4");
			var bucket5 = BMSTestHelper.CreateBucket(system, "Bucket 5");
			var bucket6 = BMSTestHelper.CreateBucket(system, "Bucket 6");
			var bucket7 = BMSTestHelper.CreateBucket(system, "Bucket 7");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");

			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1, isReleaseGate: false); //no filter - filter is not required
			Assert(!link1.HasNoFilterWhenRequired);

			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 2, isReleaseGate: false);

			var link3 = BMSTestHelper.LinkComponents(bucket3, buffer1, sequence: 3, isReleaseGate: true); //no filter - filter is not required
			Assert(!link3.HasNoFilterWhenRequired);

			var link4 = BMSTestHelper.LinkComponents(buffer1, bucket4, sequence: 4, isReleaseGate: false);

			var link5 = BMSTestHelper.LinkComponents(bucket4, bucket5, sequence: 5, isReleaseGate: false); //no filter, but filter is required - this link will be deactivated
			Assert(link5.HasNoFilterWhenRequired);

			var link6 = BMSTestHelper.LinkComponents(bucket5, buffer2, sequence: 6, isReleaseGate: false);
			var link7 = BMSTestHelper.LinkComponents(bucket5, bucket6, sequence: 7, isReleaseGate: false); //no filter, but filter is required - this link will be deactivated
			Assert(link7.HasNoFilterWhenRequired);

			var link8 = BMSTestHelper.LinkComponents(bucket6, bucket7, sequence: 1, isActive: false); // No filter, but link is inactive.

			AddLinkFilterForTest(link2.FilterRule, "Test 1");
			Assert(!link2.HasNoFilterWhenRequired);

			AddLinkFilterForTest(link4.FilterRule, "Test 2");
			Assert(!link4.HasNoFilterWhenRequired);

			AddLinkFilterForTest(link6.FilterRule, "Test 3");
			Assert(!link3.HasNoFilterWhenRequired);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			Factory.Save();

			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var newFactory = new BusinessObjectFactory();
			var linkLoaded = newFactory.Load<BMComponentLink>(link5.PK);
			Assert("Component link 'link5' that is not the link from the entry component and had no filters should be deactivated", !linkLoaded.FL_TransferRulesEnabled);

			linkLoaded = newFactory.Load<BMComponentLink>(link7.PK);
			Assert("Component link 'link7' that is not the link from the entry component and had no filters should be deactivated", !linkLoaded.FL_TransferRulesEnabled);

			AssertEquals("Should have created one email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			AssertMultilineASCIIEquals("Status log", @"Warning|ZJ321WUNM8ISO6VDSJSPUZX5LL7F60: The following component links have been deactivated since they have no filters or invalid filters:
Bucket 4 -> Bucket 5, Sequence: 5
Bucket 5 -> Bucket 6, Sequence: 7

", log.ToString());

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Mail recipient", 1, email.Recipients.Count);
			AssertEquals("test@test.com", email.Recipients[0].Email);
			AssertEquals("Component Links with invalid filters have been deactivated.", email.Subject);
			AssertMultilineASCIIEquals("Email body", @"The following component links have been deactivated since they have no filters or invalid filters:
Bucket 4 -> Bucket 5, Sequence: 5
Bucket 5 -> Bucket 6, Sequence: 7", email.Body);
		}

		[TestDate(2014, 8, 13)]
		public void TestProcess_ShouldNotClearReleaseFailureLogWhenTransferRulesFailOnAnotherLink()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var doneBucket = BMSTestHelper.CreateBucket(config.System, "Done");
			var linkToDone = BMSTestHelper.LinkComponents(config.Bucket, doneBucket);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 6000);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow2", config.Bucket);
			workflow2.FH_VoteUpDownAmount = 100;
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, lowEstMinutes: 60);

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow3", config.Bucket);
			workflow3.FH_VoteUpDownAmount = -100;
			BMSTestHelper.CreateTask(workflow3, resource.GS_Code, lowEstMinutes: 60);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System, failureLogService: ReleaseLogFailureService);

			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(config.Bucket.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(config.Bucket.PK, workflow3.FH_FC_CurrentComponent);

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -103.5 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T00001002 (1.5 hours)".StripTaskIds(), workflow3.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			workflow2.Reload();
			workflow3.Reload();

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());

			AssertMultilineASCIIEquals("Should not consider work already in the buffer when considering a workflow's place in the queue",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Frodo Baggins: required 1.5 hours, currently has -103.5 hours of available capacity at 2nd place in the queue for this resource.
		Tasks assigned: T00001002 (1.5 hours)".StripTaskIds(), workflow3.GetReleaseFailureReasonForBuffer(config.Buffer).StripTaskIds());
		}

		[TestDate(2014, 7, 6)]
		public void TestRun_ShouldLogFailureOnWorkflow()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Benedict Cumberbatch");

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2");

			BMSTestHelper.LinkComponents(bucket, buffer1);
			var link = BMSTestHelper.LinkComponents(bucket, buffer2);

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "We will never get here",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 6000);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 60);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);

			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow2);

			AssertEquals(bucket, workflow2.CurrentComponent);

			const string expectedReleaseMessage = @"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Benedict Cumberbatch: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)"
			;

			AssertMultilineASCIIEquals("Release failure", expectedReleaseMessage.StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer1).StripTaskIds());

			var transferRuleRunner = new TestTransferRuleRunner(system, log);
			transferRuleRunner.Process_ForTest();

			AssertMultilineASCIIEquals("Running transfer rules shouldn't clear the release failure reason, since the transfer rules pass", expectedReleaseMessage.StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer1).StripTaskIds());
		}

		[TestDate(2014, 7, 6)]
		public void TestRun_WhenLoggingTransferProgress_ShouldLogFailureOnWorkflow()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MFM", "Martin Freeman");

			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 6000);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 60);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow2);

			AssertEquals(bucket, workflow2.CurrentComponent);
			AssertMultilineASCIIEquals("",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Martin Freeman: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			var transferRuleRunner = new TestTransferRuleRunner(system, log);
			transferRuleRunner.Process_ForTest();

			AssertMultilineASCIIEquals("",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Martin Freeman: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());
		}

		[TestDate(2014, 7, 6)]
		public void TestRun_ShouldNotEraseReleaseFailureWhenBlockedByOtherComponents()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel Stephen Keogh");

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket2", sequence: 2);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var link1 = BMSTestHelper.LinkComponents(bucket1, buffer, 0);
			var link2 = BMSTestHelper.LinkComponents(bucket1, bucket2, 1);

			FilterStripsTestHelper.AddFilterStrips(link2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "We will never get here",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 6000);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", bucket1);
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 60);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow2);

			AssertEquals(bucket1, workflow2.CurrentComponent);

			AssertMultilineASCIIEquals("Workflow was blocked by transfer into second component link, but release failure should be preserved",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Stephen Keogh: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());

			serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			log = InitialiseTaskSchedule(serviceTask);
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow2);

			AssertMultilineASCIIEquals("Workflow was blocked by transfer into second component link, but release failure should be preserved",
@"There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.
	Daniel Stephen Keogh: required 1.5 hours, currently has -102 hours of available capacity at 1st place in the queue for this resource.
		Tasks assigned: T00001001 (1.5 hours)".StripTaskIds(), workflow2.GetReleaseFailureReasonForBuffer(buffer).StripTaskIds());
		}

		public void TestRun_WorkflowShouldMoveForwardThroughEntireLengthOfSystem()
		{
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory);
			system.FS_Name = "Buckets R Us";

			var bucket4 = BMSTestHelper.CreateBucket(system, "Bucket 4", sequence: 4);
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			AddLinkFilterForTest(link1_2.FilterRule, "Job Workflow");

			var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3);
			AddLinkFilterForTest(link2_3.FilterRule, "Job Workflow");

			var link3_4 = BMSTestHelper.LinkComponents(bucket3, bucket4);
			AddLinkFilterForTest(link3_4.FilterRule, "Job Workflow");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FillWithValidTestData();
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Squanch", bucket1); // TODO: this is an example of how to create a workflow nicely

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow);

			Assert(workflow.FH_IsActive);
			AssertEquals(bucket4.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertMultilineASCIIEquals("Service task log", $@"Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 1 to Bucket 2
Information|Buckets R Us: Component link [Bucket 1 -> Bucket 2, Sequence: 0]: 1 workflow transferred
Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 2 to Bucket 3
Information|Buckets R Us: Component link [Bucket 2 -> Bucket 3, Sequence: 0]: 1 workflow transferred
Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 3 to Bucket 4
Information|Buckets R Us: Component link [Bucket 3 -> Bucket 4, Sequence: 0]: 1 workflow transferred", log.ToString());
		}

		[TestDate(2023, 1, 2, 3, 4, 5)]
		public void TestRun_WhenRecurringLoopDetected_ShouldDisableWorkflow() => TestRecurringLoop(false);
		[TestDate(2023, 1, 2, 3, 4, 5)]
		public void TestRun_WhenRecurringLoopDetected_WithoutDataRefresh_ShouldDisableWorkflow() => TestRecurringLoop(true);

		void TestRecurringLoop(bool disableDataRefresh)
		{
			IDisposable disposable = null;
			if (disableDataRefresh)
			{
				disposable = new DataRefreshManager.DisableRefreshForServiceTask();
			}
			using (disposable)
			{
				BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var system = BMSTestHelper.CreateSystem(Factory);
				system.FS_Name = "Buckets R Us";

				var bucket4 = BMSTestHelper.CreateBucket(system, "Bucket 4", sequence: 4);
				var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
				var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
				var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);

				var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);
				AddLinkFilterForTest(link1_2.FilterRule, "Job Workflow");

				var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3);
				AddLinkFilterForTest(link2_3.FilterRule, "Job Workflow");

				var link3_4 = BMSTestHelper.LinkComponents(bucket3, bucket4);
				AddLinkFilterForTest(link3_4.FilterRule, "Job Workflow");

				var link4_1 = BMSTestHelper.LinkComponents(bucket4, bucket1);
				AddLinkFilterForTest(link4_1.FilterRule, "Job Workflow");

				var job = Factory.NewWithValidTestData<OrgHeader>();
				job.OH_Code = "MAIORGSYD";
				var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
				var workflow = jobHeader.ProcessHeaders[0];
				workflow.FillWithValidTestData();
				workflow.FH_FC_CurrentComponent = bucket1.PK;

				Factory.Save();

				var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
				var log = InitialiseTaskSchedule(serviceTask);
				RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, workflow);

				AssertEquals(false, workflow.FH_IsActive);
				AssertEquals(bucket1.FC_Name, workflow.CurrentComponent.FC_Name);
				AssertMultilineASCIIEquals("Service task log", $@"Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 1 to Bucket 2
Information|Buckets R Us: Component link [Bucket 1 -> Bucket 2, Sequence: 0]: 1 workflow transferred
Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 2 to Bucket 3
Information|Buckets R Us: Component link [Bucket 2 -> Bucket 3, Sequence: 0]: 1 workflow transferred
Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 3 to Bucket 4
Information|Buckets R Us: Component link [Bucket 3 -> Bucket 4, Sequence: 0]: 1 workflow transferred
Information|Buckets R Us: Moved workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) from component Bucket 4 to Bucket 1
Information|Buckets R Us: Component link [Bucket 4 -> Bucket 1, Sequence: 0]: 1 workflow transferred
Warning|Buckets R Us: Workflow Job Workflow (PK = {workflow.PK}, Job = Organization (MAIORGSYD)) has been deactivated since it has completed a loop of the system Buckets R Us via [(Bucket 1,2023-01-02T03:04:05.0000000Z),(Bucket 2,2023-01-02T03:04:05.0000000Z),(Bucket 2,2023-01-02T03:04:05.0000000Z),(Bucket 3,2023-01-02T03:04:06.0000000Z),(Bucket 3,2023-01-02T03:04:06.0000000Z),(Bucket 4,2023-01-02T03:04:07.0000000Z),(Bucket 4,2023-01-02T03:04:07.0000000Z),(Bucket 1,2023-01-02T03:04:08.0000000Z)]. Please address the issue and reactivate the workflow.
", log.ToString());

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		[TestDate(2014, 12, 8)]
		public void TestRun_IgnoresInactiveSystems()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "EEE", "Squeeeeeeeps");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.System.FS_IsLive = false;

			var doneBucket = BMSTestHelper.CreateBucket(config.System, "Done");
			var linkToDone = BMSTestHelper.LinkComponents(config.Buffer, doneBucket);
			AddLinkFilterForTest(linkToDone.FilterRule, "Job Workflow");

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, lowEstMinutes: 60);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var newFactory = Factory.CreateNewFactory();
			var newWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			AssertEquals(config.Buffer.PK, newWorkflow1.FH_FC_CurrentComponent);

			config.System.FS_IsLive = true;

			Factory.Save();

			serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			var anotherNewFactory = Factory.CreateNewFactory();
			var reloadedNewWorkflow1 = anotherNewFactory.Load<ProcessHeader>(workflow1.PK);
			AssertEquals(doneBucket.PK, reloadedNewWorkflow1.FH_FC_CurrentComponent);
		}
		
		public void TestRun_ShouldSetProcessAllTransferRulesLinksOnNextBMSRunToFalse_WhenIsTrue()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "b3");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 0);
			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			workflow.FH_FC_CurrentComponent = bucket1.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();

			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link1.FilterRule);
			BMComponentLinkTest.AddAllFiltersThatCanBeSkiped(link2.FilterRule);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertEquals("Workflow was transfered", workflow.FH_FC_CurrentComponent, bucket3.PK);
			AssertEquals("Should set to false to avoid skip links next", false, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

			var logText = log.ToString();
			AssertContains(@"Component link [b1 -> b2, Sequence: 0]: not skipped because ProcessAllTransferRulesLinksOnNextBMSRun is true", logText);
			AssertContains(@"Component link [b2 -> b3, Sequence: 1]: not skipped because ProcessAllTransferRulesLinksOnNextBMSRun is true", logText);
		}

		#endregion

		#region Normal Run

		[TestDate(2013, 2, 25)]
		public void TestRun()
		{
			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "test@test.com";

			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var log = InitialiseTaskSchedule(serviceTask);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Zubin";
			system.FS_Description = "Rakhsh";

			var component1 = BMSTestHelper.CreateBucket(system, "Bucket1", sequence: 1);
			var component2 = BMSTestHelper.CreateBucket(system, "Bucket2", sequence: 2);
			var component3 = BMSTestHelper.CreateBuffer(system, "Buffer", timespanMinutes: 100, sequence: 3);
			var component4 = BMSTestHelper.CreateBucket(system, "Bucket3", sequence: 4);
			var inactiveComponent = BMSTestHelper.CreateBucket(system, "Inactive bucket", sequence: 5);
			inactiveComponent.FC_IsActive = false;

			// comp1 -> comp2
			// comp2 -> comp3 or comp 4
			// comp3 -> comp4
			// comp4 -> inactiveComp
			var link1_2 = BMSTestHelper.LinkComponents(component1, component2);
			var link2_3 = BMSTestHelper.LinkComponents(component2, component3);
			var link2_4 = BMSTestHelper.LinkComponents(component2, component4);
			var link3_4 = BMSTestHelper.LinkComponents(component3, component4);
			var link4_inactive = BMSTestHelper.LinkComponents(component4, inactiveComponent);

			AddLinkFilterForTest(link1_2.FilterRule, "We might get here");
			AddLinkFilterForTest(link2_3.FilterRule, "We might get here");
			AddLinkFilterForTest(link2_4.FilterRule, "We will never get here");
			AddLinkFilterForTest(link3_4.FilterRule, "We will never get here");
			AddLinkFilterForTest(link4_inactive.FilterRule, "We will never get here");

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.OH_Code = "MAIFRISTORG";
			job1.OH_SystemCreateTimeUtc = ZDateTime.Today.AddHours(1);
			var job1Header = ProcessJobHeader.GetForParent(job1, Factory);
			job1Header.FH_CompletionStatement = "Job1";
			var job1Header1 = job1Header.ProcessHeaders[0];
			job1Header1.FH_FC_CurrentComponent = component1.PK;
			job1Header1.FH_CompletionStatement = "Job1_1";
			BMSTestHelper.CreateTask(job1Header1, GlbStaff.CurrentUser.GS_Code);

			var job1Header2 = job1Header.ProcessHeaders.AddNew();
			job1Header2.FH_FC_CurrentComponent = component1.PK;
			job1Header2.FH_CompletionStatement = "Job1_2";
			BMSTestHelper.CreateTask(job1Header2, GlbStaff.CurrentUser.GS_Code);

			var job1Header3Inactive = job1Header.ProcessHeaders.AddNew();
			job1Header3Inactive.FH_FC_CurrentComponent = inactiveComponent.PK;
			job1Header3Inactive.FH_CompletionStatement = "Job1_3";
			job1Header3Inactive.FH_IsActive = false;
			BMSTestHelper.CreateTask(job1Header3Inactive, GlbStaff.CurrentUser.GS_Code);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_Code = "MAISCNDORG";
			job2.OH_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-1);
			var job2Header = ProcessJobHeader.GetForParent(job2, Factory);
			job2Header.FH_CompletionStatement = "Job2";
			var job2Header1 = job2Header.ProcessHeaders[0];
			job2Header1.FH_FC_CurrentComponent = component1.PK;
			job2Header1.FH_CompletionStatement = "Job2_1";
			BMSTestHelper.CreateTask(job2Header1, GlbStaff.CurrentUser.GS_Code);

			var job2Header2 = job2Header.ProcessHeaders.AddNew();
			job2Header2.FH_FC_CurrentComponent = component3.PK;
			job2Header2.FH_CompletionStatement = "Job2_2";
			BMSTestHelper.CreateTask(job2Header1, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.CreateTask(job2Header2, staff.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);

			job2Header2.GetOrCreateDependencyLink(job2Header1);

			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			job3.OH_Code = "RYANDZAY";
			job3.OH_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-1);
			var job3Header = ProcessJobHeader.GetForParent(job3, Factory);
			job3Header.FH_CompletionStatement = "Job3";
			var job3Header1 = job3Header.ProcessHeaders[0];
			job3Header1.FH_FC_CurrentComponent = component1.PK;
			job3Header1.FH_CompletionStatement = "Job3_1";
			BMSTestHelper.CreateTask(job3Header1, GlbStaff.CurrentUser.GS_Code);

			var job3Header2 = job3Header.ProcessHeaders.AddNew();
			job3Header2.FH_FC_CurrentComponent = component3.PK;
			job3Header2.FH_CompletionStatement = "Job3_2";
			BMSTestHelper.CreateTask(job3Header2, GlbStaff.CurrentUser.GS_Code);

			var job3HeaderCompleted = job3Header.ProcessHeaders.AddNew();
			job3HeaderCompleted.FH_CompletionStatement = "Job3_3";

			var completedTask = job3.WorkflowItems.Tasks.AddNew();
			completedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			completedTask.P9_FH_ProcessHeader = job3HeaderCompleted.PK;
			completedTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			job3HeaderCompleted.GetOrCreateDependencyLink(job3Header1);

			Factory.Save();

			var originalDate = TestDateAttribute.Date;
			TestDateAttribute.Date = originalDate.AddDays(-15);

			Factory.Save();

			TestDateAttribute.Date = originalDate;

			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);

			AssertEquals("No movement, as all components are blocked by filters", component1.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("No movement, as all components are blocked by filters", component1.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("No movement, as all components are blocked by filters", component1.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("No movement, as all components are blocked by filters", component3.PK, job2Header2.FH_FC_CurrentComponent);
			AssertEquals("No movement, as all components are blocked by filters", inactiveComponent.PK, job1Header3Inactive.FH_FC_CurrentComponent);

			FilterStripsTestHelper.AddFilterStrips(link1_2.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition { });
			Factory.Save();
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);

			AssertEquals("All tasks moved to comp2", component2.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("All tasks moved to comp2", component2.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("All tasks moved to comp2", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at component3 - no movement", component3.PK, job2Header2.FH_FC_CurrentComponent);
			AssertEquals("Remains at inactiveComponent - no movement", inactiveComponent.PK, job1Header3Inactive.FH_FC_CurrentComponent);

			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("No further movement", component2.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component2.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component3.PK, job2Header2.FH_FC_CurrentComponent);
			AssertEquals("No further movement", inactiveComponent.PK, job1Header3Inactive.FH_FC_CurrentComponent);

			// true for job 1
			job1Header1.FH_CompletionStatement = "We might get here";
			job1Header2.FH_CompletionStatement = "We might get here";
			Factory.Save();
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Moves to 3", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Moves to 3", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Does not move", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component3.PK, job2Header2.FH_FC_CurrentComponent);

			FilterStripsTestHelper.AddFilterStrips(link2_4.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition { });

			Factory.Save();
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Remains at 3", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Cannot be moved to 4, because no filters for the link 'component2 -> component4' and link is deactivated", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component3.PK, job2Header2.FH_FC_CurrentComponent);

			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Remains at 3", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Remains at 2", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("No further movement", component3.PK, job2Header2.FH_FC_CurrentComponent);

			FilterStripsTestHelper.AddFilterStrips(link3_4.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition { });
			Factory.Save();
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Remains at 3, link component3 -> component 4 is deactivated because no filters", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3, link component3 -> component 4 is deactivated because no filters", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Remains at 2", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3, link component3 -> component 4 is deactivated because no filters", component3.PK, job2Header2.FH_FC_CurrentComponent);

			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Remains at 3", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Remains at 2", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job2Header2.FH_FC_CurrentComponent);
			AssertEquals("No movement", inactiveComponent.PK, job1Header3Inactive.FH_FC_CurrentComponent);

			FilterStripsTestHelper.AddFilterStrips(link4_inactive.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition { });
			Factory.Save();
			RunTaskAndReleaseGateThenReload(serviceTask, system, ReleaseLogFailureService, job1Header1, job1Header2, job1Header3Inactive, job2Header1, job2Header2);
			AssertEquals("Remains at 3", component3.PK, job1Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job1Header2.FH_FC_CurrentComponent);
			AssertEquals("Remains at 2", component2.PK, job2Header1.FH_FC_CurrentComponent);
			AssertEquals("Remains at 3", component3.PK, job2Header2.FH_FC_CurrentComponent);
			AssertEquals("No movement - inactive", inactiveComponent.PK, job1Header3Inactive.FH_FC_CurrentComponent);

			AssertContainsExactLinesInAnyOrder("Correct Log", $@"Information|Zubin: Moved workflow Job1_1 (PK = {job1Header1.PK}, Job = Organization (MAIFRISTORG)) from component Bucket1 to Bucket2
Information|Zubin: Moved workflow Job3_1 (PK = {job3Header1.PK}, Job = Organization (RYANDZAY)) from component Bucket1 to Bucket2
Information|Zubin: Moved workflow Job1_2 (PK = {job1Header2.PK}, Job = Organization (MAIFRISTORG)) from component Bucket1 to Bucket2
Information|Zubin: Moved workflow Job3_3 (PK = {job3HeaderCompleted.PK}, Job = Organization (RYANDZAY)) from component Bucket1 to Bucket2
Information|Zubin: Moved workflow Job2_1 (PK = {job2Header1.PK}, Job = Organization (MAISCNDORG)) from component Bucket1 to Bucket2
Information|Zubin: Component link [Bucket1 -> Bucket2, Sequence: 0]: 5 workflows transferred
Information|Zubin: Set dedicated buffer Buffer on workflow We might get here (PK = {job1Header1.PK}, Job = Organization (MAIFRISTORG))
Information|Zubin: Set dedicated buffer Buffer on workflow We might get here (PK = {job1Header2.PK}, Job = Organization (MAIFRISTORG))
Information|Zubin: Component link [Bucket2 -> Buffer, Sequence: 0]: calculated dedicated buffer for 2 workflows, updated on 2 workflows
Warning|Zubin: The following component links have been deactivated since they have no filters or invalid filters:
Bucket2 -> Bucket3, Sequence: 0
Warning|Zubin: The following component links have been deactivated since they have no filters or invalid filters:
Buffer -> Bucket3, Sequence: 0
", log.ToString());
		}

		#endregion

		public void TestShouldNotApplyWorkflowTemplatesDuringServiceTaskRun()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "OPP");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "OPP");
			var templateTask = template.WorkflowItems.AddNew();
			templateTask.P9_Description = "Template task";

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Count);

			Db.Connection.ExecuteNonQuery(string.Format("DELETE dbo.ProcessTasks WHERE P9_ParentId = '{0}'", job.PK)); // This is a unit test

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);

			serviceTask.RunTask();

			var loadedJob = new BusinessObjectFactory().Load<OrgOpportunity>(job.PK);
			AssertEquals(0, loadedJob.WorkflowItems.Count);
		}
		
		void AddLinkFilterForTest(StmModuleFilter filterRule, ZString propertyValue)
		{
			FilterStripsTestHelper.AddFilterStrips(filterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = propertyValue,
			});
		}

		public void TestProcess_ShouldBypassInactiveBuffers_EvenIfMadeInactiveDuringRun()
		{
			var emailNotificationResource = Factory.NewWithValidTestData<GlbStaff>();
			emailNotificationResource.GS_EmailAddress = "test@test.com";

			var emailNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			emailNotificationGroup.Staff.Add(emailNotificationResource);

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailNotificationGroup.PK.ToGuid());

			var systemHappy = BMSTestHelper.CreateSystem(Factory);
			var systemSad = BMSTestHelper.CreateSystem(Factory);

			var bucketHappyBase = BMSTestHelper.CreateBucket(systemHappy, name: "happy bucket");
			var bucketHappy1 = BMSTestHelper.CreateBucket(systemHappy, name: "happy bucket 1");
			var bucketHappy2 = BMSTestHelper.CreateBucket(systemHappy, name: "happy bucket 2");

			var bucketSadBase = BMSTestHelper.CreateBucket(systemSad, name: "sad bucket");
			var bucketSad1 = BMSTestHelper.CreateBucket(systemSad, name: "sad bucket 1");
			var bucketSad2 = BMSTestHelper.CreateBucket(systemSad, name: "sad bucket 2");

			BMSTestHelper.LinkComponents(bucketHappyBase, bucketHappy2);
			BMSTestHelper.LinkComponents(bucketSadBase, bucketSad2);

			var linkToDeactivate_Live =
				BMSTestHelper.LinkComponents(
					bucketHappy2,
					bucketHappy1,
					sequence: 1,
					isReleaseGate: false); //no filter, but filter is required - this link will be deactivated

			var linkToDeactivate_NotLive =
				BMSTestHelper.LinkComponents(
					bucketSad2,
					bucketSad1,
					sequence: 2,
					isReleaseGate: false); //no filter, but filter is required - this link will be deactivated

			systemHappy.FS_Name = "Live System";
			systemHappy.FS_IsLive = true;

			systemSad.FS_Name = "NotLive System";
			systemSad.FS_IsLive = true;

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			serviceTask.OnSystemsLoadedAction = () =>
			{
				systemSad.FS_IsLive = false;
				Factory.Save();
			};
			var logger = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertMultilineASCIIEquals("",
@"Warning|Live System: The following component links have been deactivated since they have no filters or invalid filters:
happy bucket 2 -> happy bucket 1, Sequence: 1", logger.ToString());
			CombineAssertions("We finished the service task run and our system liveliness changed properly", () =>
			{
				AssertEquals("SystemHappy should still be alive!", true, systemHappy.FS_IsLive);
				AssertEquals("SystemSad should be dead! Sad!", false, systemSad.FS_IsLive);
			});
		}

		#region Branch Environment

		public void TestRun_WithFiltersThatRequireEnvironment_ShouldNotReportErrors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;

			FilterStripsTestHelper.AddFilterStrip<OpenTaskEstimateRangeFilter>(config.ComponentLink.FilterRule, OpenTaskEstimateRangeFilter.Schema.Identifier, filter =>
			{
				// This particular filter strip needs to load the company's task types (which is weird, and yet it's the world in which we live)
				filter.MinStdEstimate = new ZDateTime(ZDateTime.Now.Year, 1, 1, 0, 1, 0);
				filter.MaxStdEstimate = new ZDateTime(ZDateTime.Now.Year, 1, 1, 12, 0, 0);
				filter.Scope = ScopeRangeList.Codes.InsideRange;
			});

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Bucket);
			BMSTestHelper.CreateTask(workflow, lowEstMinutes: 10);

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);

			AssertNoExceptionThrown(serviceTask.RunTask);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			workflow.Reload();
			AssertEquals("The fact that the workflow moved proves that the link was processed. Otherwise this test passing would be meaningless.", config.Buffer.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestRun_WithParentJobFilter_ShouldNotReportErrors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Bucket);
			BMSTestHelper.CreateTask(workflow, lowEstMinutes: 10);

			FilterStripsTestHelper.AddFilterStrip<ParentJobModuleFilter>(config.ComponentLink.FilterRule, ProcessHeader.ModuleFilterConstants.ParentJob, filter =>
			{
				// This particular filter strip loads all the workflow provider module names by creating all workflow provider controllers,
				// and one of those controllers accesses the registry in its constructor.
				filter.SelectedModule = ModuleIDs.ProcessTasks.Name;
				filter.Property = workflow.FH_ParentId;
			});

			Factory.Save();

			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			InitialiseTaskSchedule(serviceTask);

			AssertNoExceptionThrown(serviceTask.RunTask);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

			workflow.Reload();
			AssertEquals("The fact that the workflow moved proves that the link was processed. Otherwise this test passing would be meaningless.", config.Buffer.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestRun_WhenAccessBufferBranch_ShouldNotReportErrorAndShouldUseBufferBranch()
		{
			BMSRegistry.Instance.LogWorkflowInfoOnTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			config.ComponentLink.FL_IsReleaseGateRuleApplied = false;
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", config.Bucket);
			BMSTestHelper.CreateTask(workflow, lowEstMinutes: 10);
			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
			var stmScheduleTask = Factory.LoadTop1<StmScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, TransferRuleRunnerServiceTask.Code));
			stmScheduleTask.S5_GB = testBranch.PK;
			config.Buffer.FC_GB_AgingBranch = testBranch.PK;

			Factory.Save();

			var log = InitialiseTaskSchedule(serviceTask);
			IBranch filterCurrentBranch = null;

			ZQuery GetQuery()
			{
				filterCurrentBranch = Env.CurrentBranch;
				return new ZQuery();
			}

			using (BMSGUITestHelper.SubstituteWithMockFilter(config.ComponentLink, mockFilterGetQueryMethod: GetQuery))
			{
				AssertNoExceptionThrown(serviceTask.RunTask);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}

			AssertEquals("Filter should use stmScheduleTask branch", testBranch.PK, filterCurrentBranch?.PK);
			AssertEquals("Workflow should move to buffer", config.Buffer.FC_Name, workflow.CurrentComponent.FC_Name);
			AssertContains("Should access the Parent Job to get its parent job description (Dummy Business Object Default)", $"Moved workflow Workflow (PK = {workflow.PK}, Job = Dummy Business Object Default) from component bucket to buffer", log.ToString());
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		static void RunTaskAndReleaseGateThenReload(TransferRuleRunnerServiceTask_ForTest task, BMSystem system, ReleaseGateFailureLogService_ForTest service, params BusinessObject[] bizosToReload)
		{
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			{
				task.RunTask();
			}

			ReleaseGateKeeperTest.RunReleaseGate(system, failureLogService: service);

			foreach (var bizo in bizosToReload)
			{
				bizo.Reload();
				bizo.Factory.ClearQueryCache(StmNoteSchema.Constants.TableName);
			}
		}

		protected override TransferRuleRunnerServiceTask GetNewServiceTask()
		{
			return new TransferRuleRunnerServiceTask_ForTest(ReleaseLogFailureService);
		}

		protected override void InitializeProcessHeaders()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			system.FS_Name = "Buckets R Us";

			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
			var bucket4 = BMSTestHelper.CreateBucket(system, "Bucket 4", sequence: 4);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var link3_4 = BMSTestHelper.LinkComponents(bucket3, bucket4);

			AddLinkFilterForTest(link1_2.FilterRule, "Todd Gurley III");
			AddLinkFilterForTest(link3_4.FilterRule, "Mark Ingram");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Todd Gurley III", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Mark Ingram", bucket3);

			var task = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			Factory.Save();
		}

		protected override void SetUpCore()
		{
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.LogWorkflowLoadTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Enterprise.VisualBoards.Business.Test.VisualBoardsTestCase.SetupAndClearTables();

			disposables = new DisposableList(1)
			{
				Enterprise.VisualBoards.Business.Test.VisualBoardsTestCase.DisableAsyncBehaviour()
			};

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}

	class TransferRuleRunnerServiceTask_ForTest : TransferRuleRunnerServiceTask
	{
		public TransferRuleRunnerServiceTask_ForTest(ReleaseGateFailureLogService_ForTest service)
		{
			this.service = service;
			this.branchPK = Env.CurrentBranchPK;
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

		protected override void RunTaskCore(CancellationToken token)
		{
			using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
			{
				base.RunTaskCore(token);
			}
		}

		protected override IPAVEProcessor GetProcessor(BMSystem system, ReleaseGateLogger releaseGateLogger)
		{
			var logger = new TransferRuleRunnerLogger(ServiceLogger, system);
			var dataAccessor = new TransferRuleRunnerDataAccessor(logger);

			return new TestTransferRuleRunner(system, ServiceLogger, transferRuleRunnerParams, dataAccessor);
		}

		public Action OnSystemsLoadedAction { get; set; }
	}

	class TransferRuleRunnerNonTransactionedTestTest : SystemSchematicServiceTaskNonTransactionedTestCase<TransferRuleRunnerServiceTask>
	{
		public void TestRun_WithMultipleSystems_ShouldNotReportDisposableActionForDbConnectionError_AndCurrentBranchNotSetError_WhenUsingParallelForEachForManySystems()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			for (int i = 1; i <= 100; i++)
			{
				var system = BMSTestHelper.CreateSystem(Factory, i.ToString().PadLeft(3, '0'));
				system.FS_Name = "system" + i;

				var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1 in system" + i);
				var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2 in system" + i);
				BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1, isReleaseGate: false);

				BMSTestHelper.CreateWorkflows(bucket1, 1, 1);
			}

			Factory.Save();

			var logService = new ReleaseGateFailureLogService_ForTest();
			var serviceTask = new TransferRuleRunnerServiceTask_ForTest(logService) { ServiceLogger = new BufferManagementLogger() };
			AssertNoExceptionThrown("Running these service tasks multi-threaded should not throw exceptions.Not Happy!", serviceTask.RunTask);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var buckets = newFactory.Load<BMComponent>(new ZQuery(BMComponentSchema.FC_Name, SQLComparisonOperator.StartsWith, "Bucket 2 in system"));
			var bucketPks = buckets.Select(b => b.PK).ToArray();
			var workflows = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_FC_CurrentComponent, SQLComparisonOperator.NotEqual, null)).ToArray();

			foreach (var wf in workflows)
			{
				Assert("Workflow should be transferred to bucket2!", bucketPks.Contains(wf.FH_FC_CurrentComponent));
			}

			AssertEquals("Running these service tasks multi-threaded should not report errors", 0, ErrorReporter.TotalErrorCount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();
		}
	}
}
