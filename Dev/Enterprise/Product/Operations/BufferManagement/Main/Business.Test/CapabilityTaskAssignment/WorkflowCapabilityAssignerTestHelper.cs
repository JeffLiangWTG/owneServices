using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public class WorkflowCapabilityAssignerTestHelper
	{
		public WorkflowCapabilityAssignerTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		BusinessObjectFactory Factory { get; }

		public ProcessHeader CreateJobAndWorkflow(bool autoAssignTasks, BMComponent currentComponent, ZDateTime releaseDateTime, string orgHeaderCode = "MAIORGSYD", string workflowCompletionStatement = "Workflow 1")
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = orgHeaderCode;

			return CreateWorkflow(job, autoAssignTasks, currentComponent, releaseDateTime, workflowCompletionStatement);
		}

		public ProcessHeader CreateWorkflow(IWorkflowProvider job, bool autoAssignTasks, BMComponent currentComponent, ZDateTime releaseDateTime, string workflowCompletionStatement = "Workflow 1")
		{
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = workflowCompletionStatement;
			workflow.FH_AllowTaskAutoAssignment = autoAssignTasks;
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_ReleaseDateTime = releaseDateTime;

			return workflow;
		}

		public GlbStaff CreateResourceWithHomeBranchDeptSet(string fullName, params GlbCapability[] capabilities)
		{
			return CreateResourceWithGivenHomeBranchAndDept(fullName, Env.CurrentDepartmentPK, Env.CurrentBranchPK, capabilities);
		}

		public GlbStaff CreateResourceWithGivenHomeBranchAndDept(string fullName, Guid dept, Guid branch, params GlbCapability[] capabilities)
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_GE_HomeDepartment = dept;
			resource.GS_GB_HomeBranch = branch;
			resource.GS_FullName = fullName;

			foreach (var capability in capabilities)
			{
				resource.Capabilities.Add(capability);
			}

			return resource;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public ProcessTask CreateTask(ProcessHeader workflow, GlbCapability capability, GlbStaff resource, int estDurationMinutes)
		{
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_G4_RequiredCapability = capability != null ? capability.PK : ZGuid.Empty;
			task.P9_GS_NKAssignedStaffMember = resource != null ? resource.GS_Code : ZString.Empty;
			task.P9_EstDuration = new ZInt(estDurationMinutes).GetDateTimeFromMinutes();

			return task;
		}
	}
}
