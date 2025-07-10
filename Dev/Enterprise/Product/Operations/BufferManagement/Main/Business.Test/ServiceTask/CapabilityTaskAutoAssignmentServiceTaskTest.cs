using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	abstract class CapabilityTaskAutoAssignmentServiceTaskTest : BMServiceTaskWithFilteredReaderTestCase<CapabilityTaskAutoAssignmentServiceTask>, IWorkflowCapabilityAssignerTestCase
	{
		#region Auto Assign Age Overrides

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignTaskAgeWithOverridden_EmptyTaskAgeFallBack()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(180).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var @group = Factory.NewWithValidTestData<GlbGroup>();
			@group.GG_Desc = "Group";

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);

			resource1.Groups.Add(@group);
			resource2.Groups.Add(@group);

			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = @group.PK;
			link.FO_AutoAssignTasksAge = new ZInt(120).GetDateTimeFromMinutes();

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-1));
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability2, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertEquals(ZDateTime.DefaultDurationEpoch, capability1.G4_AutoAssignTasksAge);
			AssertEquals(true, capability1.G4_AllowTaskAutoAssignment);
			AssertEquals(ZDateTime.Empty, capability2.G4_AutoAssignTasksAge);
			AssertEquals(true, capability2.G4_AllowTaskAutoAssignment);
			AssertEquals((ZDateTime)TimeSpan.FromHours(2), link.FO_AutoAssignTasksAge);

			AssertEquals("Should have auto-assigned for task1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have no auto-assigned to task2", ZString.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using capability auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] since it was released at 30-Jul-2013 09:00:00, which is 1:00 working hours ago. (Minimum hours before auto assignment is 2:00)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignTaskAgeWithOverridden_ReleaseGroupLinkOverBMComponent()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(180).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "Group 1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "Group 2";

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability);

			resource1.Groups.Add(group1);
			resource2.Groups.Add(group2);

			var link1 = buffer.ReleaseGroupLinks.AddNew();
			link1.FO_GG_ReleaseGroup = group1.PK;

			var link2 = buffer.ReleaseGroupLinks.AddNew();
			link2.FO_GG_ReleaseGroup = group2.PK;
			link2.FO_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-2));
			workflow.FH_GG_ReleaseGroup = group2.PK;

			var task1 = CreateTask(workflow, capability, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 2)] to resource [Samwise Gamgee], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 45.0 (42.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignTaskAgeWithOverridden_CapacityOverBMComponent()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(180).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-2));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability2, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertEquals("Should not have auto-assigned to resource1", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 08:00:00, which is 1:00 working hours ago. (Minimum hours before auto assignment is 3:00)
Information|Auto-assigning tasks using capability auto-assigning task age.
Information|Assigned tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignTaskAgeWithOverridden_CapacityOverReleaseGroupLinkAndBMComponent()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_AutoAssignTasksAge = new ZInt(20).GetDateTimeFromMinutes();

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "FCU";
			capability3.G4_AllowTaskAutoAssignment = true;
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Group 1";

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability3);

			resource1.Groups.Add(group);
			resource2.Groups.Add(group);

			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_AutoAssignTasksAge = new ZInt(40).GetDateTimeFromMinutes();

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddMinutes(-20));
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability2, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability3, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource2", string.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task2.P9_GS_NKAssignedStaffMember);

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-40);

			Factory.Save();

			task.RunTask();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task3.P9_GS_NKAssignedStaffMember);

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-60);

			Factory.Save();

			task.RunTask();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task3.P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using capability auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] since it was released at 30-Jul-2013 09:40:00, which is 0:20 working hours ago. (Minimum hours before auto assignment is 0:40)
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] since it was released at 30-Jul-2013 09:40:00, which is 0:20 working hours ago. (Minimum hours before auto assignment is 0:40)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Assigned tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [FCU] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using component release group link auto-assigning task age.
Information|Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [FCU] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		#endregion

		#region Logging

		[TestDate(2013, 7, 30)]
		public void TestRun_LogStringShowsHours()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_AutoAssignTasksAge = new ZInt(26 * 60).GetDateTimeFromMinutes();

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Desc = "Release Group";
			releaseGroup.Staff.Add(resource1);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddMinutes(-25 * 60));
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertEquals("Should have not auto-assigned to resource1", string.Empty, task1.P9_GS_NKAssignedStaffMember);

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-900 * 60 + 1);

			Factory.Save();

			task.RunTask();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);

			Factory.Save();

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using capability auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] since it was released at 29-Jul-2013 09:00:00, which is 9:00 working hours ago. (Minimum hours before auto assignment is 26:00)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using capability auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

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

			var task = new CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLogger();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			AssertContains("Warning|Performance: Capability Task Auto-Assignment, Loading time: [00:06:00] Batches processed: [6]", log.ToString());
		}

		#endregion

		#region Simple Assignment

		[TestDate(2013, 7, 30)]
		public void TestRun_SimpleAutoAssign()
		{
			var task = new CapabilityTaskAutoAssignmentServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun) => taskToRun.RunTask();

			AssertSimpleAutoAssign(task, runTaskAction);
		}

		void AssertSimpleAutoAssign(CapabilityTaskAutoAssignmentServiceTask task, Action<CapabilityTaskAutoAssignmentServiceTask> runTaskAction)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();
			buffer.FC_GB_AgingBranch = branch.PK;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			Assert("Precondition", !TaskAssignmentHelper.DifRestrictionsExistInRegistry(Factory));

			var log = InitialiseTaskSchedule(task);

			runTaskAction(task);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignToMemberOfReleaseGroup()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "DPR", "DPIB Review");
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Igor", capability);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Dave", capability);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Baaber", capability);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "Group 1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "Group 2";
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Desc = "Group 3";

			resource1.Groups.Add(group1);
			resource2.Groups.Add(group2);
			resource3.Groups.Add(group3);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "boop", buffer, ZDateTime.Today.AddDays(-1), group1.PK);
			workflow1.FH_AllowTaskAutoAssignment = true;

			var task1 = CreateTask(workflow1, capability, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow1, capability, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Fun", buffer, ZDateTime.Today.AddDays(-1), group2.PK);
			workflow2.FH_AllowTaskAutoAssignment = true;
			var task3 = CreateTask(workflow2, capability, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow2, capability, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var tasksForWorkflow1 = loadedWorkflow1.Tasks.ToArray();
			var tasksForWorkflow2 = loadedWorkflow2.Tasks.ToArray();

			AssertEquals("Should have auto-assigned to resource1 because resource is in releasegroup for workflow", resource1.GS_Code, tasksForWorkflow1[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1 because resource is in releasegroup for workflow", resource1.GS_Code, tasksForWorkflow1[1].P9_GS_NKAssignedStaffMember);

			AssertEquals("Should have auto-assigned to resource2 because it is the only resource in capability in the releasegroup of the workflow", resource2.GS_Code, tasksForWorkflow2[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Resource 3 was directly assigned to this task. So should ignore capability", resource3.GS_Code, tasksForWorkflow2[1].P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_SimpleAutoAssign_OneCapabilityDoesNotAutoAssignTasks()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = false;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT auto-assign since the capability doesn't allow it", string.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldNotAutoAssignOutsideReleaseGroup()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "Group 1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "Group 2";

			resource1.Groups.Add(group1);
			resource1.Groups.Add(group2);
			resource2.Groups.Add(group2);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_GG_ReleaseGroup = group1.PK;

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT have auto-assigned to resource2 outside workflow release group", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			workflow.FH_GG_ReleaseGroup = group2.PK;
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 2)] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_InactiveStaff()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			resource2.GS_IsActive = false;
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT have assigned to inactive resource2", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			resource2.GS_IsActive = true;
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssign_ShouldNotAssignCancelledTasks()
		{
			var task = new CapabilityTaskAutoAssignmentServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun) => taskToRun.RunTask();

			AssertShouldNotAssignCancelledOrClosedTasks(task, runTaskAction);
		}

		void AssertShouldNotAssignCancelledOrClosedTasks(CapabilityTaskAutoAssignmentServiceTask task, Action<CapabilityTaskAutoAssignmentServiceTask> runTaskAction)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			config.Buffer.FC_AutoAssignTasksAge = new ZInt(1).GetDateTimeFromMinutes();
			capability.G4_AllowTaskAutoAssignment = true;

			resource.Capabilities.Add(capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "168 Job Days", config.Buffer, autoAssignTasks: true);

			var task1 = BMSTestHelper.CreateTask(workflow, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, capability: capability, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task3 = BMSTestHelper.CreateTask(workflow, capability: capability, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);

			var log = InitialiseTaskSchedule(task);

			runTaskAction(task);

			var newFactory = Factory.CreateNewFactory();
			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			var loadedTask3 = newFactory.Load<ProcessTask>(task3.PK);

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign cancelled task", string.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign closed task", string.Empty, task3.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2017, 6, 30)]
		public void TestAutoAssignWorkflows_WhenFirstWorkflowInBatchIsInBucket_ShouldNotCauseDeveloperError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			config.Bucket.FC_AutoAssignTasksAge = new ZInt(1).GetDateTimeFromMinutes();
			capability.G4_AllowTaskAutoAssignment = true;

			resource.Capabilities.Add(capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "168 Job Days", config.Bucket, autoAssignTasks: true);

			BMSTestHelper.CreateTask(workflow, capability: capability);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask();
			InitialiseTaskSchedule(serviceTask);

			AssertNoExceptionThrown(@"If the service task runs on a workflow in a bucket, an exception will be thrown, so the service task should ignore bucket components.", () => serviceTask.RunTask());
		}

		[TestDate(2017, 5, 30)]
		public void TestAutoAssignWorkflows_BufferComponent_ShouldNotThrowInvalidOperation()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket? bucket!");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AllowTaskAutoAssignment = true;

			var resource = CreateResourceWithHomeBranchDeptSet("Bilbo Swaggins", capability);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task = CreateTask(workflow, capability, null, 60);
			Assert("Pre-condition", task.RequiresResourceWithCapability);

			Factory.Save();

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask_ForTest();
			InitialiseTaskSchedule(serviceTask);

			serviceTask.PreAssignmentAction_ForTest = workflows =>
			{
				workflow.FH_FC_CurrentComponent = bucket.PK;
				Factory.Save();
			};
			AssertNoExceptionThrown(() => serviceTask.RunTask());

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertEquals("Should have auto-assigned task to correct resource", resource.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2017, 5, 30)]
		public void TestAutoAssignWorkflows_BufferComponent_ChangingDuringPreviousBatches_ShouldNotThrowInvalidOperation()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket? bucket!");

			BMSRegistry.Instance.CapabilityAutoAssignmentBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AllowTaskAutoAssignment = true;

			CreateResourceWithHomeBranchDeptSet("Bilbo Swaggins", capability);

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
			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask_ForTest()
			{
				PreAssignmentAction_ForTest = workflows =>
				{
					batches.Add(workflows);
					if (workflows.Length == 10)
					{
						var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
						var loadedWorkflow = newFactory.Load<ProcessHeader>(allWorkflows[10].PK);

						loadedWorkflow.FH_FC_CurrentComponent = bucket.PK;

						newFactory.Save();
					}
				}
			};
			InitialiseTaskSchedule(serviceTask);

			AssertNoExceptionThrown(() => serviceTask.RunTask());
			AssertEquals(2, batches.Count);
			AssertArrayEqualsByElements("The first batch should contain the first 10 workflows", allWorkflows.Take(10).Select(x => x.FH_CompletionStatement).ToArray(), batches[0].Select(x => x.FH_CompletionStatement).ToArray());
			AssertEquals("The second batch should contain one workflow", 1, batches[1].Length);
			AssertEquals(allWorkflows[10].FH_CompletionStatement, batches[1].Single().FH_CompletionStatement);
			AssertEquals(bucket.PK, batches[1].Single().FH_FC_CurrentComponent);
		}

		#endregion

		#region Flags to Disable Assignment

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldNotAutoAssign_WhenTaskAutoAssignment_IsNotAllowedOnComponent()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			tasks = loadedWorkflow.Tasks.ToArray();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldNotAutoAssign_WhenTaskAutoAssignment_IsNotAllowedOnWorkflow()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(false, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			workflow.FH_AllowTaskAutoAssignment = true;
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_InactiveWorkflow()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_IsActive = false;

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			workflow.FH_IsActive = true;
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2021, 1, 18)]
		public void TestRun_NonReleasedWorkflow()
		{
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			bucket.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, bucket, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			workflow.MoveToComponent(buffer);
			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2021, 1, 18)]
		public void TestRun_DeletedWorkflow()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer, ZDateTime.Today.AddDays(-1), autoAssignTasks: true);

			var task1 = helper.CreateTask(workflow1, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow1, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", buffer, ZDateTime.Today.AddDays(-1), autoAssignTasks: true);
			workflow2.FH_AllowTaskAutoAssignment = true;
			var task3 = helper.CreateTask(workflow2, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow2, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			workflow2.Delete();

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var tasks1 = loadedWorkflow1.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks1[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks1[1].P9_GS_NKAssignedStaffMember);

			var loadedWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			AssertNull(loadedWorkflow2);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (XVBQP68SIYXQ) - workflow1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		#endregion

		#region Cross-Country Auto Assignment

		[TestDate(2018, 8, 20)]
		public void TestAutoAssigner_ShouldAssignRegardlessOfCompany_ShouldAssignForeignTaskToLocalResource()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "MEL";
			newBranch.GB_RL_NKHomePort = "AUMEL";

			var task = GetNewServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun)
			{
				// run service task in a foreign context
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					taskToRun.RunTask();
				}
			}

			AssertAutoAssignmentWorksRegardlessOfCountry(newBranch.PK.ToGuid(), newCompany.CompanyName, task, runTaskAction);
		}

		[TestDate(2018, 8, 20)]
		public void TestAutoAssigner_ShouldAssignRegardlessOfCompany_ShouldAssignLocalTaskToForeignResource()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "MEL";
			newBranch.GB_RL_NKHomePort = "AUMEL";

			var task = GetNewServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun) => task.RunTask();

			AssertAutoAssignmentWorksRegardlessOfCountry(newBranch.PK.ToGuid(), newCompany.CompanyName, task, runTaskAction);
		}

		void AssertAutoAssignmentWorksRegardlessOfCountry(Guid newBranchPK, string newCompanyName, CapabilityTaskAutoAssignmentServiceTask task, Action<CapabilityTaskAutoAssignmentServiceTask> runTaskAction)
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_AutoAssignTasksAge = buffer.FC_AutoAssignTasksAge;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);

			Factory.Save();

			ProcessHeader workflow1;
			ProcessHeader workflow2;
			ProcessTask task1;
			ProcessTask task2;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranchPK, Env.CurrentDepartmentPK))
			{
				workflow1 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
				workflow1.FH_IsActive = false;
				workflow1.FH_CompletionStatement = "I am number one";

				task1 = CreateTask(workflow1, capability1, null, 60);
				Assert(task1.RequiresResourceWithCapability);
			}

			workflow2 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "MAIORMEL");
			workflow2.FH_IsActive = false;
			workflow2.FH_CompletionStatement = "That man is a fraud one";

			task2 = CreateTask(workflow2, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			AssertEquals("PRE: We expect that since we made this task in a different UserContext, they have a new Branch.", task1.Company.CompanyName, newCompanyName);
			AssertEquals("PRE: We expect that since we made this task locally, they have the CurrentBranch.", task2.Company.CompanyName, Env.CurrentCompany.Name);

			var log = InitialiseTaskSchedule(task);

			runTaskAction(task);

			var newFactory = Factory.CreateNewFactory();
			var reloadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var reloadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals("Should not have auto-assigned", ZString.Empty, reloadedTask1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, reloadedTask2.P9_GS_NKAssignedStaffMember);

			workflow1.FH_IsActive = true;
			workflow2.FH_IsActive = true;
			Factory.Save();

			runTaskAction(task);

			newFactory = Factory.CreateNewFactory();
			var reReloadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var reReloadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, reReloadedTask1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, reReloadedTask2.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_SimpleAutoAssign_ShouldIgnoreCompanyServiceTaskCurrentCompany()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "MEL";
			newBranch.GB_RL_NKHomePort = "AUMEL";

			AssertNotEquals("PRE: We want our newCompany to NOT be the currentCompany!", newCompany.PK, Env.CurrentCompany.PK);
			AssertNotEquals("PRE: We want our newBranch to NOT be the currentBranch!", newBranch.PK, Env.CurrentBranch.PK);

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					taskToRun.RunTask();
				}
			}

			AssertSimpleAutoAssign(task, runTaskAction);
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssign_ShouldNotAssignCancelledTasks_EvenAcrossCompanies()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "MEL";
			newBranch.GB_RL_NKHomePort = "AUMEL";

			AssertNotEquals("PRE: We want our newCompany to NOT be the currentCompany!", newCompany.PK, Env.CurrentCompany.PK);
			AssertNotEquals("PRE: We want our newBranch to NOT be the currentBranch!", newBranch.PK, Env.CurrentBranch.PK);

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			void runTaskAction(CapabilityTaskAutoAssignmentServiceTask taskToRun)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					taskToRun.RunTask();
				}
			}

			AssertShouldNotAssignCancelledOrClosedTasks(task, runTaskAction);
		}

		#endregion

		#region Auto Assign Age

		[TestDate(2013, 7, 30)]
		public void TestRun_NotReleasedWithinTargetAge()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Now);

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 10:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 1:00)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 10:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 1:00)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]
Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssignAge_WhenLocalTimeOfServiceTaskIsDifferentToBufferTime_ShouldUseBufferLocalTime()
		{
			var bufferBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			bufferBranch.GB_Code = "MEL";
			bufferBranch.GB_RL_NKHomePort = "AUMEL";

			var serviceTaskBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			serviceTaskBranch.GB_Code = "LON";
			serviceTaskBranch.GB_RL_NKHomePort = "GBLON";

			const int autoAssignDelayMinutes = 10;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = bufferBranch.PK;
			config.Buffer.FC_AutoAssignTasksAge = new ZInt(autoAssignDelayMinutes).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "NO", "No one's found out about this defect", autoAssignTasks: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Dave Eäst", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Finite dimensional irreducible representation", config.Buffer, autoAssignTasks: true);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, lowEstMinutes: 10, capability: capability);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, serviceTaskBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				serviceTask.RunTask();
			}

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);

			AssertEquals(resource.GS_Code, task.P9_GS_NKAssignedStaffMember);
			AssertMultilineASCIIEquals("", $@"Information|Auto-assigning tasks in component [WTGDEV - ORG]: [buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [NO] in workflow [Organization (XVBQP68SIYXQ) - Finite dimensional irreducible representation] to resource [Dave Eäst], consuming [0.25] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Dave Eäst: 47.75 (47.50 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		#endregion

		#region Capacity

		[TestDate(2013, 7, 30)]
		public void TestRun_NoResourcesHaveCapacity()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var env = new CapacityAllocationTestingEnvironment(Factory, system);

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			env.Task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			BufferCapacityCache.Clear();
			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"
Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto assign tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto assign tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2016, 10, 18)]
		public void TestRun_NoResourcesHaveCapacity_AutoAssignRegistryIsOn()
		{
			BMSTestHelper.EnableBMSInRegistry();
			Assert("Precondition: this registry item should be 'true' by default", BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.Value);

			var env = new CapacityAllocationTestingEnvironment(Factory, system);

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should auto-assign to resource1 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should auto-assign to resource1 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should auto-assign to resource2 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			env.Task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource1", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource1", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource2", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: -99.0 (-102.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: -97.5 (-99.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_NoResourcesHaveCapability()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins");
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee");
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			resource1.Capabilities.Add(capability1);
			resource2.Capabilities.Add(capability2);
			Factory.Save();

			task.RunTask();

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"
Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001000, T00001001] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [COD] capability.
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]

Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]
".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldAutoAssignToResourceWithMostCapacity()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability1);
			var resource3 = CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability1);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow, capability1, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow, null, resource1, 120);
			Assert(!task4.RequiresResourceWithCapability);

			var task5 = CreateTask(workflow, null, resource3, 120);
			Assert(!task5.RequiresResourceWithCapability);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"
Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001, T00001002] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [4.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (42.0 after assignment)
Frodo Baggins: 43.5
Gandalf the Grey: 43.5
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldNotAutoAssignBeyondCapacity()
		{
			BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer", timespanMinutes: 1200);
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "COX";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1, capability2);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow, null, resource1, 30);
			var task2 = CreateTask(workflow, capability1, null, 60);
			var task3 = CreateTask(workflow, capability1, null, 60);
			var task4 = CreateTask(workflow, capability2, null, 60);
			var task5 = CreateTask(workflow, capability2, null, 60);

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Should have not auto-assigned to resource1", resource1.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Should have not auto-assigned to resource1", resource1.GS_Code, tasks[4].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Service task log", $@"
Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001001, T00001002] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 3.25 (0.25 after assignment)
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto assign tasks [T00001003, T00001004] requiring capability [COX] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_DoesNotTakeADumpOnResourceWithMostCapacity()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("AAASome Poor Guy", capability1);
			resource1.GS_Code = "CDL";
			var resource2 = CreateResourceWithHomeBranchDeptSet("IWONTBEDUMPEDUPON", capability1);
			resource2.GS_Code = "GGG";
			var resource3 = CreateResourceWithHomeBranchDeptSet("Zubin has a smart name with zzzzzz", capability1);
			resource3.GS_Code = "ZZZ";

			var workflow1 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "AAAORGSYD", workflowCompletionStatement: "AAA");

			var task1_1 = CreateTask(workflow1, capability1, null, 120);
			var task1_2 = CreateTask(workflow1, capability1, null, 120);
			var task1_3 = CreateTask(workflow1, capability1, null, 120);

			var workflow2 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "BBBORGSYD", workflowCompletionStatement: "BBB");

			var task2_1 = CreateTask(workflow2, capability1, null, 120);
			var task2_2 = CreateTask(workflow2, capability1, null, 120);
			var task2_3 = CreateTask(workflow2, capability1, null, 120);

			var workflow3 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "CCCORGSYD", workflowCompletionStatement: "CCC");

			var task3_1 = CreateTask(workflow3, capability1, null, 120);
			var task3_2 = CreateTask(workflow3, capability1, null, 120);
			var task3_3 = CreateTask(workflow3, capability1, null, 120);

			var workflow4 = CreateJobAndWorkflow(false, buffer, ZDateTime.UtcNow); // Ensures resource assignment sequence is deterministic
			CreateTask(workflow4, null, resource1, 1);
			CreateTask(workflow4, null, resource2, 2);
			CreateTask(workflow4, null, resource3, 3);

			Factory.Save();

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask_ForTest();
			var log = InitialiseTaskSchedule(serviceTask);

			serviceTask.RunTask();

			var anotherFactory = new BusinessObjectFactory();

			var loadedWorkflow1Tasks = anotherFactory.Load<ProcessHeader>(workflow1.PK).Tasks.ToArray();
			var loadedWorkflow2Tasks = anotherFactory.Load<ProcessHeader>(workflow2.PK).Tasks.ToArray();
			var loadedWorkflow3Tasks = anotherFactory.Load<ProcessHeader>(workflow3.PK).Tasks.ToArray();

			AssertEquals(resource1.GS_Code, loadedWorkflow1Tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource1.GS_Code, loadedWorkflow1Tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource1.GS_Code, loadedWorkflow1Tasks[2].P9_GS_NKAssignedStaffMember);

			AssertEquals(resource2.GS_Code, loadedWorkflow2Tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource2.GS_Code, loadedWorkflow2Tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource2.GS_Code, loadedWorkflow2Tasks[2].P9_GS_NKAssignedStaffMember);

			AssertEquals(resource3.GS_Code, loadedWorkflow3Tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource3.GS_Code, loadedWorkflow3Tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals(resource3.GS_Code, loadedWorkflow3Tasks[2].P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_WhenCapacityCacheInUse_ShouldDistributeBetweenResources()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			const int autoAssignDelayMinutes = 10;

			config.Buffer.FC_AutoAssignTasksAge = new ZInt(autoAssignDelayMinutes).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "ROB", "Rob Cotter wants this ASAP", autoAssignTasks: true);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Ben Gorringe", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PET", "Peter Cronin", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SHA", "Shane D'Aprile", capability);

			config.ReleaseGroup.Staff.AddRange(new[] { resource1, resource2, resource3 });

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1", autoAssignTasks: true);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2", autoAssignTasks: true);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow3", autoAssignTasks: true);

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, lowEstMinutes: 60, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, lowEstMinutes: 60, capability: capability);
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, lowEstMinutes: 60, capability: capability);

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow already in the buffer, making capacity sequence deterministic", config.Buffer);
			BMSTestHelper.CreateTask(workflow4, resource1.GS_Code, lowEstMinutes: 1); // Ben has the least capacity consumed, so gets assigned stuff first.
			BMSTestHelper.CreateTask(workflow4, resource2.GS_Code, lowEstMinutes: 2);
			BMSTestHelper.CreateTask(workflow4, resource3.GS_Code, lowEstMinutes: 3);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(config.Buffer, workflow1.CurrentComponent);
			AssertEquals(config.Buffer, workflow2.CurrentComponent);
			AssertEquals(config.Buffer, workflow3.CurrentComponent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask_ForTest();
			var log = InitialiseTaskSchedule(serviceTask);

			serviceTask.RunTask();

			var newFactory = Factory.CreateNewFactory();

			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			var loadedTask3 = newFactory.Load<ProcessTask>(task3.PK);

			CombineAssertions("Tasks should be assigned between the available resources rather than dumped on one person alone", () =>
			{
				AssertEquals("task1", resource1.GS_Code, loadedTask1.P9_GS_NKAssignedStaffMember);
				AssertEquals("task2", resource2.GS_Code, loadedTask2.P9_GS_NKAssignedStaffMember);
				AssertEquals("task3", resource3.GS_Code, loadedTask3.P9_GS_NKAssignedStaffMember);
			});

			AssertMultilineASCIIEquals("Service task log", @"
Information|Auto-assigning tasks in component [WTGDEV - ORG]: [buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow1] to resource [Ben Gorringe], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Ben Gorringe: 46.47 (44.97 after assignment)
Peter Cronin: 46.45
Shane D'Aprile: 46.42
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001001] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow2] to resource [Peter Cronin], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Peter Cronin: 46.45 (44.95 after assignment)
Shane D'Aprile: 46.42
Ben Gorringe: 44.97
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001002] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow3] to resource [Shane D'Aprile], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Shane D'Aprile: 46.42 (44.92 after assignment)
Ben Gorringe: 44.97
Peter Cronin: 44.95
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_WhenCapacityCacheInUse_AndOtherTasksExistInJob_ShouldNotReduceAvailableCapacityByThatAmount()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			const int autoAssignDelayMinutes = 10;

			config.Buffer.FC_AutoAssignTasksAge = new ZInt(autoAssignDelayMinutes).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "ROB", "Rob Cotter wants this ASAP", autoAssignTasks: true);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Ben Gorringe", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PET", "Peter Cronin", capability);

			config.ReleaseGroup.Staff.AddRange(new[] { resource1, resource2 });

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1", autoAssignTasks: true);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2", autoAssignTasks: true);

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, lowEstMinutes: 60, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, lowEstMinutes: 60, capability: capability);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals(config.Buffer, workflow1.CurrentComponent);
			AssertEquals(config.Buffer, workflow2.CurrentComponent);

			AssertEquals(47.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity);
			AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask();
			InitialiseTaskSchedule(serviceTask);

			serviceTask.RunTask();

			var newFactory = Factory.CreateNewFactory();

			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);

			CombineAssertions("Tasks should be assigned between the available resources rather than dumped on one person alone", () =>
			{
				AssertEquals("task1 should be auto-assigned to BEN", resource1.GS_Code, loadedTask1.P9_GS_NKAssignedStaffMember);
				AssertEquals("task2 was already assigned", resource2.GS_Code, loadedTask2.P9_GS_NKAssignedStaffMember);
			});

			AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity);
			AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity);
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_WhenCapacityCacheInUse_ShouldDistributeBetweenResources_ConsideringResourceAssignmentRules()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);

			const int autoAssignDelayMinutes = 10;

			BMSTestHelper.AddTaskTypesToRegistry("ORG", "AAA", "BBB");
			BMSTestHelper.ClearTaskAssignmentRestrictions();
			BMSTestHelper.AddTaskAssignmentRestriction("ORG", "AAA", new[] { "BBB" }, RestrictionTypeList.Codes.SameResource);

			config.Buffer.FC_AutoAssignTasksAge = new ZInt(autoAssignDelayMinutes).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "ROB", "Rob Cotter wants this ASAP", autoAssignTasks: true);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Ben Gorringe", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PET", "Peter Cronin", capability);

			config.ReleaseGroup.Staff.AddRange(new[] { resource1, resource2 });

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1 - will be assigned first", autoAssignTasks: true);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "ZZZ workflow1_2 - will be assigned by task assignment restriction as a side effect", autoAssignTasks: true);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2 - will be assigned second", autoAssignTasks: true);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3 - already assigned and in the buffer - reduces Pete's capacity", config.Buffer);

			var task1_1 = BMSTestHelper.CreateTask(workflow1_1, string.Empty, lowEstMinutes: 5, taskType: "AAA", capability: capability); // Small estimate. Without considering SAM task assignment cascading, resource1 would get the next task too.
			var task1_2 = BMSTestHelper.CreateTask(workflow1_2, string.Empty, lowEstMinutes: 60, taskType: "BBB", capability: capability); // Task type BBB gets assigned to same resource as AAA.
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, lowEstMinutes: 60, capability: capability);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource2.GS_Code, lowEstMinutes: 60);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow1_1.Reload();
			workflow1_2.Reload();
			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(config.Buffer, workflow1_1.CurrentComponent);
			AssertEquals(config.Buffer, workflow1_2.CurrentComponent);
			AssertEquals(config.Buffer, workflow2.CurrentComponent);
			AssertEquals(config.Buffer, workflow3.CurrentComponent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTask_ForTest();
			InitialiseTaskSchedule(serviceTask);

			serviceTask.RunTask();

			var newFactory = Factory.CreateNewFactory();

			var loadedTask1_1 = newFactory.Load<ProcessTask>(task1_1.PK);
			var loadedTask1_2 = newFactory.Load<ProcessTask>(task1_2.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			var loadedTask3 = newFactory.Load<ProcessTask>(task3.PK);

			CombineAssertions("Tasks should be assigned between the available resources rather than dumped on one person alone", () =>
			{
				AssertEquals("task1_1", resource1.GS_Code, loadedTask1_1.P9_GS_NKAssignedStaffMember);
				AssertEquals("task1_2 should cascade since there is a SAM task assignment restriction", resource1.GS_Code, loadedTask1_2.P9_GS_NKAssignedStaffMember);
				AssertEquals("task2 should be assigned to the next most available person, considering resource1 now has less capacity because of the cascading assignment", resource2.GS_Code, loadedTask2.P9_GS_NKAssignedStaffMember);
				AssertEquals("task3 should already be assigned", resource2.GS_Code, loadedTask3.P9_GS_NKAssignedStaffMember);
			});
		}

		#endregion

		#region Auto Task Assignment with Task Assignment Restrictions

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopeWorkflow()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Workflow);
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskPRE_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskPST_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskCLD_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopeJob()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPRE.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPST.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskCLD.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopeJob_OneWorkflowInBucket_OneWorkflowInBuffer()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.CurrentWorkflow.FH_FC_CurrentComponent = env.Buffer.PK;
				env.PreReqWorkflow.FH_FC_CurrentComponent = env.Bucket.PK;
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPRE.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPRE2.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPST.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskCLD.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopePrerequisite()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Prerequisites);
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPRE.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskPST_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskCLD_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopePostrequisite()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Postrequisites);
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskPRE_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskPST.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskCLD_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_ScopeChild()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Child);
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskPRE_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskPST_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.TaskCLD.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2015, 6, 23)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_DoesNotOverwritePreAssignedResource()
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Workflow);
				env.TaskWFL1.P9_GS_NKAssignedStaffMember = env.Resource2.GS_Code;
				env.TaskWFL2.P9_GS_NKAssignedStaffMember = env.Resource2.GS_Code;
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				Assert("Should have auto-assigned to the resource in capability", !env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Resource preassigned", !env.TaskWFL1_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should maintain preassigned resource", env.Resource2.GS_Code, env.TaskWFL1.P9_GS_NKAssignedStaffMember);
				Assert("Resource preassigned", !env.TaskWFL2_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				AssertEquals("Should maintain preassigned resource", env.Resource2.GS_Code, env.TaskWFL2.P9_GS_NKAssignedStaffMember);
				Assert("Should have not assigned to anyone", env.TaskPRE_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskPST_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskCLD_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		void TestRun_WithTaskAssignmentSAMRestrictions_DBHits(Dictionary<string, int> hits)
		{
			using (var env = new RestrictionsTestingEnvironment(this))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				Factory.Save();

				var serviceTask = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(serviceTask);

				RowFactory.ResetCacheAfterDbUpgrade();

				using (AssertDbHitsForAllFactories(hits, useOnlyNewFactories: true, includeFactoryPredicate: f =>
						f.NameForDebugging.Contains("CapabilityTaskAutoAssignmentServiceTask")
						|| f.NameForDebugging.Contains("WorkflowCapabilityAssigner"),
					ignoreHitsFromTablesCachedInUberFactory: true))
				{
					serviceTask.RunTask();
				}
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestRun_WithTaskAssignmentSAMRestrictions_DBHits_NotPlanningManagement()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var hits = new Dictionary<string, int>()
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 0 }, // should not hit shapes when not in planning management mode
				{ BMSystemSchema.Constants.TableName, 2 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
				{ GlbCapabilitySchema.Constants.TableName, 1 },
				{ GlbCapabilityGroupPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbStaffHolidaySchema.Constants.TableName, 1 },
				{ GlbWorkTimeSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 6 }, //+1 Logging STC event on saving ProcessTasks that includes penetration and zone information
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, UsingSimpleQuery ? 1 : 0 },
			};

			TestRun_WithTaskAssignmentSAMRestrictions_DBHits(hits);
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_RespectsTaskAssignmentDIFRestrictions_ScopeWorkflow()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error);
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Workflow, NotificationTypeList.Codes.Warning);
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_RespectsTaskAssignmentDIFRestrictions_ScopePrerequisite()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Error);
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Prerequisites, NotificationTypeList.Codes.Warning);
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_RespectsTaskAssignmentDIFRestrictions_ScopePostrequisite()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Postrequisites, NotificationTypeList.Codes.Error);
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Postrequisites, NotificationTypeList.Codes.Warning);
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_RespectsTaskAssignmentDIFRestrictions_ScopeChild()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Child, NotificationTypeList.Codes.Error);
				env.CheckDIFRestrictionsRespected(ScopeList.Codes.Child, NotificationTypeList.Codes.Warning);
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_RespectsTaskAssignmentDIFRestrictions_ScopeJob()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.CheckDIFRestrictionsRespected_ScopeJob(NotificationTypeList.Codes.Error);
				env.CheckDIFRestrictionsRespected_ScopeJob(NotificationTypeList.Codes.Warning);
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_LoggingWithDIFRestrictions()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions: ScopeList.Codes.Workflow, scopeForDIFTask: ScopeList.Codes.Workflow, notificationType: NotificationTypeList.Codes.Error);
				env.TaskCapability_DIF.P9_GS_NKAssignedStaffMember = env.Resource1.GS_Code;
				Factory.Save();
				env.ReloadTasksWithNewFactory();
				Assert("Prerequisite", env.TaskCapability_Reloaded.RequiresResourceWithCapability);
				Assert("Prerequisite", !env.TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				AssertMultilineASCIIEquals("Service task log", @"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto assign tasks [T00001000] requiring capability [COD] in workflow [Organization (MYORG) - WFL] because no resources have enough capacity or they are not allowed to be assigned by task autoassignment restrictions.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());
			}
		}

		[TestDate(2016, 12, 7)]
		public void TestRun_LoggingWithDIFRestrictions_WhenSAMRulesCrossCapability_AndRegistryForbidsIt()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSTestHelper.AddTaskTypesToRegistry("ORG", "INV", "CDU", "CDF", "SHV");
			BMSTestHelper.ClearTaskAssignmentRestrictions();
			BMSTestHelper.AddTaskAssignmentRestriction("ORG", "INV", new[] { "CDU", "CDF", "SHV" }, RestrictionTypeList.Codes.SameResource, scope: ScopeList.Codes.Workflow);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Crazy Vaclav", capability1);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "INV", capability: capability1, lowEstMinutes: 10, description: "Zagreb");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDU", capability: capability1, lowEstMinutes: 10, description: "Ebnom");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "CDF", capability: capability2, lowEstMinutes: 10, description: "Zlotik");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "SHV", capability: capability2, lowEstMinutes: 10, description: "Diev");

			Assert(task1.RequiresResourceWithCapability);
			Assert(task2.RequiresResourceWithCapability);
			Assert(task3.RequiresResourceWithCapability);
			Assert(task4.RequiresResourceWithCapability);

			AssertEquals(string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(string.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(string.Empty, task3.P9_GS_NKAssignedStaffMember);
			AssertEquals(string.Empty, task4.P9_GS_NKAssignedStaffMember);

			Factory.Save();

			void sortWorkflows(ProcessHeader[] processHeaders)
			{
				processHeaders = processHeaders.OrderBy(ph => ph.FH_WorkflowType).ToArray();
			}
			var task = new CapabilityTaskAutoAssignmentServiceTask_ForTest(sortWorkflows);
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var loadedTasks = loadedWorkflow.Tasks.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Should have auto-assigned task1 to resource1", resource1.GS_Code, loadedTasks.Single(t => t.P9_Description == "Zagreb").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned task2 to resource1", resource1.GS_Code, loadedTasks.Single(t => t.P9_Description == "Ebnom").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should NOT have assigned task3 to different capability resource1", ZString.Empty, loadedTasks.Single(t => t.P9_Description == "Zlotik").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should NOT have assigned task4 to different capability resource1", ZString.Empty, loadedTasks.Single(t => t.P9_Description == "Diev").P9_GS_NKAssignedStaffMember);

				var logString = Regex.Replace(log.ToString(), @"\[\d\d:\d\d:\d\d\]", @"[PUTITINH]"); // sorting our workflows to make the times they're changed deterministic adds a slight delay to the test, this removes that

				AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Crazy Vaclav], consuming [0.50] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Crazy Vaclav: 47.50 (47.00 after assignment)
