using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Business
{
	public class TaskInclusionHelper
	{
		public IEnumerable<IWorkflowTask> GetTasksForCapacityCheck(IWorkflow workflow)
		{
			return GetTasks(workflow, returnIgnoredTasks: false);
		}

		public IEnumerable<IWorkflowTask> GetIncompleteIgnoredTasks(IWorkflow workflow)
		{
			return GetTasks(workflow, returnIgnoredTasks: true);
		}

		public static bool IsIgnoredTaskTypeForChannelingAndChunkingConsiderations(WorkflowTaskType taskType)
		{
			return taskType.IsExcludedFromTransferRules || taskType.IsCompletionStatementTaskType;
		}

		IEnumerable<IWorkflowTask> GetTasks(IWorkflow workflow, bool returnIgnoredTasks)
		{
			var ignoredTaskTypes = GetTaskTypesToIgnore(workflow).ToHashSet();

			foreach (var task in workflow.Tasks)
			{
				if (task.IsOpen() && returnIgnoredTasks == ignoredTaskTypes.Contains(task.P9_Type))
				{
					yield return task;
				}
			}
		}

		IEnumerable<string> GetTaskTypesToIgnore(IWorkflow workflow)
		{
			var workflowType = workflow.WorkflowType;

			if (!string.IsNullOrEmpty(workflowType))
			{
				return ignoredTaskTypesByWorkflowType.GetOrAdd(workflowType, () => GetTaskTypesToIgnoreCore(workflowType));
			}
			else
			{
				return Enumerable.Empty<string>();
			}
		}

		static IEnumerable<string> GetTaskTypesToIgnoreCore(string workflowType)
		{
			foreach (WorkflowTaskType taskType in WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(workflowType))
			{
				if (IsIgnoredTaskTypeForChannelingAndChunkingConsiderations(taskType))
				{
					yield return taskType.Code;
				}
			}
		}

		readonly Dictionary<string, IEnumerable<string>> ignoredTaskTypesByWorkflowType = new Dictionary<string, IEnumerable<string>>();
	}
}
