using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	abstract class WorkflowCapabilityAssignerTestCase : TestCaseWithFactory, IWorkflowCapabilityAssignerTestCase
	{
		#region Suppress Template Application

		[TestDate(2022, 9, 14)]
		public void TestRun_ShouldNotApplyWorkflowTemplates()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XXX";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "XXX";
			Factory.Save();

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(180).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.G4_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var @group = Factory.NewWithValidTestData<GlbGroup>();
			@group.GG_Desc = "Group";

			var resource = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability);
			resource.Groups.Add(@group);

			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = @group.PK;
			link.FO_AutoAssignTasksAge = new ZInt(120).GetDateTimeFromMinutes();

			var job = Factory.NewWithValidTestData<DummyWithWorkflowAndTemplateApplicationOnSaving>();
			job.Z0_Code = "JOB";
			var workflow = helper.CreateWorkflow(job, true, buffer, ZDateTime.Today.AddHours(-1), "OG Workflow");
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = helper.CreateTask(workflow, capability, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflowAndTemplateApplicationOnSaving);

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var templateFactory = new BusinessObjectFactory { RefreshEnabled = false };
				BMSTestHelper.CreateSystem(templateFactory, "DUM");

				var template = BMSTestHelper.CreateWorkflowTemplate(templateFactory, DummyWorkflowDescriptor.Instance.Code, name: "Template");
				var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "This Workflow Should Not Appear In This Job");
				BMSTestHelper.CreateTask(template, templateWorkflow, description: "Nor Should This Task");

				templateFactory.Save();

				var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK, (workflows) =>
				{
					(workflows.First().Parent as DummyWithWorkflowAndTemplateApplicationOnSaving).Z0_Description = "Change the job to induce template application.";
				});

				AssertEquals("Should have auto-assigned task1", resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertContains("Auto-assigning tasks using capability auto-assigning task age.", log.ToString());
			}

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflows = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));
			var workflowDescriptions = loadedWorkflows.Select(w => w.FH_CompletionStatement);

			AssertCollectionContains("OG Workflow", workflowDescriptions);
			AssertCollectionNotContains("Since templates should not be applied when auto assigning tasks, this workflow which originated from a template should not be applied",
				"This Workflow Should Not Appear In This Job", workflowDescriptions);
		}

		#endregion

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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);

			resource1.Groups.Add(@group);
			resource2.Groups.Add(@group);

			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = @group.PK;
			link.FO_AutoAssignTasksAge = new ZInt(120).GetDateTimeFromMinutes();

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-1));
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals(ZDateTime.DefaultDurationEpoch, capability1.G4_AutoAssignTasksAge);
			AssertEquals(true, capability1.G4_AllowTaskAutoAssignment);
			AssertEquals(ZDateTime.Empty, capability2.G4_AutoAssignTasksAge);
			AssertEquals(true, capability2.G4_AllowTaskAutoAssignment);
			AssertEquals((ZDateTime)TimeSpan.FromHours(2), link.FO_AutoAssignTasksAge);

			AssertEquals("Should have auto-assigned for task1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have no auto-assigned to task2", ZString.Empty, task2.P9_GS_NKAssignedStaffMember);

			if (IsSchedulingAllowed)
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: (45.0 after assignment)
Auto-assigning tasks using component release group link auto-assigning task age.
Tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] at [30-Jul-13 02:00:00]. The earliest tasks requiring assignment are as follows:
- [T00001001] with required capability [RVW]".StripTaskIds(), log.ToString().StripTaskIds());
			}
			else
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Auto-assigning tasks using component release group link auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group)] since it was released at 30-Jul-2013 09:00:00, which is 1:00 working hours ago. (Minimum hours before auto assignment is 2:00)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
			}

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability);

			resource1.Groups.Add(group1);
			resource2.Groups.Add(group2);

			var link1 = buffer.ReleaseGroupLinks.AddNew();
			link1.FO_GG_ReleaseGroup = group1.PK;

			var link2 = buffer.ReleaseGroupLinks.AddNew();
			link2.FO_GG_ReleaseGroup = group2.PK;
			link2.FO_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-2));
			workflow.FH_GG_ReleaseGroup = group2.PK;

			var task1 = helper.CreateTask(workflow, capability, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log",
$@"Auto-assigning tasks using component release group link auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 2)] to resource [Samwise Gamgee], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 45.0 (42.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddHours(-2));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should not have auto-assigned to resource1", string.Empty, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			if (IsSchedulingAllowed)
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: (45.0 after assignment)
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1] at [30-Jul-13 03:00:00]. The earliest tasks requiring assignment are as follows:
- [T00001000] with required capability [COD]".StripTaskIds(), log.ToString().StripTaskIds());
			}
			else
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 08:00:00, which is 1:00 working hours ago. (Minimum hours before auto assignment is 3:00)
Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
			}

			AssertContextSwitchCount(1);
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignTaskAgeWithOverridden_ReleaseGroupCapabilityOverDefaultValueCapability()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(180).GetDateTimeFromMinutes();

			var groupAlpha = BMSTestHelper.CreateGroup(Factory, "ALPRG", "Alpha");
			var groupDos = BMSTestHelper.CreateGroup(Factory, "DOSRG", "Dos");

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = false;

			var pivot1 = capability1.ReleaseGroupPivots.AddNew();
			pivot1.GGC_AllowTaskAutoAssignment = true;
			pivot1.GGC_GG_Group = groupAlpha.PK;
			pivot1.GGC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var pivot2 = capability1.ReleaseGroupPivots.AddNew();
			pivot2.GGC_AllowTaskAutoAssignment = true;
			pivot2.GGC_GG_Group = groupDos.PK;
			pivot2.GGC_AutoAssignTasksAge = new ZInt(1).GetDateTimeFromMinutes();

			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;
			capability2.G4_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var pivot3 = capability2.ReleaseGroupPivots.AddNew();
			pivot3.GGC_AllowTaskAutoAssignment = true;
			pivot3.GGC_GG_Group = groupDos.PK;
			pivot3.GGC_AutoAssignTasksAge = new ZInt(1).GetDateTimeFromMinutes();

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);

			resource1.Groups.Add(groupAlpha);
			resource1.Groups.Add(groupDos);
			resource2.Groups.Add(groupDos);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";

			var workflow1 = helper.CreateWorkflow(job, true, buffer, ZDateTime.Today.AddHours(-2));
			workflow1.FH_GG_ReleaseGroup = groupAlpha.PK;
			var workflow2 = helper.CreateWorkflow(job, true, buffer, ZDateTime.Today.AddHours(-2), "Workflow 2");
			workflow2.FH_GG_ReleaseGroup = groupAlpha.PK;

			var task1 = helper.CreateTask(workflow1, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow2, capability2, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			var log1 = RunAutoAssignmentAndGetLog(buffer, workflow1.PK);
			var log2 = RunAutoAssignmentAndGetLog(buffer, workflow2.PK);

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource2", string.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability3);

			resource1.Groups.Add(group);
			resource2.Groups.Add(group);

			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_AutoAssignTasksAge = new ZInt(40).GetDateTimeFromMinutes();

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddMinutes(-20));
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability3, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource2", string.Empty, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task2.P9_GS_NKAssignedStaffMember);

			if (IsSchedulingAllowed)
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: (45.0 after assignment)
Auto-assigning tasks using component release group link auto-assigning task age.
Tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] require delayed assignment.
Auto-assigning tasks using component release group link auto-assigning task age.
Tasks [T00001002] requiring capability [FCU] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] at [30-Jul-13 00:40:00]. The earliest tasks requiring assignment are as follows:
- [T00001001] with required capability [RVW]
- [T00001002] with required capability [FCU]".StripTaskIds(), log.ToString().StripTaskIds());
			}
			else
			{
				AssertLog("Service task log", $@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Auto-assigning tasks using component release group link auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] since it was released at 30-Jul-2013 09:40:00, which is 0:20 working hours ago. (Minimum hours before auto assignment is 0:40)
Auto-assigning tasks using component release group link auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] since it was released at 30-Jul-2013 09:40:00, which is 0:20 working hours ago. (Minimum hours before auto assignment is 0:40)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
			}

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-40);

			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task3.P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log",
