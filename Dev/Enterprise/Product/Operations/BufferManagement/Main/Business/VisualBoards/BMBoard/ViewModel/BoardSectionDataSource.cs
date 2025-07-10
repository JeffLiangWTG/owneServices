using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionDataSource
	{
		public BoardSectionDataSource(BMBoardSection section, BMBoardSectionViewModel viewModel)
		 : this(section, new BoardSectionDataSourceParameters(

			 viewModel.Cache,
			 viewModel.OverriddenWorkflowSectionFilter ?? section.WorkflowSectionFilter,
			 viewModel.OverriddenTaskSectionFilter ?? section.TaskSectionFilter,
			 viewModel.BMBoardChannels,
			 viewModel.AllChannels
		 ))
		{
		}

		public BoardSectionDataSource(BMBoardSection section, BoardSectionDataSourceParameters parameters)
		{
			this.factory = section.Factory;
			this.section = section;

			this.Parameters = parameters;
		}

		readonly BusinessObjectFactory factory;
		readonly BMBoardSection section;
		public BoardSectionDataSourceParameters Parameters { get; }

		#region Tasks

		BoardSectionEntities GetIncompleteWorkflows()
		{
			var sectionComponent = section.Component;
			if (sectionComponent == null)
			{
				return BoardSectionEntities.Empty;
			}

			ProcessHeader[] workflows;
			if (section.SectionConfiguration.IsReleaseScheduler)
			{
				workflows = WorkflowLoader.LoadReleaseSchedulerWorkflows(section, Parameters.BMBoardChannels, Parameters.WorkflowSectionFilter, Parameters.TaskSectionFilter);
			}
			else if (section.SectionConfiguration.ShowJobWorkflowCards)
			{
				workflows = WorkflowLoader.LoadJobLevelWorkflows(section, Parameters.BMBoardChannels, Parameters.WorkflowSectionFilter, Parameters.TaskSectionFilter);
			}
			else
			{
				workflows = WorkflowLoader.LoadWorkflows(section, Parameters.BMBoardChannels, Parameters.WorkflowSectionFilter, Parameters.TaskSectionFilter);
			}

			var workflowsToFetchRelated = BMSRegistry.Instance.SynchroniseBufferPenetration.Value ? GetWorkflowsForFetchingRelatedEntities(factory, workflows).ToArray() : workflows;
			var allTasks = WorkflowLoader.LoadTasksForWorkflows(section, workflows);

			PrefetchRelatedRecords(section, workflowsToFetchRelated, factory, allTasks);

			return BoardSectionEntities.ForStandardRefresh(allTasks, workflows, workflowsToFetchRelated);
		}

		BoardSectionEntities GetIncompleteWorkflowsAndTasks()
		{
			var channels = Parameters.BMBoardChannels.ToArray();
			var tasks = SimpleQueryBoardLoader.LoadTasks(section, channels, Parameters.WorkflowSectionFilter, Parameters.TaskSectionFilter);

			var workflows = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, tasks.Select(t => t.P9_FH_ProcessHeader).Where(p => p.IsValid).Distinct()));
			var workflowsToFetchRelated = BMSRegistry.Instance.SynchroniseBufferPenetration.Value ? GetWorkflowsForFetchingRelatedEntities(factory, workflows).ToArray() : workflows;
			var allTasks = WorkflowLoader.LoadTasksForWorkflows(section, workflowsToFetchRelated);

			PrefetchRelatedRecords(section, workflowsToFetchRelated, factory, tasks);

			return BoardSectionEntities.ForStandardRefresh(tasks, workflows, workflowsToFetchRelated);
		}

		public TaskChannelMap GetIncompleteTasksForCurrentChannels()
		{
			VisualBoardChannelFetchHintProvider.FetchChannelEntities(factory, Parameters.AllChannels);

			foreach (var channel in Parameters.AllChannels.OfType<VisualBoardChannel>())
			{
				channel.SeedCacheWithChannelMatcher(factory);
			}

			if (section.SectionConfiguration.ShowWorkflowOrJobWorkflowCards)
			{
				return CreateTaskChannelMap(GetIncompleteWorkflows());
			}
			else
			{
				var entities = GetIncompleteWorkflowsAndTasks();
				return CreateTaskChannelMap(entities);
			}
		}

		public TaskChannelMap GetIncompleteTasksForUnchannelled()
		{
			var unchanneledChannels = new[] { new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled) };

			if (section.SectionConfiguration.ShowWorkflowOrJobWorkflowCards)
			{
				return CreateTaskChannelMap(GetIncompleteWorkflows(), unchanneledChannels);
			}

			var entities = GetIncompleteWorkflowsAndTasks();

			return CreateTaskChannelMap(entities, unchanneledChannels);
		}

		TaskChannelMap CreateTaskChannelMap(BoardSectionEntities entities, IEnumerable<IVisualBoardChannel> overriddenChannels = null)
		{
			return TaskChannelMap.Create(section, overriddenChannels ?? Parameters.AllChannels, entities);
		}

		#endregion

		#region Fetch Hints

		static void PrefetchRelatedRecords(BMBoardSection section, ICollection<ProcessHeader> workflows, BusinessObjectFactory factory, IEnumerable<ProcessTask> tasks)
		{
			var tasksForFetching = tasks == null ?
				workflows.SelectMany(w => w.GetTasksWithoutAccessingWorkflowParent()).ToArray()
				: tasks.ToArray();

			PrefetchGroups(workflows, tasksForFetching, factory);
			PrefetchTagLinks(workflows, tasksForFetching, factory);

			var synchroniseBufferPenetration = BMSRegistry.Instance.SynchroniseBufferPenetration.Value;

			foreach (var workflow in workflows)
			{
				if (synchroniseBufferPenetration)
				{
					factory.AddFetchHint(BMNCNShapeSchema.BNS_RelatedEntityID, workflow.PK);
				}

				if (section.SectionConfiguration.ShowJobWorkflowCards)
				{
					factory.AddFetchHint(ProcessHeaderSchema.FH_FH_ParentHeader, workflow.PK);
				}
				else
				{
					if (synchroniseBufferPenetration)
					{
						factory.AddFetchHint(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK);
						factory.AddFetchHint(BMNCNShapeSchema.BNS_RelatedEntityID, workflow.FH_FH_ParentHeader);
						factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, workflow.FH_FH_ParentHeader);
						factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderTo, workflow.FH_FH_ParentHeader);
					}

					factory.AddFetchHint(ProcessHeaderSchema.PK, workflow.FH_FH_ParentHeader);
				}
			}

			var staffNeedingHints = GetValidStaffCodes(tasksForFetching).ToHashSet();
			if (staffNeedingHints.Count > 0)
			{
				var query = new ZQuery(BMComponentResourceLinkSchema.FD_GS_NKResource, staffNeedingHints);
				factory.AddFetchHint(BMComponentResourceLinkSchema.Instance, query);
			}

			foreach (var parentTableCodeGroup in workflows.GroupBy(w => w.FH_ParentTableCode))
			{
				var parentTableCode = parentTableCodeGroup.Key;
				var parentType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(parentTableCode, false);

				if (parentType != null)
				{
					foreach (var jobHeader in parentTableCodeGroup)
					{
						jobHeader.AddDeepFetchHintForParentType(specificFactory: factory, parentType: parentType);
					}
				}
			}
		}

		static void PrefetchGroups(IEnumerable<ProcessHeader> workflows, IEnumerable<ProcessTask> tasks, BusinessObjectFactory factory)
		{
			var groupPks = tasks.Select(task => task.P9_GG_AssignedGroup)
				.Concat(workflows.Select(workflow => workflow.FH_GG_ReleaseGroup))
				.Where(groupPK => groupPK.IsValid)
				.Distinct();

			factory.AddFetchHint(GlbGroupLinkSchema.Instance, new ZQuery(GlbGroupLinkSchema.GK_GG, groupPks));
			factory.AddFetchHint(GlbGroupSchema.Instance, new ZQuery(GlbGroupSchema.PK, groupPks));
		}

		public static void PrefetchTagLinks(IEnumerable<ProcessHeader> workflows, IEnumerable<ProcessTask> tasks, BusinessObjectFactory factory)
		{
			var pKsForFetch = workflows.Select(w => w.PK)
				.Concat(workflows.Select(w => w.FH_FH_ParentHeader));

			if (BMSRegistry.Instance.BoardShowTaskTags.Value)
			{
				pKsForFetch = pKsForFetch.Concat(tasks.Select(t => t.PK));
			}

			pKsForFetch = pKsForFetch.Where(pk => pk.IsValid).Distinct();

			if (BMSRegistry.Instance.BoardBatchTagDBHits.Value)
			{
				foreach (var pk in pKsForFetch)
				{
					factory.AddFetchHint(TagLinkSchema.TGL_ParentId, pk);
				}
			}
			else
			{
				factory.AddFetchHint(TagLinkSchema.Instance, new ZQuery(TagLinkSchema.TGL_ParentId, pKsForFetch));
			}
		}

		static ICollection<ProcessHeader> GetWorkflowsForFetchingRelatedEntities(BusinessObjectFactory factory, ProcessHeader[] workflowsMatchingSectionFilters)
		{
			var result = workflowsMatchingSectionFilters.ToHashSet();

			var currentlyLoadedWorkflowAndParentHeaderPKs = GetProcessHeaderPKsAndParentHeaders(workflowsMatchingSectionFilters);
			var loadedWorkflows = workflowsMatchingSectionFilters.Select(w => w.PK).ToHashSet();

			do
			{
				var newlyLoadedWorkflows = GetAndLoadWorkflowsConnectedByAParentChildRelationship(factory, currentlyLoadedWorkflowAndParentHeaderPKs, loadedWorkflows);
				currentlyLoadedWorkflowAndParentHeaderPKs = newlyLoadedWorkflows.Select(w => w.FH_FH_ParentHeader).Where(pK => !loadedWorkflows.Contains(pK)).ToList();

				foreach (var workflow in newlyLoadedWorkflows)
				{
					result.Add(workflow);
					loadedWorkflows.Add(workflow.PK);
				}
			}
			while (currentlyLoadedWorkflowAndParentHeaderPKs.Any());

			return result;
		}

		static List<ZGuid> GetProcessHeaderPKsAndParentHeaders(ProcessHeader[] headers)
		{
			var result = new List<ZGuid>();

			foreach (var header in headers)
			{
				result.Add(header.PK);
				result.Add(header.FH_FH_ParentHeader);
			}

			return result;
		}

		static ProcessHeader[] GetAndLoadWorkflowsConnectedByAParentChildRelationship(BusinessObjectFactory factory, List<ZGuid> workflows, HashSet<ZGuid> loadedWorkflows)
		{
			if (workflows.Any())
			{
				var loadedPKs = loadedWorkflows;
				var notLoadedPKs = workflows;
				var pksToLoad = new List<ZGuid>();
				do
				{
					var links = ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(factory, notLoadedPKs, shouldForceSeek: true).Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild).ToArray();
					notLoadedPKs = GetNotLoadedPKsFromLinks(links, loadedPKs);

					foreach (var pk in notLoadedPKs)
					{
						loadedPKs.Add(pk);
						pksToLoad.Add(pk);
					}
				}
				while (notLoadedPKs.Any());
				var workflowQuery = new ZQuery { AllowTableValuedParameters = true };
				workflowQuery.AddToFilter(ProcessHeaderSchema.PK, pksToLoad);
				return factory.Load<ProcessHeader>(workflowQuery);
			}

			return System.Array.Empty<ProcessHeader>();
		}

		static List<ZGuid> GetNotLoadedPKsFromLinks(ProcessHeaderLink[] links, HashSet<ZGuid> loadedPKs)
		{
			var result = new List<ZGuid>();
			foreach (var link in links)
			{
				if (!loadedPKs.Contains(link.FP_FH_HeaderTo))
				{
					result.Add(link.FP_FH_HeaderTo);
				}
				if (!loadedPKs.Contains(link.FP_FH_HeaderFrom))
				{
					result.Add(link.FP_FH_HeaderFrom);
				}
			}
			return result.Distinct().ToList();
		}

		static IEnumerable<ZString> GetValidStaffCodes(IEnumerable<ProcessTask> tasks)
		{
			foreach (var task in tasks)
			{
				var code = task.P9_GS_NKAssignedStaffMember;
				if (!string.IsNullOrEmpty(code))
				{
					yield return code;
				}
			}
		}

		#endregion

		#region For Test
#if DEBUG

		public IEnumerable<ProcessTask> GetIncompleteTasks_ForTest()
		{
			var workflows = GetIncompleteWorkflowsAndTasks();

			return workflows.TasksShownOnSection;
		}

		public ICollection<ProcessHeader> GetIncompleteWorkflows_ForTest()
		{
			return GetIncompleteWorkflows().WorkflowsShownOnSection;
		}

#endif
		#endregion
	}
}
