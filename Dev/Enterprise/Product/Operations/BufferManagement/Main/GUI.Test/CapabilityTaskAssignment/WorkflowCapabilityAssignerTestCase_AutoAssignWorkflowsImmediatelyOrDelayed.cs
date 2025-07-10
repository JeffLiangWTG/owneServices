using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	abstract class WorkflowCapabilityAssignerTestCase_AutoAssignWorkflowsImmediatelyOrDelayed : WorkflowCapabilityAssignerTestCase
	{
		#region Scheduled Assignment

		[TestDate(2021, 2, 22, 10, 0, 0)]
		public void TestShouldScheduleImmediately_AndAlsoScheduleAssignment_WhenItIsTooEarlyForSomeTasksToBeAssigned()
		{
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

			var scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			Assert("Precondition", !scheduledActions.Any());

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			var autoAssignmentActions = scheduledActions.Where(a => a.ActionCode == "ACT");

			AssertEquals("Should schedule one action for the workflow", 1, autoAssignmentActions.Count());
			var workflowAction = autoAssignmentActions.SingleOrDefault(a => a.TargetPK == workflow.PK);

			AssertNotNull(workflowAction);
			AssertEquals("Should schedule the earliest", timeIn20minutes, workflowAction.ExecutionDateTimeUtc);

			AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [AAA] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: (42.0 after assignment)
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001002, T00001003] requiring capability [BBB] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001004] requiring capability [CCC] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001005] requiring capability [DDD] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1] at [22-Feb-21 10:20:00]. The earliest tasks requiring assignment are as follows:
- [T00001002, T00001003] with required capability [BBB]
- [T00001005] with required capability [DDD]", log.ToString());

			task1_1.Reload();
			task1_2.Reload();
			task2_1.Reload();
			task2_2.Reload();
			task3_1.Reload();
			task4_1.Reload();
			task4_2.Reload();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be scheduled)", string.Empty, task2_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be scheduled)", string.Empty, task2_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task3_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early (should be scheduled)", string.Empty, task4_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource4", resource4.GS_Code, task4_2.P9_GS_NKAssignedStaffMember);
		}

		public void TestShouldUseAgingBranchAndDepartment_ForDelayedAssignment()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var agingBranch = Factory.NewWithValidTestData<GlbBranch>();
			agingBranch.GB_GC = company.PK;
			agingBranch.GB_Code = "BR1";

			var agingDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			agingDepartment.GE_Code = "DP1";

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();
			buffer.FC_GB_AgingBranch = agingBranch.PK;
			buffer.FC_GE_AgingDepartment = agingDepartment.PK;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "AAA";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes();

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.UtcNow); // just released
			Assert("Precondition", workflow.FH_AllowTaskAutoAssignment);

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			AssertNotEquals("Precondition", agingBranch.PK, Env.CurrentBranchPK);
			AssertNotEquals("Precondition", agingDepartment.PK, Env.CurrentDepartmentPK);

			var scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			Assert("Precondition", !scheduledActions.Any());

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			scheduledActions = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false }, new ZQuery()).Cast<IActionSchedule>().ToArray();
			var autoAssignmentActions = scheduledActions.Where(a => a.ActionCode == "ACT");

			AssertEquals("Should schedule one action for the workflow", 1, autoAssignmentActions.Count());
			var workflowAction = autoAssignmentActions.SingleOrDefault(a => a.TargetPK == workflow.PK);

			AssertNotNull(workflowAction);

			AssertEquals("Should store aging branch for execution", agingBranch.PK, workflowAction.ExecutionBranch);
			AssertEquals("Should store aging department for execution", agingDepartment.PK, workflowAction.ExecutionDepartment);
		}

		#endregion

		#region Run Auto Assignment

		protected override void RunAutoAssignmentAndGetLogCore(WorkflowCapabilityAssigner assigner, IWorkflowCapabilityAssignerDataAccessor dataAccessor, ILogger logger, BMComponent buffer, IEnumerable<ZGuid> workflowPKs)
		{
			assigner.AutoAssignWorkflowsImmediatelyOrDelayed(buffer.Factory, workflowPKs.Select(pk => pk.ToGuid()).ToArray(), logger);
		}

		#endregion

		protected override bool IsSchedulingAllowed => true;
	}
}
