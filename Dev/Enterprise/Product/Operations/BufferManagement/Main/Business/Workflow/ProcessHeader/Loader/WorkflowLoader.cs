using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowLoader
	{
		#region API

		public static ProcessHeader[] LoadWorkflowsByPKUsingTableValuedParameter(BusinessObjectFactory factory, IEnumerable<ZGuid> pks, ZQuery additionalFilter = null)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(pks, nameof(pks));

			var query = new ZQuery { AllowTableValuedParameters = true }.AddToFilter(ProcessHeaderSchema.PK, pks);

			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}

			return factory.Load<ProcessHeader>(query);
		}

		public static ZQuery GetStmModuleFilterQuery(StmModuleFilter layout, bool shouldOptimiseForWorkflowsOnly)
		{
			if (shouldOptimiseForWorkflowsOnly)
			{
				return RelatedModuleFiltersHelper.GetFilterQuerySafe(layout, filterBizo =>
				{
					if (filterBizo is IJobOrWorkflowOptimisable optimisable)
					{
						optimisable.ShouldOptimiseQueryForWorkflowOnly = true;
					}
				});
			}

			return RelatedModuleFiltersHelper.GetFilterQuerySafe(layout);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ProcessHeader[] LoadReleaseSchedulerWorkflows(BMBoardSection boardSection, IEnumerable<BMBoardSectionChannel> channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter, ICollection<ZGuid> workflowScope = null)
		{
			var queryBuilder = new VisualBoardQuery();
			var releaseGroup = boardSection.Factory.Load<GlbGroup>(boardSection.SectionConfiguration.ReleaseGroupPK);

			if (releaseGroup != null)
			{
				queryBuilder.Groups.Add(releaseGroup.PK);

				var releaseGroupMembers = releaseGroup.Staff.Cast<GlbStaff>().Where(s => s.GS_IsActive);

				releaseGroupMembers.ForEach(member => AddQueriable(member, queryBuilder));
			}

			var taskSectionFilters = GetTasksQueries(ProcessTasksSchema.P9_FH_ProcessHeader, taskSectionFilter, new[] { queryBuilder });

			var headerQuery = new ZQuery();
			var componentLinkQueries = boardSection.Component.GetFeedingComponentLinks()
				.Select(link => new { Component = link.FL_FC_ComponentFrom, WorkflowQuery = GetStmModuleFilterQuery(link.FilterRule, shouldOptimiseForWorkflowsOnly: false) })
				.Append(new { Component = boardSection.MS_FC_Component, WorkflowQuery = new ZQuery() })
				.GroupBy(tuple => tuple.WorkflowQuery.FilterString);

			foreach (var group in componentLinkQueries)
			{
				var componentsSubquery = new ZDBOnlyQuery(typeof(ProcessHeader));

				var workflowQuery = group.First().WorkflowQuery;
				if (workflowQuery != null)
				{
					componentsSubquery.AddToFilter(workflowQuery);
				}
				componentsSubquery.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, group.Select(s => s.Component).ToArray());

				headerQuery.AddToFilter(componentsSubquery, JoinCondition.Or);
			}

			var componentData = new WorkflowComponentsData(taskSectionFilters, headerQuery, Array.Empty<ZGuid>());

			var query = GetQueryForBoardSection(boardSection, workflowSectionFilter, componentData);

			AddBoardInfoToQueryForDebug(boardSection, query);
			return FilterResult(boardSection.Factory.Load<ProcessHeader>(query), boardSection);
		}

		public static ProcessJobHeader[] LoadJobLevelWorkflows(BMBoardSection boardSection, IEnumerable<BMBoardSectionChannel> channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter)
		{
			Argument.NotNull(boardSection, nameof(boardSection));
			Argument.NotNull(channels, nameof(channels));
			Argument.NotNull(taskSectionFilter, nameof(taskSectionFilter));
			Argument.NotNull(workflowSectionFilter, nameof(workflowSectionFilter));
			var sectionConfig = boardSection.SectionConfiguration;

			if (sectionConfig.IsReleaseScheduler)
			{
				throw new InvalidOperationException("It is not possible to show job-level workflow tickets for a release scheduler board section");
			}
			else if (sectionConfig.CellsPerSubsection == 0)
			{
				return Array.Empty<ProcessJobHeader>();
			}

			var taskSectionFilters = GetTasksQueries(ProcessTasksSchema.P9_FH_ProcessHeader, boardSection.Factory, taskSectionFilter, channels);
			var componentData = new WorkflowComponentsData(taskSectionFilters, workflowSectionFilter, boardSection.AllShownComponentPKs);
			componentData.ReleaseGroupPK = GetReleaseGroup(sectionConfig);
			var query = GetQueryForBoardSection(boardSection, workflowSectionFilter, componentData);

			AddBoardInfoToQueryForDebug(boardSection, query);
			return FilterResult(boardSection.Factory.Load<ProcessJobHeader>(query), boardSection);
		}

		public static ProcessHeader[] LoadWorkflows(BMBoardSection boardSection, IEnumerable<BMBoardSectionChannel> channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter, ICollection<ZGuid> workflowScope = null)
		{
			Argument.NotNull(boardSection, nameof(boardSection));
			Argument.NotNull(channels, nameof(channels));
			taskSectionFilter = taskSectionFilter ?? new ZQuery();
			workflowSectionFilter = workflowSectionFilter ?? new ZQuery();

			if (boardSection.SectionConfiguration.CellsPerSubsection == 0)
			{
				return Array.Empty<ProcessHeader>();
			}

			var taskSectionFilters = GetTasksQueries(ProcessTasksSchema.P9_FH_ProcessHeader, boardSection.Factory, taskSectionFilter, channels);
			var componentData = new WorkflowComponentsData(taskSectionFilters, workflowSectionFilter, boardSection.AllShownComponentPKs);
			var query = GetQueryForBoardSection(boardSection, workflowSectionFilter, componentData);

			AddBoardInfoToQueryForDebug(boardSection, query);
			if (!workflowScope.IsNullOrEmpty())
			{
				query.AddToFilter(ProcessHeaderSchema.PK, workflowScope);
			}

			var result = boardSection.Factory.Load<ProcessHeader>(query);
			return FilterResult(result, boardSection);
		}

		static T[] FilterResult<T>(T[] headers, BMBoardSection boardSection) where T : ProcessHeader
		{
			var result = headers.Where(h => h.IsOpen);
			if (boardSection.SectionConfiguration.ShowWorkInReleaseGroupOnly)
			{
				var releaseGroupPK = boardSection.SectionConfiguration.ReleaseGroupPK;
				if (releaseGroupPK.IsValid)
				{
					result = result.Where(w => IsWorkflowInGroup(w, releaseGroupPK)).ToArray();
				}
			}

			return result.ToArray();
		}

		public static ZDBOnlyQuery GetTasksQuery(ZQuery workflowQuery, IEnumerable<ZGuid> componentPks)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			if (workflowQuery != null)
			{
				workflowSubQuery.AddToFilter(workflowQuery);
			}
			query.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());
			query.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflowSubQuery, JoinCondition.And);
			if (componentPks.Any())
			{
				query.AddToFilter(ProcessTasksSchema.P9_FC_CurrentComponent, componentPks);
			}
			query.IncludeBlob(ProcessTasksSchema.P9_Notes);
			query.MaximumRows = MaxRowsForBoardSectionQueries;

			return query;
		}

		public static ProcessTask[] LoadTasks(BMBoardSection boardSection, IEnumerable<BMBoardSectionChannel> channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter, ICollection<ZGuid> taskScope = null)
		{
			Argument.NotNull(boardSection, nameof(boardSection));
			Argument.NotNull(channels, nameof(channels));
			var tasks = boardSection.Factory.Load<ProcessTask>(BuildBoardQuery(boardSection, channels, workflowSectionFilter, taskSectionFilter, taskScope));

			if (boardSection.SectionConfiguration.TimeField == TimeProgressionFieldList.Codes.WorkingTimeSinceStartable)
			{
				tasks = tasks.Where(t => t.TimeBecameStartable != ZDateTime.Empty).ToArray();
			}

			var releaseGroupPK = boardSection.SectionConfiguration.ReleaseGroupPK;
			if (releaseGroupPK.IsValid && boardSection.SectionConfiguration.ShowWorkInReleaseGroupOnly)
			{
				tasks = tasks.Where(t => t.GetProcessHeader() is ProcessHeader w && IsWorkflowInGroup(w, releaseGroupPK)).ToArray();
			}

			return tasks;
		}

		public static ZDBOnlyQuery BuildBoardQuery(BMBoardSection boardSection, IEnumerable<BMBoardSectionChannel> channels, ZQuery workflowSectionFilter, ZQuery taskSectionFilter, ICollection<ZGuid> taskScope = null)
		{
			workflowSectionFilter = workflowSectionFilter ?? new ZQuery();
			taskSectionFilter = taskSectionFilter ?? new ZQuery();

			var componentData = new WorkflowComponentsData(Array.Empty<ZQuery>(), workflowSectionFilter, boardSection.AllShownComponentPKs);

			var taskQuery = new ZDBOnlyQuery(typeof(ProcessTask));

			var workflowQuery =
				GetQueryForBoardSection(boardSection, workflowSectionFilter, componentData: componentData);
			taskQuery = GetTasksQuery(workflowQuery, componentData.ComponentPKs);

			var taskSectionFilters = GetTasksQueries(ProcessTasksSchema.PK, boardSection.Factory, taskSectionFilter, channels);
			var releaseGroup = GetReleaseGroup(boardSection.SectionConfiguration);

			if (releaseGroup.IsValid)
			{
				var taskGroupSubQuery = GetInTaskOrWorkflowGroupSubQuery(releaseGroup);
				taskQuery.AddSubQuery(taskGroupSubQuery, JoinCondition.And);
			}

			var split = taskSectionFilters.Split(t => t is ZDBOnlySubQuery);
			split.MatchingSet.ForEach(subQuery => taskQuery.AddSubQuery(ProcessTasksSchema.PK, (ZDBOnlySubQuery)subQuery, JoinCondition.And));
			split.NonMatchingSet.ForEach(nonSubQuery => taskQuery.AddToFilter(nonSubQuery));

			if (!taskScope.IsNullOrEmpty())
			{
				taskQuery.AddToFilter(new ZQuery { AllowTableValuedParameters = true }.AddToFilter(ProcessTasksSchema.PK, taskScope));
			}
			return taskQuery;
		}

		static ZDBOnlySubQuery GetInTaskOrWorkflowGroupSubQuery(ZGuid releaseGroup)
		{
			var taskGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.PK);
			taskGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, releaseGroup);
			taskGroupSubQuery.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());
			taskGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());

			var taskNoGroupQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.PK);
			taskNoGroupQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);
			taskNoGroupQuery.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());
			taskNoGroupQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());

			var workflowGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			workflowGroupSubQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, releaseGroup);
			taskNoGroupQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflowGroupSubQuery, JoinCondition.And);
			taskGroupSubQuery.AddAsUnionQuery(taskNoGroupQuery, addAsUnionAll: true);

			return taskGroupSubQuery;
		}

		public static ProcessTask[] LoadTasksForWorkflows(BMBoardSection section, ProcessHeader[] workflows)
		{
			var workflowPKs = workflows.Select(x => x.PK);
			var query = new ZDBOnlyQuery(typeof(ProcessTask)) { AllowTableValuedParameters = true };

			if (section.SectionConfiguration.ShowJobWorkflowCards)
			{
				var parentQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				parentQuery.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, section.AllShownComponentPKs); // filter tasks only for relevant section's components
				var subQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				subQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null); // Not required for functionality to work, but performs slightly better.
				subQuery.AddToFilter(new ZQuery { AllowTableValuedParameters = true }.AddToFilter(ProcessHeaderSchema.PK, workflowPKs));
				parentQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, subQuery, JoinCondition.And);
				query.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, parentQuery, JoinCondition.And);
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentID, workflows.Select(wf => wf.FH_ParentId));
				query.AddToFilter(ProcessTasksSchema.P9_FH_ProcessHeader, workflowPKs);
			}

			query.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());
			query.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());
			AddBoardInfoToQueryForDebug(section, query, nameof(LoadTasksForWorkflows));

			return section.Factory.Load<ProcessTask>(query);
		}

		#endregion

		#region Implementation

		static int MaxRowsForBoardSectionQueries => BMSRegistry.Instance.MaxNumberOfItemsOnBoards.Value + 1; // Extra one to detect if we actually passed the maximum

		static ZQuery GetQueryForBoardSection(BMBoardSection boardSection, ZQuery workflowSectionFilter, WorkflowComponentsData componentData)
		{
			if (boardSection.Component == null)
			{
				throw new InvalidOperationException("Cannot load the component for the board section.");
			}

			if (boardSection.Component.IsChildComponent)
			{
				throw new InvalidOperationException("Cannot show a board section for a child component.");
			}

			if (boardSection.Board == null)
			{
				throw new InvalidOperationException("Cannot load the board for the board section.");
			}

			var sectionConfig = boardSection.SectionConfiguration;
			var releaseGroup = GetReleaseGroup(sectionConfig);

			var queryConfig = new WorkflowLoadConfig
			{
				ReleaseGroupPK = sectionConfig.ShowJobWorkflowCards || sectionConfig.Channels.Any() ? ZGuid.Empty : releaseGroup,
				MaxRowsToLoad = MaxRowsForBoardSectionQueries,
				LoadNonClosedWorkflowsOnly = true,
			};

			var extendedQueryWithDebugInfo = GetQuery(queryConfig, componentData, sectionConfig.ShowJobWorkflowCards, boardSection.AreWorkflowFiltersSpecified);
			if (boardSection.AreWorkflowFiltersSpecified)
			{
				extendedQueryWithDebugInfo.AddToFilter(workflowSectionFilter);
			}
			else
			{
				AddBoardInfoToQueryForDebug(boardSection, extendedQueryWithDebugInfo);
			}

			return extendedQueryWithDebugInfo;
		}

		public static void AddBoardInfoToQueryForDebug(BMBoardSection boardSection, ZQuery query, string moreInfo = null)
		{
			var paramFilter = "1=1";
			var paramValue = string.Format(CultureInfo.InvariantCulture, BoardSectionNameSQLAdditionalInfo, Db.DatabaseName, boardSection.Board.MB_Name, boardSection.SectionName, moreInfo);
			var parameterCollection = new ZSqlParameterCollection();

			parameterCollection.Add(ZSqlParameter.New("@BoardInfo", paramValue, BMBoardSchema.MB_Description));
			query.AddFilterAndZSQLParameterCollection(paramFilter, parameterCollection);
		}

		static ZGuid GetReleaseGroup(BMComponentSectionConfiguration configuration)
		{
			return configuration.ShowWorkInReleaseGroupOnly ? configuration.ReleaseGroupPK : ZGuid.Empty;
		}

		public const string BoardSectionNameSQLAdditionalInfo = @"
			1=1 -- true condition to make compile SQL with empty AND() brackets
			-- Database: {0}
			-- Board:    {1}
			-- Section:  {2}
			-- Type: Board Load {3}
			--
			";

		static ICollection<ZQuery> GetTasksQueries(SchemaColumn schemaColumn, BusinessObjectFactory factory, ZQuery taskSectionFilter, IEnumerable<BMBoardSectionChannel> channels)
		{
			Argument.NotNull(schemaColumn, nameof(schemaColumn));
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(taskSectionFilter, nameof(taskSectionFilter));
			Argument.NotNull(channels, nameof(channels));

			var queryBuilders = new List<VisualBoardQuery>();

			var primaryChannelBuilder = GetChannelQueryBuilder(channels.Where(c => c.MSC_Axis == ChannelAxisCodeList.Codes.Primary), factory);
			var secondaryChannelBuilder = GetChannelQueryBuilder(channels.Where(c => c.MSC_Axis == ChannelAxisCodeList.Codes.Secondary), factory);

			if (primaryChannelBuilder != null)
			{
				queryBuilders.Add(primaryChannelBuilder);
			}

			if (secondaryChannelBuilder != null)
			{
				queryBuilders.Add(secondaryChannelBuilder);
			}

			return GetTasksQueries(schemaColumn, taskSectionFilter, queryBuilders);
		}

		static ICollection<ZQuery> GetTasksQueries(SchemaColumn schemaColumn, ZQuery taskSectionFilter, IEnumerable<VisualBoardQuery> queryBuilders)
		{
			var additionalTaskFilter = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());
			additionalTaskFilter.AddToFilter(taskSectionFilter);

			var channelQueries = queryBuilders.Select(qb => qb.CreateTaskQuery(additionalTaskFilter, schemaColumn)).ToArray();

			if (channelQueries.All(q => q == null))
			{
				return new[] { additionalTaskFilter };
			}
			else
			{
				return channelQueries;
			}
		}

		static VisualBoardQuery GetChannelQueryBuilder(IEnumerable<BMBoardSectionChannel> channelsSubset, BusinessObjectFactory factory)
		{
			if (channelsSubset.Any())
			{
				var queryBuilder = new VisualBoardQuery();

				foreach (var channel in channelsSubset)
				{
					var bizo = channel.GetChannelBusinessObject(factory);

					AddQueriable(bizo, queryBuilder);
				}

				return queryBuilder;
			}
			else
			{
				return null;
			}
		}

		static void AddQueriable(BusinessObject bizo, VisualBoardQuery queryBuilder)
		{
			if (bizo == null)
			{
				return;
			}
			if (bizo is GlbStaff staff)
			{
				var capabilities = staff.CapabilityPivots
					.Select(capabilityPivot => new CapabilityScope(capabilityPivot.G5_G4_Capability, capabilityPivot.Capability.G4_CapacityScope))
					.ToArray();
				queryBuilder.AddStaff(new StaffCode(staff.PK, staff.GS_Code), capabilities);
			}
			if (bizo is GlbGroup group)
			{
				var resources = group.Staff.Cast<GlbStaff>().Where(s => !string.IsNullOrEmpty(s.GS_Code)).ToHashSet();
				var capabilityPks = group.Staff.Cast<GlbStaff>().SelectMany(s => s.Capabilities).Select(c => c.PK).Distinct().ToArray();

				queryBuilder.AddGroup(group.PK, resources, capabilityPks);
			}
			if (bizo is GlbCapability capability)
			{
				queryBuilder.AddCapability(capability.PK);
			}
			if (bizo is WorkQueue || bizo is TagMagnitude)
			{
				queryBuilder.AddTagMagnitude(bizo.PK);
			}
		}

		public static ZQuery GetQuery(WorkflowLoadConfig loadConfig, WorkflowComponentsData workflowComponentsData, bool showJobWorkflows = false, bool areWorkflowFiltersSpecified = true)
		{
			var query = new ZQuery();
			if (loadConfig.ReleaseGroupPK.IsValid)
			{
				query.AddToFilter(GetIsWorkflowInGroupQuery(loadConfig.ReleaseGroupPK));
			}

			var workflowComponentBaseQuery =
				AddFiltersFromWorkflowComponents(workflowComponentsData, showJobWorkflows);
			query.AddToFilter(workflowComponentBaseQuery);
			if (loadConfig.MaxRowsToLoad != null)
			{
				query.MaximumRows = loadConfig.MaxRowsToLoad;
			}

			return query;
		}

		static ZDBOnlyQuery AddFiltersFromWorkflowComponents(WorkflowComponentsData workflowComponentsData, bool showJobWorkflows)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			query.AddToFilter(workflowComponentsData.WorkflowSectionFilter);

			void MaybeAddTaskQuery(ZDBOnlyQuery dbOnlyQuery)
			{
				if (workflowComponentsData.HasTaskQuery)
				{
					var split = workflowComponentsData.TaskSubQueries.Split(t => t is ZDBOnlySubQuery);
					foreach (ZDBOnlySubQuery subQuery in split.MatchingSet)
					{
						dbOnlyQuery.AddSubQuery(ProcessHeaderSchema.PK, subQuery, JoinCondition.And);
					}
					var taskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader);
					foreach (var nonSubQuery in split.NonMatchingSet)
					{
						taskSubQuery.AddToFilter(nonSubQuery);
					}

					dbOnlyQuery.AddSubQuery(ProcessHeaderSchema.PK, taskSubQuery, JoinCondition.And);
				}
			}

			if (showJobWorkflows)
			{
				var jobHeaderParentQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
				var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_FH_ParentHeader);

				if (workflowComponentsData.ReleaseGroupPK.IsValid)
				{
					query.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, workflowComponentsData.ReleaseGroupPK);
				}

				if (workflowComponentsData.ComponentPKs.Any())
				{
					workflowSubQuery.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, workflowComponentsData.ComponentPKs);
				}

				MaybeAddTaskQuery(workflowSubQuery);

				jobHeaderParentQuery.AddSubQuery(ProcessHeaderSchema.PK, workflowSubQuery, JoinCondition.And);

				query.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null); // Not required for functionality to work, but performs slightly better.
				query.AddToFilter(jobHeaderParentQuery);
			}
			else
			{
				if (workflowComponentsData.ComponentPKs.Any())
				{
					query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, workflowComponentsData.ComponentPKs);
				}

				MaybeAddTaskQuery(query);
			}
			return query;
		}

		static bool IsWorkflowInGroup(ProcessHeader header, ZGuid groupPk)
		{
			var tasks = header.Tasks.Where(t => t.IsOpen);
			return (header.FH_GG_ReleaseGroup == groupPk && tasks.Any(t => t.P9_GG_AssignedGroup.IsEmpty))
			|| tasks.Any(t => t.P9_GG_AssignedGroup == groupPk);
		}

		static ZDBOnlyQuery GetIsWorkflowInGroupQuery(ZGuid releaseGroupPK)
		{
			var wokflowOrTaskInGroupQuery = new ZDBOnlyQuery(typeof(ProcessHeader));

			var wokflowOrTaskInGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			wokflowOrTaskInGroupSubQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, releaseGroupPK);

			var taskGroupIsNullSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_FH_ProcessHeader);
			taskGroupIsNullSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);
			taskGroupIsNullSubQuery.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());
			taskGroupIsNullSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());

			wokflowOrTaskInGroupSubQuery.AddSubQuery(taskGroupIsNullSubQuery, JoinCondition.And);

			var taskInGroupSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.P9_FH_ProcessHeader);
			taskInGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, releaseGroupPK);
			taskInGroupSubQuery.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());
			taskInGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_Status, ProcessTask.GetOpenTaskStatuses());

			wokflowOrTaskInGroupSubQuery.AddAsUnionQuery(taskInGroupSubQuery, addAsUnionAll: true);

			wokflowOrTaskInGroupQuery.AddSubQuery(wokflowOrTaskInGroupSubQuery, JoinCondition.And);

			return wokflowOrTaskInGroupQuery;
		}

		#endregion
	}
}
