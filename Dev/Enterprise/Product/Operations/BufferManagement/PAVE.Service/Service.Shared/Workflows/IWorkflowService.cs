using System;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Service.Shared.Workflows.Dtos;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IWorkflowService
	{
		bool TryReorderTask(Guid workflowId, Guid taskId, WorkflowTaskReorderRequest workflowTaskReorderRequest, out PaveError error);

		bool TryGroupTask(Guid workflowId, int groupId, Guid taskId, WorkflowGroupTaskRequest workflowGroupTaskRequest, out PaveError error);

		public bool TryReorderGroup(Guid workflowId, int groupId, WorkflowGroupReorderRequest workflowGroupReorderRequest, out PaveError error);

		public bool TryMergeGroups(Guid workflowId, int groupId, int sourceGroupId, WorkflowGroupMergeRequest workflowGroupMergeRequest, out PaveError error);

		public bool TryMergeGroupOnTask(Guid workflowId, Guid taskId, int sourceGroupId, WorkflowMergeGroupOnTaskRequest request, out PaveError error);
	}
}
