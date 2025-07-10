using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.CustomerService.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public delegate ZQuery JobTextPropertyQuery(SQLComparisonOperator comparisonOperator, ZString workflowTypeCode, ZString jobPropertyName, ZString jobPropertyValue);

	public class ProcessHeaderFilterBusinessObject : FilterStripBusinessObject, IJobOrWorkflowOptimisable
	{
		public const string DateAcceptability = "DATEACCEPTABILITY";
		public const string JobOrWorkflow = "JOBORWORKFLOW";
		public const string LastTransferType = "LASTTRANSFERTYPE";
		public const string Template = "TEMPLATE";
		public const string WorkflowCategory = "WORKFLOWCATEGORY";
		public const string WorkflowStatus = "WORKFLOWSTATUS";
		public const string WorkflowType = "WORKFLOWTYPE";

		public const string WorkItem = "WORKITEM";
		public const string Incident = "INCIDENT";
		public const string Task = "TASK";

		public const string WorkItemType = "WORKITEMTYPE";
		public const string WorkItemArea = "WORKITEMAREA";
		public const string WorkItemActivityType = "WORKITEMACTIVITYTYPE";
		public const string WorkItemActivitySubtype = "WORKITEMACTIVITYSUBTYPE";
		public const string WorkItemPriority = "WORKITEMPRIORITY";
		public const string WorkItemStatus = "WORKITEMSTATUS";

		public const string IncidentCriticality = "INCIDENTCRITICALITY";
		public const string IncidentProduct = "INCIDENTPRODUCT";
		public const string IncidentService = "INCIDENTSERVICE";

		public const string TaskStatusStr = "TASKSTATUS";
		public const string TaskType = "TASKTYPE";

		static CodeDescriptionPairList TemplateOptions => new TemplateFilterOptions();
		static CodeDescriptionPairList JobAndWorkflowOptions => new IndexJobOrWorkflowFilterOptions();

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddProcessHeaderFiltersToModuleFilterCollection(filters, parametersForTagFilters);
			return filters;
		}

		protected override ModuleFilterCollection GetModuleFiltersFromGlowCore()
		{
			var filters = base.GetModuleFiltersFromGlowCore();
			AdjustFilters(filters);
			AddTagFiltersForIndexSearch(filters);

			if (filters.FirstOrDefault(f => f.Description.EqualsIgnoringCase(FilterDescriptions.ActiveStatus)) is IndexSearchModuleTextFilter activeFilter)
			{
				activeFilter.DefaultProperty = StatusAll;
				activeFilter.Visibility = FilterVisibility.AlwaysApplied;
			}

			if (filters.FirstOrDefault(f => f.Description.EqualsIgnoringCase(Template)) is IndexSearchModuleTextFilter templateFilter)
			{
				templateFilter.DefaultProperty = TemplateFilterOptions.Codes.NonTemplate;
				templateFilter.Visibility = FilterVisibility.AlwaysApplied;
			}

			return filters;
		}

		void AdjustFilters(ModuleFilterCollection filters)
		{
			var incidentCategory = new FilterCategory(ResString.GetMultilingualString("7b7e7a38-19aa-4f6b-81c6-e3a8fc7c2849", "Incidents"));
			var workItemCategory = new FilterCategory(ResString.GetMultilingualString("a43f5c71-db32-4181-adbd-2c0ca8e83f48", "Work Items"));
			var taskCategory = new FilterCategory(ResString.GetMultilingualString("053b0258-013a-46ac-b4d7-4d310254e2e3", "Tasks"));

			foreach (var filter in filters.ToList())
			{
				var code = filter.Code.ToUpper();
				if (code.StartsWith(Incident))
				{
					filter.Category = incidentCategory;
				}
				else if (code.StartsWith(WorkItem))
				{
					filter.Category = workItemCategory;
				}
				else if (code.StartsWith(Task))
				{
					filter.Category = taskCategory;
				}

				var prefix = ResString.GetMultilingualString("000f60a2-6806-4371-8bec-08dc1d94ac0b", "Work Item");
				switch (code)
				{
					case WorkItemType:
						filter.MultilingualDescription = MultilingualString.Join(" ", prefix, (MultilingualString)ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemCriterionLabel(1));
						break;
					case WorkItemArea:
						filter.MultilingualDescription = MultilingualString.Join(" ", prefix, (MultilingualString)ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemCriterionLabel(2));
						break;
					case WorkItemActivityType:
						filter.MultilingualDescription = MultilingualString.Join(" ", prefix, (MultilingualString)ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemCriterionLabel(3));
						break;
					case WorkItemActivitySubtype:
						filter.MultilingualDescription = MultilingualString.Join(" ", prefix, (MultilingualString)ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemCriterionLabel(4));
						break;
					case WorkItemPriority:
						filter.MultilingualDescription = MultilingualString.Join(" ", prefix, (MultilingualString)ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemCriterionLabel(5));
						break;
					default:
						break;
				}
			}
		}

		void AddTagFiltersForIndexSearch(ModuleFilterCollection filters)
		{
			var bufferManagementCategory = new FilterCategory(BMGlobalConstants.BufferManagementCategoryDescription);

			var searchFieldTag = IndexSearchFields?.DefaultHiddenIndexSearchFields.FirstOrDefault(x => x.FieldName == "TAG");
			if (searchFieldTag != null)
			{
				var filterTag = new IndexSearchTagWithJobOrWorkflowFilter(searchFieldTag, bufferManagementCategory, ModuleIDs.BMTagMagnitude, GetTagMagnitudeFilter, new TagMagnitudeCollection(Factory), useJobOrWorkflowFilter: false);
				filterTag.MultilingualDescription = ResString.GetMultilingualString("51cb02db-7288-44ed-a5e7-7f028c78e2da", "Tag");
				filterTag.Category = bufferManagementCategory;
				filters.AddFilter(filterTag);
			}

			var searchFieldTagGroup = IndexSearchFields?.DefaultHiddenIndexSearchFields.FirstOrDefault(x => x.FieldName == "TAGGROUP");
			if (searchFieldTagGroup != null)
			{
				var filterTagGroup = new IndexSearchTagWithJobOrWorkflowFilter(searchFieldTagGroup, bufferManagementCategory, ModuleIDs.BMTagDefinition, GetTagDefinitionFilter, new TagDefinitionCollection(Factory), useJobOrWorkflowFilter: false);
				filterTagGroup.MultilingualDescription = ResString.GetMultilingualString("2c928c6b-ff30-40e1-a5c0-84a1699abd83", "Tag Group");
				filterTagGroup.Category = bufferManagementCategory;
				filters.AddFilter(filterTagGroup);
			}
		}

		IGlowQuery GetTagMagnitudeFilter(string fieldName, ZGuid property, ZString dropDownTypeName, string comparisonOperator)
		{
			return IndexSearchTagWithJobOrWorkflowFilter.GetGlowIndexQueryForTag(fieldName, property, comparisonOperator, Factory);
		}

		IGlowQuery GetTagDefinitionFilter(string fieldName, ZGuid property, ZString dropDownTypeName, string comparisonOperator)
		{
			return IndexSearchTagWithJobOrWorkflowFilter.GetGlowIndexQueryForTagGroup(fieldName, property, comparisonOperator);
		}

		readonly ZSqlParameterCollection parametersForTagFilters = new ZSqlParameterCollection();

		public ZSqlParameterCollection ParametersForTagFilters
		{
			get
			{
				return parametersForTagFilters;
			}
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return new[] { typeof(WorkflowFilterStripsHelper) }; }
		}

		protected override void SetExternalDefaultsCore(FilterBusinessObjectDefault filterDefault, IEnumerable<ZString> skippedOrCategory = null)
		{
			base.SetExternalDefaultsCore(filterDefault);
			if (filterDefault.FilterName == ProcessHeader.ModuleFilterConstants.JobCode)
			{
				SetInitialCodeForSearch((ZString)filterDefault.Value, filterDefault.PropertyName);
			}
		}

		public override ZQuery Filter
		{
			get
			{
				parametersForTagFilters.Clear();

				return base.Filter;
			}
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case DateAcceptability:
					var dateAcceptabilityFilter = new IndexSearchModuleTextFilter(searchField, new DateAcceptabilityList());
					return new SearchFieldOverride(searchField, indexFilterOverride: dateAcceptabilityFilter);
				case JobOrWorkflow:
					var jobOrWorklflowFilter = new IndexJobOrWorkflowFilter(searchField, JobAndWorkflowOptions, FilterCategories.StatusAndFlags);
					return new SearchFieldOverride(searchField, indexFilterOverride: jobOrWorklflowFilter);
				case LastTransferType:
					var lastTransferTypeFilter = new IndexSearchModuleTextFilter(searchField, new TransferTypeList());
					return new SearchFieldOverride(searchField, indexFilterOverride: lastTransferTypeFilter);
				case Template:
					var templateFilter = new IndexTemplateFilter(searchField, TemplateOptions, FilterCategories.StatusAndFlags);
					return new SearchFieldOverride(searchField, indexFilterOverride: templateFilter);
				case WorkflowCategory:
					var types = GetWorkflowTypeList(Factory).GetAllCodes();
					var categories = new CodeDescriptionPairList(); 
					foreach (var t in types)
					{
						categories.AddRangeOverwriteIfExists(BMSRegistry.Instance.WorkflowCategories.Value.GetCategoriesFromWorkflowCode(t).GetCodeDescriptionPairList());
					}
					categories.Sort();
					var categoryFilter = new IndexSearchModuleTextFilter(searchField, categories);
					return new SearchFieldOverride(searchField, indexFilterOverride: categoryFilter);
				case WorkflowStatus:
					var list = new WorkflowStatusList();
					list.RemoveCode(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites);
					var workflowStatusFilter = new IndexSearchModuleTextFilter(searchField, list);
					return new SearchFieldOverride(searchField, indexFilterOverride: workflowStatusFilter);
				case WorkflowType:
					var workflowTypeFilter = new IndexSearchModuleTextFilter(searchField, GetWorkflowTypeList(Factory));
					return new SearchFieldOverride(searchField, indexFilterOverride: workflowTypeFilter);

				case WorkItemType:
					var typeFilter = new IndexSearchModuleTextFilter(
						searchField,
						ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemTypes(activeOnly: false));
					return new SearchFieldOverride(searchField, indexFilterOverride: typeFilter);
				case WorkItemArea:
					var areaFilter = new IndexSearchModuleTextFilter(
						searchField,
						ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetWorkItemAreas(string.Empty, activeOnly: false));
					return new SearchFieldOverride(searchField, indexFilterOverride: areaFilter);
				case WorkItemActivityType:
					var activityTypeFilter = new IndexSearchModuleTextFilter(
						searchField,
						ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetActivityTypes(string.Empty, string.Empty, activeOnly: false));
					return new SearchFieldOverride(searchField, indexFilterOverride: activityTypeFilter);
				case WorkItemActivitySubtype:
					var activitySubtypeFilter = new IndexSearchModuleTextFilter(
						searchField,
						ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetActivitySubtypes(string.Empty, string.Empty, string.Empty, activeOnly: false));
					return new SearchFieldOverride(searchField, indexFilterOverride: activitySubtypeFilter);
				case WorkItemPriority:
					var priorityFilter = new IndexSearchModuleTextFilter(
						searchField,
						ObjectFactory.Get<IWorkItemTypeTreeProvider>().GetPriorities(string.Empty, string.Empty, string.Empty, string.Empty, activeOnly: false));
					return new SearchFieldOverride(searchField, indexFilterOverride: priorityFilter);
				case WorkItemStatus:
					var statusFilter = new StatusIndexFilter(searchField, new ProcessTaskFilterBusinessObject().Statuses);
					return new SearchFieldOverride(searchField, indexFilterOverride: statusFilter);

				case IncidentCriticality:
					var criticalityFilter = new IndexSearchModuleTextFilter(searchField, new IncidentApprovalLookups(null).CriticalityList);
					return new SearchFieldOverride(searchField, indexFilterOverride: criticalityFilter);
				case IncidentProduct:
					var prodList = new CodeDescriptionPairList();
					prodList.AddPair(IncidentApprovalLookups.EnterpriseProductCode, Res.GetString("CustomerService|ProductList|CargoWise", "Enterprise"));
					var productFilter = new IndexSearchModuleTextFilter(searchField, prodList);
					return new SearchFieldOverride(searchField, indexFilterOverride: productFilter);
				case IncidentService:
					var modList = new IncidentApprovalLookups(null).Cr9ModuleList;
					modList.Sort();
					var serviceFilter = new IndexSearchModuleTextFilter(searchField, modList);
					return new SearchFieldOverride(searchField, indexFilterOverride: serviceFilter);

				case TaskStatusStr:
					var taskStatusFilter = new StatusIndexFilter(searchField, new ProcessTaskFilterBusinessObject().Statuses);
					return new SearchFieldOverride(searchField, indexFilterOverride: taskStatusFilter);
				case TaskType:
					var tasks = new CodeDescriptionPairList();
					foreach (CategorisedWorkflowTaskTypes workflowType in WorkflowDataRegistry.Instance.TaskTypes.Value)
					{
						foreach (ICodeDescription taskType in workflowType.TaskTypes)
						{
							tasks.AddPairIfNotExist(taskType.Code, taskType.Description);
						}
					}
					tasks.Sort();
					var taskTypeFilter = new IndexSearchModuleTextFilter(searchField, tasks);
					return new SearchFieldOverride(searchField, indexFilterOverride: taskTypeFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		#region IJobOrWorkflowOptimisable Members

		bool IJobOrWorkflowOptimisable.ShouldOptimiseQueryForWorkflowOnly { get; set; }

		#endregion

		#region Static Helper Methods	

		public void AddProcessHeaderFiltersToModuleFilterCollection(ModuleFilterCollection filters, ZSqlParameterCollection parameters)
		{
			AddTextFilterWithOnlyExactAndNotEqualComparisonOperators(filters, ProcessHeader.ModuleFilterConstants.WorkflowType, ProcessHeaderSchema.FH_WorkflowType, () => GetWorkflowTypeList(Factory), ResString.GetMultilingualString("CB0ED946-72C1-4B2B-BF36-52E65546FC07", "Workflow Type"));
			AddTextFilterWithOnlyExactAndNotEqualComparisonOperators(filters, ProcessHeader.ModuleFilterConstants.LastTransferType, ProcessHeaderSchema.FH_LastTransferType, () => new TransferTypeList(), ResString.GetMultilingualString("cb26c00c-c651-4092-bd74-3d31bb8be2c8", "Last Transfer Type"));

			filters.AddFilter(new JobCodeFilter(() => GetWorkflowTypeList(Factory)));
			filters.AddFilter(new JobDescriptionFilter(() => GetWorkflowTypeList(Factory)));
			filters.AddFilter(new JobOrWorkflowFilter());
			filters.AddFilter(new JobTextPropertyFilter(GetJobTextPropertyQuery, () => GetWorkflowTypeList(Factory)));
			filters.AddFilter(new WorkflowCategoryFilter(() => GetWorkflowTypeList(Factory)));
			filters.AddFilter(new TaskStatusFilter(Factory));
			filters.AddFilter(new TaskAssignedFilter(Factory));
			filters.AddFilter(new OpenTaskEstimateRangeFilter(Factory));

			filters.AddTextFilter(ProcessHeader.ModuleFilterConstants.CompletionStatement, ProcessHeaderSchema.FH_CompletionStatement).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|CompletionStatement", "Description");
			filters.AddTextFilter(ProcessHeader.ModuleFilterConstants.DateAcceptability, new GetTextQueryWithOperator(GetDateAcceptabilityQuery), () => new DateAcceptabilityList()).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|DateAcceptability", "Date Acceptability");

			AddTaskFilters(Factory, filters);
			AddStatusFilters(filters, this);

			filters.AddDateFilter(ProcessHeader.ModuleFilterConstants.BufferReleaseDate, ProcessHeaderSchema.FH_ReleaseDateTime, convertFromLocalToUTC: true).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ReleaseDate", "Last Transfer Date");
			filters.AddDateFilter(ProcessHeader.ModuleFilterConstants.StaggeredReleaseDelayExpiry, ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|StaggeredReleaseDelayExpiry", "Staggered Release Delay Expiry");
			filters.AddFilter(new DateFilterWithJobFallback(ProcessHeader.ModuleFilterConstants.EarliestStartDate, ProcessHeaderSchema.FH_DoNotStartBeforeDate, true, ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|EarliestStartDate", "Earliest Start Date")));
			filters.AddFilter(new DateFilterWithJobFallback(ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate, ProcessHeaderSchema.FH_AgreedDeliveryDate, true, ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|AgreedDeliveryDate", "Agreed Delivery Date")));
			filters.AddFilter(new LeadTimeFilter(Factory));

			AddApprovedScheduleFilters(filters);

			filters.AddFlagsFilter(ProcessHeader.ModuleFilterConstants.AutoAssignTasks, new[] { Res.GetString("73854d68-99d3-4971-9e3a-24b22be04734", "Auto Assign Tasks") }, new GetFlagsQuery[] { GetAllowTaskAutoAssignmentQuery }).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|AutoAssignTasks", "Auto Assign Tasks");
			filters.AddFlagsFilter(ProcessHeader.ModuleFilterConstants.CriticalHandover, new[] { Res.GetString("a61248bf-caa7-41b7-a2a4-bc143a609052", "Critical Handover") }, new GetFlagsQuery[] { GetCriticalHandoverQuery }).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|CriticalHandover", "Critical Handover");
			filters.AddFlagsFilter(ProcessHeader.ModuleFilterConstants.StandbyTask, new[] { Res.GetString("5dfd86c1-3c8a-4050-b925-e1a6e7be46b4", "Standby Task") }, new GetFlagsQuery[] { GetIsStandbyTaskQuery }).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|StandbyTask", "Standby Task");

			if (BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
			{
				filters.AddFlagsFilter(ProcessHeader.ModuleFilterConstants.Approved, new[] { Res.GetString("a0568edb-3ec8-4801-8db0-f03055a311ce", "Approved") }, new GetFlagsQuery[] { GetIsApprovedQuery }).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|Approved", "Approved");
				filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.Branch, ModuleIDs.GlbBranch, ProcessHeaderSchema.FH_GB_Branch, () => new GlbBranchCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|Branch", "Branch");
				filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.Department, ModuleIDs.GlbDepartment, ProcessHeaderSchema.FH_GE_Department, () => new GlbDepartmentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|Department", "Department");
				filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.EffectiveBranch, ModuleIDs.GlbBranch, ProcessHeaderSchema.FH_GB_EffectiveBranch, () => new GlbBranchCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|EffectiveBranch", "Effective Branch");
				filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.EffectiveDepartment, ModuleIDs.GlbDepartment, ProcessHeaderSchema.FH_GE_EffectiveDepartment, () => new GlbDepartmentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|EffectiveDepartment", "Effective Department");
				filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.BufferTimespan, ModuleIDs.BMBufferTimespan, ProcessHeaderSchema.FH_BMT_BufferTimespan, () => new BMBufferTimespanCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|BufferTimespan", "Buffer Timespan Name");

				var effectiveBufferDurationRangeFilter = new ModuleDurationFilter(ProcessHeader.ModuleFilterConstants.EffectiveBufferDuration, Factory);
				effectiveBufferDurationRangeFilter.SetQueryDelegate(GetEffectiveBufferDurationRangeQuery);
				effectiveBufferDurationRangeFilter.MultilingualDescription = ResString.GetMultilingualString(
					"BufferManagement|ProcessHeaderFilterBusinessObject|EffectiveBufferDuration",
					"Effective Buffer Duration (Hours)");
				effectiveBufferDurationRangeFilter.Category = NumbersAndReferencesCategory;
				effectiveBufferDurationRangeFilter.MaxDurationMinutes = 100000;
				filters.AddFilter(effectiveBufferDurationRangeFilter);

				filters.AddDateFilter(ProcessHeader.ModuleFilterConstants.LatestAcceptableReleaseDate, ProcessHeaderSchema.FH_LatestAcceptableReleaseDateUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|LatestAcceptableReleaseDate", "Latest Acceptable Release Date (UTC)");
				filters.AddDateFilter(ProcessHeader.ModuleFilterConstants.ReleaseSequenceSortDate, ProcessHeaderSchema.FH_ReleaseSequenceSortDateUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ReleaseSequenceSortDate", "Release Sequence Sort Date (UTC)");

				filters.AddDateFilter(ProcessHeader.ModuleFilterConstants.EffectiveAgreedDeliveryDate, ProcessHeaderSchema.FH_EffectiveAgreedDeliveryDateUtc, convertFromLocalToUTC: false).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|EffectiveAgreedDeliveryDate", "Effective Agreed Delivery Date (UTC)");

				AddStatusFilter(filters,
					new DeadlineTypeFilter(),
					ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|DeadlineType", "Deadline Type"));

				if (GetType() != typeof(BMFilterRuleFilterBusinessObject))
				{
					// this filter is not allowed for transfer rules
					filters.AddFilter(new DedicatedBufferFilter(ProcessHeader.ModuleFilterConstants.DedicatedBuffer, ModuleIDs.BMComponent, () => new BMComponentCollection(Factory)));
				}
			}

			var plannedDurationFilter = new ModuleDurationFilter(
			   ProcessHeader.ModuleFilterConstants.PlannedDuration,
			   Factory,
			   ProcessHeaderSchema.FH_PlannedDurationInMinutes);
			plannedDurationFilter.Category = NumbersAndReferencesCategory;
			plannedDurationFilter.MaxDurationMinutes = 100000;
			plannedDurationFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|PlannedDuration", "Planned Duration (hours)");
			filters.AddFilter(plannedDurationFilter);

			filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.ReleaseGroup, ModuleIDs.GlbGroup, ProcessHeaderSchema.FH_GG_ReleaseGroup, () => ReleaseGroups(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ReleaseGroup", "Release Group");
			filters.AddFilter(new CurrentComponentFilter(ProcessHeader.ModuleFilterConstants.CurrentComponent, ModuleIDs.BMComponent, () => new BMComponentCollection(Factory)));

			AddNumericFilters(filters);
			AddTagsFilters(this, Factory, filters, parameters);

			filters.AddFilter(new ParentJobModuleFilter(ProcessHeader.ModuleFilterConstants.ParentJob, ProcessHeaderSchema.FH_ParentId, Factory));
			filters.AddFilter(new ParentWorkflowsFilter(ProcessHeader.ModuleFilterConstants.ParentWorkflows, () => new ProcessHeaderCollection(Factory)));
			filters.AddFilter(new PrerequisiteWorkflowsFilter(ProcessHeader.ModuleFilterConstants.PrerequisiteWorkflows, () => new ProcessHeaderCollection(Factory)));
			filters.AddFilter(new DependentWorkflowsFilter(ProcessHeader.ModuleFilterConstants.DependentWorkflows, () => new ProcessHeaderCollection(Factory)));
			filters.AddFilter(new WorkflowsModuleFilter(ProcessHeader.ModuleFilterConstants.WorkflowsForJob, ProcessHeaderSchema.FH_ParentId, ProcessHeaderSchema.FH_ParentId, () => new ProcessHeaderCollection(Factory), typeof(ProcessHeader)));
			filters.AddFilter(new ShownOnDiagramFilter(ProcessHeader.ModuleFilterConstants.ShownOnNetworkDiagram, ModuleIDs.NetworkDiagram, () => new DiagramShapeCollection(Factory)));
			filters.AddFilter(new JobLevelWorkflowFilter(ProcessHeader.ModuleFilterConstants.JobLevelWorkflow, new ProcessJobHeaderCollection(Factory)));
			filters.AddFilter(new SequencedWorkflowsFilter());
		}

		static void AddTextFilterWithOnlyExactAndNotEqualComparisonOperators(ModuleFilterCollection filters, ZString description, SchemaStringColumn filterColumn, GetList listDelegate, MultilingualString multilingualDescription)
		{
			var filter = filters.AddTextFilter(description, filterColumn, listDelegate);
			filter.MultilingualDescription = multilingualDescription;
			filter.ComparisonOperator_List.Clear();
			filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			filter.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
		}

		static CodeDescriptionPairList GetWorkflowTypeList(BusinessObjectFactory factory) => factory.GetCachedValue("IWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>());

		#region Tasks

		static void AddTaskFilters(BusinessObjectFactory factory, ModuleFilterCollection filters)
		{
			filters.AddFilter(new AssignedResourceOnCurrentTaskFilter(ProcessHeader.ModuleFilterConstants.ResourceAssignedToStartableTask, ModuleIDs.GlbStaff, new GlbStaffCollection(factory), TasksFilterCategory));
			filters.AddFilter(new AssignedResourceFilter(ProcessHeader.ModuleFilterConstants.ResourceAssignedToAnyTask, ModuleIDs.GlbStaff, new GlbStaffCollection(factory), TasksFilterCategory));

			var tasksFilter = new TasksModuleFilter("Tasks", ProcessHeaderSchema.PK, ProcessTasksSchema.P9_FH_ProcessHeader, new ProcessTaskCollection(factory), typeof(ProcessHeader));
			tasksFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|Tasks", "Tasks");
			tasksFilter.Category = TasksFilterCategory;
			filters.AddFilter(tasksFilter);

			var componentChangeLogsFilter = new ComponentChangeLogsFilter(factory);
			componentChangeLogsFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ComponentChangeLogs", "Component Change Logs");
			filters.AddFilter(componentChangeLogsFilter);

			filters.AddFilter(new CapabilityFilter("Capability Required on Any Task", ModuleIDs.GlbCapability, new GlbCapabilityCollection(factory), TasksFilterCategory));
		}

		#endregion

		#region Numeric Filters

		static void AddNumericFilters(ModuleFilterCollection filters)
		{
			var bufferZoneFilter = filters.AddNumberRangeFilter(ProcessHeader.ModuleFilterConstants.BufferZone, GetBufferZoneQuery);
			bufferZoneFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|BufferZone", "Buffer Zone");
			bufferZoneFilter.Decimals = 0;
			bufferZoneFilter.MaxValue = BMConstants.NumberOfZones;
			bufferZoneFilter.MinValue = 0;
		}

		#endregion

		#region Date Acceptability

		static ZQuery GetDateAcceptabilityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			var workflowInheritsFromParentQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			workflowInheritsFromParentQuery.AddToFilter(ProcessHeaderSchema.FH_DateAcceptability, ZString.Empty);

			var jobSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			jobSubQuery.AddToFilter(ProcessHeaderSchema.FH_DateAcceptability, comparisonOperator, value);
			workflowInheritsFromParentQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobSubQuery, JoinCondition.And);

			var processHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			processHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_DateAcceptability, comparisonOperator, value);

			workflowInheritsFromParentQuery.AddAsUnionQuery(processHeaderQuery);

			query.AddSubQuery(workflowInheritsFromParentQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Buffer Zone

		static ZQuery GetBufferZoneQuery(INumericZType value1, INumericZType value2)
		{
			var zoneFrom = (ZInt)(ZDecimal)value1;
			var zoneTo = (ZInt)(ZDecimal)value2;

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@UtcNow", ZDateTime.UtcNow, Schema.GenericDateTimeColumn),
				ZSqlParameter.New("@ZoneFrom", zoneFrom, Schema.GenericIntSchemaColumn),
				ZSqlParameter.New("@ZoneTo", zoneTo, Schema.GenericIntSchemaColumn),
			};

			query.AddFilterAndZSQLParameterCollection(@"
FH_PK IN
(
	SELECT FH_PK
	FROM dbo.ProcessHeader
	JOIN dbo.BMComponent on FH_FC_CurrentComponent = FC_PK
	CROSS APPLY dbo.GetBufferZone
	(
		FH_PK,
		FC_GB_AgingBranch,
		FC_GE_AgingDepartment,
		@UtcNow
	) Zone
	WHERE 1=1
		AND FC_Type = 'BUF'
		AND Zone.BufferZone between @ZoneFrom AND @ZoneTo
)
", parameters);

			return query;
		}

		#endregion

		#region Status Filters

		static void AddStatusFilters(ModuleFilterCollection filters, ProcessHeaderFilterBusinessObject filterBusinessObject)
		{
			AddActiveStatusFilterWithoutEnforcingDefault(filters);

			AddWorkflowStatusFilter(filters);

			AddStatusFilter(filters,
				new ModuleTextFilter(ProcessHeader.ModuleFilterConstants.PrerequisiteStatus, GetWorkflowPrerequisitesQuery, () => new WorkflowPrerequisiteStatusList()),
				ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|PrerequisiteStatus", "Prerequisite Status"));

			AddStatusFilter(filters, new QueueStatusFilter(), ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|QueueStatus", "Queue Status"));

			AddStatusFilter(filters,
				new ModuleTextFilter(ProcessHeader.ModuleFilterConstants.ConstraintStatus, GetConstraintStatusQuery, () => new ConstraintStatusList()),
				ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ConstraintStatus", "Constraint Status"));

			filters.AddFilter(new ProcessHeaderTemplateTextFilter(filterBusinessObject));
			filters.AddFilter(new QualityIterationFilter());
		}

		static void AddActiveStatusFilterWithoutEnforcingDefault(ModuleFilterCollection filters)
		{
			CodeDescriptionPairList GetOptions()
			{
				var list = new CodeDescriptionPairList();
				list.AddPair((NoResString)"Active", StatusActive);
				list.AddPair((NoResString)"Inactive", StatusInactive);
				list.AddPair((NoResString)"All", StatusAll);
				return list;
			}

			ZQuery GetQuery(ZString status)
			{
				status = status.Trim();

				if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
				{
					return new ZQuery(ProcessHeaderSchema.FH_IsActive, true);
				}
				else if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
				{
					return new ZQuery(ProcessHeaderSchema.FH_IsActive, false);
				}

				return new ZQuery();
			}

			AddStatusFilter(filters,
				new ModuleTextFilter(ProcessHeader.ModuleFilterConstants.ActiveStatus, GetQuery, GetOptions),
				ResString.GetMultilingualString("626cf270-8eba-4703-8eef-36ed0e389f4c", "Active Status"));
		}

		static void AddWorkflowStatusFilter(ModuleFilterCollection filters)
		{
			// Since we split CLS into CLS and COP, we have CLS look at both of these for the sake of backwards compatibility
			WorkflowStatusList GetWorkflowStatusList()
			{
				var list = new WorkflowStatusList();
				list.RemoveCode(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites);
				return list;
			}

			ZQuery GetQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(value, WorkflowStatusList.Codes.Closed))
				{
					if (comparisonOperator.In(SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith))
					{
						return new ZQuery(ProcessHeaderSchema.FH_Status, SQLComparisonOperator.NotEqual, new[] { WorkflowStatusList.Codes.Closed, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites });
					}

					return new ZQuery(ProcessHeaderSchema.FH_Status, comparisonOperator, new[] { WorkflowStatusList.Codes.Closed, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites });
				}
				else
				{
					return new ZQuery(ProcessHeaderSchema.FH_Status, comparisonOperator, value);
				}
			}

			AddStatusFilter(filters,
				new ModuleTextFilter(ProcessHeader.ModuleFilterConstants.WorkflowStatus, GetQuery, GetWorkflowStatusList),
				ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|WorkflowStatus", "Workflow Status"));
		}

		static void AddStatusFilter(ModuleFilterCollection filters, ModuleFilter filter, MultilingualString multilingualDescription)
		{
			filter.MultilingualDescription = multilingualDescription;
			filter.Category = FilterCategories.StatusAndFlags;
			filters.AddFilter(filter);
		}

		#endregion

		#region ApprovedScheduleFilters

		static void AddApprovedScheduleFilters(ModuleFilterCollection filters)
		{
			var category = new FilterCategory(ResString.GetMultilingualString("ff0358a6-2163-4ec6-9fb3-2580200094e1", "Approved Schedule"));

			var startTimeFilter = ApprovedWorkflowScheduledTimeFilter.GetScheduledStartTimeFilter();
			var finishTimeFilter = ApprovedWorkflowScheduledTimeFilter.GetScheduledFinishTimeFilter();
			var scheduleTypeFilter = new ApprovedWorkflowScheduleTypeFilter();

			startTimeFilter.Category = category;
			finishTimeFilter.Category = category;
			scheduleTypeFilter.Category = category;

			filters.AddFilter(startTimeFilter);
			filters.AddFilter(finishTimeFilter);
			filters.AddFilter(scheduleTypeFilter);
		}

		#endregion

		#region Tag Filters

		static void AddTagsFilters(ProcessHeaderFilterBusinessObject filterBusinessObject, BusinessObjectFactory factory, ModuleFilterCollection filters, ZSqlParameterCollection parameters)
		{
			filters.AddAppliedToSubCollectionFilter(ProcessHeader.ModuleFilterConstants.TagTaskMagnitude, TagsFilterCategory, ModuleIDs.BMTagMagnitude, TagQueryProvider.GetTagTaskMagnitudeFilter, new TagMagnitudeCollection(factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TagTaskMagnitude", "Task Tag");
			filters.AddAppliedToSubCollectionFilter(ProcessHeader.ModuleFilterConstants.TagTaskDefinitionCode, TagsFilterCategory, ModuleIDs.BMTagDefinition, TagQueryProvider.GetTagTaskDefinitionCodeFilter, new TagDefinitionCollection(factory))
				.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TagTaskDefinitionCode", "Task Tag Group Code");

			var tagDefinitionFilter = filters.AddAppliedToSubCollectionFilterWithInherited(ProcessHeader.ModuleFilterConstants.TagDefinitionCode, TagsFilterCategory, ModuleIDs.BMTagDefinition, queryDelegate: GetTagDefinitionCodeQuery, shouldReevaluateQuery: false, new TagDefinitionCollection(factory), parameters, factory);
			tagDefinitionFilter.SubGroup = new TagDefinitionCodeSubGroup(filterBusinessObject, tagDefinitionFilter.Description);
			tagDefinitionFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TagDefinitionCode", "Tag Group Code");

			var tagMagnitudeFilter = filters.AddAppliedToSubCollectionFilterWithInherited(ProcessHeader.ModuleFilterConstants.TagMagnitude, TagsFilterCategory, ModuleIDs.BMTagMagnitude, TagQueryProvider.GetTagMagnitudeFilter, true, new TagMagnitudeCollection(factory), parameters, factory);
			tagMagnitudeFilter.SubGroup = new TagMagnitudeSubGroup(filterBusinessObject, tagMagnitudeFilter.Description);
			tagMagnitudeFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TagMagnitude", "Tag");
		}

		#endregion

		#region Filter Categories

		public static FilterCategory TasksFilterCategory
		{
			get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("2ce10bc0-e3d0-4842-bd28-f1c384f2153f", "Tasks")); }
		}

		public static FilterCategory TagsFilterCategory
		{
			get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("ef04d446-9552-4e7f-9be5-0900cb701874", "Tags")); }
		}

		public static FilterCategory NumbersAndReferencesCategory
		{
			get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("fe03082a-65a8-455b-b71a-979d21d9c3ae", "Numbers and References")); }
		}

		#endregion

		#region Queries

		static ZQuery GetCriticalHandoverQuery(ZBool value)
		{
			return new ZQuery(ProcessHeaderSchema.FH_IsCriticalHandover, value);
		}

		static ZQuery GetAllowTaskAutoAssignmentQuery(ZBool value)
		{
			return new ZQuery(ProcessHeaderSchema.FH_AllowTaskAutoAssignment, value);
		}

		static ZQuery GetIsStandbyTaskQuery(ZBool value)
		{
			return new ZQuery(ProcessHeaderSchema.FH_IsStandby, value);
		}

		static ZQuery GetIsApprovedQuery(ZBool value)
		{
			return new ZQuery(ProcessHeaderSchema.FH_IsApproved, value);
		}

		static ZQuery GetTagDefinitionCodeQuery(ZGuid pk, bool notIn, bool includeInherited, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
		{
			return TagQueryProvider.GetTagDefinitionCodeFilter(pk, notIn, includeInherited, new ZSqlParameterCollection());
		}

		static ZQuery GetEffectiveBufferDurationRangeQuery(ZDecimal minValue, ZDecimal maxValue, ZString scope)
		{
			var minValueInt = (int)minValue;
			var maxValueInt = (int)maxValue;

			var finalQuery = new ZDBOnlyQuery(typeof(ProcessHeader));

			SQLComparisonOperator op = SQLComparisonOperator.Equal;

			if (scope == ModuleDurationFilter.SearchTexts.GreaterThanOrEqualTo.ToString())
			{
				op = SQLComparisonOperator.GreaterThanOrEqualTo;
				BuildQueryWithOperator(finalQuery, op, minValueInt);
			}
			else if (scope == ModuleDurationFilter.SearchTexts.LessThanOrEqualTo.ToString())
			{
				op = SQLComparisonOperator.LessThanOrEqualTo;
				BuildQueryWithOperator(finalQuery, op, minValueInt);
			}
			else if (scope == ModuleDurationFilter.SearchTexts.EqualTo.ToString())
			{
				op = SQLComparisonOperator.Equal;
				BuildQueryWithOperator(finalQuery, op, minValueInt);
			}
			else
			{
				var case1 = new ZDBOnlyQuery(typeof(ProcessHeader));
				case1.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.NotEqual, null);
				case1.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);
				var bmtSubQuery = new ZDBOnlySubQuery(typeof(BMBufferTimespan), BMBufferTimespanSchema.PK);
				bmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, SQLComparisonOperator.GreaterThanOrEqualTo, minValueInt);
				bmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, SQLComparisonOperator.LessThanOrEqualTo, maxValueInt);
				case1.AddSubQuery(ProcessHeaderSchema.FH_BMT_BufferTimespan, bmtSubQuery, JoinCondition.And);

				var case2 = new ZDBOnlyQuery(typeof(ProcessHeader));
				case2.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
				var parentHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				var parentBmtSubQuery = new ZDBOnlySubQuery(typeof(BMBufferTimespan), BMBufferTimespanSchema.PK);
				parentBmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, SQLComparisonOperator.GreaterThanOrEqualTo, minValueInt);
				parentBmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, SQLComparisonOperator.LessThanOrEqualTo, maxValueInt);
				parentHeaderSubQuery.AddSubQuery(ProcessHeaderSchema.FH_BMT_BufferTimespan, parentBmtSubQuery, JoinCondition.And);
				case2.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, parentHeaderSubQuery, JoinCondition.And);

				var case3 = new ZDBOnlyQuery(typeof(ProcessHeader));
				case3.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
				case3.AddToFilter(ProcessHeaderSchema.FH_FC_DedicatedBuffer, SQLComparisonOperator.NotEqual, null);
				var nullBufferParentSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				nullBufferParentSubQuery.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
				case3.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, nullBufferParentSubQuery, JoinCondition.And);
				var bufferComponentSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.PK);
				bufferComponentSubQuery.AddToFilter(BMComponentSchema.FC_BufferTimespanInMinutes, SQLComparisonOperator.GreaterThanOrEqualTo, minValueInt);
				bufferComponentSubQuery.AddToFilter(BMComponentSchema.FC_BufferTimespanInMinutes, SQLComparisonOperator.LessThanOrEqualTo, maxValueInt);
				case3.AddSubQuery(ProcessHeaderSchema.FH_FC_DedicatedBuffer, bufferComponentSubQuery, JoinCondition.And);

				finalQuery.AddToFilter(case1, JoinCondition.And);
				finalQuery.AddToFilter(case2, JoinCondition.Or);
				finalQuery.AddToFilter(case3, JoinCondition.Or);
			}

			return finalQuery;
		}

		static void BuildQueryWithOperator(ZDBOnlyQuery finalQuery, SQLComparisonOperator op, int valueInt)
		{
			var case1 = new ZDBOnlyQuery(typeof(ProcessHeader));
			case1.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.NotEqual, null);
			var bmtSubQuery = new ZDBOnlySubQuery(typeof(BMBufferTimespan), BMBufferTimespanSchema.PK);
			bmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, op, valueInt);
			case1.AddSubQuery(ProcessHeaderSchema.FH_BMT_BufferTimespan, bmtSubQuery, JoinCondition.And);

			var case2 = new ZDBOnlyQuery(typeof(ProcessHeader));
			case2.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
			case2.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);
			var parentHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			var parentBmtSubQuery = new ZDBOnlySubQuery(typeof(BMBufferTimespan), BMBufferTimespanSchema.PK);
			parentBmtSubQuery.AddToFilter(BMBufferTimespanSchema.BMT_BufferTimespanInMinutes, op, valueInt);
			parentHeaderSubQuery.AddSubQuery(ProcessHeaderSchema.FH_BMT_BufferTimespan, parentBmtSubQuery, JoinCondition.And);
			case2.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, parentHeaderSubQuery, JoinCondition.And);

			var case3 = new ZDBOnlyQuery(typeof(ProcessHeader));
			case3.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
			case3.AddToFilter(ProcessHeaderSchema.FH_FC_DedicatedBuffer, SQLComparisonOperator.NotEqual, null);
			var nullBufferParentSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			nullBufferParentSubQuery.AddToFilter(ProcessHeaderSchema.FH_BMT_BufferTimespan, SQLComparisonOperator.Equal, null);
			case3.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, nullBufferParentSubQuery, JoinCondition.And);
			var bufferComponentSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.PK);
			bufferComponentSubQuery.AddToFilter(BMComponentSchema.FC_BufferTimespanInMinutes, op, valueInt);
			case3.AddSubQuery(ProcessHeaderSchema.FH_FC_DedicatedBuffer, bufferComponentSubQuery, JoinCondition.And);

			var combinedQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
			combinedQuery.AddToFilter(case1, JoinCondition.And);
			combinedQuery.AddToFilter(case2, JoinCondition.Or);
			combinedQuery.AddToFilter(case3, JoinCondition.Or);

			finalQuery.AddToFilter(combinedQuery, JoinCondition.And);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList TaskStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ProcessTaskStatusCodeList>();
		}

		public static GlbGroupCollection ReleaseGroups(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ProcessHeaderFilterBusinessObject|ReleaseGroups", () => new GlbGroupCollection(factory));
		}

		#endregion

		#region Workflow Prerequisites

		static ZQuery GetWorkflowPrerequisitesQuery(ZString value)
		{
			var query = new ZQuery();

			switch (value)
			{
				case WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites:
					query.AddToFilter(ProcessHeaderSchema.FH_Status, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites);
					break;
				case WorkflowPrerequisiteStatusList.Codes.Blocked:
					var statusIsBlocked = new ZQuery(ProcessHeaderSchema.FH_Status, new[] { WorkflowStatusList.Codes.Blocked, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites });
					var noRelevantStaggeredStart = new ZQuery(ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow);
					noRelevantStaggeredStart.AddToFilter(JoinCondition.Or, ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry, SQLComparisonOperator.Equal, ZDateTime.Empty);
					query.AddToFilter(new ZQuery(statusIsBlocked, JoinCondition.And, noRelevantStaggeredStart));
					break;
				case WorkflowPrerequisiteStatusList.Codes.ClearedToStart:
					query.AddToFilter(new ZQuery(ProcessHeaderSchema.FH_Status, new[] { WorkflowStatusList.Codes.Open, WorkflowStatusList.Codes.Closed }));
					break;
				case WorkflowPrerequisiteStatusList.Codes.ClearedToRelease:
					var statusIsntBlocked = new ZQuery(ProcessHeaderSchema.FH_Status, new[] { WorkflowStatusList.Codes.Open, WorkflowStatusList.Codes.Closed });
					var staggeredStartIsRelevant = new ZQuery(ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry, SQLComparisonOperator.LessThan, ZDateTime.UtcNow);
					staggeredStartIsRelevant.AddToFilter(ProcessHeaderSchema.FH_StaggeredReleaseDelayExpiry, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					query.AddToFilter(new ZQuery(statusIsntBlocked, JoinCondition.Or, staggeredStartIsRelevant));
					break;
			}

			return query;
		}

		#endregion

		#region Constraint Status

		static ZQuery GetConstraintStatusQuery(ZString value)
		{
			var constraintStatus = ConstrainedModeHelper.GetConstraintStatusFromString(value);
			return ConstrainedModeHelper.GetConstraintStatusQuery(constraintStatus);
		}

		#endregion

		#region Job Property Filter

		static ZQuery GetJobTextPropertyQuery(SQLComparisonOperator comparisonOperator, ZString workflowTypeCode, ZString jobPropertyName, ZString jobPropertyValue)
		{
			WorkflowDescriptor descriptor;
			if (WorkflowDescriptors.Instance.TryGetValue(workflowTypeCode, out descriptor))
			{
				var schemaColumn = JobTextPropertyFilter.SchemaColumnFromPropertyName(jobPropertyName);
				if (schemaColumn != null)
				{
					if (schemaColumn.HasMaxLength && jobPropertyValue.Length > schemaColumn.MaxLength)
					{
						jobPropertyValue = jobPropertyValue.Substring(0, schemaColumn.MaxLength);
					}
					var result = new ZDBOnlyQuery(typeof(ProcessHeader));
					var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
					var jobSubQuery = ProcessTaskTypeDecider.GetInstance().GetParentJobSubQuery(descriptor, ProcessHeaderSchema.FH_ParentId, notIn);

					jobSubQuery.AddToFilter(schemaColumn, comparisonOperator, jobPropertyValue);
					result.AddSubQuery(jobSubQuery, JoinCondition.And);

					return result;
				}
			}

			return ZQuery.NoResultQuery;
		}

		#endregion

		#endregion
	}
}
