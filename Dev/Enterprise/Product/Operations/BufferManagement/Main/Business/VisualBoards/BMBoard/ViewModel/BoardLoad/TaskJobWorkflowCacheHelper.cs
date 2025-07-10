using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class TaskJobWorkflowCacheHelper
	{
		public static ImmutableHashSet<ZGuid> GetApplicableTagMagnitudes(ZGuid identifier, PropertyCache cache)
		{
			return cache.GetCachedValue<ImmutableHashSet<ZGuid>>(identifier, CacheConstants.ApplicableTags);
		}

		public static void PopulateCacheForTasks(TaskChannelMap tasks,
			BMBoardSection section,
			PropertyCache cache,
			Action<TaskStartabilityService> replaceService = null)
		{
			if (!tasks.Any())
			{
				return;
			}

			var interner = new SetInterner<ZGuid>();

			var penetration = PopulateTaskBufferPenetrationCache(tasks, section, cache);
			PopulateCacheForStartableTasks(tasks, cache, section, replaceService);

			var boardShowTaskTags = BMSRegistry.Instance.BoardShowTaskTags.Value;
			foreach (var task in tasks)
			{
				if (boardShowTaskTags)
				{
					cache.GetCachedValue(task.PK, CacheConstants.ApplicableTags, () =>
						interner.Intern(new HashSet<ZGuid>(task.GetApplicableTags(false).Select(m => m.PK))));
				}
				else
				{
					cache.GetCachedValue(task.PK, CacheConstants.ApplicableTags, () =>
						interner.Intern(new HashSet<ZGuid>(task.ProcessHeader.GetApplicableTags(false).Select(m => m.PK))));
				}
			}
		}

		public static void PopulateCacheForJobWorkflow(TaskChannelMap tasks, BMBoardSection section, PropertyCache cache)
		{
			if (!tasks.Any())
			{
				return;
			}

			var interner = new SetInterner<ZGuid>();

			var factory = tasks.First().Factory;
			var tasksWithWorkflowsDistinct = tasks.Where(t => t.P9_FH_ProcessHeader.IsValid).DistinctBy(x => x.P9_FH_ProcessHeader);

			AddFetchHintsToPopulateJobWorkflow(factory, tasksWithWorkflowsDistinct);
			PopulateJobWorkflowBufferPenetrationCache(tasks, section, cache);

			foreach (var task in tasksWithWorkflowsDistinct)
			{
				var jobWorkflow = task.ProcessHeader.JobHeader;

				cache.GetCachedValue(jobWorkflow.PK, CacheConstants.WorkflowHasOpenPrerequisites, () => (bool)jobWorkflow.HasOpenPrerequisites);
				cache.GetCachedValue(jobWorkflow.PK, CacheConstants.ApplicableTags, () =>
					interner.Intern(new HashSet<ZGuid>(jobWorkflow.GetApplicableTags(false).Select(m => m.PK))));
			}
		}

		public static void PopulateCacheForWorkflows(TaskChannelMap tasks, BMBoardSection section, PropertyCache cache)
		{
			if (!tasks.Any())
			{
				return;
			}

			var interner = new SetInterner<ZGuid>();

			PopulateWorkflowBufferPenetrationCache(tasks, section, cache);

			foreach (var task in tasks)
			{
				task.Factory.AddFetchHint(ProcessHeaderSchema.PK, task.P9_FH_ProcessHeader);
			}

			foreach (var task in tasks)
			{
				var workflow = task.ProcessHeader;

				cache.GetCachedValue(task.P9_FH_ProcessHeader, CacheConstants.WorkflowHasOpenPrerequisites, () => (bool)workflow.HasOpenPrerequisites);
				cache.GetCachedValue(workflow.PK, CacheConstants.ApplicableTags, () =>
					interner.Intern(new HashSet<ZGuid>(workflow.GetApplicableTags(false).Select(m => m.PK))));
			}
		}

		static void PopulateCacheForStartableTasks(TaskChannelMap tasks, PropertyCache cache, BMBoardSection section, Action<TaskStartabilityService> replaceService)
		{
			var currentnessMap = StartabilityProvider.CalculateStartability(tasks, section, replaceService);

			foreach (var task in tasks)
			{
				cache.GetCachedValue(task.PK, CacheConstants.IsCurrent, () => currentnessMap.IsStartable(task, IProcessTaskExtensions.IsStartable));
			}
		}

		static void AddFetchHintsToPopulateJobWorkflow(BusinessObjectFactory factory, IEnumerable<ProcessTask> tasksWithWorkflowsDistinct)
		{
			var processHeaderPKs = new HashSet<ZGuid>();

			foreach (var task in tasksWithWorkflowsDistinct)
			{
				task.Factory.AddFetchHint(ProcessHeaderSchema.PK, task.P9_FH_ProcessHeader);
				task.Factory.AddFetchHint(ProcessHeaderSchema.PK, task.ProcessHeader.FH_FH_ParentHeader);

				processHeaderPKs.Add(task.P9_FH_ProcessHeader);
				processHeaderPKs.Add(task.ProcessHeader.FH_FH_ParentHeader);
			}

			factory.AddFetchHint(BMNCNShapeSchema.Instance, new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeaderPKs));
			factory.AddFetchHint(TagLinkSchema.Instance, new ZQuery(TagLinkSchema.TGL_ParentId, processHeaderPKs));

			ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(factory, processHeaderPKs);
		}

		static Dictionary<ZGuid, decimal> PopulateWorkflowBufferPenetrationCache(TaskChannelMap tasks, BMBoardSection section, PropertyCache cache)
		{
			var penetration = new Dictionary<ZGuid, decimal>();
			if (section.Component.IsBuffer && !section.SectionConfiguration.IsReleaseScheduler)
			{
				var context = WorkingTimeContext.Create(section);

				foreach (var workflow in tasks.AllWorkflows)
				{
					cache.GetCachedValue(workflow.PK, CacheConstants.Penetration, () => GetPenetration(workflow, null, context, penetration));
				}
			}

			return penetration;
		}

		static Dictionary<ZGuid, decimal> PopulateTaskBufferPenetrationCache(TaskChannelMap tasks, BMBoardSection section, PropertyCache cache)
		{
			var penetration = new Dictionary<ZGuid, decimal>();
			if (section.Component.IsBuffer && !section.SectionConfiguration.IsReleaseScheduler)
			{
				var context = WorkingTimeContext.Create(section);

				foreach (var task in tasks)
				{
					cache.GetCachedValue(task.PK, CacheConstants.Penetration, () => GetPenetration((ProcessHeader)task.ProcessHeader, task, context, penetration, isForTask: true));
				}
			}

			return penetration;
		}

		static void PopulateJobWorkflowBufferPenetrationCache(TaskChannelMap tasks, BMBoardSection section, PropertyCache cache)
		{
			if (section.Component.IsBuffer && !section.SectionConfiguration.IsReleaseScheduler)
			{
				var context = WorkingTimeContext.Create(section);

				foreach (var processHeader in tasks.AllWorkflows)
				{
					var max = decimal.MinValue;
					var processJobHeader = processHeader.JobHeader;

					foreach (ProcessHeader workflow in processJobHeader.ProcessHeaders)
					{
						max = Math.Max(max, GetPenetrationCore(workflow, null, context));
					}

					cache.GetCachedValue(processJobHeader.PK, CacheConstants.Penetration, () => max);
				}
			}
		}

		static decimal GetPenetration(ProcessHeader workflow, ProcessTask task, WorkingTimeContext context, Dictionary<ZGuid, decimal> cachedBufferPenetrations, bool isForTask = false)
		{
			var key = isForTask ? task.PK : workflow.PK;

			if (cachedBufferPenetrations.ContainsKey(key))
			{
				return cachedBufferPenetrations[key];
			}
			else if (workflow != null)
			{
				var penetration = GetPenetrationCore(workflow, task, context);
				return cachedBufferPenetrations[key] = penetration;
			}
			else
			{
				return cachedBufferPenetrations[key] = 0m;
			}
		}

		static decimal GetPenetrationCore(ProcessHeader workflow, ProcessTask task, WorkingTimeContext context)
		{
			var service = BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement && BMSRegistry.Instance.SynchroniseBufferPenetration.Value ?
				workflow.Factory.ServiceContainer.GetService<ApprovedShapeBufferPenetrationService>() :
				null;

			var approvedShapeDetails = service?.GetApprovedShapeDetails(workflow);

			if (approvedShapeDetails != null && approvedShapeDetails.HasRelatedBuffers)
			{
				return approvedShapeDetails.BufferPenetration;
			}
			else
			{
				return task == null ?
					workflow.CalculatePenetrationPercentage(context, workflow.Factory, includeNetworkBuffers: false) :
						workflow.CalculatePenetrationPercentage(task, context,
						includeNetworkBuffers: false);
			}
		}

		#region CacheConstants

		public static class CacheConstants
		{
			public const string Penetration = "BufferPenetration";
			public const string IsCurrent = "IsCurrent";
			public const string ApplicableTags = "ApplicableTags";
			public const string WorkflowHasOpenPrerequisites = "WorkflowHasOpenPrerequisites";
			public const string IsVisibleAfterFilterApplication = "IsApplicable";
		}

		#endregion
	}
}