$@"Auto-assigning tasks using component release group link auto-assigning task age.
Assigned tasks [T00001001] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Auto-assigning tasks using component release group link auto-assigning task age.
Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [FCU] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-60);

			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned to resource3", string.Empty, task3.P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log",
@"Auto-assigning tasks using component release group link auto-assigning task age.
Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [FCU] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Desc = "Release Group";
			releaseGroup.Staff.Add(resource1);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddMinutes(-25 * 60));
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have not auto-assigned to resource1", string.Empty, task1.P9_GS_NKAssignedStaffMember);

			if (IsSchedulingAllowed)
			{
				AssertLog("Service task log", @"Auto-assigning tasks using capability auto-assigning task age.
Tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] at [31-Jul-13 02:00:00]. The earliest tasks requiring assignment are as follows:
- [T00001000] with required capability [COD]".StripTaskIds(), log.ToString().StripTaskIds());
			}
			else
			{
				AssertLog("Service task log", @"Auto-assigning tasks using capability auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] since it was released at 29-Jul-2013 09:00:00, which is 9:00 working hours ago. (Minimum hours before auto assignment is 26:00)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
			}

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddMinutes(-900 * 60 + 1);

			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);

			Factory.Save();

			AssertLog("Service task log",
$@"Auto-assigning tasks using capability auto-assigning task age.
Assigned tasks [T00001000] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Release Group)] to resource [Frodo Baggins], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
		}

		#endregion

		#region Simple Assignment

		[TestDate(2013, 7, 30)]
		public void TestRun_SimpleAutoAssign()
		{
			AssertSimpleAutoAssign((buffer, workflowPK) => RunAutoAssignmentAndGetLog(buffer, workflowPK));

			AssertContextSwitchCount(1);
		}

		void AssertSimpleAutoAssign(Func<BMComponent, ZGuid, ILogger> runTaskAction)
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_GB_AgingBranch = newBranch.PK;
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

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			Assert("Precondition", !TaskAssignmentHelper.DifRestrictionsExistInRegistry(Factory));

			var log = runTaskAction(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_AutoAssignToMemberOfReleaseGroup()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "DPR", "DPIB Review");
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Igor", capability);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Dave", capability);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Baaber", capability);

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

			var task1 = helper.CreateTask(workflow1, capability, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow1, capability, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Fun", buffer, ZDateTime.Today.AddDays(-1), group2.PK);
			workflow2.FH_AllowTaskAutoAssignment = true;
			var task3 = helper.CreateTask(workflow2, capability, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow2, capability, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow1.PK, workflow2.PK });

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var tasksForWorkflow1 = loadedWorkflow1.Tasks.ToArray();
			var tasksForWorkflow2 = loadedWorkflow2.Tasks.ToArray();

			AssertEquals("Should have auto-assigned to resource1 because resource is in releasegroup for workflow", resource1.GS_Code, tasksForWorkflow1[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1 because resource is in releasegroup for workflow", resource1.GS_Code, tasksForWorkflow1[1].P9_GS_NKAssignedStaffMember);

			AssertEquals("Should have auto-assigned to resource2 because it is the only resource in capability in the releasegroup of the workflow", resource2.GS_Code, tasksForWorkflow2[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Resource 3 was directly assigned to this task. So should ignore capability", resource3.GS_Code, tasksForWorkflow2[1].P9_GS_NKAssignedStaffMember);

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT auto-assign since the capability doesn't allow it", string.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "Group 1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "Group 2";

			resource1.Groups.Add(group1);
			resource1.Groups.Add(group2);
			resource2.Groups.Add(group2);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_GG_ReleaseGroup = group1.PK;

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT have auto-assigned to resource2 outside workflow release group", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 1)] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			workflow.FH_GG_ReleaseGroup = group2.PK;
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1 (Group 2)] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			resource2.GS_IsActive = false;
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should NOT have assigned to inactive resource2", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());

			resource2.GS_IsActive = true;
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssign_ShouldNotAssignCancelledTasks()
		{
			void runTaskAction(BMComponent buffer, ZGuid workflowPK) => RunAutoAssignmentAndGetLog(buffer, workflowPK);
			AssertShouldNotAssignCancelledOrClosedTasks(runTaskAction);
			AssertContextSwitchCount(1);
		}

		void AssertShouldNotAssignCancelledOrClosedTasks(Action<BMComponent, ZGuid> runTaskAction)
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

			runTaskAction(config.Buffer, workflow.PK);

			var newFactory = Factory.CreateNewFactory();
			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			var loadedTask3 = newFactory.Load<ProcessTask>(task3.PK);

			AssertEquals(resource.GS_Code, loadedTask1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign cancelled task", string.Empty, loadedTask2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not auto-assign closed task", string.Empty, loadedTask3.P9_GS_NKAssignedStaffMember);
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

			AssertNoExceptionThrown(@"If the assigner runs on a workflow in a bucket, an exception will be thrown, so the assigner should ignore bucket components.", () => RunAutoAssignmentAndGetLog(config.Buffer, workflow.PK));
		}

		[TestDate(2017, 5, 30)]
		public void TestAutoAssignWorkflows_BufferComponent_ShouldNotThrowInvalidOperation()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();
			var bucket = BMSTestHelper.CreateBucket(system, "bucket? bucket!");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AllowTaskAutoAssignment = true;

			var resource = helper.CreateResourceWithHomeBranchDeptSet("Bilbo Swaggins", capability);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task = helper.CreateTask(workflow, capability, null, 60);
			Assert("Pre-condition", task.RequiresResourceWithCapability);

			Factory.Save();

			AssertNoExceptionThrown(() => RunAutoAssignmentAndGetLog(buffer, workflow.PK,
				preAssignmentAction: workflows =>
				{
					workflow.FH_FC_CurrentComponent = bucket.PK;
					Factory.Save();
				}));

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();

			AssertEquals("Should have auto-assigned task to correct resource", resource.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2025, 04, 01)]
		public void TestAutoAssignWorkflows_ShouldNotAutoAssign_WhenAllowTaskAutoAssignmentIsFalse()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var groupAutoAssignEnabled = BMSTestHelper.CreateGroup(Factory, "GP1", "GP1");
			var groupAutoAssignDisabled = BMSTestHelper.CreateGroup(Factory, "GP2", "GP2");
			var otherGroup = BMSTestHelper.CreateGroup(Factory, "GP3", "GP3");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var pivot1 = capability.ReleaseGroupPivots.AddNew();
			pivot1.GGC_AllowTaskAutoAssignment = true;
			pivot1.GGC_GG_Group = groupAutoAssignEnabled.PK;
			pivot1.GGC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var pivot2 = capability.ReleaseGroupPivots.AddNew();
			pivot2.GGC_AllowTaskAutoAssignment = false;
			pivot2.GGC_GG_Group = groupAutoAssignDisabled.PK;
			pivot2.GGC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();

			var staff = helper.CreateResourceWithHomeBranchDeptSet("STAFF", capability);

			staff.Groups.Add(groupAutoAssignEnabled);
			staff.Groups.Add(groupAutoAssignDisabled);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "AAA", buffer, ZDateTime.Today.AddDays(-1), groupAutoAssignEnabled.PK);
			workflow1.FH_AllowTaskAutoAssignment = true;
			var taskOnWorkflowWithAutoAssignEnabled = helper.CreateTask(workflow1, capability, null, 10);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "BBB", buffer, ZDateTime.Today.AddDays(-1), groupAutoAssignDisabled.PK);
			workflow2.FH_AllowTaskAutoAssignment = true;
			var taskOnWorkflowWithAutoAssignDisabled = helper.CreateTask(workflow2, capability, null, 10);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "CCC", buffer, ZDateTime.Today.AddDays(-1), otherGroup.PK);
			workflow3.FH_AllowTaskAutoAssignment = true;
			var taskWithAutoAssignEnabled = helper.CreateTask(workflow3, capability, null, 10);
			taskWithAutoAssignEnabled.P9_GG_AssignedGroup = groupAutoAssignEnabled.PK;
			var taskWithAutoAssignDisabled = helper.CreateTask(workflow3, capability, null, 10);
			taskWithAutoAssignDisabled.P9_GG_AssignedGroup = groupAutoAssignDisabled.PK;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();

			RunAutoAssignmentAndGetLog(buffer, workflow1.PK);
			taskOnWorkflowWithAutoAssignEnabled = newFactory.Load<ProcessTask>(taskOnWorkflowWithAutoAssignEnabled.PK);
			AssertEquals("Should have auto-assigned to staff because workflow is in a group with autoAssignEnabled", staff.GS_Code, taskOnWorkflowWithAutoAssignEnabled.P9_GS_NKAssignedStaffMember);

			RunAutoAssignmentAndGetLog(buffer, workflow2.PK);
			taskOnWorkflowWithAutoAssignDisabled = newFactory.Load<ProcessTask>(taskOnWorkflowWithAutoAssignDisabled.PK);
			AssertEquals("Should NOT have auto-assigned to staff because workflow is not is in a group with autoAssignEnabled", ZString.Empty, taskOnWorkflowWithAutoAssignDisabled.P9_GS_NKAssignedStaffMember);

			RunAutoAssignmentAndGetLog(buffer, workflow3.PK);
			taskWithAutoAssignEnabled = newFactory.Load<ProcessTask>(taskWithAutoAssignEnabled.PK);
			AssertEquals("Should have auto-assigned to staff because task is in a group with autoAssignEnabled", staff.GS_Code, taskWithAutoAssignEnabled.P9_GS_NKAssignedStaffMember);
			taskWithAutoAssignDisabled = newFactory.Load<ProcessTask>(taskWithAutoAssignDisabled.PK);
			AssertEquals("Should have auto-assigned to staff because task is not in a group with autoAssignEnabled", ZString.Empty, taskWithAutoAssignDisabled.P9_GS_NKAssignedStaffMember);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			tasks = loadedWorkflow.Tasks.ToArray();

			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(false, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]", log.ToString());

			workflow.FH_AllowTaskAutoAssignment = true;
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_IsActive = false;

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]", log.ToString());

			workflow.FH_IsActive = true;
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
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

			var log = RunAutoAssignmentAndGetLog(bucket, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]", log.ToString());

			workflow.MoveToComponent(buffer);
			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());
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

			var log = RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow1.PK, workflow2.PK });

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var tasks1 = loadedWorkflow1.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks1[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks1[1].P9_GS_NKAssignedStaffMember);

			var loadedWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow2.PK);
			AssertNull(loadedWorkflow2);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (XVBQP68SIYXQ) - workflow1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());

			AssertContextSwitchCount(1);
		}

		#endregion

		#region Cross-Country Auto Assignment

		[TestDate(2018, 8, 20)]
		public void TestAutoAssigner_ShouldAssignRegardlessOfCompany_ShouldAssignForeignTaskToLocalResource()
		{
			void runTaskAction(BMComponent buffer, IEnumerable<ZGuid> workflowPKs)
			{
				// run service task in a foreign context
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					RunAutoAssignmentAndGetLog(buffer, workflowPKs);
				}
			}

			AssertAutoAssignmentWorksRegardlessOfCountry(newBranch.PK.ToGuid(), newCompany.CompanyName, runTaskAction);
			AssertContextSwitchCount(1);
		}

		[TestDate(2018, 8, 20)]
		public void TestAutoAssigner_ShouldAssignRegardlessOfCompany_ShouldAssignLocalTaskToForeignResource()
		{
			void runTaskAction(BMComponent buffer, IEnumerable<ZGuid> workflowPKs) => RunAutoAssignmentAndGetLog(buffer, workflowPKs);

			AssertAutoAssignmentWorksRegardlessOfCountry(newBranch.PK.ToGuid(), newCompany.CompanyName, runTaskAction);
			AssertContextSwitchCount(1);
		}

		void AssertAutoAssignmentWorksRegardlessOfCountry(Guid newBranchPK, string newCompanyName, Action<BMComponent, IEnumerable<ZGuid>> runTaskAction)
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			capability1.G4_AutoAssignTasksAge = buffer.FC_AutoAssignTasksAge;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);

			Factory.Save();

			ProcessHeader workflow1;
			ProcessHeader workflow2;
			ProcessTask task1;
			ProcessTask task2;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranchPK, Env.CurrentDepartmentPK))
			{
				workflow1 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
				workflow1.FH_IsActive = false;
				workflow1.FH_CompletionStatement = "I am number one";

				task1 = helper.CreateTask(workflow1, capability1, null, 60);
				Assert(task1.RequiresResourceWithCapability);
			}

			workflow2 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "MAIORMEL");
			workflow2.FH_IsActive = false;
			workflow2.FH_CompletionStatement = "That man is a fraud one";

			task2 = helper.CreateTask(workflow2, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			Factory.Save();

			AssertEquals("PRE: We expect that since we made this task in a different UserContext, they have a new Branch.", task1.Company.CompanyName, newCompanyName);
			AssertEquals("PRE: We expect that since we made this task locally, they have the CurrentBranch.", task2.Company.CompanyName, Env.CurrentCompany.Name);

			runTaskAction(buffer, new ZGuid[] { workflow1.PK, workflow2.PK });

			var newFactory = Factory.CreateNewFactory();
			var reloadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var reloadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals("Should not have auto-assigned", ZString.Empty, reloadedTask1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, reloadedTask2.P9_GS_NKAssignedStaffMember);

			workflow1.FH_IsActive = true;
			workflow2.FH_IsActive = true;
			Factory.Save();

			runTaskAction(buffer, new ZGuid[] { workflow1.PK, workflow2.PK });

			newFactory = Factory.CreateNewFactory();
			var reReloadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var reReloadedTask2 = newFactory.Load<ProcessTask>(task2.PK);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, reReloadedTask1.P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, reReloadedTask2.P9_GS_NKAssignedStaffMember);
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_SimpleAutoAssign_ShouldIgnoreCompanyServiceTaskCurrentCompany()
		{
			AssertNotEquals("PRE: We want our newCompany to NOT be the currentCompany!", newCompany.PK, Env.CurrentCompany.PK);
			AssertNotEquals("PRE: We want our newBranch to NOT be the currentBranch!", newBranch.PK, Env.CurrentBranch.PK);

			ILogger runTaskAction(BMComponent buffer, ZGuid workflowPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					return RunAutoAssignmentAndGetLog(buffer, workflowPK);
				}
			}

			AssertSimpleAutoAssign(runTaskAction);
			AssertContextSwitchCount(1);
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssign_ShouldNotAssignCancelledTasks_EvenAcrossCompanies()
		{
			AssertNotEquals("PRE: We want our newCompany to NOT be the currentCompany!", newCompany.PK, Env.CurrentCompany.PK);
			AssertNotEquals("PRE: We want our newBranch to NOT be the currentBranch!", newBranch.PK, Env.CurrentBranch.PK);

			void runTaskAction(BMComponent buffer, ZGuid workflowPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					RunAutoAssignmentAndGetLog(buffer, workflowPK);
				}
			}

			AssertShouldNotAssignCancelledOrClosedTasks(runTaskAction);
			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability2);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Now);

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			if (IsSchedulingAllowed)
			{
				AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] require delayed assignment.
