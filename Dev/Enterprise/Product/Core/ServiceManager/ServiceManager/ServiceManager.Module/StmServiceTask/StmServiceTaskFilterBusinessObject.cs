using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Module
{
	public class StmServiceTaskFilterBusinessObject : FilterStripBusinessObject
	{
		public StmServiceTaskFilterBusinessObject()
		{
			QueryObjectType = typeof(StmServiceTask);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddOrganizationsFilters(filters);
			AddDateFilters(filters);
			AddCustomTextFilters(filters);
			AddHostFilters(filters);
			DisableFiltersMatchForAuditFilters(filters);
			return filters;
		}

		static void DisableFiltersMatchForAuditFilters(ModuleFilterCollection filters)
		{
			filters.FilterAdded += (sender, args) =>
			{
				var filter = args.AddedFilter as ModuleNkFilter;

				if (filter != null && (filter.Description == FilterDescriptions.CreatingUser || filter.Description == FilterDescriptions.LastEditUser))
				{
					filter.SupportsFiltersMatchComparisonOperator = false;
				}
			};
		}

		#region Text Filters
		void AddTextFilters(ModuleFilterCollection filters)
		{
			var codeFilter = filters.AddTextFilter("Code", StmServiceTaskSchema.SST_ServiceTaskCode);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Code", "Code");
			codeFilter.GroupOrCategory = FilterOrCategory.None;
			codeFilter.IsGroupOrCategoryReadOnly = false;
			codeFilter.OrCategory = FilterOrCategory.None;
			codeFilter.IsOrCategoryReadOnly = false;
			codeFilter.MaxLength = StmServiceTaskSchema.SST_ServiceTaskCode.MaxLength;
		}
		#endregion

		#region Custom Filters
		void AddCustomTextFilters(ModuleFilterCollection filters)
		{
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.RegisteredOnHosts, GetRegisteredOnHostsQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Registered On Hosts", "Registered On Hosts"), StmServiceTaskStatusColumnProvider.RegisteredOnHosts);
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.BindingTypes, GetBindingTypesQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Binding Types", "Binding Types"), StmServiceTaskStatusColumnProvider.BindingTypes);
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.ProcessId, GetProcessIdQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Process #", "Process #"), StmServiceTaskStatusColumnProvider.ProcessId);
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.SecondsInQueue, GetSecondsInQueueQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Seconds In Queue", "Seconds In Queue"), StmServiceTaskStatusColumnProvider.SecondsInQueue);
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.SecondsRunning, GetSecondsRunningQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Seconds Running", "Seconds Running"), StmServiceTaskStatusColumnProvider.SecondsRunning);
			AddTextFilter(filters, StmServiceTaskCustomFilterNameConstants.PlaceInQueue, GetPlaceInQueueQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Place In Queue", "Place In Queue"), StmServiceTaskStatusColumnProvider.PlaceInQueue);
			AddIntFilter(filters, StmServiceTaskCustomFilterNameConstants.BindingsCount, GetBindingsCountQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Bindings Count", "Bindings Count"));
			AddIntFilter(filters, StmServiceTaskCustomFilterNameConstants.RunningCount, GetRunningCountQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Running Count", "Running Count"));
			AddIntFilter(filters, StmServiceTaskCustomFilterNameConstants.ErrorCountLast24Hours, GetErrorCountLast24HoursQuery, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Error Count Last 24 Hours", "Error Count Last 24 Hours"));

			AddCategoryFilter(filters);
			AddDescriptionFilter(filters);
		}

		static void AddTextFilter(ModuleFilterCollection filters, ZString description, GetTextQueryWithOperator getTextQuery, MultilingualString multilingualDescription, SchemaStringColumn column)
		{
			filters.AddCustomFilter(new ModuleTextFilter(description, getTextQuery)
			{
				MultilingualDescription = multilingualDescription,
				GroupOrCategory = FilterOrCategory.None,
				IsGroupOrCategoryReadOnly = false,
				OrCategory = FilterOrCategory.None,
				IsOrCategoryReadOnly = false,
				MaxLength = column.MaxLength
			});
		}

		static void AddIntFilter(ModuleFilterCollection filters, ZString description, GetNumberRangeQuery getNumberRangeQuery, MultilingualString multilingualDescription)
		{
			filters.AddCustomFilter(new ModuleNumberRangeFilter(description, getNumberRangeQuery)
			{
				MinValue = 0,
				MaxValue = Int32.MaxValue,
				Decimals = 0,
				Category = FilterCategories.NumbersAndReferences,
				MultilingualDescription = multilingualDescription,
				GroupOrCategory = FilterOrCategory.None,
				IsGroupOrCategoryReadOnly = false,
				OrCategory = FilterOrCategory.None,
				IsOrCategoryReadOnly = false
			});
		}

		void AddCategoryFilter(ModuleFilterCollection filters)
		{
			var categoryFilter = filters.AddTextFilter("ProductArea", GetCategoryQuery, Categories);
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Product Area", "Product Area");
			categoryFilter.GroupOrCategory = FilterOrCategory.None;
			categoryFilter.IsGroupOrCategoryReadOnly = false;
			categoryFilter.OrCategory = FilterOrCategory.None;
			categoryFilter.IsOrCategoryReadOnly = false;
			categoryFilter.MaxLength = StmServiceTaskMetadataColumnProvider.Category.MaxLength;
		}

		void AddDescriptionFilter(ModuleFilterCollection filters)
		{
			filters.AddFiltersForTranslatableText(
				StmServiceTaskCustomFilterNameConstants.Description,
				StmServiceTaskMetadataColumnProvider.Description,
				new StmScheduleTaskDescriptionCaptionSource(),
				ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Description", "Description"));
			filters["Description"].GroupOrCategory = FilterOrCategory.None;
			filters["Description"].IsGroupOrCategoryReadOnly = false;
			filters["Description"].OrCategory = FilterOrCategory.None;
			filters["Description"].IsOrCategoryReadOnly = false;
			filters["Description"].MaxLength = StmServiceTaskMetadataColumnProvider.Description.MaxLength;
		}

		ZQuery GetPlaceInQueueQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.PlaceInQueue, comparisonOperator, value);
		}

		ZQuery GetSecondsRunningQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.SecondsRunning, comparisonOperator, value);
		}

		ZQuery GetSecondsInQueueQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.SecondsInQueue, comparisonOperator, value);
		}

		ZQuery GetBindingTypesQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.BindingTypes, comparisonOperator, value);
		}

		ZQuery GetRegisteredOnHostsQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.RegisteredOnHosts, comparisonOperator, value);
		}

		ZQuery GetProcessIdQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskStatusColumnProvider.ProcessId, comparisonOperator, value);
		}

		ZQuery GetBindingsCountQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), StmServiceTaskStatusColumnProvider.BindingsCount, value1, value2, ZCalcEditPropertyType.Int);
		}

		ZQuery GetRunningCountQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), StmServiceTaskStatusColumnProvider.RunningCount, value1, value2, ZCalcEditPropertyType.Int);
		}

		ZQuery GetErrorCountLast24HoursQuery(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), StmServiceTaskStatusColumnProvider.ErrorCountLast24Hours, value1, value2, ZCalcEditPropertyType.Int);
		}

		ZQuery GetMutuallyExclusiveGroup(ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskMetadataColumnProvider.MutuallyExclusiveGroup, value);
		}

		ZQuery GetCategoryQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter(StmServiceTaskMetadataColumnProvider.Category, comparisonOperator, value);
		}

		#endregion

		#region Status and Flags Filters
		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var activeStatusFilter = filters.AddTextFilter("Active Status", GetActiveStatusQuery, CancelledStatusList);
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			activeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|ActiveStatus", "Active Status");
			activeStatusFilter.GroupOrCategory = FilterOrCategory.None;
			activeStatusFilter.IsGroupOrCategoryReadOnly = false;
			activeStatusFilter.OrCategory = FilterOrCategory.None;
			activeStatusFilter.IsOrCategoryReadOnly = false;

			var statusFilter = filters.AddTextFilter("Status", StmServiceTaskStatusColumnProvider.StatusString, StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Status", "Status");
			statusFilter.GroupOrCategory = FilterOrCategory.None;
			statusFilter.IsGroupOrCategoryReadOnly = false;
			statusFilter.OrCategory = FilterOrCategory.None;
			statusFilter.IsOrCategoryReadOnly = false;
			statusFilter.MaxLength = StmServiceTaskStatusColumnProvider.StatusString.MaxLength;

			var mutuallyExclusiveGroupFilter = filters.AddTextFilter("Mutually Exclusive Group", GetMutuallyExclusiveGroup, MutuallyExclusiveGroupList);
			mutuallyExclusiveGroupFilter.Category = FilterCategories.StatusAndFlags;
			mutuallyExclusiveGroupFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|MutuallyExclusiveGroup", "Mutually Exclusive Group");
			mutuallyExclusiveGroupFilter.GroupOrCategory = FilterOrCategory.None;
			mutuallyExclusiveGroupFilter.IsGroupOrCategoryReadOnly = false;
			mutuallyExclusiveGroupFilter.OrCategory = FilterOrCategory.None;
			mutuallyExclusiveGroupFilter.IsOrCategoryReadOnly = false;
		}

		CodeDescriptionPairList fStatusList;
		CodeDescriptionPairList StatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new CodeDescriptionPairList();
					fStatusList.AddPair(ServiceManagerHelper.StatusBlocked, "Blocked");
					fStatusList.AddPair(ServiceManagerHelper.StatusRunning, ServiceManagerHelper.StatusRunning);
					fStatusList.AddPair(ServiceManagerHelper.StatusLastRunFailed, ServiceManagerHelper.StatusLastRunFailed);
					fStatusList.AddPair(ServiceManagerHelper.StatusIdle, ServiceManagerHelper.StatusIdle);
					fStatusList.AddPair(ServiceManagerHelper.StatusUnknown, "Unknown");
					fStatusList.Sort();
				}
				return fStatusList;
			}
		}

		CodeDescriptionPairList mutuallyExclusiveGroupList;
		CodeDescriptionPairList MutuallyExclusiveGroupList
		{
			get
			{
				if (mutuallyExclusiveGroupList == null)
				{
					mutuallyExclusiveGroupList = new CodeDescriptionPairList();

					foreach (var group in Enum.GetNames(typeof(MutuallyExclusiveServiceTaskGroups)))
					{
						if (group == nameof(MutuallyExclusiveServiceTaskGroups.NoGroup))
						{
							mutuallyExclusiveGroupList.AddPair(group, $"Show non-grouped only");
						}
						else
						{
							mutuallyExclusiveGroupList.AddPair(group, $"Show '{group}' only");
						}
					}

					mutuallyExclusiveGroupList.Sort();
				}
				return mutuallyExclusiveGroupList;
			}
		}

		ZQuery GetActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();
			status = status.Trim();

			if (status.IsEmpty)
			{
				return query;
			}

			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(StmServiceTaskSchema.SST_Active, false);
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(StmServiceTaskSchema.SST_Active, true);
			}

			return query;
		}

		#endregion

		#region Organizations Filters
		void AddOrganizationsFilters(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, StmServiceTaskSchema.SST_GB_Branch, BindingLists.Branches);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Branch", "Branch");
			branchFilter.GroupOrCategory = FilterOrCategory.None;
			branchFilter.IsGroupOrCategoryReadOnly = false;
			branchFilter.OrCategory = FilterOrCategory.None;
			branchFilter.IsOrCategoryReadOnly = false;
			branchFilter.SupportsFiltersMatchComparisonOperator = false;
		}

		BindToLists BindingLists => BindToLists.GetCachedLists(Factory);
		#endregion

		#region Date Filters
		void AddDateFilters(ModuleFilterCollection filters)
		{
			AddDateFilter(filters, "Next Run Time",  StmServiceTaskStatusColumnProvider.NextRunTime, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Next Run Time", "Next Run Time"));
			AddDateFilter(filters, "Last Run Time", StmServiceTaskStatusColumnProvider.LastRunTime, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Last Run Time", "Last Run Time"));
			AddDateFilter(filters, "Last Error Time", StmServiceTaskStatusColumnProvider.LastErrorTime, ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Last Error Time", "Last Error Time"));
		}

		static void AddDateFilter(ModuleFilterCollection filters, ZString description, SchemaDateTimeColumn schemaDateTimeColumn, MultilingualString multilingualDescription)
		{
			ModuleFilter newFilter = filters.AddDateFilter(description, schemaDateTimeColumn, convertFromLocalToUTC: true);
			newFilter.Category = FilterCategories.Dates;
			newFilter.MultilingualDescription = multilingualDescription;
			newFilter.GroupOrCategory = FilterOrCategory.None;
			newFilter.IsGroupOrCategoryReadOnly = false;
			newFilter.OrCategory = FilterOrCategory.None;
			newFilter.IsOrCategoryReadOnly = false;
		}

		#endregion

		#region Host Filters
		protected virtual void AddHostFilters(ModuleFilterCollection filters)
		{
			var processHostFilter = filters.AddTextFilter("ProcessHost", StmServiceTaskStatusColumnProvider.RegisteredOnHosts, HostList);
			processHostFilter.Category = FilterCategories.TextSearch;
			processHostFilter.MultilingualDescription = ResString.GetMultilingualString("ServiceManager|ServiceTaskFilter|Process Host", "Process Host");
			processHostFilter.GroupOrCategory = FilterOrCategory.None;
			processHostFilter.IsGroupOrCategoryReadOnly = false;
			processHostFilter.OrCategory = FilterOrCategory.None;
			processHostFilter.IsOrCategoryReadOnly = false;
		}

		CodeDescriptionPairList hostList;
		CodeDescriptionPairList HostList
		{
			get
			{
				if (hostList == null)
				{
					hostList = new CodeDescriptionPairList();

					foreach (var hostName in Factory.Load<StmServiceHost>(new ZQuery()).Select(host => host.SH_HostName))
					{
						hostList.AddPair(hostName);
					}

					hostList.Sort();
				}

				return hostList;
			}
		}
		#endregion

		public CodeDescriptionPairList Categories
		{
			get
			{
				if (categories == null)
				{
					categories = new CodeDescriptionPairList();
					var isProductivityWiseModeEnabled = DataRegistry.Instance.ProductivityWiseModeEnabled;

					foreach (var category in ServiceTaskCategoryDescriptors.Get(x => !isProductivityWiseModeEnabled || x.IsShownInProductivityWiseMode))
					{
						categories.AddPair(category.Code, category.Description);
					}

					categories.Sort();
				}

				return categories;
			}
		}

		CodeDescriptionPairList categories;
	}
}