Task assignment resulted in the assigning of related tasks.
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [PUTITINH] Batches processed: [2]".StripTaskIds(), logString.Replace("Organisation", "Organization").StripTaskIds());
			});
		}

		[TestDate(2019, 11, 20)]
		public void TestRun_LoggingWithDIFRestrictions_WhenSAMRulesCrossCapabilityAndCrossWorkflow_AndRegistryForbidsIt()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BMSTestHelper.AddTaskTypesToRegistry("ORG", "INV", "CDU", "CDF", "SHV");
			BMSTestHelper.ClearTaskAssignmentRestrictions();
			BMSTestHelper.AddTaskAssignmentRestriction("ORG", "INV", new[] { "CDU", "CDF", "SHV" }, RestrictionTypeList.Codes.SameResource);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			var resource1 = CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);

			var workflow1 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			var workflow2 = CreateWorkflow(workflow1.Parent, true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = CreateTask(workflow1, capability1, null, 60);
			task1.P9_Type = "INV";
			Assert(task1.RequiresResourceWithCapability);

			var task2 = CreateTask(workflow2, capability1, null, 60);
			task2.P9_Type = "CDU";
			Assert(task2.RequiresResourceWithCapability);

			var task3 = CreateTask(workflow1, capability2, null, 60);
			task3.P9_Type = "CDF";
			Assert(task3.RequiresResourceWithCapability);

			var task4 = CreateTask(workflow2, capability2, null, 60);
			task4.P9_Type = "SHV";
			Assert(task4.RequiresResourceWithCapability);

			Factory.Save();

			void sortWorkflows(ProcessHeader[] processHeaders)
			{
				processHeaders = processHeaders.OrderBy(ph => ph.FH_WorkflowType).ToArray();
			}
			var task = new CapabilityTaskAutoAssignmentServiceTask_ForTest(sortWorkflows);
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var loadedTasks1 = loadedWorkflow1.Tasks.ToArray();
			var loadedTasks2 = loadedWorkflow2.Tasks.ToArray();

			AssertNotNull("Should have auto-assigned to resource1", loadedTasks1.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember == resource1.GS_Code));
			AssertNotNull("Should have auto-assigned to resource1", loadedTasks2.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember == resource1.GS_Code));
			AssertNotNull("Should NOT have assigned to different capability resource1", loadedTasks1.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember.IsEmpty));
			AssertNotNull("Should NOT have assigned to different capability resource1", loadedTasks2.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember.IsEmpty));

			var logString = Regex.Replace(log.ToString(), @"\[\d\d:\d\d:\d\d\]", @"[IRRELEPHANT]"); // sorting our workflows to make the times they're changed deterministic adds a slight delay to the test, this removes that

			AssertMultilineASCIIEquals("Service task log", $@"Information|Auto-assigning tasks in component [WTGDEV]: [Buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Task assignment resulted in the assigning of related tasks.
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug|Performance: Capability Task Auto-Assignment, Loading time: [IRRELEPHANT] Batches processed: [2]".StripTaskIds(), logString.Replace("Organisation", "Organization").StripTaskIds());
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_DoesNotViolateTaskAssignmentDIFRestrictionsThroughSAMRestrictions()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(scopeForRestrictions: ScopeList.Codes.Workflow, scopeForDIFTask: ScopeList.Codes.Workflow, notificationType: NotificationTypeList.Codes.Error, difInvolvedThroughSam: true);
				env.TaskCapability_DIF.P9_GS_NKAssignedStaffMember = env.Resource1.GS_Code;
				Factory.Save();
				env.ReloadTasksWithNewFactory();
				Assert("Task taskCapability requires a resource in capability", env.TaskCapability.RequiresResourceWithCapability);
				Assert("Task taskCapability requires a resource in capability", env.TaskCapability_Reloaded.RequiresResourceWithCapability);
				Assert("Task taskWFL2 which is under SAM restriction with the main task is not assigned, but does not require a resource in capability", env.TaskWFL2_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty && !env.TaskWFL2_Reloaded.RequiresResourceWithCapability);
				Assert("Task taskCapability_DIF which is under DIF restrictions with another task (taskWFL2) which is, in turn, under SAM restriction with the main task was already assigned", !env.TaskCapability_DIF_Reloaded.RequiresResourceWithCapability);

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				InitialiseTaskSchedule(task);

				task.RunTask();
				env.ReloadTasksWithNewFactory();
				Assert("The main task should have not auto-assigned to a resource when assigning in accordance with SAM restrictions violates DIF restrictions",
					env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_ShouldAssignTasksOfSameType_WhenTypeIsInTheRestriction()
		{
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				var restrictions = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value;

				foreach (var restriction in restrictions.Cast<TaskTypeRestrictions>())
				{
					var sameTaskType = restriction.TaskTypesCollection.AddNew();
					sameTaskType.Code = restriction.TaskType;
				}

				WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, restrictions);
				var newTask = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, estDurationMinutes: 60);
				newTask.P9_Type = env.TaskCapability.P9_Type;

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();
				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability_Reloaded.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				AssertEquals("Should have auto-assigned to the resource in capability", newTask.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				Assert("Should have not assigned to anyone", env.TaskWithUndefinedType_Reloaded.P9_GS_NKAssignedStaffMember.IsEmpty);
			}
		}

		#region Auto Task Assignment When Task Assignment Restrictions of SAM Type Conflict with Required Capabilities

		[TestDate(2019, 11, 20)]
		public void TestRun_WhenSAMRestrictionsDoNotConflictWithCapabilities()
		{
			using (var env = new RestrictionsAndCapabilitiesTestingEnvironment(this))
			{
				env.TaskCapability2.P9_Type = "UDF"; // does not involve SAM restrictions

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				AssertEquals("Should have auto-assigned to the resource with capability 1", env.ResourceWithCapability1.GS_Code, env.TaskCapability1_Reloaded.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource with capability 2", env.ResourceWithCapability2.GS_Code, env.TaskCapability2_Reloaded.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 11, 20)]
		public void TestRun_WhenSAMRestrictionsConflictWithCapabilities()
		{
			using (var env = new RestrictionsAndCapabilitiesTestingEnvironment(this))
			{
				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				AssertEquals("Should have auto-assigned to the resource with capability 1", env.ResourceWithCapability1.GS_Code, env.TaskCapability1_Reloaded.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource with the same capability 1 due to the SAM restrictions", env.ResourceWithCapability1.GS_Code, env.TaskCapability2_Reloaded.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 11, 20)]
		public void TestRun_RespectsTaskSequenceNumbers_WhenSAMRestrictionsConflictWithCapabilities()
		{
			using (var env = new RestrictionsAndCapabilitiesTestingEnvironment(this))
			{
				env.TaskCapability1.P9_Sequence = 100;
				env.TaskCapability2.P9_Sequence = 1; // expect this capability to be processed first

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				env.ReloadTasksWithNewFactory();

				AssertEquals("Should have auto-assigned to the resource with capability 2 due to the SAM restrictions and sequence numbers", env.ResourceWithCapability2.GS_Code, env.TaskCapability1_Reloaded.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource with capability 2", env.ResourceWithCapability2.GS_Code, env.TaskCapability2_Reloaded.P9_GS_NKAssignedStaffMember);
			}
		}

		#endregion

		#endregion

		#region Auto Task Assignment When Tasks is company specific

		[TestDate(2019, 12, 17)]
		public void TestRun_WhenCompanySpecificTask_ShouldAssignRelatedCompanySpecificTasks()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.TaskCapability.P9_GC = otherCompany.PK;
				env.TaskCapability.P9_ShareTasksForAllCompanies = false;
				env.PostReqWorkflow.FH_FC_CurrentComponent = env.Bucket.PK;

				var otherCompanySpecificTaskInCurrentWorkflow = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				otherCompanySpecificTaskInCurrentWorkflow.P9_GC = otherCompany.PK;
				otherCompanySpecificTaskInCurrentWorkflow.P9_ShareTasksForAllCompanies = false;
				otherCompanySpecificTaskInCurrentWorkflow.P9_Type = "CDU";

				var otherCompanySpecificTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanySpecificTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanySpecificTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = false;
				otherCompanySpecificTaskInPostReqWorkflow.P9_Type = "CDF";

				var otherCompanySpecificTaskInChildWorkflow = CreateTask(env.ChildWorkflow, env.Capability, resource: null, 0);
				otherCompanySpecificTaskInChildWorkflow.P9_GC = otherCompany.PK;
				otherCompanySpecificTaskInChildWorkflow.P9_ShareTasksForAllCompanies = false;
				otherCompanySpecificTaskInChildWorkflow.P9_Type = "CDU";

				var otherCompanyGlobalTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanyGlobalTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = true;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_Type = "CDF";

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				Assert("PRECONDITION - Should run using different company context", GlbCompany.CurrentCompany.PK != otherCompany.PK);
				env.TaskCapability.Reload();
				otherCompanySpecificTaskInCurrentWorkflow.Reload();
				otherCompanySpecificTaskInPostReqWorkflow.Reload();
				otherCompanySpecificTaskInChildWorkflow.Reload();
				otherCompanyGlobalTaskInPostReqWorkflow.Reload();

				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				AssertContains("Task assignment resulted in the assigning of related tasks", log.ToString());
				AssertEquals("Company SPECIFIC Task in same workflow should be assigned", env.Resource1.GS_Code, otherCompanySpecificTaskInCurrentWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("Company SPECIFIC Task in other workflow should be assigned", env.Resource1.GS_Code, otherCompanySpecificTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("Company SPECIFIC Task in child workflow should be assigned", env.Resource1.GS_Code, otherCompanySpecificTaskInChildWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("GLOBAL Task in other workflow should not be assigned", ZString.Empty, otherCompanyGlobalTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_WhenCompanySpecificTask_ShouldNotAssignRelatedGlobalTasks()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.TaskCapability.P9_GC = otherCompany.PK;
				env.TaskCapability.P9_ShareTasksForAllCompanies = false;
				env.PostReqWorkflow.FH_FC_CurrentComponent = env.Bucket.PK;

				var otherCompanyGlobalTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanyGlobalTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = true;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_Type = "CDF";

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				Assert("PRECONDITION - Should run using different company context", GlbCompany.CurrentCompany.PK != otherCompany.PK);
				env.TaskCapability.Reload();
				otherCompanyGlobalTaskInPostReqWorkflow.Reload();

				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				AssertNotContains("Task assignment resulted in the assigning of related tasks", log.ToString());
				AssertEquals("Global Task in other workflow should not be assigned", ZString.Empty, otherCompanyGlobalTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_WhenTaskSharedForAllCompanies_ShouldAssignRelatedTasksSharedForAllCompanies()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.TaskCapability.P9_ShareTasksForAllCompanies = true;
				env.PostReqWorkflow.FH_FC_CurrentComponent = env.Bucket.PK;

				var otherCompanyGlobalTaskInCurrentWorkflow = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				otherCompanyGlobalTaskInCurrentWorkflow.P9_GC = otherCompany.PK;
				otherCompanyGlobalTaskInCurrentWorkflow.P9_ShareTasksForAllCompanies = true;
				otherCompanyGlobalTaskInCurrentWorkflow.P9_Type = "CDU";

				var otherCompanyGlobalTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanyGlobalTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = true;
				otherCompanyGlobalTaskInPostReqWorkflow.P9_Type = "CDF";

				var otherCompanyGlobalTaskInChildWorkflow = CreateTask(env.ChildWorkflow, env.Capability, resource: null, 0);
				otherCompanyGlobalTaskInChildWorkflow.P9_GC = otherCompany.PK;
				otherCompanyGlobalTaskInChildWorkflow.P9_ShareTasksForAllCompanies = true;
				otherCompanyGlobalTaskInChildWorkflow.P9_Type = "CDU";

				var otherCompanySpecificTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanySpecificTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanySpecificTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = false;
				otherCompanySpecificTaskInPostReqWorkflow.P9_Type = "CDF";

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				Assert(GlbCompany.CurrentCompany.PK != otherCompany.PK);
				env.TaskCapability.Reload();
				otherCompanyGlobalTaskInCurrentWorkflow.Reload();
				otherCompanyGlobalTaskInPostReqWorkflow.Reload();
				otherCompanyGlobalTaskInChildWorkflow.Reload();
				otherCompanySpecificTaskInPostReqWorkflow.Reload();

				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				AssertContains("Task assignment resulted in the assigning of related tasks", log.ToString());
				AssertEquals("GLOBAL Task in same workflow should be assigned", env.Resource1.GS_Code, otherCompanyGlobalTaskInCurrentWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("GLOBAL Task in other workflow should be assigned", env.Resource1.GS_Code, otherCompanyGlobalTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("GLOBAL Task in child workflow should be assigned", env.Resource1.GS_Code, otherCompanyGlobalTaskInChildWorkflow.P9_GS_NKAssignedStaffMember);
				AssertEquals("SPECIFIC Task in other workflow should not be assigned", ZString.Empty, otherCompanySpecificTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_WhenTaskSharedForAllCompanies_ShouldNotAssignRelatedCompanySpecificTasks()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.TaskCapability.P9_GC = otherCompany.PK;
				env.TaskCapability.P9_ShareTasksForAllCompanies = true;
				env.PostReqWorkflow.FH_FC_CurrentComponent = env.Bucket.PK;

				var otherCompanySpecificlTaskInPostReqWorkflow = CreateTask(env.PostReqWorkflow, env.Capability, resource: null, 0);
				otherCompanySpecificlTaskInPostReqWorkflow.P9_GC = otherCompany.PK;
				otherCompanySpecificlTaskInPostReqWorkflow.P9_ShareTasksForAllCompanies = false;
				otherCompanySpecificlTaskInPostReqWorkflow.P9_Type = "CDF";

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				Assert("PRECONDITION - Should run using different company context", GlbCompany.CurrentCompany.PK != otherCompany.PK);
				env.TaskCapability.Reload();
				otherCompanySpecificlTaskInPostReqWorkflow.Reload();

				AssertEquals("Should have auto-assigned to the resource in capability", env.TaskCapability.P9_GS_NKAssignedStaffMember, env.Resource1.GS_Code);
				AssertNotContains("Task assignment resulted in the assigning of related tasks", log.ToString());
				AssertEquals("Company SPECIFIC Task in other workflow should not be assigned", ZString.Empty, otherCompanySpecificlTaskInPostReqWorkflow.P9_GS_NKAssignedStaffMember);
			}
		}

		[TestDate(2019, 12, 17)]
		public void TestRun_WhenTasksSharedForAllCompaniesAndSpecificTasksInTheBuffer_ShouldAssignAll()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var tasks = new List<ProcessTask>();

			using (var env = new RestrictionsTestingEnvironment(this, bothResourcesOwnCapability: false))
			{
				env.SetupRegistryAndTasksForRestrictionsTesting(ScopeList.Codes.Job);
				env.TaskCapability.P9_ShareTasksForAllCompanies = true;
				tasks.Add(env.TaskCapability);

				var task2 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task2.P9_ShareTasksForAllCompanies = true;
				task2.P9_Type = "CDU";
				tasks.Add(task2);

				var task3 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task3.P9_ShareTasksForAllCompanies = false;
				task3.P9_Type = "INV";
				tasks.Add(task3);

				var task4 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task4.P9_ShareTasksForAllCompanies = false;
				task4.P9_Type = "CDF";
				tasks.Add(task4);

				var task5 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task5.P9_GC = otherCompany.PK;
				task5.P9_ShareTasksForAllCompanies = false;
				task5.P9_Type = "INV";
				tasks.Add(task5);

				var task6 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task6.P9_GC = otherCompany.PK;
				task6.P9_ShareTasksForAllCompanies = false;
				task6.P9_Type = "CDU";
				tasks.Add(task6);

				var task7 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task7.P9_GC = otherCompany.PK;
				task7.P9_ShareTasksForAllCompanies = true;
				task7.P9_Type = "INV";
				tasks.Add(task7);

				var task8 = CreateTask(env.CurrentWorkflow, env.Capability, resource: null, 0);
				task8.P9_GC = otherCompany.PK;
				task8.P9_ShareTasksForAllCompanies = true;
				task8.P9_Type = "CDF";
				tasks.Add(task8);

				Factory.Save();

				var task = new CapabilityTaskAutoAssignmentServiceTask();
				var log = InitialiseTaskSchedule(task);

				task.RunTask();

				Assert(GlbCompany.CurrentCompany.PK != otherCompany.PK);
				tasks.ForEach(t => t.Reload());

				AssertContains("Task assignment resulted in the assigning of related tasks", log.ToString());
				Assert("All tasks should be assigned", tasks.All(t => t.P9_GS_NKAssignedStaffMember == env.Resource1.GS_Code));
			}
		}

		#endregion

		#region Capacity Disabled in registry

		[TestDate(2016, 10, 18)]
		public void TestRun_WhenCapacityIsDisabled_ShouldDoNothing()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Precondition: this registry item should be 'true' by default", BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.Value);

			var env = new CapacityAllocationTestingEnvironment(Factory, system);

			var log = RunAutoAssignmentAndGetLog();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not auto-assign because capacity is disabled.", string.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign because capacity is disabled.", string.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign because capacity is disabled.", string.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertMultilineASCIIEquals("Debug|Service task not run because the [Disable Capacity Calculations] registry item is enabled.", log.ToString());
		}

		#endregion

		#region Hosted Service Requirements

		public void TestServiceTask_WhenCapacityCalculationDisabled_ShouldNotMeetHostedServiceRequirement()
		{
			AssertEquals("Should be enabled by default if buffer management is enabled.", string.Empty, CapabilityTaskAutoAssignmentServiceTask.CheckCapacityCalculationsNotDisabled());

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations' requires a value other than 'True'.", CapabilityTaskAutoAssignmentServiceTask.CheckCapacityCalculationsNotDisabled());
		}

		public void TestRequirements_WhenPlanningManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals("The service task should meet the requirements because PLN is enabled.", string.Empty, CapabilityTaskAutoAssignmentServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBufferManagementEnabled_ShouldBeEmpty()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals("The service task should meet the requirements because BUF is enabled.", string.Empty, CapabilityTaskAutoAssignmentServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenEnhancedWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", CapabilityTaskAutoAssignmentServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		public void TestRequirements_WhenBasicWorkflowManagementEnabled_ShouldHaveError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			AssertMultilineASCIIEquals("The registry setting 'Workflow Manager -> Buffer Management -> Workflow Management Mode' requires one of the following values: 'BUF', 'PLN'.", CapabilityTaskAutoAssignmentServiceTask.CheckSufficientWorkflowModeEnabled());
		}

		#endregion

		#region WorkflowManagementMode Restrictions

		public void TestProcess_WhenPlanningManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capability Task Auto-Assignment.", log.ToString());
		}

		public void TestProcess_WhenBufferManagementEnabled_ShouldRun()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertNotContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capability Task Auto-Assignment.", log.ToString());
		}

		public void TestProcess_WhenEnhancedWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capability Task Auto-Assignment.", log.ToString());
		}

		public void TestProcess_WhenBasicWorkflowManagementEnabled_ShouldDoNothing()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var serviceTask = GetNewServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			serviceTask.RunTask();

			AssertContains("The registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] must be set to a higher level to use Capability Task Auto-Assignment.", log.ToString());
		}
		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Exception Handling

		[TestDate(2015, 7, 14)]
		public void TestRun_WhenSomeoneHasClaimedTaskAboutToBeAssigned_ShouldIgnoreConcurrencyErrorAndRetainHumanAssignment()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_AutoAssignTasksAge = new ZInt(1).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "CON", "Currency", autoAssignTasks: true);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ONE", "Onesource", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWO", "Twosource", capability);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Threesource. Ah ah ahh.", config.Buffer, ZDateTime.UtcNow.AddDays(-1), autoAssignTasks: true);
			var capabilityTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 10, capability: capability);
			var otherTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, lowEstMinutes: 20);
			var otherTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, lowEstMinutes: 30); // resource2 has more capacity consumed, so resource1 would be the ideal auto-assignment candidate for the capability task.

			Factory.Save();

			var task = new CapabilityTaskAutoAssignmentServiceTask_ForTest();
			var log = InitialiseTaskSchedule(task);

			task.RunTask();

			var resource1Capacity = "47.38 (47.13 after assignment)";
#if NETFRAMEWORK
			if (UsingSimpleQuery) //different because the old query round up, adding a little more capacity, that is wrong, the correct is the bellow, we should remove old one after full test simple one instead of fix
			{
				resource1Capacity = "47.37 (47.12 after assignment)";
			}
#endif

			AssertMultilineASCIIEquals($"Precondition: The service task should pick resource1 as they have the most capacity", $@"Information|Auto-assigning tasks in component [WTGDEV - ORG]: [buffer].
Information|Auto-assigning tasks using CurrentComponent auto-assigning task age.
Information|Assigned tasks [T00001000] requiring capability [CON] in workflow [Organization (XVBQP68SIYXQ) - Threesource. Ah ah ahh.] to resource [Onesource], consuming [0.25] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Onesource: {resource1Capacity}
Twosource: 47.12
Debug|Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().Replace("Organisation", "Organization").StripTaskIds());

			AssertEquals(resource1.GS_Code, capabilityTask.P9_GS_NKAssignedStaffMember);

			// Un-assign the task so the service task can have another go at it.

			capabilityTask.P9_GS_NKAssignedStaffMember = ZString.Empty;
			Factory.Save();

			task.PreAssignmentAction_ForTest = workflows =>
			{
				// Whilst the service task is running, force all the tasks to be loaded, then assign the capability task to another resource in a different factory.

				var tasks = workflows.SelectMany(w => w.TaskCollection).ToArray();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedTask = newFactory.Load<ProcessTask>(capabilityTask.PK);

				loadedTask.P9_GS_NKAssignedStaffMember = resource2.GS_Code;

				newFactory.Save();
			};

			log.ClearLog();
			task.RunTask();

			capabilityTask.Reload();

			AssertEquals("The service task should not undo the assignment done by a human.", resource2.GS_Code, capabilityTask.P9_GS_NKAssignedStaffMember);
		}

		#endregion

		#region Implementation

		ProcessHeader CreateJobAndWorkflow(bool autoAssignTasks, BMComponent currentComponent, ZDateTime releaseDateTime, string orgHeaderCode = "MAIORGSYD", string workflowCompletionStatement = "Workflow 1")
		{
			return helper.CreateJobAndWorkflow(autoAssignTasks, currentComponent, releaseDateTime, orgHeaderCode, workflowCompletionStatement);
		}

		ProcessHeader CreateWorkflow(IWorkflowProvider job, bool autoAssignTasks, BMComponent currentComponent, ZDateTime releaseDateTime, string workflowCompletionStatement = "Workflow 1")
		{
			return helper.CreateWorkflow(job, autoAssignTasks, currentComponent, releaseDateTime, workflowCompletionStatement);
		}

		GlbStaff CreateResourceWithHomeBranchDeptSet(string fullName, params GlbCapability[] capabilities)
		{
			return helper.CreateResourceWithHomeBranchDeptSet(fullName, capabilities);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		ProcessTask CreateTask(ProcessHeader workflow, GlbCapability capability, GlbStaff resource, int estDurationMinutes)
		{
			return helper.CreateTask(workflow, capability, resource, estDurationMinutes);
		}

		#region IWorkflowCapabilityAssignerTestCase Members

		BusinessObjectFactory IWorkflowCapabilityAssignerTestCase.Factory => Factory;

		BMSystem IWorkflowCapabilityAssignerTestCase.System => system;

		ILogger IWorkflowCapabilityAssignerTestCase.RunAutoAssignmentAndGetLog(BMComponent buffer, ZGuid workflowPK, Action<ProcessHeader[]> preassignmentAction)
		{
			return RunAutoAssignmentAndGetLog();
		}

		ILogger IWorkflowCapabilityAssignerTestCase.RunAutoAssignmentAndGetLog(BMComponent buffer, IEnumerable<ZGuid> workflowPKs, Action<ProcessHeader[]> preassignmentAction)
		{
			return RunAutoAssignmentAndGetLog();
		}

		ILogger RunAutoAssignmentAndGetLog()
		{
			var task = new CapabilityTaskAutoAssignmentServiceTask();
			var log = InitialiseTaskSchedule(task);
			task.RunTask();
			return log;
		}

		#endregion

		#region SetUp And TearDown

		protected BMSystem system;
		protected WorkflowCapabilityAssignerTestHelper helper;

		protected override void SetUpCore()
		{
			Enterprise.VisualBoards.Business.Test.VisualBoardsTestCase.SetupAndClearTables();
			base.SetUpCore();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
			BMSTestHelper.EnableBMSInRegistry();
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());

			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); // To help us quickly identify dates that are formatted incorrectly, rather than waiting for amnesties.

			helper = new WorkflowCapabilityAssignerTestHelper(Factory);

			system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";
		}

		protected virtual bool UsingSimpleQuery => false;

		protected override void TearDownCore()
		{
			base.TearDownCore();

			BufferCapacityCache.Clear();
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		CultureInfo originalCulture;

		#endregion

		protected override CapabilityTaskAutoAssignmentServiceTask GetNewServiceTask()
		{
			return new CapabilityTaskAutoAssignmentServiceTask_ForTest();
		}

		protected override void InitializeProcessHeaders()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);

			var workflow = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			var workflow2 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), "FOO");
			var workflow3 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), "BAR");
			var workflow4 = CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), "BAZ");

			CreateTask(workflow, capability1, null, 60);
			CreateTask(workflow2, capability1, null, 60);
			CreateTask(workflow3, capability1, null, 60);
			CreateTask(workflow4, capability1, null, 60);

			Factory.Save();
		}

		#endregion

		#region Test Classes

		class CapabilityTaskAutoAssignmentServiceTask_ForTest : CapabilityTaskAutoAssignmentServiceTask
		{
			public CapabilityTaskAutoAssignmentServiceTask_ForTest(Action<ProcessHeader[]> preAssignmentAction_ForTest = null)
			{
				PreAssignmentAction_ForTest = preAssignmentAction_ForTest;
			}

			public Action<ProcessHeader[]> PreAssignmentAction_ForTest { get; set; }

			protected override WorkflowCapabilityAssigner GetWorkflowCapabilityAssigner() => new WorkflowCapabilityAssigner_ForTest(PreAssignmentAction_ForTest);

			protected override IWorkflowCapabilityAssignerDataAccessor GetDataAccessor() => new WorkflowCapabilityAssignerDataAccessor_ForTest(ServiceLogger);

			protected override void RunTaskCore(CancellationToken token)
			{
				using (Env.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTaskCore(token);
				}
			}
		}

		class CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLogger : CapabilityTaskAutoAssignmentServiceTask
		{
			protected override IWorkflowCapabilityAssignerDataAccessor GetDataAccessor()
			{
				return new CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLoggerDataAccessor(ServiceLogger);
			}
		}

		#endregion
	}
}
