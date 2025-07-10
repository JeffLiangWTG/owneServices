using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.ResString;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class IncidentManagementGroupFilterBusinessObject : FilterStripBusinessObject
	{
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowTasksHelper = new WorkflowFilterStripsHelper(typeof(IncidentManagementGroup), EDIJobInvoicingConsumerTypes.Incident.Code, Factory);
			workflowTasksHelper.SetShouldAddWorkflowCustomFieldsFilters(true, "ING");
			var processHeaderHelper = new EDIProcessHeaderFilterStripsHelper(typeof(IncidentManagementGroup), Factory);

			helpers.AddRange(new IFilterStripsHelper[]
			{
				workflowTasksHelper,
				processHeaderHelper
			});
			return helpers;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddWorkflowTasksFilters(filters);
			AddFlagsFilters(filters);
			AddRelatedObjectFilters(filters);
			AddMenuFilters(filters);
			AddParticipantsFilters(filters);
			return filters;
		}

		void AddParticipantsFilters(ModuleFilterCollection filters)
		{
			var category = FilterCategories.GetOrCreateFilterCategory((NoResString)"Participants");

			var relatedContactsFilter = new IncidentGroupEConversationParticipantsFilter<OrgContact>("Contact Participants", ModuleIDs.OrgContacts, IncidentManagementGroupSchema.PK, new OrgContactCollection(Factory), typeof(IncidentManagementGroup));
			relatedContactsFilter.MultilingualDescription = ResString.GetMultilingualString("57fc66e2-573e-464d-93fa-4dae03b4c43b", "Contact Participants");
			relatedContactsFilter.Category = category;
			filters.AddFilter(relatedContactsFilter);

			var relatedStaffFilter = new IncidentGroupEConversationParticipantsFilter<GlbStaff>("Staff Participants", ModuleIDs.GlbStaff, IncidentManagementGroupSchema.PK, new GlbStaffCollection(Factory), typeof(IncidentManagementGroup));
			relatedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("2d137337-9af1-4274-9350-618e3eb12595", "Staff Participants");
			relatedStaffFilter.Category = category;
			filters.AddFilter(relatedStaffFilter);

			var relatedGroupsFilter = new IncidentGroupEConversationParticipantsFilter<GlbGroup>("Group Participants", ModuleIDs.GlbGroup, IncidentManagementGroupSchema.PK, new GlbGroupCollection(Factory), typeof(IncidentManagementGroup));
			relatedGroupsFilter.MultilingualDescription = ResString.GetMultilingualString("1a1c6ed3-60a1-409d-baf9-3c49c9fa5504", "Group Participants");
			relatedGroupsFilter.Category = category;
			filters.AddFilter(relatedGroupsFilter);

			var relatedOrganisationsFilter = new IncidentGroupEConversationParticipantsFilter<OrgHeader>("Organization Participants", ModuleIDs.Organisation, IncidentManagementGroupSchema.PK, new OrgHeaderCollection(Factory), typeof(IncidentManagementGroup));
			relatedOrganisationsFilter.MultilingualDescription = ResString.GetMultilingualString("41d3a84c-0cfa-4982-892e-b8dde559b7ce", "Organization Participants");
			relatedOrganisationsFilter.Category = category;
			filters.AddFilter(relatedOrganisationsFilter);

			var emailFilter = filters.AddTextFilter("Has Email Participant", GetEmailParticipantQuery);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			emailFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			emailFilter.Category = category;
		}

		ZQuery GetEmailParticipantQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(IncidentManagementGroup));

			var subQueryParticipant = new ZDBOnlySubQuery(typeof(JobConversationParticipant), JobConversationParticipantSchema.JCP_JCC_Conversation,
				notIn: false);
			subQueryParticipant.AddToFilter(JobConversationParticipantSchema.JCP_EmailAddress, comparisonOperator, value);
			subQueryParticipant.AddToFilter(JobConversationParticipantSchema.JCP_ParticipantTableCode, string.Empty);

			var subQueryConversation = new ZDBOnlySubQuery(typeof(JobConversation), JobConversationSchema.JCC_ParentID);
			subQueryConversation.AddSubQuery(subQueryParticipant, JoinCondition.And);

			query.AddSubQuery(subQueryConversation, JoinCondition.And);
			return query;
		}

		#region Text Filters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Group Number", IncidentManagementGroupSchema.ING_IncidentGroupNumber, "ING");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", IncidentManagementGroupSchema.ING_Description);
			filters.AddTextFilter("Product", IncidentManagementGroupSchema.ING_Product, Lookups.ProductList);
			filters.AddTextFilter("Product Area", IncidentManagementGroupSchema.ING_ProductArea, ProductAreaList);
			filters.AddTextFilter("Country/Region", IncidentManagementGroupSchema.ING_RN_NKCountry, Country);
			filters.AddTextFilter("Group Type", IncidentManagementGroupSchema.ING_Type, Lookups.Types);
			filters.AddTextFilter("Stage", IncidentManagementGroupSchema.ING_Status, StageList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Criticality", IncidentManagementGroupSchema.ING_Priority, Lookups.CriticalityList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Urgency Code", IncidentManagementGroupSchema.ING_Urgency, Lookups.UrgencyList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Business Impact", IncidentManagementGroupSchema.ING_BusinessImpact, Lookups.BusinessImpactList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter("Service Outage Status", IncidentManagementGroupSchema.ING_ServiceOutage, Lookups.ServiceOutageList).Category = FilterCategories.StatusAndFlags;
		}

		CodeDescriptionPairList ProductAreaList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddRange(EDIDataRegistry.Instance.ProductAreas.Value);
				list.Sort();
				return list;
			}
		}

		RefCountryCollection country;
		RefCountryCollection Country => country ?? (country = new RefCountryCollection(Factory));

		CodeDescriptionPairList StageList
		{
			get
			{
				if (stageList == null)
				{
					stageList = new CodeDescriptionPairList();
					var allStages = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value.GetAllStages();
					allStages.Cast<IncidentGroupStatusConfiguration>().ForEach(x => stageList.AddPair(x.Code, x.DescriptionOnGroup));
				}
				return stageList;
			}
		}
		CodeDescriptionPairList stageList;

		#endregion

		#region Workflow Tasks Filters

		class TaskFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(IncidentManagementGroup));
				var taskQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
				taskQuery.AddToFilter(filter);
				result.AddSubQuery(taskQuery, JoinCondition.And);
				return result;
			}
		}

		void AddWorkflowTasksFilters(ModuleFilterCollection filters)
		{
			var standardWorkflowFilter = filters.FirstOrDefault(filter => filter.Category.Description.GetUnresolvedString() == "Workflow Tasks");
			var category = standardWorkflowFilter != null ? standardWorkflowFilter.Category : FilterCategories.GetOrCreateFilterCategory((NoResString)"Workflow Tasks");

			var subGroup = new TaskFilterSubGroup();

			var capabilityFilter = filters.AddGuidFilter("Capability (Current Task)", ModuleIDs.GlbCapability, GetTaskCapabilityQuery, CapabilityList);
			capabilityFilter.Category = category;
			capabilityFilter.SubGroup = subGroup;

			var taskStatusFilter = filters.AddTextFilter("Task Status (Current Task)", GetTaskStatusQuery, TaskStatusList);
			taskStatusFilter.Category = category;
			taskStatusFilter.SubGroup = subGroup;
		}

		ZQuery GetTaskCapabilityQuery(ZGuid capabilityPk)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var currentTaskOnlyQuery = GetCurrentTaskOnlyQuery(true);
			query.AddToFilter(currentTaskOnlyQuery);
			query.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ZString.Empty);
			query.AddToFilter(ProcessTasksSchema.P9_G4_RequiredCapability, capabilityPk);

			return query;
		}

		ZQuery GetCurrentTaskOnlyQuery(ZBool currentTaskOnly)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var p9_PK = ProcessTasksSchema.Constants.PK;

			if (currentTaskOnly)
			{
				var registry = ObjectFactory.Get<IBMSRegistry>();
				var functionHelper = ObjectFactory.Get<IBMSQLFunctionHelper>();
				var dbFunctionToRun = registry.BufferManagementEnabled ? functionHelper.GetCurrentTasks + "()" : "dbo.GetCurrentTasksNotInWorkflows()";
				var sql = FormattableString.Invariant($"{p9_PK} IN (SELECT {p9_PK} FROM {dbFunctionToRun})");
				query.AddFilterAndZSQLParameterCollection(sql, null);
			}
			return query;
		}

		GlbCapabilityCollection CapabilityList
		{
			get { return new GlbCapabilityCollection(Factory); }
		}

		ZQuery GetTaskStatusQuery(ZString status)
		{
			var query = new ZQuery();
			if (status == "NCM") // Not Complete
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Closed);
				query.AddToFilter(ProcessTasksSchema.P9_Status, SQLComparisonOperator.NotEqual, ProcessTaskStatusCodeList.Codes.Cancelled);
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, status);
			}
			return query;
		}

		CodeDescriptionPairList TaskStatusList
		{
			get
			{
				if (taskStatusList == null)
				{
					taskStatusList = new CodeDescriptionPairList();
					taskStatusList.AddRange(new ProcessTaskStatusCodeList());
					taskStatusList.AddPair("NCM", "Not Complete");
				}
				return taskStatusList;
			}
		}
		CodeDescriptionPairList taskStatusList;

		#endregion

		#region Flags Filters

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Has Auto-Reply", GetHasAutoReplyQuery, AutoReplyList).Category = FilterCategories.StatusAndFlags;
			filters.AddFlagsFilter("Has Flagged Incident", new string[] { "Yes" }, new GetFlagsQuery[] { GetHasCommunicationQuery }).Category = FilterCategories.StatusAndFlags;
			filters.AddFlagsFilter("Group Completed", new string[] { "Group Completed" }, new GetFlagsQuery[] { GetGroupCompleted }).Category = FilterCategories.StatusAndFlags;
		}

		CodeDescriptionPairList AutoReplyList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("DFT", (NoResString)"Draft");
				list.AddPair("PUB", (NoResString)"Published");
				list.AddPair("ENB", (NoResString)"Enabled");
				return list;
			}
		}

		ZQuery GetHasAutoReplyQuery(ZString autoreply)
		{
			var query = new ZDBOnlyQuery(typeof(IncidentManagementGroup));
			var messageSubQuery = new ZDBOnlySubQuery(typeof(IncidentManagementGroupMessage), IncidentManagementGroupMessageSchema.IGM_ING_Group);

			if (autoreply == "DFT")
			{
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_IsPublished, false);
			}
			else if (autoreply == "PUB")
			{
				query.AddToFilter(IncidentManagementGroupSchema.ING_IsAutoReply, false);
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_IsPublished, true);
			}
			else if (autoreply == "ENB")
			{
				query.AddToFilter(IncidentManagementGroupSchema.ING_IsAutoReply, true);
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_Type, IncidentManagementGroupMessageTypePairList.Codes.AutoReply);
				messageSubQuery.AddToFilter(IncidentManagementGroupMessageSchema.IGM_IsPublished, true);
			}

			query.AddSubQuery(messageSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetHasCommunicationQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(IncidentManagementGroup));
			var queryInNotIn = value ? "IN" : "NOT IN";

			string sqlQuery = $@"
