using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class BMFilterStripsHelper : FilterStripsHelper, IBMFilterStripsHelper
	{
		protected BMFilterStripsHelper()
		{
		}

		public BMFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, factory)
		{
			this.templateCode = templateCode;
		}

		readonly FilterCategory bufferManagementCategory = new FilterCategory(BMGlobalConstants.BufferManagementCategoryDescription);
		ZString templateCode;

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return overriddenSupportedWorkflowTypes != null || typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType);
		}

		public override void Initialise(Type businessObjectType, BusinessObjectFactory factory)
		{
			base.Initialise(businessObjectType, factory);
			SetTemplateCodeFromBusinessObjectType();
		}

		void SetTemplateCodeFromBusinessObjectType()
		{
			if (overriddenSupportedWorkflowTypes == null && IsApplicableToBizOTypeIsAssignableFrom() && string.IsNullOrEmpty(templateCode))
			{
				templateCode = WorkflowFilterStripsHelper.GetTemplateCodeForBusinessObjectType(BusinessObjectType);
			}
		}

		public override bool CanAddFilters()
		{
			var result = base.CanAddFilters() && ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled;

			if (overriddenSupportedWorkflowTypes != null)
			{
				return result && overriddenSupportedWorkflowTypes.Any(item => IsAssociatedWithBMSystem(item));
			}
			else
			{
				return result && !string.IsNullOrEmpty(templateCode) && IsAssociatedWithBMSystem(templateCode);
			}
		}

		public SchemaColumn SubColumnOverride { get; set; }

		public Type BusinessObjectTypeOverride { get; set; }

		public void SetOverriddenSupportedWorkflowTypes(IEnumerable<string> workflowTypes)
		{
			overriddenSupportedWorkflowTypes = workflowTypes;
		}
		IEnumerable<string> overriddenSupportedWorkflowTypes;

		bool IsAssociatedWithBMSystem(ZString code)
		{
			return BMSystem.GetSystemForWorkflowType(code, Factory) != null;
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			if (CanAddFilters())
			{
				AddComponentFilter(filters);
				AddScheduledTimesFilters(filters);
				AddTagFilters(filters);
				AddWorkflowFilters(filters);
			}
		}

		void AddComponentFilter(ModuleFilterCollection filters)
		{
			var bufferComponentFilter = filters.AddGuidFilter(ProcessHeader.ModuleFilterConstants.BufferManagementComponent, ModuleIDs.BMComponent, GetBufferComponentQuery, GetBufferComponentList);
			bufferComponentFilter.Category = bufferManagementCategory;
			bufferComponentFilter.IsPublishedOnWeb = false;
			bufferComponentFilter.MultilingualDescription = BufferManagementComponentDescription;
		}

		void AddWorkflowFilters(ModuleFilterCollection filters)
		{
			var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(BusinessObjectType);
			var tasksFilter = new WorkflowsModuleFilter(ProcessHeader.ModuleFilterConstants.WorkflowsForJob, parentTableSchema.PK, ProcessHeaderSchema.FH_ParentId, () => new ProcessHeaderCollection(Factory), BusinessObjectType);
			tasksFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|Module|BMFilterStripsHelper|Workflows", "Workflows for Job");
			tasksFilter.Category = WorkflowFilterStripsHelper.GetTasksCategory();

			filters.AddFilter(tasksFilter);
		}

		protected override void AddFilterStripsForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			if (CanAddFilters())
			{
				AddComponentFilterForIndexSearch(filters, defaultHiddenIndexSearchFields);
				AddTagFiltersForIndexSearch(filters, defaultHiddenIndexSearchFields);
			}
		}

		void AddComponentFilterForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			var searchField = defaultHiddenIndexSearchFields?.FirstOrDefault(x => x.FieldName == IndexSearchFilterHelper.DefaultHiddenPrefix + "BUFFERMANAGEMENTCOMPONENT");
			if (searchField != null)
			{
				var filter = new IndexSearchModuleGuidFilter(searchField, ModuleIDs.BMComponent, GetBufferComponentList());
				filter.MultilingualDescription = BufferManagementComponentDescription;
				filter.Category = bufferManagementCategory;
				filters.AddFilter(filter);
			}
		}

		void AddTagFiltersForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			var searchFieldTagGroup = defaultHiddenIndexSearchFields?.FirstOrDefault(x => x.FieldName == IndexSearchFilterHelper.DefaultHiddenPrefix + "TAGGROUP");
			if (searchFieldTagGroup != null)
			{
				var filterTagGroup = new IndexSearchTagWithJobOrWorkflowFilter(searchFieldTagGroup, bufferManagementCategory, ModuleIDs.BMTagDefinition, GetTagDefinitionCodeFilter, new TagDefinitionCollection(Factory));
				filterTagGroup.MultilingualDescription = TagGroupDescription;
				filterTagGroup.Category = bufferManagementCategory;
				filters.AddFilter(filterTagGroup);
			}

			var searchFieldTag = defaultHiddenIndexSearchFields?.FirstOrDefault(x => x.FieldName == IndexSearchFilterHelper.DefaultHiddenPrefix + "TAG");
			if (searchFieldTag != null)
			{
				var filterTag = new IndexSearchTagWithJobOrWorkflowFilter(searchFieldTag, bufferManagementCategory, ModuleIDs.BMTagMagnitude, GetTagMagnitudeFilter, new TagMagnitudeCollection(Factory));
				filterTag.MultilingualDescription = TagDescription;
				filterTag.Category = bufferManagementCategory;
				filters.AddFilter(filterTag);
			}
		}

		IGlowQuery GetTagDefinitionCodeFilter(string fieldName, ZGuid property, ZString dropDownTypeName, string comparisonOperator)
		{
			if (dropDownTypeName == JobOrWorkflow.Codes.Job)
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "JOBTAGGROUP";
			}
			else if (dropDownTypeName == JobOrWorkflow.Codes.Wfl)
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "WORKFLOWTAGGROUP";
			}
			else
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "TAGGROUP";
			}

			return IndexSearchTagWithJobOrWorkflowFilter.GetGlowIndexQueryCore(fieldName, property, comparisonOperator);
		}

		IGlowQuery GetTagMagnitudeFilter(string fieldName, ZGuid property, ZString dropDownTypeName, string comparisonOperator)
		{
			if (dropDownTypeName == JobOrWorkflow.Codes.Job)
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "JOBTAG";
			}
			else if (dropDownTypeName == JobOrWorkflow.Codes.Wfl)
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "WORKFLOWTAG";
			}
			else
			{
				fieldName = IndexSearchFilterHelper.DefaultHiddenPrefix + "TAG";
			}

			return IndexSearchTagWithJobOrWorkflowFilter.GetGlowIndexQueryCore(fieldName, property, comparisonOperator);
		}

		ResourceString BufferManagementComponentDescription => ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|BufferManagementComponent", ProcessHeader.ModuleFilterConstants.BufferManagementComponent);

		ResourceString TagGroupDescription => ResString.GetMultilingualString("b284af4d-ceb1-43c0-9b7b-a5e91ac37d26", "Tag Group");

		ResourceString TagDescription => ResString.GetMultilingualString("d0ea13db-3e93-4bdb-999e-f7dd15e29a97", "Tag");

		#region Sub-Column and Business Object Type Override

		ZDBOnlyQuery CreateQuery(Type originalType, Type overrideType)
		{
			return new ZDBOnlyQuery(overrideType ?? originalType);
		}

		void AddSubQueryUsingSubColumnOverride(ZDBOnlyQuery query, ZDBOnlySubQuery subQuery, JoinCondition join)
		{
			if (SubColumnOverride != null)
			{
				query.AddSubQuery(SubColumnOverride, subQuery, join);
			}
			else
			{
				query.AddSubQuery(subQuery, join);
			}
		}

		#endregion

		#region Current Component

		BMComponentCollection GetBufferComponentList()
		{
			if (bufferComponentList == null)
			{
				var systems = Factory.Load<BMSystem>(new ZQuery());
				foreach (var system in systems)
				{
					if (system.IsForWorkflowType(templateCode))
					{
						bufferComponentList = system.Components;
						break;
					}
				}

				if (bufferComponentList == null)
				{
					bufferComponentList = new BMComponentCollection(Factory);
				}
			}

			return bufferComponentList;
		}
		BMComponentCollection bufferComponentList;

		ZQuery GetBufferComponentQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			ZGuid componentPK;
			if (!ZGuid.TryParse(value, out componentPK))
			{
				componentPK = ZGuid.Empty;
			}

			bool notIn = false;

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				notIn = true;
			}

			var dbOnlyQuery = CreateQuery(BusinessObjectType, BusinessObjectTypeOverride);

			var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId, notIn);
			processHeaderSubQuery.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, comparisonOperator, componentPK);

			AddSubQueryUsingSubColumnOverride(dbOnlyQuery, processHeaderSubQuery, JoinCondition.And);
			return dbOnlyQuery;
		}

		#endregion

		#region Scheduled Times

		void AddScheduledTimesFilters(ModuleFilterCollection filters)
		{
			var typeToUse = BusinessObjectTypeOverride ?? BusinessObjectType;

			var scheduledStartFilter = ApprovedJobScheduledTimeFilter.GetScheduledStartTimeFilter(typeToUse);
			var scheduledFinishFilter = ApprovedJobScheduledTimeFilter.GetScheduledFinishTimeFilter(typeToUse);
			var scheduleTypeFilter = new ApprovedJobScheduleTypeFilter(typeToUse);

			scheduledStartFilter.Category = scheduledFinishFilter.Category = scheduleTypeFilter.Category = bufferManagementCategory;

			filters.AddFilter(scheduledStartFilter);
			filters.AddFilter(scheduledFinishFilter);
			filters.AddFilter(scheduleTypeFilter);
		}

		#endregion

		#region Tags

		void AddTagFilters(ModuleFilterCollection filters)
		{
			var tagDefinationFilter = filters.AddToTagWithJobOrWorkflowFilter(ProcessHeader.ModuleFilterConstants.TagDefinitionCode, bufferManagementCategory, ModuleIDs.BMTagDefinition, GetTagDefinitionCodeFilter, new TagDefinitionCollection(Factory));
			tagDefinationFilter.MultilingualDescription = TagGroupDescription;

			var tagMagnitudeFilter = filters.AddToTagWithJobOrWorkflowFilter(ProcessHeader.ModuleFilterConstants.TagMagnitude, bufferManagementCategory, ModuleIDs.BMTagMagnitude, GetTagMagnitudeFilter, new TagMagnitudeCollection(Factory));
			tagMagnitudeFilter.MultilingualDescription = TagDescription;
		}

		ZQuery GetTagMagnitudeFilter(ZGuid value, bool notIn, ZString jobOrWorkflow)
		{
			var query = CreateQuery(BusinessObjectType, BusinessObjectTypeOverride);

			var processHeaderSubquery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId, notIn);

			if (jobOrWorkflow == JobOrWorkflow.Codes.Job)
			{
				processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			}
			else if (jobOrWorkflow == JobOrWorkflow.Codes.Wfl)
			{
				processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);
			}

			var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			linkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, value);

			var jobLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			jobLinkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, value);

			processHeaderSubquery.AddSubQuery(linkSubQuery, JoinCondition.And);
			AddSubQueryUsingSubColumnOverride(query, processHeaderSubquery, JoinCondition.And);

			return query;
		}

		ZQuery GetTagDefinitionCodeFilter(ZGuid value, bool notIn, ZString jobOrWorkflow)
		{
			var query = CreateQuery(BusinessObjectType, BusinessObjectTypeOverride);

			var processHeaderSubquery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId, notIn);

			if (jobOrWorkflow == JobOrWorkflow.Codes.Job)
			{
				processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			}
			else if (jobOrWorkflow == JobOrWorkflow.Codes.Wfl)
			{
				processHeaderSubquery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null);
			}

			var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			linkSubQuery.AddToFilter(TagLinkSchema.TGL_ParentId, SQLComparisonOperator.NotEqual, ZGuid.Empty);

			var magnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);
			magnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, value);

			linkSubQuery.AddSubQuery(magnitudeSubQuery, JoinCondition.And);
			processHeaderSubquery.AddSubQuery(linkSubQuery, JoinCondition.And);
			AddSubQueryUsingSubColumnOverride(query, processHeaderSubquery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Static Helper Methods

		public static bool IsJobOrWorkflowFilterPresentInGroupWithJobOnlySelected(ModuleFilter otherFilterInGroup)
		{
			var jobOrWorkflowFilters = otherFilterInGroup.FilterBusinessObject.FindFiltersOfTypeInGroup<JobOrWorkflowFilter>(otherFilterInGroup);

			return jobOrWorkflowFilters.Any(x => x.IsJobOnly);
		}

		static bool IsJobOrWorkflowFilterPresentInGroupWithWorkflowOnlySelected(ModuleFilter otherFilterInGroup)
		{
			var jobOrWorkflowFilters = otherFilterInGroup.FilterBusinessObject.FindFiltersOfTypeInGroup<JobOrWorkflowFilter>(otherFilterInGroup);

			return jobOrWorkflowFilters.Any(x => x.IsWorkflowOnly);
		}

		public static bool ShouldOptimiseQueryForWorkflowOnly(ModuleFilter filter)
		{
			var isOptimisationForced = filter.FilterBusinessObject is IJobOrWorkflowOptimisable optimisable && optimisable.ShouldOptimiseQueryForWorkflowOnly;

			return isOptimisationForced || IsJobOrWorkflowFilterPresentInGroupWithWorkflowOnlySelected(filter);
		}

		public static Tuple<string, string> GetFiltersMatchSql(IModuleFilterWithSelectedFilters filter, string paramName, ZSqlParameterCollection parameters)
		{
			var query = filter.GetSubFilterQueryIncludingCollectionFilters();
			var sql = query.ParameterisedText.ParameterisedQueryText;

			var queryParametersInDescendingOrder = query.Params.OrderByDescending(c => c.ParameterName.Length);

			var counter = 0;
			foreach (var parameter in queryParametersInDescendingOrder)
			{
				var newName = paramName + ++counter;
				parameters.Add(ZSqlParameter.New(newName, parameter.Value, parameter.SchemaColumn, parameter.ComparisonOperator));
				sql = sql.Replace(parameter.ParameterName, newName);
			}

			var whereKeyword = string.IsNullOrWhiteSpace(sql) ? string.Empty : "WHERE ";

			return Tuple.Create(whereKeyword, sql);
		}

		#endregion

		#region TestFiltersWorkForAllModules
#if DEBUG
		public override string GetAutomaticFilterTestCaseName_ForObjectFactory() => "BMFilterStripsHelperAutomaticFilterTest";
#endif
		#endregion
	}
}
