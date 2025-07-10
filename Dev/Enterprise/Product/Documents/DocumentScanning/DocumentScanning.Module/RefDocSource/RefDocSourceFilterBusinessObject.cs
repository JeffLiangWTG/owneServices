
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Module
{
	public class RefDocSourceFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Document Source", RefDocSourceSchema.RDS_Code).MultilingualDescription = ResString.GetMultilingualString("DocumentScanning|RefDocSourceFilter|DocumentSource", "Document Source");
			filters.AddFiltersForTranslatableText("Description", RefDocSourceSchema.RDS_Desc, typeof(RefDocSource), ResString.GetMultilingualString("DocumentScanning|RefDocSourceFilter|Description", "Description"));
		}
	}
}
