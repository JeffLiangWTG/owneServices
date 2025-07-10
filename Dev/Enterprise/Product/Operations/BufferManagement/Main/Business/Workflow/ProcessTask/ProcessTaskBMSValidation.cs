using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Business
{
	class ProcessTaskBMSValidation : IProcessTaskBMSValidation
	{
		#region IProcessTaskBMSValidation Members

		void IProcessTaskBMSValidation.CheckP9_EstDuration(IProcessTask task)
		{
			if (isBMSEnabled)
			{
				if (task.P9_EstDuration.IsEmpty && task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed && task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					var processHeader = task.GetProcessHeader();
					if (processHeader != null && processHeader.CurrentComponent != null && processHeader.CurrentComponent.FC_Type == BMComponentTypeList.Codes.Buffer && !task.IsCompletionTask() && !IsIgnoredTaskType(task))
					{
						task.P9_EstDurationInfo.AddWarning(Res.GetString("c883c01c-a138-4b01-b1ee-567c1d81cb1d", "Tasks on workflows that are in a Buffer component should have estimates entered. If they do not, transfer rules may not operate correctly."));
					}
				}
			}
		}

		void IProcessTaskBMSValidation.CheckP9_GS_NKAssignedStaffMember(IProcessTask task)
		{
			if (isBMSEnabled)
			{
				if (task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_G4_RequiredCapability.IsEmpty)
				{
					var processHeader = task.GetProcessHeader();
					if (processHeader != null && processHeader.CurrentComponent != null && processHeader.CurrentComponent.FC_Type == BMComponentTypeList.Codes.Buffer && !IsIgnoredTaskType(task))
					{
						task.P9_GS_NKAssignedStaffMemberInfo.AddWarning(Res.GetString("638d55da-cac7-4aee-b08a-e51caa248090", "Tasks on workflows that are in a Buffer component should be assigned. If they are not, transfer rules may not operate correctly."));
					}
				}
			}
		}

		void IProcessTaskBMSValidation.CheckP9_FH_ProcessHeader(IProcessTask task)
		{
			var concreteTask = (ProcessTask)task;
			var parent = concreteTask.Parent;
			var isBMSEnabledForWorkflow = parent != null && ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(parent, concreteTask.Factory);

			if (isBMSEnabledForWorkflow && !(task is TemplateProcessTask))
			{
				if (concreteTask.IsInDatabase && concreteTask.P9_FH_ProcessHeaderInfo.OriginalValue.IsEmpty && !concreteTask.P9_FH_ProcessHeaderInfo.HasChanges)
				{
					concreteTask.P9_FH_ProcessHeaderInfo.AddWarning(Res.GetString("346b7fea-4a41-435b-870b-0376ff970489", "Please enter a Workflow."));
				}
				else
				{
					MandatoryValidation.CheckEntered(concreteTask.P9_FH_ProcessHeaderInfo);
				}
			}

			if (parent == null)
			{
				ListValidation.ErrorIfInvalidPK(concreteTask.P9_FH_ProcessHeaderInfo);
			}

			CheckTemplateTaskBelongsToTemplateWorkflow(task);
		}

		void CheckTemplateTaskBelongsToTemplateWorkflow(IProcessTask task)
		{
			var concreteTask = (ProcessTask)task;

			if (concreteTask.P9_FH_ProcessHeader.IsEmpty && concreteTask.IsTemplateTask)
			{
				var template = concreteTask.Factory.Load<ProcessTaskTemplate>(concreteTask.P9_ParentID);

				if (template != null && template.ProcessHeaders.Count > 1) // there is always a job header
				{
					concreteTask.P9_FH_ProcessHeaderInfo.AddError(Res.GetString("F29071B7-201C-4B57-B706-678223304072", "This workflow template includes at least one workflow. Please assign all tasks to a workflow from the template or remove all workflows from the template."));
				}
			}
		}

		#endregion

		#region Implementation

		static bool IsIgnoredTaskType(IProcessTask task)
		{
			var taskParent = task.GetProcessHeader().Parent;
			if (taskParent != null)
			{
				var taskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(taskParent.WorkflowType).Cast<WorkflowTaskType>().FirstOrDefault(t => t.Code == task.P9_Type);
				if (taskType != null)
				{
					return taskType.IsExcludedFromTransferRules;
				}
			}

			return false;
		}

		readonly bool isBMSEnabled = BMSRegistryProvider.IsBufferManagementEnabled;

		#endregion
	}
}
