using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class CapabilityTaskAssignmentSchedulerActionTest : TestCaseWithFactory
	{
		[TestDate(2021, 2, 22, 10, 0, 0)]
		public void TestExecute_ShouldAssignOrRescheduleAllCapabilityTasksInWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";

			var helper = new WorkflowCapabilityAssignerTestHelper(Factory);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "AAA";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes(); // has higher priority than the age on buffer

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "BBB";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes(); // has higher priority than the age on buffer

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "CCC";
			capability3.G4_AllowTaskAutoAssignment = true;

			var capability4 = Factory.NewWithValidTestData<GlbCapability>();
			capability4.G4_Code = "DDD";
			capability4.G4_AllowTaskAutoAssignment = true;
			capability4.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes(); // has higher priority than the age on buffer

			var timeIn20minutes = ZDateTime.UtcNow.AddMinutes(20);

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Aragorn II Elessar", capability2);
			var resource4 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability4);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.UtcNow); // just released
			Assert("Precondition", workflow.FH_AllowTaskAutoAssignment);

			var task1_1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1_1.RequiresResourceWithCapability);

			var task1_2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1_2.RequiresResourceWithCapability);

			var task2_1 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task2_1.RequiresResourceWithCapability);

			var task2_2 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task2_2.RequiresResourceWithCapability);

			var task3_1 = helper.CreateTask(workflow, capability3, null, 60);
			Assert(task3_1.RequiresResourceWithCapability);

			var task4_1 = helper.CreateTask(workflow, capability4, null, 60);
			Assert(task4_1.RequiresResourceWithCapability);

			var task4_2 = helper.CreateTask(workflow, capability4, resource4, 60);
			Assert(!task4_2.RequiresResourceWithCapability);

			Factory.Save();

			Assert("Precondition", !TaskAssignmentHelper.DifRestrictionsExistInRegistry(Factory));
			Assert("Precondition: Capacity Calculations should not be disabled in the registry", !BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			Assert("Precondition", !scheduledActions.Any());

			var action = new CapabilityTaskAssignmentSchedulerAction();
			var logger = new BufferManagementLogger();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				action.Execute(new CancellationToken(), Factory, logger, workflow.PK, "FH", null, ZDateTime.UtcNow);
			}

			scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			var autoAssignmentActions = scheduledActions.Where(a => a.ActionCode == "ACT");

			AssertEquals("Should schedule one action for the workflow", 1, autoAssignmentActions.Count());
			var workflowAction = autoAssignmentActions.SingleOrDefault(a => a.TargetPK == workflow.PK);

			AssertNotNull(workflowAction);
			AssertEquals("Should schedule the earliest", timeIn20minutes, workflowAction.ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [AAA] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 16.0 (13.0 after assignment)
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001002, T00001003] requiring capability [BBB] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001004] requiring capability [CCC] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001005] requiring capability [DDD] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1] at [22-Feb-21 10:20:00]. The earliest tasks requiring assignment are as follows:
- [T00001002, T00001003] with required capability [BBB]
- [T00001005] with required capability [DDD]".StripTaskIds(), logger.ToString().StripTaskIds());

			task1_1.Reload();
			task1_2.Reload();
			task2_1.Reload();
			task2_2.Reload();
			task3_1.Reload();
			task4_1.Reload();
			task4_2.Reload();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task2_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task2_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task3_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task4_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource4", resource4.GS_Code, task4_2.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2021, 2, 22, 10, 0, 0)]
		public void TestExecute_ShouldAssignOrRescheduleAllCapabilityTasksInWorkflowByReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";

			var helper = new WorkflowCapabilityAssignerTestHelper(Factory);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var groupAlpha = BMSTestHelper.CreateGroup(Factory, "ALPRG", "Alpha");
			var groupDos = BMSTestHelper.CreateGroup(Factory, "DOSRG", "Dos");
			var groupGimel = BMSTestHelper.CreateGroup(Factory, "GIMRG", "Gimel");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "AAA";
			capability1.G4_AllowTaskAutoAssignment = false;
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var groupPivot11 = capability1.ReleaseGroupPivots.AddNew();
			groupPivot11.GGC_GG_Group = groupAlpha.PK;
			groupPivot11.GGC_AllowTaskAutoAssignment = true;
			groupPivot11.GGC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var groupPivot12 = capability1.ReleaseGroupPivots.AddNew();
			groupPivot12.GGC_GG_Group = groupDos.PK;
			groupPivot12.GGC_AllowTaskAutoAssignment = true;
			groupPivot12.GGC_AutoAssignTasksAge = new ZInt(25).GetDateTimeFromMinutes();

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "BBB";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes(); // has higher priority than the age on buffer
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var groupPivot2 = capability2.ReleaseGroupPivots.AddNew();
			groupPivot2.GGC_GG_Group = groupGimel.PK;
			groupPivot2.GGC_AllowTaskAutoAssignment = true;
			groupPivot2.GGC_AutoAssignTasksAge = new ZInt(30).GetDateTimeFromMinutes();

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "CCC";
			capability3.G4_AllowTaskAutoAssignment = true;
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability4 = Factory.NewWithValidTestData<GlbCapability>();
			capability4.G4_Code = "DDD";
			capability4.G4_AllowTaskAutoAssignment = true;
			capability4.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes(); // has higher priority than the age on buffer
			capability4.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var timeIn20minutes = ZDateTime.UtcNow.AddMinutes(20);

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Aragorn II Elessar", capability2);
			var resource4 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability4);

			resource1.Groups.Add(groupAlpha);
			resource1.Groups.Add(groupDos);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var workflow1 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 1");
			workflow1.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow2 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 2");
			workflow2.FH_GG_ReleaseGroup = groupDos.PK;
			var workflow3 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 3");
			workflow3.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow4 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 4");
			workflow4.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow5 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 5");
			workflow5.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow6 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 6");
			workflow6.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow7 = helper.CreateWorkflow(job, true, buffer, ZDateTime.UtcNow, workflowCompletionStatement: "Workflow 7");
			workflow7.FH_GG_ReleaseGroup = groupAlpha.PK;

			Assert("Precondition", workflow1.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow2.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow3.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow4.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow5.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow6.FH_AllowTaskAutoAssignment);
			Assert("Precondition", workflow7.FH_AllowTaskAutoAssignment);
			Assert("Precondition: Capacity Calculations should not be disabled in the registry", !BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var task1_1 = helper.CreateTask(workflow1, capability1, null, 60);
			Assert(task1_1.RequiresResourceWithCapability);

			var task1_2 = helper.CreateTask(workflow2, capability1, null, 60);
			Assert(task1_2.RequiresResourceWithCapability);

			var task2_1 = helper.CreateTask(workflow3, capability2, null, 60);
			Assert(task2_1.RequiresResourceWithCapability);

			var task2_2 = helper.CreateTask(workflow4, capability2, null, 60);
			Assert(task2_2.RequiresResourceWithCapability);

			var task3_1 = helper.CreateTask(workflow5, capability3, null, 60);
			Assert(task3_1.RequiresResourceWithCapability);

			var task4_1 = helper.CreateTask(workflow6, capability4, null, 60);
			Assert(task4_1.RequiresResourceWithCapability);

			var task4_2 = helper.CreateTask(workflow7, capability4, resource4, 60);
			Assert(!task4_2.RequiresResourceWithCapability);

			Factory.Save();

			Assert("Precondition", !TaskAssignmentHelper.DifRestrictionsExistInRegistry(Factory));

			var scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			Assert("Precondition", !scheduledActions.Any());

			var action = new CapabilityTaskAssignmentSchedulerAction();
			var logger = new BufferManagementLogger();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				action.Execute(new CancellationToken(), Factory, logger, workflow1.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow2.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow3.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow4.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow5.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow6.PK, "FH", null, ZDateTime.UtcNow);
				action.Execute(new CancellationToken(), Factory, logger, workflow7.PK, "FH", null, ZDateTime.UtcNow);
			}

			AssertMultilineASCIIEquals("Service task log", $@"Auto-assigning tasks using release group capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [AAA] in workflow [Organization (MAIORGSYD) - Workflow 1 (Alpha)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 16.0 (14.5 after assignment)
Auto-assigning tasks using release group capability auto-assigning task age.
Tasks [T00001001] requiring capability [AAA] in workflow [Organization (MAIORGSYD) - Workflow 2 (Dos)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 2 (Dos)] at [22-Feb-21 10:25:00]. The earliest tasks requiring assignment are as follows:
- [T00001001] with required capability [AAA]
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001002] requiring capability [BBB] in workflow [Organization (MAIORGSYD) - Workflow 3 (Alpha)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 3 (Alpha)] at [22-Feb-21 10:20:00]. The earliest tasks requiring assignment are as follows:
- [T00001002] with required capability [BBB]
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001003] requiring capability [BBB] in workflow [Organization (MAIORGSYD) - Workflow 4 (Alpha)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 4 (Alpha)] at [22-Feb-21 10:20:00]. The earliest tasks requiring assignment are as follows:
- [T00001003] with required capability [BBB]
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001004] requiring capability [CCC] in workflow [Organization (MAIORGSYD) - Workflow 5 (Alpha)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 5 (Alpha)] at [22-Feb-21 11:00:00]. The earliest tasks requiring assignment are as follows:
- [T00001004] with required capability [CCC]
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001005] requiring capability [DDD] in workflow [Organization (MAIORGSYD) - Workflow 6 (Alpha)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 6 (Alpha)] at [22-Feb-21 10:20:00]. The earliest tasks requiring assignment are as follows:
- [T00001005] with required capability [DDD]".StripTaskIds(), logger.ToString().StripTaskIds());

			task1_1.Reload();
			task1_2.Reload();
			task2_1.Reload();
			task2_2.Reload();
			task3_1.Reload();
			task4_1.Reload();
			task4_2.Reload();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task1_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task2_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task2_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task3_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be rescheduled)", string.Empty, task4_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource4", resource4.GS_Code, task4_2.P9_GS_NKAssignedStaffMember);
		}

		public void TestExecute_WhenCapacityCalculationDisabled_ShouldNotExecute_WithLogMessage()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";
			var helper = new WorkflowCapabilityAssignerTestHelper(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.UtcNow); // just released

			Factory.Save();

			Assert("Precondition", workflow.FH_AllowTaskAutoAssignment);
			Assert("Precondition", !TaskAssignmentHelper.DifRestrictionsExistInRegistry(Factory));
			Assert("Precondition: Capacity Calculations should be disabled in the registry", BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			Assert("Precondition", !scheduledActions.Any());

			var action = new CapabilityTaskAssignmentSchedulerAction();
			var logger = new BufferManagementLogger();
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				AssertEquals(CapabilityTaskAssignmentSchedulerAction.CapacityCalculationsDisabledResult, action.Execute(new CancellationToken(), Factory, logger, workflow.PK, "FH", null, ZDateTime.UtcNow));
			}

			AssertMultilineASCIIEquals("Scheduled Action log", "Capability Task Auto-Assignment not run because the [Disable Capacity Calculations] registry item is enabled.", logger.ToString());
			AssertNotEquals("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
