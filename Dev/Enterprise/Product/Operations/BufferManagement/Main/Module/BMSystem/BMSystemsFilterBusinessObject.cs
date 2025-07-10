using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMSystemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", BMSystemSchema.FS_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMSystemFilter|Name", "Name");
			filters.AddTextFilter("Description", BMSystemSchema.FS_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMSystemFilter|Description", "Description");
		}
	}
}
