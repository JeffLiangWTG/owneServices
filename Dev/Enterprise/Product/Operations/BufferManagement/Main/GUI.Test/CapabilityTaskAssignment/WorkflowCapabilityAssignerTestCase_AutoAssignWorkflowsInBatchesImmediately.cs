using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	abstract class WorkflowCapabilityAssignerTestCase_AutoAssignWorkflowsInBatchesImmediately : WorkflowCapabilityAssignerTestCase
	{
		#region Logging

		[TestDate(2015, 7, 14)]
		public void TestRun_LoggingShouldIndicateBatchSize()
		{
			const int numWorkflows = 50;
			const int numTasks = 10;

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
			BMSRegistry.Instance.CapabilityAutoAssignmentBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var capability = BMSTestHelper.CreateCapability(Factory, "SCH", "Schwifty", autoAssignTasks: true);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_AutoAssignTasksAge = new ZInt(10).GetDateTimeFromMinutes();

			BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RIK", "Rick", capability);
			BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MTY", "Morty", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			for (var i = 0; i < numWorkflows; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Wubba lubba dub dub " + i.ToString("00"), config.Buffer, autoAssignTasks: true);

				for (var j = 0; j < numTasks; j++)
				{
					BMSTestHelper.CreateTask(workflow, string.Empty, 1, capability: capability);
				}
			}

			Factory.Save();

			TestDateAttribute.AddMinutes(11);

			var log = RunAutoAssignmentWithTimeAdvancingLoggerAndGetLog(config.Buffer, null);

			AssertContains("Warning - Performance: Capability Task Auto-Assignment, Loading time: [00:06:00] Batches processed: [6]", log.ToString());
		}

		#endregion

		#region Simple Assignment

		[TestDate(2017, 5, 30)]
		public void TestAutoAssignWorkflows_BufferComponent_ChangingDuringPreviousBatches_ShouldNotThrowInvalidOperation()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket? bucket!");

			BMSRegistry.Instance.CapabilityAutoAssignmentBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AllowTaskAutoAssignment = true;

			helper.CreateResourceWithHomeBranchDeptSet("Bilbo Swaggins", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var allWorkflows = new List<ProcessHeader>();
			for (var i = 0; i < 11; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Many workflows making many manifests " + i.ToString("00"), buffer, autoAssignTasks: true);
				BMSTestHelper.CreateTask(workflow, string.Empty, 1, capability: capability);

				allWorkflows.Add(workflow);
			}

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(11);

			var batches = new List<ProcessHeader[]>();

			AssertNoExceptionThrown(() => RunAutoAssignmentAndGetLog(buffer, allWorkflows.Select(w => w.PK),
				preAssignmentAction: workflows =>
				{
					batches.Add(workflows);
					if (workflows.Length == 10)
					{
						var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
						var loadedWorkflow = newFactory.Load<ProcessHeader>(allWorkflows[10].PK);

						loadedWorkflow.FH_FC_CurrentComponent = bucket.PK;

						newFactory.Save();
					}
				}));

			AssertEquals(2, batches.Count);
			AssertArrayEqualsByElements("The first batch should contain the first 10 workflows", allWorkflows.Take(10).Select(x => x.FH_CompletionStatement).ToArray(), batches[0].Select(x => x.FH_CompletionStatement).ToArray());
			AssertEquals("The second batch should contain one workflow", 1, batches[1].Length);
			AssertEquals(allWorkflows[10].FH_CompletionStatement, batches[1].Single().FH_CompletionStatement);
			AssertEquals(bucket.PK, batches[1].Single().FH_FC_CurrentComponent);
		}

		#endregion

		#region Scheduling

		[TestDate(2021, 2, 22, 10, 0, 0)]
		public void TestShouldNotScheduleAssignment()
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
			Assert("Should not schedule autoassignment", !scheduledActions.Any());

			AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [AAA] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using capability auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 22-Feb-2021 20:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 0:20)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 22-Feb-2021 20:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 1:00)
Auto-assigning tasks using capability auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 22-Feb-2021 20:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 0:20)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			task1_1.Reload();
			task1_2.Reload();
			task2_1.Reload();
			task2_2.Reload();
			task3_1.Reload();
			task4_1.Reload();
			task4_2.Reload();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task2_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task2_2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task3_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned as it was too early", string.Empty, task4_1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource4", resource4.GS_Code, task4_2.P9_GS_NKAssignedStaffMember);
		}

		#endregion

		#region Run Auto Assignment

		protected ILogger RunAutoAssignmentWithTimeAdvancingLoggerAndGetLog(BMComponent buffer, IEnumerable<ZGuid> workflowPKs)
		{
			var assigner = new WorkflowCapabilityAssigner();
			var logger = new BufferManagementLogger();
			var dataAccessor = new CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLoggerDataAccessor(logger);
			RunAutoAssignmentAndGetLogCore(assigner, buffer, workflowPKs, logger, dataAccessor);
			return logger;
		}

		protected override void RunAutoAssignmentAndGetLogCore(WorkflowCapabilityAssigner assigner, IWorkflowCapabilityAssignerDataAccessor dataAccessor, ILogger logger, BMComponent buffer, IEnumerable<ZGuid> workflowPKs)
		{
			RunAutoAssignmentAndGetLogCore(assigner, buffer, workflowPKs, logger, dataAccessor);
		}

		void RunAutoAssignmentAndGetLogCore(WorkflowCapabilityAssigner assigner, BMComponent buffer, IEnumerable<ZGuid> workflowPKs, ILogger logger, IWorkflowCapabilityAssignerDataAccessor dataAccessor)
		{
			assigner.AutoAssignWorkflowsInBatchesImmediately(buffer, logger, dataAccessor, "Capability Task Auto-Assignment");
		}

		#endregion

		protected override bool IsSchedulingAllowed => false;
	}
}
