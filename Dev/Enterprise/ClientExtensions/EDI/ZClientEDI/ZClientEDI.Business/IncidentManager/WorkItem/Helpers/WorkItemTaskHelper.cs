using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.ServiceClient.Assess;

namespace Enterprise.Client.EDI
{
	public static class WorkItemTaskHelper
	{
		/// <summary>
		/// Gets closed review tasks (type = CBC or CBR) that are lower in sequence and within the same Work Item as the specified task. This function considers Quality Iteration parent workflows, and any prerequisites of the specified task's workflow and prerequisites of QI parent workflows.
		/// </summary>
		/// <param name="submissionTask">The task from which to begin looking for review tasks.</param>
		/// <returns>A sequence of review tasks ordered first by the distance from the starting task's workflow, then by Sequence descending and Completed Time descending within workflow. Distance refers to number of steps up the QI hierarchy, then number of steps back through the prerequisite tree.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static ImmutableArray<ProcessTask> GetCompletedReviewTasks(ProcessTask submissionTask)
			=> GetReviewTasks(submissionTask, ProcessTaskStatusCodeList.Codes.Closed);

		/// <summary>
		/// Gets closed or cancelled review tasks (type = CBC or CBR) that are lower in sequence and within the same Work Item as the specified task. This function considers Quality Iteration parent workflows, and any prerequisites of the specified task's workflow and prerequisites of QI parent workflows.
		/// </summary>
		/// <param name="submissionTask">The task from which to begin looking for review tasks.</param>
		/// <returns>A sequence of review tasks ordered first by the distance from the starting task's workflow, then by Sequence descending and Completed Time descending within workflow. Distance refers to number of steps up the QI hierarchy, then number of steps back through the prerequisite tree.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static ImmutableArray<ProcessTask> GetCompletedOrCancelledReviewTasks(ProcessTask submissionTask)
			=> GetReviewTasks(submissionTask, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled);

		public static ImmutableArray<ProcessTask> GetReviewTasks(ProcessTask submissionTask, params string[] reviewStatuses)
		{
			Argument.NotNull(submissionTask, nameof(submissionTask));
			if (submissionTask.ProcessHeader == null)
			{
				return ImmutableArray<ProcessTask>.Empty;
			}

			var workflowsToConsider = GetWorkflowsWithinJobThatContainAndPrecedeTask(submissionTask).ToArray();

			return (
				from workflow in workflowsToConsider
				from ProcessTask task in workflow.Tasks.OrderByDescending(t => t.P9_Sequence).ThenByDescending(t => t.P9_CompletedTime)
				where task.P9_Type.ToString().In(WorkItemProcessTask.CodeReviewTaskType, WorkItemProcessTask.GeneralReviewTaskType)
				where task.P9_Status.ToString().In(reviewStatuses)
				where !task.P9_GS_NKAssignedStaffMember.IsEmpty
				where task.P9_FH_ProcessHeader != submissionTask.P9_FH_ProcessHeader || task.P9_Sequence < submissionTask.P9_Sequence
				select task
				).ToImmutableArray();
		}

