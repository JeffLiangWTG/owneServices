using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.BufferManagement.Service.Shared.Workflows.Dtos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Service
{
	public class WorkflowService : IWorkflowService
	{
		public bool TryReorderTask(Guid workflowId, Guid taskId, WorkflowTaskReorderRequest request, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService) };

			if (request.PreviousTaskId is null && request.NextTaskId is null)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskSequenceRequiresPreviousOrNextMessage, out error);
			}

			if (request.PreviousTaskId == request.NextTaskId)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeDifferentMessage, out error);
			}

			var workflow = factory.Load<ProcessHeader>(workflowId);
			if (workflow == null)
			{
				return NotFound(nameof(workflowId), workflowId.ToString(), out error);
			}

			if (workflow.Tasks.Count() <= 1)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskReorderingRequiresAtLeastTwoTasks, out error);
			}

			var task = factory.Load<ProcessTask>(taskId);
			if (task == null)
			{
				return NotFound(nameof(taskId), taskId.ToString(), out error);
			}
			if (task.P9_Sequence != request.ActualSequence)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			var tasks = workflow.Tasks.Select(t => t).ApplyDefaultTaskSort().ToArray();
			var otherTasks = tasks.Where(t => t.PK != task.PK).Select(t => t).ToArray();

			if (!TryCommonTargetCheck(workflow, tasks, otherTasks, request.PreviousTaskId, request.PreviousTaskSequence, request.NextTaskId, request.NextTaskSequence, out var newPreviousTask, out var newNextTask, taskId, request.IsUngrouping, out error))
			{
				return false;
			}

			var linkedList = new LinkedList<ProcessTask>(tasks);
			var currentTaskNode = linkedList.Find(task);

			if (currentTaskNode == null)
			{
				throw new ArgumentNullException(nameof(taskId), nameof(currentTaskNode));
			}

			if (currentTaskNode.Previous?.Value?.PK != request.ActualPreviousTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (currentTaskNode.Next?.Value?.PK != request.ActualNextTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (request.IsUngrouping is null or false)
			{
				var samePrevious = (request.PreviousTaskId is null && tasks.First().PK == task.PK) || (request.PreviousTaskId is not null && currentTaskNode.Previous != null && currentTaskNode.Previous.Value?.PK.ToGuid() == request.PreviousTaskId);
				var sameNext = (request.NextTaskId is null && tasks.Last().PK == task.PK) || (request.NextTaskId is not null && currentTaskNode.Previous != null && currentTaskNode.Next?.Value?.PK.ToGuid() == request.NextTaskId);
				if (samePrevious && sameNext && task.P9_Sequence == request.ActualSequence)
				{
					return Error(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, out error);
				}
			}

			Resequence(request.PreviousTaskId, request.NextTaskId, new ProcessTask[] { task }, newPreviousTask, newNextTask, otherTasks);

			factory.Save();

			return true;
		}

		static bool TryCommonTargetCheck(ProcessHeader workflow, ProcessTask[] tasks, ProcessTask[] otherTasks, Guid? previousTaskId, int? previousTaskSequence, Guid? nextTaskId, int? nextTaskSequence, out ProcessTask targetPreviousTask, out ProcessTask targetNextTask, Guid? taskId, bool? isUngroupingTask, out PaveError error)
		{
			error = null;
			targetPreviousTask = null;
			targetNextTask = null;

			var newPreviousTask = workflow.Tasks.SingleOrDefault(t => t.PK.ToGuid() == previousTaskId);
			if (newPreviousTask is not null && newPreviousTask.P9_Sequence != previousTaskSequence)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}
			targetPreviousTask = newPreviousTask;

			var newNextTask = workflow.Tasks.SingleOrDefault(t => t.PK.ToGuid() == nextTaskId);
			if (newNextTask is not null && newNextTask.P9_Sequence != nextTaskSequence)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}
			targetNextTask = newNextTask;

			if (newPreviousTask is not null && newNextTask is not null && tasks.IndexOf(t => t.PK == newPreviousTask.PK) >= tasks.IndexOf(t => t.PK == newNextTask.PK))
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeInSequentialOrderMessage, out error);
			}

			if (newPreviousTask is not null && tasks.Last(t => t.P9_Sequence == newPreviousTask.P9_Sequence && (isUngroupingTask is false or null || t.PK != taskId)).PK != newPreviousTask.PK)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.PreviousMustBeTheLastInPreviousGroup, out error);
			}

			if (newNextTask is not null && tasks.First(t => t.P9_Sequence == newNextTask.P9_Sequence && (isUngroupingTask is false or null || t.PK != taskId)).PK != newNextTask.PK)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.NextMustBeTheFirstInNextGroup, out error);
			}

			if (nextTaskId is null && otherTasks.Last().PK != previousTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (previousTaskId is null && otherTasks.First().PK != nextTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			return true;
		}

		void Resequence(Guid? previousTaskId, Guid? nextTaskId, ProcessTask[] tasksToResequence, ProcessTask newPreviousTask, ProcessTask newNextTask, ProcessTask[] otherTasks)
		{
			//First position
			if (previousTaskId is null)
			{
				var actualFirstTask = otherTasks.First();

				if (actualFirstTask.P9_Sequence > 1)
				{
					tasksToResequence.ForEach(task => task.P9_Sequence = actualFirstTask.P9_Sequence - 1);
				}
				else
				{
					tasksToResequence.ForEach(task => task.P9_Sequence = actualFirstTask.P9_Sequence);

					ResequenceTask(0, otherTasks);
				}
			}
			//Last position
			else if (nextTaskId is null)
			{
				var actualLastTask = otherTasks.Last();

				tasksToResequence.ForEach(task => task.P9_Sequence = actualLastTask.P9_Sequence + 1);
			}
			//In between
			else
			{
				var newSequence = newPreviousTask.P9_Sequence + 1;
				tasksToResequence.ForEach(task => task.P9_Sequence = newSequence);

				if (newSequence < newNextTask.P9_Sequence)
				{
					return;
				}

				ResequenceTask(otherTasks.IndexOf(t => t.PK == newNextTask.PK), otherTasks);
			}
		}

		void ResequenceTask(int index, ProcessTask[] otherTasks)
		{
			if (index > otherTasks.Length - 1)
			{
				return;
			}

			var actualSequence = otherTasks[index].P9_Sequence;

			do
			{
				otherTasks[index].P9_Sequence++;
				index++;
			}
			while (index < otherTasks.Length && actualSequence == otherTasks[index].P9_Sequence);

			if (index < otherTasks.Length && otherTasks[index].P9_Sequence == actualSequence + 1)
			{
				ResequenceTask(index, otherTasks);
			}
		}

		static bool Error(string errorMessage, out PaveError error, PaveErrorCode errorCode = PaveErrorCode.Other)
		{
			error = new PaveError(errorCode, new string[] { errorMessage });
			return false;
		}

		static bool NotFound(string parameterName, string parameterValue, out PaveError error)
		{
			error = new PaveError(PaveErrorCode.Other, new string[] { $"{LocalizationHelper.WorkflowTaskReordering.RecordNotFoundMessage} {parameterName}: {parameterValue}." });
			return false;
		}

		static bool TaskSequenceChangedByAnotherUser(out PaveError error)
		{
			error = new PaveError(PaveErrorCode.Other, new string[] { LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage });
			return false;
		}

		public bool TryGroupTask(Guid workflowId, int groupId, Guid taskId, WorkflowGroupTaskRequest workflowGroupTaskRequest, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService) };

			var workflow = factory.Load<ProcessHeader>(workflowId);
			if (workflow == null)
			{
				return NotFound(nameof(workflowId), workflowId.ToString(), out error);
			}
			var task = factory.Load<ProcessTask>(taskId);
			if (task == null)
			{
				return NotFound(nameof(taskId), taskId.ToString(), out error);
			}
			if (task.P9_Sequence != workflowGroupTaskRequest.ActualSequence)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (workflow.Tasks.Count() <= 1 || groupId == task.P9_Sequence)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskGroupingRequiresAtLeastTwoTasksWithDifferentSequenceNumber, out error);
			}

			var tasks = workflow.Tasks.Select(t => t).ApplyDefaultTaskSort().ToArray();

			var linkedList = new LinkedList<ProcessTask>(tasks);
			var currentTaskNode = linkedList.Find(task);

			if (currentTaskNode == null)
			{
				throw new ArgumentNullException(nameof(taskId), nameof(currentTaskNode));
			}

			if (currentTaskNode.Previous?.Value?.PK != workflowGroupTaskRequest.ActualPreviousTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (currentTaskNode.Next?.Value?.PK != workflowGroupTaskRequest.ActualNextTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			task.P9_Sequence = groupId;

			factory.Save();

			return true;
		}

		public bool TryReorderGroup(Guid workflowId, int groupId, WorkflowGroupReorderRequest request, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService) };

			if (request.ActualGroupTasks == null || request.ActualGroupTasks.Length < 2)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, out error);
			}

			if (request.PreviousTaskId is null && request.NextTaskId is null)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskSequenceRequiresPreviousOrNextMessage, out error);
			}

			if (request.PreviousTaskId == request.NextTaskId)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeDifferentMessage, out error);
			}

			var workflow = factory.Load<ProcessHeader>(workflowId);
			if (workflow == null)
			{
				return NotFound(nameof(workflowId), workflowId.ToString(), out error);
			}
			if (workflow.Tasks.Count() <= 1)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskReorderingRequiresAtLeastTwoTasks, out error);
			}

			var tasks = workflow.Tasks.Select(t => t).ApplyDefaultTaskSort().ToArray();
			var otherTasks = tasks.Where(t => t.P9_Sequence != groupId).Select(t => t).ToArray();
			if (!TryCommonTargetCheck(workflow, tasks, otherTasks, request.PreviousTaskId, request.PreviousTaskSequence, request.NextTaskId, request.NextTaskSequence, out var newPreviousTask, out var newNextTask, null, null, out error))
			{
				return false;
			}

			if (tasks.Count(t => t.P9_Sequence == groupId) != request.ActualGroupTasks.Length)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupChangedByAnotherUserMessage, out error);
			}

			if (!tasks.Where(t => t.P9_Sequence == groupId).Select(t => t.PK.ToGuid()).OrderBy(t => t).SequenceEqual(request.ActualGroupTasks.OrderBy(t => t)))
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupChangedByAnotherUserMessage, out error);
			}

			var groupTasks = tasks.Where(t => request.ActualGroupTasks.Contains(t.PK.ToGuid())).Select(t => t).ToArray();

			var firstGroupTaskId = request.ActualGroupTasks[0];
			var firstGroupTask = groupTasks[0];
			if (firstGroupTask == null)
			{
				return NotFound(nameof(firstGroupTaskId), firstGroupTaskId.ToString(), out error);
			}

			var lastGroupTaskId = request.ActualGroupTasks.Last();
			var lastGroupTask = groupTasks.Last();
			if (lastGroupTask == null)
			{
				return NotFound(nameof(lastGroupTaskId), lastGroupTaskId.ToString(), out error);
			}

			var linkedList = new LinkedList<ProcessTask>(tasks);
			var firstTaskNode = linkedList.Find(firstGroupTask);
			if (firstTaskNode == null)
			{
				throw new ArgumentNullException(nameof(groupId), nameof(firstTaskNode));
			}

			var lastTaskNode = linkedList.Find(lastGroupTask);
			if (lastTaskNode == null)
			{
				throw new ArgumentNullException(nameof(groupId), nameof(lastTaskNode));
			}

			if (firstTaskNode.Previous?.Value?.PK != request.ActualPreviousTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			if (lastTaskNode.Next?.Value?.PK != request.ActualNextTaskId)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			var samePrevious = (request.PreviousTaskId is null && tasks.First().PK == firstGroupTask.PK) || (request.PreviousTaskId is not null && firstTaskNode.Previous != null && firstTaskNode.Previous.Value.PK.ToGuid() == request.PreviousTaskId);
			var sameNext = (request.NextTaskId is null && tasks.Last().PK == lastGroupTask.PK) || (request.NextTaskId is not null && lastTaskNode.Previous != null && lastTaskNode.Next?.Value.PK.ToGuid() == request.NextTaskId);
			if (samePrevious && sameNext)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, out error);
			}

			Resequence(request.PreviousTaskId, request.NextTaskId, groupTasks, newPreviousTask, newNextTask, otherTasks);

			factory.Save();

			return true;
		}

		public bool TryMergeGroups(Guid workflowId, int sourceGroupId, int targetGroupId, WorkflowGroupMergeRequest request, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService) };

			if (sourceGroupId == targetGroupId)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.MergeRequiresTwoDifferentGroups, out error);
			}

			if (request.SourceGroupTasks == null || request.SourceGroupTasks.Length < 2)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, out error);
			}

			if (request.TargetGroupTasks == null || request.TargetGroupTasks.Length < 2)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, out error);
			}

			var workflow = factory.Load<ProcessHeader>(workflowId);
			if (workflow == null)
			{
				return NotFound(nameof(workflowId), workflowId.ToString(), out error);
			}
			if (workflow.Tasks.Count() <= 1)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskReorderingRequiresAtLeastTwoTasks, out error);
			}

			var tasks = workflow.Tasks.Select(t => t).ToArray();

			var existingSourceGroupTasks = tasks.Where(t => t.P9_Sequence == sourceGroupId).ToArray();
			if (!existingSourceGroupTasks.Select(t => t.PK.ToGuid()).OrderBy(t => t).SequenceEqual(request.SourceGroupTasks.OrderBy(t => t)))
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupChangedByAnotherUserMessage, out error);
			}

			var existingTargetGroupTasks = tasks.Where(t => t.P9_Sequence == targetGroupId).ToArray();
			if (!existingTargetGroupTasks.Select(t => t.PK.ToGuid()).OrderBy(t => t).SequenceEqual(request.TargetGroupTasks.OrderBy(t => t)))
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupChangedByAnotherUserMessage, out error);
			}

			existingSourceGroupTasks.ForEach(task => task.P9_Sequence = targetGroupId);

			factory.Save();

			return true;
		}

		public bool TryMergeGroupOnTask(Guid workflowId, Guid taskId, int sourceGroupId, WorkflowMergeGroupOnTaskRequest request, out PaveError error)
		{
			error = null;
			var factory = new BusinessObjectFactory() { NameForDebugging = nameof(WorkItemService) };

			if (request.SourceGroupTasks == null || request.SourceGroupTasks.Length < 2)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, out error);
			}

			if (sourceGroupId == request.TargetTaskSequence)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, out error);
			}

			var task = factory.Load<ProcessTask>(taskId);
			if (task == null)
			{
				return NotFound(nameof(taskId), taskId.ToString(), out error);
			}
			if (task.P9_Sequence != request.TargetTaskSequence)
			{
				return TaskSequenceChangedByAnotherUser(out error);
			}

			var workflow = factory.Load<ProcessHeader>(workflowId);
			if (workflow == null)
			{
				return NotFound(nameof(workflowId), workflowId.ToString(), out error);
			}
			if (workflow.Tasks.Count() <= 1)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TaskReorderingRequiresAtLeastTwoTasks, out error);
			}

			var tasks = workflow.Tasks.Select(t => t).ToArray();

			if (tasks.Count(t => t.P9_Sequence == request.TargetTaskSequence) > 1)
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.TargetTaskBelongsToGroup, out error);
			}

			var existingSourceGroupTasks = tasks.Where(t => t.P9_Sequence == sourceGroupId).ToArray();
			if (!existingSourceGroupTasks.Select(t => t.PK.ToGuid()).OrderBy(t => t).SequenceEqual(request.SourceGroupTasks.OrderBy(t => t)))
			{
				return Error(LocalizationHelper.WorkflowTaskReordering.GroupChangedByAnotherUserMessage, out error);
			}

			existingSourceGroupTasks.ForEach(t => t.P9_Sequence = request.TargetTaskSequence);

			factory.Save();

			return true;
		}
	}

	public static class HelperExtensions
	{
		public static IOrderedEnumerable<ProcessTask> ApplyDefaultTaskSort(this IEnumerable<ProcessTask> source)
		{
			return source.OrderBy(t => t.P9_Sequence).ThenBy(t => t.P9_Description).ThenBy(t => t.P9_TaskID);
		}
	}
}
