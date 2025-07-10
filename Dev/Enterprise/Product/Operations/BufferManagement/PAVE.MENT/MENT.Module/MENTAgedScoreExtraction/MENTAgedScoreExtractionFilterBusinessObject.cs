using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Module
{
	public class MENTAgedScoreExtractionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddGuidFilters(filters);
			return filters;
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var relatedQueryFilter = filters.AddGuidFilter("Related Query", ModuleIDs.MENTAgedScoreQuery, MENTAgedScoreExtractionSchema.MEX_MAQ, () => new MENTAgedScoreQueryCollection(Factory));
			relatedQueryFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|MENT|MENTAgedScoreExtraction|Related Query", "Related Query");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", MENTAgedScoreExtractionSchema.MEX_Name).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|MENT|MENTAgedScoreExtraction|Name", "Name");
		}
	}
}