		public static IEnumerable<IProcessHeader> GetWorkflowsWithinJobThatContainAndPrecedeTask(ProcessTask startingTask)
		{
			var workflows = FindWorkflowsUpTheQualityIterationHierarchy(startingTask).ToArray();
			var seen = new HashSet<IProcessHeader>();

			foreach (var workflow in workflows)
			{
				if (seen.Add(workflow))
				{
					yield return workflow;
				}
			}

			var workflowPKs = workflows.Select(w => w.PK);
			startingTask.Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, workflowPKs));
			startingTask.Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, workflowPKs));

			foreach (var workflow in workflows)
			{
				foreach (var prerequisite in workflow.GetPrerequisitesUpTheTree())
				{
					if (prerequisite.FH_ParentId == startingTask.P9_ParentID && seen.Add(prerequisite))
					{
						yield return prerequisite;
					}
				}
			}
		}

		public static IEnumerable<IProcessHeader> GetWorkflowsWithinJobThatMayContainReviewTask(ProcessTask startingTask)
		{
			var workflows = GetWorkflowsWithinJobThatContainAndPrecedeTask(startingTask).ToHashSet();
			var startingWorkflows = workflows.ToArray();
			PopulateDependentWorkflows(startingWorkflows);
			PopulateChildWorkflows(startingWorkflows);

			return workflows;

			void PopulateDependentWorkflows(IProcessHeader[] thisLevelWorkflows)
			{
				foreach (var workflow in thisLevelWorkflows)
				{
					workflow.Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, workflow.PK));
				}

				foreach (var workflow in thisLevelWorkflows)
				{
					var links = workflow.LinksFromMeToOthers.Cast<IProcessHeaderLink>().Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency).ToArray();
					var workflowQuery = new ZQuery(ProcessHeaderSchema.PK, links.Select(l => l.FP_FH_HeaderTo));
					workflowQuery.AddToFilter(ProcessHeaderSchema.FH_ParentId, workflow.FH_ParentId);

					var dependentWorkflows = workflow.Factory.Load<IProcessHeader>(workflowQuery).Where(w => workflows.Add(w)).ToArray();
					PopulateDependentWorkflows(dependentWorkflows);
				}
			}

			void PopulateChildWorkflows(IProcessHeader[] thisLevelWorkflows)
			{
				foreach (var workflow in thisLevelWorkflows)
				{
					workflow.Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, workflow.PK));
				}

				foreach (var workflow in thisLevelWorkflows)
				{
					var links = workflow.LinksFromOthersToMe.Cast<IProcessHeaderLink>().Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild).ToArray();
					var workflowQuery = new ZQuery(ProcessHeaderSchema.PK, links.Select(l => l.FP_FH_HeaderFrom));
					workflowQuery.AddToFilter(ProcessHeaderSchema.FH_ParentId, workflow.FH_ParentId);

					var childWorkflows = workflow.Factory.Load<IProcessHeader>(workflowQuery).Where(w => workflows.Add(w)).ToArray();
					PopulateChildWorkflows(childWorkflows);
				}
			}
		}

		public static IEnumerable<IProcessHeader> FindWorkflowsUpTheQualityIterationHierarchy(ProcessTask startingTask)
		{
			var results = new List<ZGuid>();
			CollectWorkflowsUpTheQualityIterationHierarchy(startingTask, new HashSet<ZGuid>(), new HashSet<ProcessTask>());

			if (results.Count == 0)
			{
				return Enumerable.Empty<IProcessHeader>();
			}

			var workflows = startingTask.Factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, results)).ToDictionary(workflow => workflow.PK);
			return results.Distinct().Select(pk => workflows[pk]); // Preserve result sequence since distance from starting task is significant.

			void CollectWorkflowsUpTheQualityIterationHierarchy(ProcessTask task, HashSet<ZGuid> seenWorkflowPKs, HashSet<ProcessTask> seenContainmentBarriers)
			{
				var workflowPK = task.P9_FH_ProcessHeader;
				if (workflowPK.IsValid)
				{
					results.Add(workflowPK);
					seenWorkflowPKs.Add(workflowPK);
				}

				var containmentBarrierTask = (ProcessTask)task.IterationPivot?.Iteration?.ContainmentBarrierTask;
				if (containmentBarrierTask != null && seenContainmentBarriers.Add(containmentBarrierTask))
				{
					CollectWorkflowsUpTheQualityIterationHierarchy(containmentBarrierTask, seenWorkflowPKs, seenContainmentBarriers);
				}
			}
		}

		public static TaskCompetencyRequirements GetTaskCompetencyRequirements(ITaskCompetencyRequirementsProvider provider, BusinessObjectFactory factory)
		{
			Argument.NotNull(provider, nameof(provider));
			Argument.NotNull(factory, nameof(factory));

			return new TaskCompetencyRequirements(provider.GetPendingCompetencyRequirements().OrderBy(r => r.AspectPK).ToImmutableArray());
		}

		public static AssessRequirementsState GetAssessRequirementsCompletionState(TaskCompetencyRequirements allCompetencyRequirements, IAssessServiceClient apiClient, params GlbStaff[] staff)
		{
			Argument.NotNull(allCompetencyRequirements, nameof(allCompetencyRequirements));
			Argument.NotNull(apiClient, nameof(apiClient));

			if (staff.Length == 0)
			{
				return AssessRequirementsState.AllRequirementsUnmet(allCompetencyRequirements, staff.ToImmutableArray());
			}

			var unmetRequirements = new List<CompetencyRequirement>();
			var failedToDetermineResult = new HashSet<CompetencyRequirement>();
			var validStaff = staff.Where(s => s.GS_PER.IsValid).ToArray();
			foreach (var requirement in allCompetencyRequirements.CompetencyRequirements)
			{
				if (requirement.AspectPK == default)
				{
					continue;
				}
				var resultWasDetermined = false;
				var anyoneHasCompletedRequirement = false;

				foreach (var personPK in validStaff.Select(s => s.GS_PER.ToGuid()))
				{
					var courseCompletionResult = apiClient.HasPassedLearningUnitAsync(requirement.AspectPK, personPK).GetAwaiter().GetResult();
					if (courseCompletionResult.IsSuccess)
					{
						resultWasDetermined = true;
						if (courseCompletionResult.IsSuccess && courseCompletionResult.Content)
						{
							anyoneHasCompletedRequirement = true;
							break;
						}
					}
				}

				if (!resultWasDetermined)
				{
					failedToDetermineResult.Add(requirement);
				}
				else if (!anyoneHasCompletedRequirement)
				{
					unmetRequirements.Add(requirement);
				}
			}

			if (failedToDetermineResult.Count > 0)
			{
				allCompetencyRequirements = new TaskCompetencyRequirements(allCompetencyRequirements.CompetencyRequirements.Except(failedToDetermineResult).ToImmutableArray());
			}

			var unmetCompetencyRequirements = new TaskCompetencyRequirements(unmetRequirements.OrderBy(r => r).ToImmutableArray());
			return AssessRequirementsState.WithUnmetRequirements(allCompetencyRequirements, unmetCompetencyRequirements, validStaff.ToImmutableArray());
		}

		public static AssessRequirementsState GetAssessRequirementsCompletionState(ProcessTask submissionTask, ITaskCompetencyRequirementsProvider provider, IAssessServiceClient apiClient)
		{
			var allCompetencyRequirements = GetTaskCompetencyRequirements(provider, submissionTask.Factory);
			return GetAssessRequirementsCompletionState(submissionTask, allCompetencyRequirements, apiClient);
		}

		public static AssessRequirementsState GetAssessRequirementsCompletionState(ProcessTask submissionTask, TaskCompetencyRequirements allCompetencyRequirements, IAssessServiceClient apiClient)
		{
			Argument.NotNull(allCompetencyRequirements, nameof(allCompetencyRequirements));

			if (allCompetencyRequirements.IsEmpty)
			{
				return AssessRequirementsState.Empty;
			}

			Argument.NotNull(submissionTask, nameof(submissionTask));
			Argument.NotNull(apiClient, nameof(apiClient));

			var reviewTasks = GetCompletedReviewTasks(submissionTask);
			var reviewers = IEnumerableExtensions.DistinctBy(reviewTasks.Select(t => t.AssignedStaffMember).Where(s => s != null), s => s.GS_Code).ToArray();
			return GetAssessRequirementsCompletionState(allCompetencyRequirements, apiClient, reviewers);
		}

		public static string GetHumanReadableText(CompetencyRequirement competencyRequirement, IAssessServiceClient apiClient)
			=> apiClient.GetLearningUnitNameOrPlaceholder(competencyRequirement.AspectPK);
	}
}