ING_PK {queryInNotIn} (
	SELECT INL_ING_Group
	FROM
		dbo.IncidentManagementLink
		INNER JOIN dbo.IncidentMain on IM_PK = INL_IM_Incident
		INNER JOIN dbo.JobConversation on JCC_ParentID = IM_INC_Request
		INNER JOIN dbo.JobConversationMessage on JCC_PK = JCM_JCC_Conversation
		INNER JOIN dbo.JobConversationParticipant on JCM_JCP_Participant = JCP_PK
	WHERE 
		JCC_ParentTableCode = 'INC'
		AND JCM_JCP_Participant IS NOT NULL
		AND JCP_ParticipantTableCode = 'OC'
		AND JCM_PostedTimeUtc > ISNULL((
			SELECT TOP 1
				SL_PostedTimeUtc
			FROM
				dbo.StmALog
			WHERE
				SL_SE_NKEvent = 'MSC' 
				AND SL_IsCancelled = 'N' 
				AND SL_Reference like '%DES=Message unflagged on%'
				AND SL_Parent = INL_PK
			ORDER BY
				SL_PostedTimeUtc DESC
		), ING_SystemCreateTimeUtc)
)";

			query.AddFilterAndZSQLParameterCollection(sqlQuery, new ZSqlParameterCollection());
			return query;
		}

		ZQuery GetGroupCompleted(ZBool value)
		{
			var comparation = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			var query = new ZDBOnlyQuery(typeof(IncidentManagementGroup));

			foreach (var item in ClosedStages)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(IncidentManagementGroup), IncidentManagementGroupSchema.PK);
				subQuery.AddToFilter(IncidentManagementGroupSchema.ING_Status, comparation, item.Value);
				subQuery.AddToFilter(IncidentManagementGroupSchema.ING_Type, item.Key);

				query.AddSubQuery(subQuery, JoinCondition.Or);
			}

			return query;
		}

		Dictionary<ZString, IEnumerable<ZString>> ClosedStages
		{
			get
			{
				if (closedStages == null)
				{
					closedStages = new Dictionary<ZString, IEnumerable<ZString>>();

					var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
					foreach (var item in registryValue)
					{
						closedStages.Add(item.GroupType, item.IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().Where(i => i.GroupCompleted).Select(i => i.Code));
					}
				}

				return closedStages;
			}
		}
		Dictionary<ZString, IEnumerable<ZString>> closedStages;

		#endregion

		#region Related Object Filters

		void AddRelatedObjectFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Related Work Items", ModuleIDs.WorkItem, GetRelatedWorkItemsQuery, Lookups.WorkItemList).Category = FilterCategories.StatusAndFlags;
			filters.AddNkFilter("Owner", IncidentManagementGroupSchema.ING_GS_NKGroupOwner, ModuleIDs.GlbStaff, StaffList).Category = FilterCategories.Organisations;
		}

		ZQuery GetRelatedWorkItemsQuery(ZGuid workItemPK)
		{
			var query = new ZDBOnlyQuery(typeof(IncidentManagementGroup));

			var subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation1ID);
			subQuery.AddToFilter(GenPivotSchema.XX_RelationType, new string[] { Core.Constants.GenPivotTypes.ProcessManagement, Core.Constants.GenPivotTypes.WorkItemCascade });
			subQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, IncidentManagementGroupSchema.Constants.Prefix);
			subQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, workItemPK);
			subQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, WorkItemSchema.Constants.Prefix);

			query.AddSubQuery(IncidentManagementGroupSchema.PK, subQuery, JoinCondition.Or);
			return query;
		}

		GlbStaffCollection staffList;
		GlbStaffCollection StaffList => staffList ?? (staffList = new GlbStaffCollection(Factory));

		#endregion

		#region Menu Filters

		void AddMenuFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Menu Item", IncidentManagementGroupSchema.ING_SourceModuleId, GetIncidentMenuItemFilterItems);
			filters.AddTextFilter("Menu Section", GetMenuSectionQuery, GetIncidentMenuSectionFilterItems)
				.WithMaxLengthOf<ModuleTextFilter>(IncidentManagementGroupSchema.ING_Module);
		}

		ICodeDescriptionPairList GetIncidentMenuItemFilterItems()
		{
			return SupportIncidentLookups.GetNewSearchableSourceModuleList();
		}

		ZQuery GetMenuSectionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var menuSectionPriorities = new[]
				{
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR5_Training,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement,
					Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest,
				};

			var query = new ZQuery(IncidentManagementGroupSchema.ING_Priority, SQLComparisonOperator.Equal, menuSectionPriorities);
			query.AddToFilter(IncidentManagementGroupSchema.ING_Module, comparisonOperator, value);

			return query;
		}

		ICodeDescriptionPairList GetIncidentMenuSectionFilterItems()
		{
			return GetIncidentModuleFilterItems(ModuleListType.MenuSection);
		}

		ICodeDescriptionPairList GetIncidentModuleFilterItems(ModuleListType moduleListType)
		{
			var productFilters = GetActiveFiltersByDescription("Product").Cast<ModuleTextFilter>();
			var productAreaFilters = GetActiveFiltersByDescription("Product Area").Cast<ModuleTextFilter>();

			return BuildIncidentModuleFilterItems(moduleListType, productFilters, productAreaFilters);
		}

		IEnumerable<ModuleFilter> GetActiveFiltersByDescription(string description)
		{
			if (moduleFiltersCreated)
			{
				var descriptionRegExp = new Regex(@"^" + description + @"(\s\([\d]\))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				foreach (ModuleFilter filter in ActiveModuleFilters)
				{
					if (descriptionRegExp.IsMatch(filter.Description))
					{
						yield return filter;
					}
				}
			}
		}
		CodeDescriptionPairList BuildIncidentModuleFilterItems(ModuleListType moduleListType, IEnumerable<ModuleTextFilter> productFilters, IEnumerable<ModuleTextFilter> productAreaFilters)
		{
			var result = new CodeDescriptionPairList();

			var products = productFilters.Select(filter => filter.Property);
			var productAreas = productAreaFilters.Select(filter => filter.Property);

			foreach (var productArea in productAreas)
			{
				result.AddRangeOverwriteIfExists(SupportIncidentLookups.GetModuleList(moduleListType, ProductTypes.Codes.Enterprise, productArea));
			}

			if (result.Count == 0)
			{
				foreach (var product in products)
				{
					result.AddRangeOverwriteIfExists(SupportIncidentLookups.GetModuleList(moduleListType, product, ZString.Empty));
				}
			}

			if (result.Count == 0)
			{
				result.AddRangeOverwriteIfExists(SupportIncidentLookups.GetModuleList(moduleListType, ZString.Empty, ZString.Empty));
			}

			result.Sort();

			return result;
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			moduleFiltersCreated = true;
		}

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			moduleFiltersCreated = false;
		}

		bool moduleFiltersCreated;

		#endregion

		#region Lookups

		protected IncidentManagementGroupLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected IncidentManagementGroupLookups GetNewLookups()
		{
			return new IncidentManagementGroupLookups(Factory);
		}

		IncidentManagementGroupLookups lookups;

		SupportIncidentLookups SupportIncidentLookups => supportIncidentLookups ?? (supportIncidentLookups = new SupportIncidentLookups(this));
		SupportIncidentLookups supportIncidentLookups;

		#endregion
	}
}