Scheduled assigning capability tasks in workflow [Organization (MAIORGSYD) - Workflow 1] at [30-Jul-13 01:00:00]. The earliest tasks requiring assignment are as follows:
- [T00001000, T00001001] with required capability [COD]
- [T00001002] with required capability [RVW]", log.ToString());
			}
			else
			{
				AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 10:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 1:00)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks in workflow [Organization (MAIORGSYD) - Workflow 1] since it was released at 30-Jul-2013 10:00:00, which is 0:00 working hours ago. (Minimum hours before auto assignment is 1:00)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
			}

			workflow.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
		}

		[TestDate(2015, 7, 14)]
		public void TestAutoAssignAge_WhenLocalTimeOfServiceTaskIsDifferentToBufferTime_ShouldUseBufferLocalTime()
		{
			var serviceTaskBranch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			serviceTaskBranch.GB_Code = "LON";
			serviceTaskBranch.GB_RL_NKHomePort = "GBLON";

			const int autoAssignDelayMinutes = 10;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			config.Buffer.FC_GB_AgingBranch = newBranch.PK;
			config.Buffer.FC_AutoAssignTasksAge = new ZInt(autoAssignDelayMinutes).GetDateTimeFromMinutes();

			var capability = BMSTestHelper.CreateCapability(Factory, "NO", "No one's found out about this defect", autoAssignTasks: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Dave Eäst", capability);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Finite dimensional irreducible representation", config.Buffer, autoAssignTasks: true);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, lowEstMinutes: 10, capability: capability);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			ILogger log;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, serviceTaskBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				log = RunAutoAssignmentAndGetLog(config.Buffer, workflow.PK);
			}

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);

			AssertEquals(resource.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000] requiring capability [NO] in workflow [Organization (XVBQP68SIYXQ) - Finite dimensional irreducible representation] to resource [Dave Eäst], consuming [0.25] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Dave Eäst: 47.75 (47.50 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]".StripTaskIds(), log.ToString().StripTaskIds());

			AssertContextSwitchCount(1);
		}

		#endregion

		#region Capacity

		[TestDate(2013, 7, 30)]
		public void TestRun_NoResourcesHaveCapacity()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var env = new CapacityAllocationTestingEnvironment(Factory, system);

			var log = RunAutoAssignmentAndGetLog(env.Buffer, new ZGuid[] { env.Workflow1.PK, env.Workflow2.PK });

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto assign tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto assign tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			env.Task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			BufferCapacityCache.Clear();

			log = RunAutoAssignmentAndGetLog(env.Buffer, new ZGuid[] { env.Workflow1.PK, env.Workflow2.PK });

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
		}

		[TestDate(2016, 10, 18)]
		public void TestRun_NoResourcesHaveCapacity_AutoAssignRegistryIsOn()
		{
			BMSTestHelper.EnableBMSInRegistry();
			Assert("Precondition: this registry item should be 'true' by default", BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.Value);

			var env = new CapacityAllocationTestingEnvironment(Factory, system);

			var log = RunAutoAssignmentAndGetLog(env.Buffer, new ZGuid[] { env.Workflow1.PK, env.Workflow2.PK });

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should auto-assign to resource1 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should auto-assign to resource1 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should auto-assign to resource2 even if capacity is negative because 'AutoAssignTasksRegardlessCapacity' registry is on by default", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: -99.0 (-102.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: -97.5 (-99.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			env.Task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			env.Task2_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			log = RunAutoAssignmentAndGetLog(env.Buffer, new ZGuid[] { env.Workflow1.PK, env.Workflow2.PK });

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(env.Workflow1.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource1", env.Resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource1", env.Resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource2", env.Resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", env.Resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [1]", log.ToString());

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins");
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee");
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey");

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability2, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, capability2, resource3, 60);
			Assert(!task4.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should not have auto-assigned", ZString.Empty, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001000, T00001001] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [COD] capability.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			resource1.Capabilities.Add(capability1);
			resource2.Capabilities.Add(capability2);
			Factory.Save();

			log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have left assigned to resource3", resource3.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [RVW] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (45.0 after assignment)
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_ShouldAutoAssignToResourceWithMostCapacity()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("Samwise Gamgee", capability1);
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Gandalf the Grey", capability1);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow, capability1, null, 60);
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow, null, resource1, 120);
			Assert(!task4.RequiresResourceWithCapability);

			var task5 = helper.CreateTask(workflow, null, resource3, 120);
			Assert(!task5.RequiresResourceWithCapability);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource2 - has the most available capacity", resource2.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001, T00001002] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Samwise Gamgee], consuming [4.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Samwise Gamgee: 46.5 (42.0 after assignment)
Frodo Baggins: 43.5
Gandalf the Grey: 43.5
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1, capability2);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow, null, resource1, 30);
			var task2 = helper.CreateTask(workflow, capability1, null, 60);
			var task3 = helper.CreateTask(workflow, capability1, null, 60);
			var task4 = helper.CreateTask(workflow, capability2, null, 60);
			var task5 = helper.CreateTask(workflow, capability2, null, 60);

			Factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tasks = loadedWorkflow.Tasks.ToArray();
			AssertEquals("Should have left assigned to resource1", resource1.GS_Code, tasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[1].P9_GS_NKAssignedStaffMember);
			AssertEquals("Should have auto-assigned to resource1", resource1.GS_Code, tasks[2].P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Should have not auto-assigned to resource1", resource1.GS_Code, tasks[3].P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Should have not auto-assigned to resource1", resource1.GS_Code, tasks[4].P9_GS_NKAssignedStaffMember);

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001001, T00001002] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 3.25 (0.25 after assignment)
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto assign tasks [T00001003, T00001004] requiring capability [COX] in workflow [Organization (MAIORGSYD) - Workflow 1] because no resources have enough capacity.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
		}

		[TestDate(2013, 7, 30)]
		public void TestRun_DoesNotTakeADumpOnResourceWithMostCapacity()
		{
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("AAASome Poor Guy", capability1);
			resource1.GS_Code = "CDL";
			var resource2 = helper.CreateResourceWithHomeBranchDeptSet("IWONTBEDUMPEDUPON", capability1);
			resource2.GS_Code = "GGG";
			var resource3 = helper.CreateResourceWithHomeBranchDeptSet("Zubin has a smart name with zzzzzz", capability1);
			resource3.GS_Code = "ZZZ";

			var workflow1 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "AAAORGSYD", workflowCompletionStatement: "AAA");

			var task1_1 = helper.CreateTask(workflow1, capability1, null, 120);
			var task1_2 = helper.CreateTask(workflow1, capability1, null, 120);
			var task1_3 = helper.CreateTask(workflow1, capability1, null, 120);

			var workflow2 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "BBBORGSYD", workflowCompletionStatement: "BBB");

			var task2_1 = helper.CreateTask(workflow2, capability1, null, 120);
			var task2_2 = helper.CreateTask(workflow2, capability1, null, 120);
			var task2_3 = helper.CreateTask(workflow2, capability1, null, 120);

			var workflow3 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "CCCORGSYD", workflowCompletionStatement: "CCC");

			var task3_1 = helper.CreateTask(workflow3, capability1, null, 120);
			var task3_2 = helper.CreateTask(workflow3, capability1, null, 120);
			var task3_3 = helper.CreateTask(workflow3, capability1, null, 120);

			var workflow4 = helper.CreateJobAndWorkflow(false, buffer, ZDateTime.UtcNow); // Ensures resource assignment sequence is deterministic
			helper.CreateTask(workflow4, null, resource1, 1);
			helper.CreateTask(workflow4, null, resource2, 2);
			helper.CreateTask(workflow4, null, resource3, 3);

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow1.PK, workflow2.PK, workflow3.PK, workflow4.PK });

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

			AssertContextSwitchCount(1);
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

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, lowEstMinutes: 60, capability: capability, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, lowEstMinutes: 60, capability: capability, description: "task2");
			var task3 = BMSTestHelper.CreateTask(workflow3, string.Empty, lowEstMinutes: 60, capability: capability, description: "task3");

			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow already in the buffer, making capacity sequence deterministic", config.Buffer);
			BMSTestHelper.CreateTask(workflow4, resource1.GS_Code, lowEstMinutes: 1); // Ben has the least capacity consumed, so gets assigned stuff first.
			BMSTestHelper.CreateTask(workflow4, resource2.GS_Code, lowEstMinutes: 2);
			BMSTestHelper.CreateTask(workflow4, resource3.GS_Code, lowEstMinutes: 3);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(config.Buffer, workflow1.CurrentComponent);
			AssertEquals(config.Buffer, workflow2.CurrentComponent);
			AssertEquals(config.Buffer, workflow3.CurrentComponent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var log = RunAutoAssignmentAndGetLog(config.Buffer, new ZGuid[] { workflow1.PK, workflow2.PK, workflow3.PK, workflow4.PK });

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

			AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow1] to resource [Ben Gorringe], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Ben Gorringe: 46.47 (44.97 after assignment)
