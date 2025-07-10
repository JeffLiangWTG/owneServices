using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagDefinitionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddMagnitudeFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", TagDefinitionSchema.TGD_Code).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagDefinitionFilterBusinessObject|Code", "Group Code");
			filters.AddFiltersForTranslatableText("Description", TagDefinitionSchema.TGD_Description, typeof(TagDefinition), ResString.GetMultilingualString("BufferManagement|TagDefinitionFilterBusinessObject|Description", "Description"));
		}

		void AddMagnitudeFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Magnitude Code", GetMagnitudeCodeQuery).WithMaxLengthOf<ModuleTextFilter>(TagMagnitudeSchema.TGM_Code).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagDefinitionFilterBusinessObject|Magnitude|Code", "Tag Code");
			filters.AddGuidFilter("Owner Group", ModuleIDs.GlbGroup, GetMagnitudeOwnerGroupQuery, new GlbGroupActiveBusinessObjectCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|TagDefinitionFilterBusinessObject|OwnerGroup", "Owner Group");
		}

		ZQuery GetMagnitudeCodeQuery(SQLComparisonOperator comparisonOperator, ZString code)
		{
			var query = new ZDBOnlyQuery(typeof(TagDefinition));
			var subquery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagMagnitudeSchema.TGM_TGD_Tag);
			subquery.AddToFilter(TagMagnitudeSchema.TGM_Code, comparisonOperator, code);

			query.AddSubQuery(subquery, JoinCondition.And);

			return query;
		}

		ZQuery GetMagnitudeOwnerGroupQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(TagDefinition));

			var subquery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagMagnitudeSchema.TGM_TGD_Tag);
			subquery.AddToFilter(TagMagnitudeSchema.TGM_GG_OwnerGroup, value);

			query.AddSubQuery(subquery, JoinCondition.And);

			return query;
		}
	}
}
