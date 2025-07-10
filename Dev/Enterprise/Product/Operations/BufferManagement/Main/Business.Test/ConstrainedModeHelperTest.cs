using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class ConstrainedModeHelperTest : BMSTestCaseWithFactory
	{
		#region CCR Resource And Tasks Determination

		public void TestShouldProcessInexistentStaffCodesCorrectly()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, "XYZ");

			AssertEquals("Should return false for unknown staff codes", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(Factory, "XYZ", config.Buffer));
			AssertEquals("Should return false for unknown staff codes", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(Factory, "XYZ", config.Buffer));
			AssertEquals("Should return false for unknown staff codes", false, ConstrainedModeHelper.IsCCRTask(task, workflow, config.Buffer));

			AssertEquals("Should return false for empty staff codes", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(Factory, ZString.Empty, config.Buffer));
			AssertEquals("Should return false for empty staff codes", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(Factory, ZString.Empty, config.Buffer));
		}

		public void TestIsCCRTaskShouldReturnFalseForWrongStaffCodes_EvenIsAssignedToCCRCapability()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability = BMSTestHelper.CreateCapability(Factory, "CCR", "capability containing CCRs only");
			config.CCR.Capabilities.Add(ccrCapability);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var correctCCRCapabilityTask = BMSTestHelper.CreateTask(workflow, sequence: 0, capability: ccrCapability);
			var taskWithCCRCapabilityAndWrongStaffCode = BMSTestHelper.CreateTask(workflow, "XYZ", sequence: 10, capability: ccrCapability);

			AssertEquals(true, ConstrainedModeHelper.IsCCRTask(correctCCRCapabilityTask, workflow, config.Buffer));
			AssertEquals("Should return false when assigned to a wrong staff code even if also assigned to CCR capability", false, ConstrainedModeHelper.IsCCRTask(taskWithCCRCapabilityAndWrongStaffCode, workflow, config.Buffer));
		}

		#endregion

		#region Task And Workflow Constraint Status

		public void TestConstraintStatus_WhenWorkflowNotInBuffer_ShouldBeUnknown()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();

			AssertEquals(ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(workflow));
		}

		public void TestPreConstraint_SameSequenceAsLastOpenCCRTask()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var ccrTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 0);
			var nonCCRTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 0);

			AssertEquals("GIVEN last-ccrTask.sequence = nonCCRTask.sequence", ccrTask.P9_Sequence, nonCCRTask.P9_Sequence);
			AssertEquals("WHEN calling GetConstraintStatus(nonCCRTask) THEN should return PreConstraint", ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(nonCCRTask));
		}

		public void TestPreConstraint()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var firstConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 0);
			var preConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);
			var secondConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 20);

			AssertEquals(ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(preConstraintTask));
		}

		public void TestPreConstraint_PreceedingCCRCapabilityTask()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability = BMSTestHelper.CreateCapability(Factory, "CCR", "capability containing CCRs only");
			config.CCR.Capabilities.Add(ccrCapability);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var firstConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 0);
			var preConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);
			var secondConstraintTask = BMSTestHelper.CreateTask(workflow, sequence: 20, capability: ccrCapability);

			AssertEquals(ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(preConstraintTask));
		}

		public void TestPostConstraint()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var constraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 20);
			var postConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 30);

			AssertEquals(ConstraintStatus.PostConstraint, ConstrainedModeHelper.GetConstraintStatus(postConstraintTask));
		}

		public void TestPostConstraint_FollowingCCRCapabilityTask()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability = BMSTestHelper.CreateCapability(Factory, "CCR", "capability containing CCRs only");
			config.CCR.Capabilities.Add(ccrCapability);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var constraintTask = BMSTestHelper.CreateTask(workflow, sequence: 20, capability: ccrCapability);
			var postConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 30);

			AssertEquals(ConstraintStatus.PostConstraint, ConstrainedModeHelper.GetConstraintStatus(postConstraintTask));
		}

		public void TestPreConstraint_CCRCantBePreConstraintOfItself()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var firstConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 10);
			var nonConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 20);
			var secondConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 30);

			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(firstConstraintTask));
			AssertEquals(ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(nonConstraintTask));
			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(secondConstraintTask));
		}

		public void TestPreConstraint_CCRCantBePreConstraintOfItself_WithCCRCapabilityTasks()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability = BMSTestHelper.CreateCapability(Factory, "CCR", "capability containing CCRs only");
			config.CCR.Capabilities.Add(ccrCapability);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var firstConstraintTask = BMSTestHelper.CreateTask(workflow, sequence: 10, capability: ccrCapability);
			var nonConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 20);
			var secondConstraintTask = BMSTestHelper.CreateTask(workflow, sequence: 30, capability: ccrCapability);

			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(firstConstraintTask));
			AssertEquals(ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(nonConstraintTask));
			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(secondConstraintTask));
		}

		public void TestPrePostConstraint_CCRIsPreConstraintOfOtherCCR()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var cCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CC2", "CCR2");
			cCR2.DesignateAsCCR(config.Buffer);
			var cCR3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CC3", "CCR3");
			cCR3.DesignateAsCCR(config.Buffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var ccr1Task = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 10);
			var ccr2Task = BMSTestHelper.CreateTask(workflow, cCR2.GS_Code, sequence: 20);
			var ccr3Task = BMSTestHelper.CreateTask(workflow, cCR3.GS_Code, sequence: 30);

			AssertEquals("Task should be ReadyForConstraint as CCR is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr1Task));
			AssertEquals("Task should be ReadyForConstraint as CCR is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr2Task));
			AssertEquals("Task should be ReadyForConstraint as CCR is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr3Task));
			AssertEquals("Workflow should be ReadyForConstraint as current task is ReadyForConstraint", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow));
		}

		public void TestPrePostConstraint_CCRIsPreConstraintOfOtherCCR_WithCCRCapabilityTasks()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability1 = BMSTestHelper.CreateCapability(Factory, "CCR", "capability 1 containing CCRs only");
			var ccrCapability2 = BMSTestHelper.CreateCapability(Factory, "CCR", "capability 2 containing CCRs only");
			var ccrCapability3 = BMSTestHelper.CreateCapability(Factory, "CCR", "capability 3 containing CCRs only");
			config.CCR.Capabilities.AddRange(new[] { ccrCapability1, ccrCapability2, ccrCapability3 });

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var ccr1Task = BMSTestHelper.CreateTask(workflow, sequence: 10, capability: ccrCapability1);
			var ccr2Task = BMSTestHelper.CreateTask(workflow, sequence: 20, capability: ccrCapability2);
			var ccr3Task = BMSTestHelper.CreateTask(workflow, sequence: 30, capability: ccrCapability3);

			AssertEquals("Task should be ReadyForConstraint as CCR capability is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr1Task));
			AssertEquals("Task should be ReadyForConstraint as CCR capability is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr2Task));
			AssertEquals("Task should be ReadyForConstraint as CCR capability is assigned", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(ccr3Task));
			AssertEquals("Workflow should be ReadyForConstraint as current task is ReadyForConstraint", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow)); // THIS MAY FAIL - PROBABLY WE NEED TO DEAL WITH WORKFLOW CONSTRAINT STATUS STRAIGHT AWAY
		}

		public void TestNonConstrained()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);

			AssertEquals(ConstraintStatus.NonConstrained, ConstrainedModeHelper.GetConstraintStatus(task));
			AssertEquals(ConstraintStatus.NonConstrained, ConstrainedModeHelper.GetConstraintStatus(workflow));
		}

		public void TestConstraintStatus_WhenWorkflowHasCCRAndNonCCRTasksWithTheSameSequence_ShouldBeConstraint()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var constraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 10);
			var nonConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow", config.Buffer);
			var nonConstraintTask2 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, sequence: 10);
			var constraintTask2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, sequence: 10);

			AssertEquals("Precondition CCR appearing first", constraintTask, workflow.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));
			AssertEquals("Precondition non-CCR appearing first", nonConstraintTask2, workflow2.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));

			CombineAssertions(() =>
			{
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow));
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow2));
				AssertEquals("jobHeader should be Unknown always", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));
				AssertEquals("jobHeader should be Unknown always", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader2));
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow.CurrentStatus);
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow2.CurrentStatus);
			});
		}

		public void TestConstraintStatus_WhenWorkflowHasCCRAndNonCCRTasksWithTheSameSequence_WithCCRCapabilityTasks_ShouldBeConstraint()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var ccrCapability = BMSTestHelper.CreateCapability(Factory, "CCR", "capability containing CCRs only");
			config.CCR.Capabilities.Add(ccrCapability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var constraintTask = BMSTestHelper.CreateTask(workflow, sequence: 10, capability: ccrCapability);
			var nonConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow", config.Buffer);
			var nonConstraintTask2 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, sequence: 10);
			var constraintTask2 = BMSTestHelper.CreateTask(workflow2, sequence: 10, capability: ccrCapability);

			AssertEquals("Precondition CCR appearing first", constraintTask, workflow.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));
			AssertEquals("Precondition non-CCR appearing first", nonConstraintTask2, workflow2.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));

			CombineAssertions(() =>
			{
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow));
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow2));
				AssertEquals("jobHeader should be Unknown always", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));
				AssertEquals("jobHeader should be Unknown always", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader2));
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow.CurrentStatus);
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow2.CurrentStatus);
			});
		}

		public void TestConstraintStatus_WhenWorkflowHasRFCAndPRETasksWithTheSameSequence_ShouldBeConstraint()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var cCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CC1", "CCR1");
			cCR1.DesignateAsCCR(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var firstConstraintTask = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, sequence: 10);
			var nonConstraintTask = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, sequence: 10);
			var secondConstraintTask = BMSTestHelper.CreateTask(workflow, cCR1.GS_Code, sequence: 20);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2", config.Buffer);
			var nonConstraintTask2 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, sequence: 10);
			var firstConstraintTask2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, sequence: 10);
			var secondConstraintTask2 = BMSTestHelper.CreateTask(workflow2, cCR1.GS_Code, sequence: 20);

			AssertEquals("Precondition CCR appearing first", firstConstraintTask, workflow.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));
			AssertEquals("Precondition non-CCR appearing first", nonConstraintTask2, workflow2.GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen));
			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(firstConstraintTask));
			AssertEquals(ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(nonConstraintTask));
			AssertEquals(ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(secondConstraintTask));

			CombineAssertions(() =>
			{
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow));
				AssertEquals("Workflow should be ReadyForConstraint no matter the order of tasks", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(workflow2));
				AssertEquals("jobHeader should be Unknown no matter the order of tasks", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));
				AssertEquals("jobHeader should be Unknown no matter the order of tasks", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader2));
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow.CurrentStatus);
				AssertMultilineASCIIEquals("Workflow Status should be Constraint no matter the order of tasks", @"buffer - Zone 3
	bufferConstraint", workflow2.CurrentStatus);
				AssertEquals("Job Workflow Status should be blank always, and yet...", string.Empty, jobHeader.CurrentStatus);
				AssertEquals("Job Workflow Status should be blank always, and yet...", string.Empty, jobHeader2.CurrentStatus);
			});
		}

		public void TestConstraintStatus_JobHeaderWithManyWorkflows_ShouldBeUnknown()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var cCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CC1", "CCR1");
			cCR1.DesignateAsCCR(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var unknownWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);

			AssertEquals("jobHeader should be Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));

			var nonConstrainedWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "non workflow", config.Buffer);
			var nonConstraintTask = BMSTestHelper.CreateTask(nonConstrainedWorkflow, config.NonCCR1.GS_Code, sequence: 10);

			AssertEquals("Workflow should now be NonConstrained", ConstraintStatus.NonConstrained, ConstrainedModeHelper.GetConstraintStatus(nonConstrainedWorkflow));
			AssertEquals("jobHeader should still be Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));

			var postConstrainedWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "post workflow", config.Buffer);
			var constraintTask = BMSTestHelper.CreateTask(postConstrainedWorkflow, config.CCR.GS_Code, sequence: 10);
			constraintTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var nonConstraintTask2 = BMSTestHelper.CreateTask(postConstrainedWorkflow, config.NonCCR1.GS_Code, sequence: 20);

			AssertEquals("workflow should be PostConstraint", ConstraintStatus.PostConstraint, ConstrainedModeHelper.GetConstraintStatus(postConstrainedWorkflow));
			AssertEquals("jobHeader should still be Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));

			var readyForConstraintWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "ready workflow", config.Buffer);
			var constraintTask2 = BMSTestHelper.CreateTask(readyForConstraintWorkflow, config.CCR.GS_Code, sequence: 10);

			AssertEquals("workflow should be ReadyForConstraint", ConstraintStatus.ReadyForConstraint, ConstrainedModeHelper.GetConstraintStatus(readyForConstraintWorkflow));
			AssertEquals("jobHeader should still be Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));

			var preConstrainedWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "pre workflow", config.Buffer);
			var nonConstraintTask4 = BMSTestHelper.CreateTask(preConstrainedWorkflow, config.NonCCR1.GS_Code, sequence: 10);
			var constraintTask3 = BMSTestHelper.CreateTask(preConstrainedWorkflow, config.CCR.GS_Code, sequence: 20);

			AssertEquals("workflow should be PreConstraint", ConstraintStatus.PreConstraint, ConstrainedModeHelper.GetConstraintStatus(preConstrainedWorkflow));
			AssertEquals("jobHeader should still be Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(jobHeader));
		}

		public void TestIsInConstrainedMode_ShouldNotHitBMSystemTable()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Test Factory" };
			var loadedBuffer = newFactory.Load<BMComponent>(config.Buffer.PK);

			using (AssertDbHitsForAllFactories(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 0 },
			}))
			{
				ConstrainedModeHelper.IsInConstrainedMode(loadedBuffer, config.ReleaseGroup.PK);
				ConstrainedModeHelper.IsInConstrainedMode(loadedBuffer, config.ReleaseGroup.PK); // multiple calls to prove BMSystemReleaseGroup is only hit once
				ConstrainedModeHelper.IsInConstrainedMode(loadedBuffer, config.ReleaseGroup.PK);
			}
		}

		public void TestIsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup_ShouldNotHitBMSystemTable()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { NameForDebugging = "Test Factory" };
			var loadedBuffer = newFactory.Load<BMComponent>(config.Buffer.PK);

			using (AssertDbHitsForAllFactories(new Dictionary<string, int>
			{
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 0 },
			}, ignoreUnspecified: true))
			{
				ConstrainedModeHelper.IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(Factory, config.CCR.GS_Code, loadedBuffer);
			}
		}

		#endregion

		#region Constraint status Query

		public void TestConstraintStatusQuery_NonConstrained()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var workflowWithNoTasks = BMSTestHelper.CreateWorkflow(Factory, "Workflow with no tasks", config.Buffer);

			var workflowWithClosedTask = BMSTestHelper.CreateWorkflow(Factory, "Workflow with all CLS tasks", config.Buffer);
			BMSTestHelper.CreateTask(workflowWithClosedTask, config.NonCCR1.GS_Code, sequence: 1, description: "Workflow with all CLS tasks: NonCCR1 task", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var query = ConstrainedModeHelper.GetConstraintStatusQuery(ConstraintStatus.NonConstrained);
			var actualWorkflows = Factory.Load<ProcessHeader>(query);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Empty workflows and closed workflows are not non-constrained", System.Array.Empty<ProcessHeader>(), actualWorkflows);

				AssertEquals("Workflow with no tasks should be flagged as Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(workflowWithNoTasks));
				AssertEquals("Workflow with all CLS tasks should be flagged as Unknown", ConstraintStatus.Unknown, ConstrainedModeHelper.GetConstraintStatus(workflowWithClosedTask));
			});
		}

		public void TestConstraintStatusQuery_WorkflowWithCCROnNonCurrentComponent()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var buffer2 = BMSTestHelper.CreateBuffer(config.System, "buffer2");
			config.NonCCR1.DesignateAsCCR(buffer2);

			Factory.Save();

			CombineAssertions("NCR1 should be considered as Non-CCR (even it is CCR but not in workflow current-component)", () =>
			{
				foreach (var testCase in BMSTestHelper.CreateConstraintStatusTestCases(Factory, config))
				{
					var query = ConstrainedModeHelper.GetConstraintStatusQuery(ConstrainedModeHelper.GetConstraintStatusFromString(testCase.ConstraintStatus));
					var actualWorkflows = Factory.Load<ProcessHeader>(query);

					AssertContainsExactElementsInAnyOrder(
						string.Format("{0} workflow should be found", testCase.ConstraintStatus),
						testCase.ExpectedProcessHeader,
						actualWorkflows);
				}
			});
		}

		public void TestConstraintStatusQuery_WorkflowWithCurrentComponentIsNonBuffer()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			CombineAssertions("Workflow should have its current-component with type BUFFER to be considered in Constraint-status query", () =>
			{
				foreach (var testCase in BMSTestHelper.CreateConstraintStatusTestCases(Factory, config, config.Bucket))
				{
					var query = ConstrainedModeHelper.GetConstraintStatusQuery(ConstrainedModeHelper.GetConstraintStatusFromString(testCase.ConstraintStatus));
					var actualWorkflows = Factory.Load<ProcessHeader>(query);

					Assert(string.Format("Given existing {0} workflow but not in BUFFER, should not be found", testCase.ConstraintStatus), actualWorkflows.Length == 0);
				}
			});
		}

		public void TestConstraintStatusQuery_MilestoneOrWorkflowTriggerOrException()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var testCases = BMSTestHelper.CreateConstraintStatusTestCases(Factory, config, config.Bucket);

			CombineAssertions("Should exclude Milestone/WorkflowTrigger/Exception in Constraint-status query", () =>
			{
				foreach (var nonTaskType in new[] { Constants.Workflow.ExceptionType, Constants.Workflow.MilestoneType, Constants.Workflow.WorkflowTriggerType })
				{
					foreach (var testCase in testCases)
					{
						testCase.ExpectedProcessHeader.GetTasksWithoutAccessingWorkflowParent().ToList().ForEach(t => t.P9_Type = nonTaskType);
					}

					Factory.Save();

					foreach (var testCase in testCases)
					{
						var query = ConstrainedModeHelper.GetConstraintStatusQuery(ConstrainedModeHelper.GetConstraintStatusFromString(testCase.ConstraintStatus));
						var actualWorkflows = Factory.Load<ProcessHeader>(query);

						Assert(string.Format("Given existing {0} workflow with ProcessTask.IsTask = false, should not be found", testCase.ConstraintStatus), actualWorkflows.Length == 0);
					}
				}
			});
		}

		public void TestConstraintStatusQuery()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);

			var ccr2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR2", "CR2");
			config.Staffs.Add(ccr2);
			ccr2.DesignateAsCCR(config.Buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(
				Factory,
				completionStatement: "Test Workflow",
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.Now,
				staffCode: config.CCR.GS_Code,
				lowEstMinutes: 15,
				description: config.CCR.GS_Code);
			var ccr2Task = BMSTestHelper.CreateTask(workflow, staffCode: ccr2.GS_Code, lowEstMinutes: 60, description: ccr2.GS_Code);
			var nonCCR1Task = BMSTestHelper.CreateTask(workflow, staffCode: config.NonCCR1.GS_Code, lowEstMinutes: 60, description: config.NonCCR1.GS_Code);
			var nonCCR2Task = BMSTestHelper.CreateTask(workflow, staffCode: config.NonCCR2.GS_Code, lowEstMinutes: 60, description: config.NonCCR2.GS_Code);

			AssertConstraintStatusQuery(config, workflow);
		}

		#endregion

		#region Implementation

		void AssertConstraintStatusQuery(ComplexConstrainedSchematicTestConfig config, ProcessHeader workflow)
		{
			int testIndex = 0;

			foreach (var testCase in ConstraintStatusTestHelper.SetupAndGetExpectedTestCases(workflow))
			{
				Factory.Save();

				var constraintStatus = testCase.Item1;

				AssertEquals("Expected status must be equal to GetConstraintStatus. Test case: " + testCase.Item2, constraintStatus, ConstrainedModeHelper.GetConstraintStatus(workflow));

				var query = ConstrainedModeHelper.GetConstraintStatusQuery(constraintStatus);
				var actualWorkflows = Factory.Load<ProcessHeader>(query);

				var ccr1Workflow = config.Workflows[0];
				var ncr1Workflow = config.Workflows[1];
				var ncr2Workflow = config.Workflows[2];

				var expectedWorkflows = new List<ProcessHeader>();
				expectedWorkflows.Add(workflow);

				switch (constraintStatus)
				{
					case ConstraintStatus.NonConstrained:
						expectedWorkflows.Add(ncr1Workflow);
						expectedWorkflows.Add(ncr2Workflow);
						AssertExactWorkflows("Querying NonConstraint, should find workflows with no CCR task.", expectedWorkflows.ToArray(), actualWorkflows);
						break;
					case ConstraintStatus.ReadyForConstraint:
						expectedWorkflows.Add(ccr1Workflow);
						AssertExactWorkflows("Querying ReadyForConstraint, should be find workflow with current task is CCR", expectedWorkflows.ToArray(), actualWorkflows);
						break;
					case ConstraintStatus.PreConstraint:
						AssertExactWorkflows("Querying PreConstraint, should only find workflow with first-open-task is NCR and last-CCR task's sequence < NCR", expectedWorkflows.ToArray(), actualWorkflows);
						break;
					case ConstraintStatus.PostConstraint:
						AssertExactWorkflows("Querying PostConstraint, should only find workflow with first-open-task is NCR and last-CCR task's sequence > NCR", expectedWorkflows.ToArray(), actualWorkflows);
						break;
					default:
						AssertEquals("Unknown constraint must return 0 results", 0, actualWorkflows.Length);
						break;
				}

				testIndex++;
			}
		}

		void AssertExactWorkflows(string assertMessage, ProcessHeader[] expectedWorkflows, ProcessHeader[] actualWorkflows)
		{
			var sortedExpectedWorkflows = expectedWorkflows.OrderBy(w => w.PK);
			var sortedActualWorkflows = actualWorkflows.OrderBy(w => w.PK);

			var expectedWorkflowsDescription = string.Join("\r\n ", sortedExpectedWorkflows.Select(w => w.FH_CompletionStatement).ToArray());
			var actualWorkflowsDescription = string.Join("\r\n", sortedActualWorkflows.Select(w => w.FH_CompletionStatement).ToArray());

			var message = string.Format("{0} \r\n\r\n* Expected: \r\n{1}\r\n\r\n* Actual: \r\n{2}\r\n\r\n.", assertMessage, expectedWorkflowsDescription, actualWorkflowsDescription);
			AssertArrayEqualsByElements(message, sortedExpectedWorkflows.ToArray(), sortedActualWorkflows.ToArray());
		}

		#endregion
	}
}