Peter Cronin: 46.45
Shane D'Aprile: 46.42
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001001] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow2] to resource [Peter Cronin], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Peter Cronin: 46.45 (44.95 after assignment)
Shane D'Aprile: 46.42
Ben Gorringe: 44.97
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001002] requiring capability [ROB] in workflow [Organization (XVBQP68SIYXQ) - workflow3] to resource [Shane D'Aprile], consuming [1.5] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Shane D'Aprile: 46.42 (44.92 after assignment)
Ben Gorringe: 44.97
Peter Cronin: 44.95
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", log.ToString());

			AssertContextSwitchCount(1);
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

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			workflow1.Reload();
			workflow2.Reload();

			AssertEquals(config.Buffer, workflow1.CurrentComponent);
			AssertEquals(config.Buffer, workflow2.CurrentComponent);

			AssertEquals(47.25m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity);
			AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(autoAssignDelayMinutes + 1);

			var log = RunAutoAssignmentAndGetLog(config.Buffer, new ZGuid[] { workflow1.PK, workflow2.PK });

			var newFactory = Factory.CreateNewFactory();

			var loadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
			var loadedTask2 = newFactory.Load<ProcessTask>(task2.PK);

			CombineAssertions("Tasks should be assigned between the available resources rather than dumped on one person alone", () =>
			{
				AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource1, config.Buffer).AvailableCapacity);
				AssertEquals(45.75m, CapacityCalculator.GetUtilisedCapacityBreakdown(resource2, config.Buffer).AvailableCapacity);

				AssertEquals("task1 should be auto-assigned to BEN", resource1.GS_Code, loadedTask1.P9_GS_NKAssignedStaffMember);
				AssertEquals("task2 was already assigned", resource2.GS_Code, loadedTask2.P9_GS_NKAssignedStaffMember);
			});

			AssertContextSwitchCount(1);
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

			var log = RunAutoAssignmentAndGetLog(config.Buffer, new ZGuid[] { workflow1_1.PK, workflow1_2.PK, workflow2.PK, workflow3.PK });

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

			AssertContextSwitchCount(1);
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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				RowFactory.ResetCacheAfterDbUpgrade();

				using (AssertDbHitsForAllFactories(hits, includeFactoryPredicate: f =>
						f.NameForDebugging.Contains("WorkflowCapabilityAssignerTestCase")
						|| f.NameForDebugging.Contains("CapabilityTaskAutoAssignmentServiceTask")
						|| f.NameForDebugging.Contains("WorkflowCapabilityAssigner")
						|| f.NameForDebugging.Contains(nameof(WorkflowCapabilityAssignerDataAccessor))
						|| f.NameForDebugging.Contains(nameof(WorkflowCapabilityAssignerRelatedDataLoader)),
					ignoreHitsFromTablesCachedInUberFactory: true))
				{
					env.RunAssignment();
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

				env.RunAssignment();

				AssertLog("Service task log", @"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto assign tasks [T00001000] requiring capability [COD] in workflow [Organization (MYORG) - WFL] because no resources have enough capacity or they are not allowed to be assigned by task autoassignment restrictions.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [00:00:00] Batches processed: [2]", env.Logger.ToString());
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Crazy Vaclav", capability1);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));

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

			var log = RunAutoAssignmentAndGetLog(buffer, workflow.PK, sortWorkflows);

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var loadedTasks = loadedWorkflow.Tasks.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Should have auto-assigned task1 to resource1", resource1.GS_Code, loadedTasks.Single(t => t.P9_Description == "Zagreb").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned task2 to resource1", resource1.GS_Code, loadedTasks.Single(t => t.P9_Description == "Ebnom").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should NOT have assigned task3 to different capability resource1", ZString.Empty, loadedTasks.Single(t => t.P9_Description == "Zlotik").P9_GS_NKAssignedStaffMember);
				AssertEquals("Should NOT have assigned task4 to different capability resource1", ZString.Empty, loadedTasks.Single(t => t.P9_Description == "Diev").P9_GS_NKAssignedStaffMember);

				var logString = Regex.Replace(log.ToString(), @"\[\d\d:\d\d:\d\d\]", @"[PUTITINH]"); // sorting our workflows to make the times they're changed deterministic adds a slight delay to the test, this removes that

				AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Crazy Vaclav], consuming [0.50] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Crazy Vaclav: 47.50 (47.00 after assignment)
