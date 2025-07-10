using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagRuleFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddGuidFilters(filters);

			return filters;
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", TagRuleSchema.TGR_Name).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|Name", "Name");
			filters.AddTextFilter("Action Type", TagRuleSchema.TGR_ActionType, new TagRuleActionTypeList()).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|ActionType", "Action Type");
			filters.AddGuidFilter("Tag Magnitude", ModuleIDs.BMTagMagnitude, GetTagMagnitudeFilter, new TagMagnitudeCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|TagMagnitude", "Tag");
			filters.AddGuidFilter("Tag Group", ModuleIDs.BMTagDefinition, GetTagGroupFilter, new TagDefinitionCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|TagDefinition", "Tag Group");
			filters.AddGuidFilter("Owner Group", ModuleIDs.GlbGroup, GetTagMagnitudeOwnerFilter, new GlbGroupActiveBusinessObjectCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|OwnerGroup", "Owner Group");
		}

		static ZQuery GetTagMagnitudeFilter(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(TagRule));
			var templateSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			templateSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, value);
			templateSubQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);

			query.AddSubQuery(TagRuleSchema.PK, templateSubQuery, JoinCondition.And);

			return query;
		}

		static ZQuery GetTagMagnitudeOwnerFilter(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(TagRule));
			var templateSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			templateSubQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);
			var tagMagnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);

			tagMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_GG_OwnerGroup, value);

			templateSubQuery.AddSubQuery(tagMagnitudeSubQuery, JoinCondition.And);
			query.AddSubQuery(TagRuleSchema.PK, templateSubQuery, JoinCondition.And);

			return query;
		}

		static ZQuery GetTagGroupFilter(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(TagRule));
			var templateSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			templateSubQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);
			var tagMagnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);

			tagMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, value);

			templateSubQuery.AddSubQuery(tagMagnitudeSubQuery, JoinCondition.And);
			query.AddSubQuery(TagRuleSchema.PK, templateSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Number Filters

		static void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Last Run Duration", TagRuleSchema.TGR_LastRunDurationInSeconds).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|LastRunDurationInSeconds", "Last Run Duration (seconds)");
		}

		#endregion

		#region Date Filters

		static void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Last Run Start Time", TagRuleSchema.TGR_LastRunStartTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|LastRunStartTimeUtc", "Last Run Start Time (Local)");
		}

		#endregion

		#region GUID Filters

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, TagRuleSchema.TGR_GB_Branch, () => new GlbBranchCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|Branch", "Branch");
			filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, TagRuleSchema.TGR_GE_Department, () => new GlbDepartmentCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagRuleFilterBusinessObject|Department", "Department");
		}

		#endregion
	}
}