Task assignment resulted in the assigning of related tasks.

Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [PUTITINH] Batches processed: [2]".StripTaskIds(), logString.StripTaskIds());
			});

			AssertContextSwitchCount(1);
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

			var resource1 = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins", capability1);

			var workflow1 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			var workflow2 = helper.CreateWorkflow(workflow1.Parent, true, buffer, ZDateTime.Today.AddDays(-1));

			var task1 = helper.CreateTask(workflow1, capability1, null, 60);
			task1.P9_Type = "INV";
			Assert(task1.RequiresResourceWithCapability);

			var task2 = helper.CreateTask(workflow2, capability1, null, 60);
			task2.P9_Type = "CDU";
			Assert(task2.RequiresResourceWithCapability);

			var task3 = helper.CreateTask(workflow1, capability2, null, 60);
			task3.P9_Type = "CDF";
			Assert(task3.RequiresResourceWithCapability);

			var task4 = helper.CreateTask(workflow2, capability2, null, 60);
			task4.P9_Type = "SHV";
			Assert(task4.RequiresResourceWithCapability);

			Factory.Save();

			void sortWorkflows(ProcessHeader[] processHeaders)
			{
				processHeaders = processHeaders.OrderBy(ph => ph.FH_WorkflowType).ToArray();
			}
			var log = RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow1.PK, workflow2.PK }, sortWorkflows);

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);
			var loadedTasks1 = loadedWorkflow1.Tasks.ToArray();
			var loadedTasks2 = loadedWorkflow2.Tasks.ToArray();

			AssertNotNull("Should have auto-assigned to resource1", loadedTasks1.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember == resource1.GS_Code));
			AssertNotNull("Should have auto-assigned to resource1", loadedTasks2.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember == resource1.GS_Code));
			AssertNotNull("Should NOT have assigned to different capability resource1", loadedTasks1.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember.IsEmpty));
			AssertNotNull("Should NOT have assigned to different capability resource1", loadedTasks2.SingleOrDefault(t => t.P9_GS_NKAssignedStaffMember.IsEmpty));

			var logString = Regex.Replace(log.ToString(), @"\[\d\d:\d\d:\d\d\]", @"[IRRELEPHANT]"); // sorting our workflows to make the times they're changed deterministic adds a slight delay to the test, this removes that

			AssertLog("Service task log", $@"Auto-assigning tasks using CurrentComponent auto-assigning task age.
Assigned tasks [T00001000, T00001001] requiring capability [COD] in workflow [Organization (MAIORGSYD) - Workflow 1] to resource [Frodo Baggins], consuming [3.0] hours of capacity.
Resource Capacity at the time of assignment (in hours):
Frodo Baggins: 45.0 (42.0 after assignment)
Task assignment resulted in the assigning of related tasks.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Auto-assigning tasks using CurrentComponent auto-assigning task age.
Could not auto-assign tasks [T00001002, T00001003] in workflow [Organization (MAIORGSYD) - Workflow 1] because capability don't have assign tasks group enabled or no active staff [RVW] capability.
Debug - Performance: Capability Task Auto-Assignment, Loading time: [IRRELEPHANT] Batches processed: [2]".StripTaskIds(), logString.StripTaskIds());

			AssertContextSwitchCount(1);
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

				env.RunAssignment();

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
				var newTask = helper.CreateTask(env.CurrentWorkflow, env.Capability, resource: null, estDurationMinutes: 60);
				newTask.P9_Type = env.TaskCapability.P9_Type;

				Factory.Save();

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

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

				env.RunAssignment();

				env.ReloadTasksWithNewFactory();

				AssertEquals("Should have auto-assigned to the resource with capability 2 due to the SAM restrictions and sequence numbers", env.ResourceWithCapability2.GS_Code, env.TaskCapability1_Reloaded.P9_GS_NKAssignedStaffMember);
				AssertEquals("Should have auto-assigned to the resource with capability 2", env.ResourceWithCapability2.GS_Code, env.TaskCapability2_Reloaded.P9_GS_NKAssignedStaffMember);
			}
		}

		#endregion

		#region Auto Task Assignment with Task Group

		[TestDate(2019, 11, 20)]
		public void TestAutoAssignTasks_ShouldConsiderOnlyWorkflowReleaseGroupAndCapabilityIntersection_WhenTaskGroupIsNotSpecified_AndCapabilityScopeIsGroup()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.G4_AllowTaskAutoAssignment = true;

			BMSTestHelper.AddTaskTypesToRegistry("ORG", capability.G4_Code);

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "ST1";
			staffMember1.Capabilities.Add(capability);
			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "ST2";
			staffMember2.Capabilities.Add(capability);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Code = "REL";
			releaseGroup.Staff.Add(staffMember1);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			var task = helper.CreateTask(workflow, capability, null, 60);

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow.PK });

			AssertNotNull(task.AssignedStaffMember);
			AssertEquals("Auto-assignment should consider the overlap between release group and capability when assigning staff member to task", staffMember1.GS_Code, task.AssignedStaffMember.GS_Code);
		}

		[TestDate(2019, 11, 20)]
		public void TestAutoAssignTasks_ShouldConsiderOnlyTaskGroupAndCapabilityIntersection_EvenWhenWorkflowReleaseGroupIsSpecified_WhenCapabilityScopeIsGroup()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.G4_AllowTaskAutoAssignment = true;

			BMSTestHelper.AddTaskTypesToRegistry("ORG", capability.G4_Code);

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "ST1";
			staffMember1.Capabilities.Add(capability);
			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "ST2";
			staffMember2.Capabilities.Add(capability);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Code = "REL";
			releaseGroup.Staff.Add(staffMember1);

			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			taskGroup.GG_Code = "SPL";
			taskGroup.Staff.Add(staffMember2);

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			var task = helper.CreateTask(workflow, capability, null, 60);
			task.P9_GG_AssignedGroup = taskGroup.PK;

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow.PK });

			AssertNotNull(task.AssignedStaffMember);
			AssertEquals("Auto-assignment should consider the overlap between task group and capability when assigning staff member to task", staffMember2.GS_Code, task.AssignedStaffMember.GS_Code);
		}

		[TestDate(2019, 11, 20)]
		public void TestAutoAssignTasks_ShouldFallbackToWorkflowReleaseGroupAndCapabilityIntersection_WhenTaskGroupIsSpecifiedButNoSuitableResourceFound_AndCapabilityScopeIsGroup()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			capability.G4_AllowTaskAutoAssignment = true;

			BMSTestHelper.AddTaskTypesToRegistry("ORG", capability.G4_Code);

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "ST1";
			staffMember1.Capabilities.Add(capability);
			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "ST2";
			staffMember2.Capabilities.Add(capability);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Code = "REL";
			releaseGroup.Staff.Add(staffMember1);

			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			taskGroup.GG_Code = "SPL";

			var workflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			var task = helper.CreateTask(workflow, capability, null, 60);
			task.P9_GG_AssignedGroup = taskGroup.PK;

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflow.PK });

			AssertNotNull(task.AssignedStaffMember);
			AssertEquals("Auto-assignment should consider the overlap between release group and capability when assigning staff member to task", staffMember1.GS_Code, task.AssignedStaffMember.GS_Code);
		}

		[TestDate(2019, 11, 20)]
		public void TestAutoAssignTasks_ShouldConsiderAllResourcesWithCapability_EvenWhenTaskAndWorkflowReleaseGroupsAreSpecified_WhenCapabilityScopeIsGlobal()
		{
			WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			capability.G4_AllowTaskAutoAssignment = true;

			BMSTestHelper.AddTaskTypesToRegistry("ORG", capability.G4_Code);

			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "ST1";
			staffMember1.Capabilities.Add(capability);

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "ST2";
			staffMember2.Capabilities.Add(capability);

			var staffMember3 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember3.GS_Code = "ST3";
			staffMember3.Capabilities.Add(capability);

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Code = "REL";
			releaseGroup.Staff.Add(staffMember1);

			var taskGroup = Factory.NewWithValidTestData<GlbGroup>();
			taskGroup.GG_Code = "SPL";
			taskGroup.Staff.Add(staffMember2);

			var assignedWorkflow = helper.CreateJobAndWorkflow(false, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "MAIFRSTOG", workflowCompletionStatement: "Assigned");
			assignedWorkflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			helper.CreateTask(assignedWorkflow, capability, staffMember1, 60);
			helper.CreateTask(assignedWorkflow, capability, staffMember2, 60);

			var unassignedWorkflow = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), orgHeaderCode: "MAISCNDOG", workflowCompletionStatement: "Unassigned");
			unassignedWorkflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			var unassignedTask = helper.CreateTask(unassignedWorkflow, capability, null, 60);
			unassignedTask.P9_GG_AssignedGroup = taskGroup.PK;

			Factory.Save();

			RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { unassignedWorkflow.PK });

			AssertNotNull(unassignedTask.AssignedStaffMember);
			AssertEquals("Auto-assignment should consider the resource with the capability that has the most capacity regardless of task or release group membership", staffMember3.GS_Code, unassignedTask.AssignedStaffMember.GS_Code);
		}

		#endregion

		#endregion

		#region Auto Assign Failures

		class WorkItemCreator : IWorkItemHelper
		{
			public void CreateWorkItem(BusinessObjectFactory factory, bool doNotCreateIfMatchesOnSummaryWithAnOpenWorkItem, ZString type,
				ZString area, ZString activityType, ZString activitySubtype, ZString priority, ZString summary, ZString details)
			{
				if (doNotCreateIfMatchesOnSummaryWithAnOpenWorkItem)
				{
					var query = new ZQuery(WorkItemSchema.WKI_Summary, SQLComparisonOperator.Equal, summary)
						.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed)
						.AddToFilter(WorkItemSchema.WKI_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);

					var existingWorkItem = factory.LoadTop1<IWorkItem>(query);

					if (existingWorkItem != null)
					{
						return;
					}
				}

				var workItem = factory.New<IWorkItem>();
				workItem.WKI_WorkItemType = type;
				workItem.WKI_WorkItemArea = area;
				workItem.WKI_ActivityType = activityType;
				workItem.WKI_ActivitySubtype = activitySubtype;
				workItem.WKI_Priority = priority;
				workItem.WKI_Summary = summary;
				workItem.WKI_Details = ZBlob.FromUTF8(details);
				factory.Save();
			}
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignFailure_InactiveCapability_ShouldCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability capability, GlbGroup group, GlbStaff resource) = AutoAssignFailureTestCommonSetup();
			capability.G4_IsActive = false;
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2, shouldAutoAssign: true, shouldCreateWI: false);
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignFailure_GlobalCapabilityWithoutActiveMembers_ShouldCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability capability, GlbGroup _, GlbStaff resource) = AutoAssignFailureTestCommonSetup();
			resource.Capabilities.Remove(capability);
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2);
			AutoAssignFailureTestRun_CreateSecondWorkItem(buffer, workflow1, workflow2, string.Empty, "Capability task auto assignment failure for capability COD", "Global Capability has no active members");
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignFailure_InactiveGroup_ShouldCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability capability, GlbGroup group, GlbStaff _) = AutoAssignFailureTestCommonSetup();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			group.GG_IsActive = false;
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2, shouldAutoAssign: true, shouldCreateWI: false);
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignFailure_GroupCapabilityWithoutActiveMembers_ShouldCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability capability, GlbGroup group, GlbStaff resource) = AutoAssignFailureTestCommonSetup();
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			resource.Groups.Remove(group);
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2);
			AutoAssignFailureTestRun_CreateSecondWorkItem(buffer, workflow1, workflow2, group.GG_Code, "Capability task auto assignment failure for capability COD and group GRU", "Group Capability - Release Group intersection has no active members");
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignFailure_NotEnoughCapacity_ShouldNotCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability _, GlbGroup _, GlbStaff resource) = AutoAssignFailureTestCommonSetup();
			var largeTask = helper.CreateTask(workflow1, null, resource, 2400);
			largeTask.P9_Description = "IgnoreMe_HighCapacityTask";
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2, shouldAutoAssign: false, shouldCreateWI: false);
		}

		[TestDate(2021, 7, 1)]
		public void TestAutoAssignSuccess_ShouldNotCreateFailureWorkItem()
		{
			(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, GlbCapability _, GlbGroup _, GlbStaff _) = AutoAssignFailureTestCommonSetup();
			Factory.Save();

			AutoAssignFailureTestRun_CreateFirstWorkItem(buffer, workflow1, workflow2, shouldAutoAssign: true, shouldCreateWI: false);
		}

		(BMComponent, ProcessHeader, ProcessHeader, GlbCapability, GlbGroup, GlbStaff) AutoAssignFailureTestCommonSetup()
		{
			BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TemplateCriteria { Enabled = true });

			var workItemCreator = new WorkItemCreator();
			ObjectFactory.Substitute<IWorkItemHelper>(workItemCreator);

			var wkiSystem = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "Workflow");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow);
			templateTask.P9_Description = "IgnoreMe_TemplateTask";

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_AllowTaskAutoAssignment = true;
			capability.G4_Code = "COD";
			capability.G4_Description = "big fish";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GRU";

			var resource = helper.CreateResourceWithHomeBranchDeptSet("Frodo Baggins");
			resource.Capabilities.Add(capability);
			resource.Groups.Add(group);

			var workflow1 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1));
			workflow1.FH_GG_ReleaseGroup = group.PK;
			var workflow2 = helper.CreateJobAndWorkflow(true, buffer, ZDateTime.Today.AddDays(-1), "MAIORGMEL");
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var task1 = helper.CreateTask(workflow1, capability, null, 60);
			var task2 = helper.CreateTask(workflow1, capability, null, 60);
			var task3 = helper.CreateTask(workflow2, capability, null, 60);
			var task4 = helper.CreateTask(workflow2, capability, null, 60);
			task1.P9_Description = task2.P9_Description = task3.P9_Description = task4.P9_Description = "lala lalala";

			return (buffer, workflow1, workflow2, capability, group, resource);
		}

		void AutoAssignFailureTestRun_CreateFirstWorkItem(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, bool shouldAutoAssign = false, bool shouldCreateWI = true)
		{
			var log = RunAutoAssignmentAndGetLog(buffer, new List<ZGuid> { workflow1.PK, workflow2.PK });

			var factory = new BusinessObjectFactory();
			var loadedWorkflow1 = factory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = factory.Load<ProcessHeader>(workflow2.PK);

			var tasks = loadedWorkflow1.Tasks.Concat(loadedWorkflow2.Tasks.ToArray());
			foreach (var task in tasks.Where(t => !t.P9_Description.StartsWith("IgnoreMe")))
			{
				if (shouldAutoAssign)
				{
					AssertNotEquals("Should have auto-assigned task to resource", string.Empty, task.P9_GS_NKAssignedStaffMember);
				}
				else
				{
					AssertEquals("Should not have auto-assigned task to resource", string.Empty, task.P9_GS_NKAssignedStaffMember);
				}
			}

			var workItems = factory.Load<IWorkItem>(new ZQuery());
			AssertEquals(shouldCreateWI ? 1 : 0, workItems.Length);
		}

		void AutoAssignFailureTestRun_CreateSecondWorkItem(BMComponent buffer, ProcessHeader workflow1, ProcessHeader workflow2, string group, string summary, string reason)
		{
			var factory = new BusinessObjectFactory();
			var tasks = factory.Load<ProcessTask>(new ZQuery()).Where(t => t.P9_ParentTemplateID.IsValid);
			tasks.First().P9_Status = "CAN";
			factory.Save();

			var log = RunAutoAssignmentAndGetLog(buffer, new List<ZGuid> { workflow1.PK, workflow2.PK });
			var workItems = factory.Load<IWorkItem>(new ZQuery());
			AssertEquals("Should have created another failure work item", 2, workItems.Length);

			foreach (var wi in workItems)
			{
				AssertEquals(summary, wi.WKI_Summary);

				var detailsActual = wi.WKI_Details.ToUTF8();
				if (detailsActual.Contains(workflow1.Parent.PK.ToString()))
				{
					AssertContains(@"HYPERLINK ""edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=" + workflow1.Parent.PK + @"&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=", detailsActual);
					AssertContains(@"Organization (MAIORGSYD)", detailsActual);
				}
				else if (detailsActual.Contains(workflow2.Parent.PK.ToString()))
				{
					AssertContains(@"HYPERLINK ""edient:Command=ShowEditForm&ControllerID=Organisation&BusinessEntityPK=" + workflow2.Parent.PK + @"&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=", detailsActual);
					AssertContains(@"Organization (MAIORGMEL)", detailsActual);
				}
				else
				{
					Assert("Job link does not match expected:\n" + detailsActual, false);
				}

				AssertEquals((@"{\rtf1
Time (UTC): 01-Jul-21 00:00:00\line
Job link was checked in previous assert
Workflow: Workflow 1\line
Release Group: " + group + @"\line
Task: lala lalala\line
Capability: COD - big fish\line
Reason: " + reason + @"
}"), Regex.Replace(detailsActual, "Job: .*", "Job link was checked in previous assert\r"));
			}
		}

		#endregion

		#region Implementation

		class DummyWithWorkflowAndTemplateApplicationOnSaving : DummyWithWorkflow
		{
			public DummyWithWorkflowAndTemplateApplicationOnSaving(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		protected void AssertLog(string message, string expectedLog, string actualLog)
		{
			// later if all methods on WorkflowCapabilityAssigner log in the same way using BatchLogger, this logic can we removed

			var actualLines = actualLog.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var lastActualLine = actualLines?.LastOrDefault();

			if (string.IsNullOrEmpty(lastActualLine) || !lastActualLine.Contains("Performance: Capability Task Auto-Assignment, Loading time:"))
			{
				var expectedLines = expectedLog.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				var lastExpectedLine = expectedLines?.LastOrDefault();

				if (!string.IsNullOrEmpty(lastExpectedLine) && lastExpectedLine.Contains("Performance: Capability Task Auto-Assignment, Loading time:"))
				{
					expectedLines = expectedLines.Take(expectedLines.Length - 1).ToArray();
					expectedLog = string.Join(System.Environment.NewLine, expectedLines);
				}
			}

			AssertMultilineASCIIEquals(message, expectedLog.StripTaskIds(), actualLog.Replace("Organisation", "Organization").StripTaskIds());
		}

		void AssertContextSwitchCount(int count)
		{
			AssertEquals(IsAutoAssignWorkflowsInBatchesImmediatelyTestCase ? 0 : count,
				assigner.ContextSwitchCount);
		}

		#region IWorkflowCapabilityAssignerTestCase Members

		#region Run Auto Assignment

		public ILogger RunAutoAssignmentAndGetLog(BMComponent buffer, ZGuid workflowPK, Action<ProcessHeader[]> preAssignmentAction = null) => RunAutoAssignmentAndGetLog(buffer, new ZGuid[] { workflowPK }, preAssignmentAction);

		public ILogger RunAutoAssignmentAndGetLog(BMComponent buffer, IEnumerable<ZGuid> workflowPKs, Action<ProcessHeader[]> preAssignmentAction = null)
		{
			assigner = new WorkflowCapabilityAssigner_ForTest(preAssignmentAction);
			var logger = new BufferManagementLogger();
			var dataAccessor = new WorkflowCapabilityAssignerDataAccessor_ForTest(logger);
			dataAccessor.WorkflowPKs.AddRange(workflowPKs);
			RunAutoAssignmentAndGetLogCore(assigner, dataAccessor, logger, buffer, workflowPKs);
			return logger;
		}

		protected abstract void RunAutoAssignmentAndGetLogCore(WorkflowCapabilityAssigner assigner, IWorkflowCapabilityAssignerDataAccessor dataAccessor, ILogger logger, BMComponent buffer, IEnumerable<ZGuid> workflowPKs);

		#endregion

		BusinessObjectFactory IWorkflowCapabilityAssignerTestCase.Factory => Factory;

		BMSystem IWorkflowCapabilityAssignerTestCase.System => system;

		WorkflowCapabilityAssigner_ForTest assigner;

		#endregion

		protected virtual bool UsingSimpleQuery => false;

		protected abstract bool IsSchedulingAllowed { get; }

		bool IsAutoAssignWorkflowsInBatchesImmediatelyTestCase => this is WorkflowCapabilityAssignerTestCase_AutoAssignWorkflowsInBatchesImmediately;

		#region SetUp And TearDown

		protected BMSystem system;
		protected WorkflowCapabilityAssignerTestHelper helper;

		CultureInfo originalCulture;
		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			Enterprise.VisualBoards.Business.Test.VisualBoardsTestCase.SetupAndClearTables();
			base.SetUp();

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			Factory.NameForDebugging = nameof(WorkflowCapabilityAssignerTestCase);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
			BMSTestHelper.EnableBMSInRegistry();
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());

			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US"); // To help us quickly identify dates that are formatted incorrectly, rather than waiting for amnesties.

			helper = new WorkflowCapabilityAssignerTestHelper(Factory);

			system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";

			newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "MEL";
			newBranch.GB_RL_NKHomePort = "AUMEL";
			Factory.Save();
			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: newBranch.PK.ToGuid());
			Factory.Save();
		}

		GlbBranch newBranch;
		GlbCompany newCompany;

		protected override void TearDown()
		{
			base.TearDown();

			instanceDetailsDisposable?.Dispose();
			BufferCapacityCache.Clear();
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		#endregion

		#endregion
	}
}
