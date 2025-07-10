using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMComponentFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			result.AddGuidFilter("BMSystem", ModuleIDs.BMSystems, BMComponentSchema.FC_FS_System, () => new BMSystemCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMComponentFilter|BMSystem", "System");
			return result;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", BMComponentSchema.FC_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMComponentFilter|Name", "Name");
			filters.AddTextFilter("Component Type", BMComponentSchema.FC_Type, () => new BMComponentTypeList()).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMComponentFilter|ComponentType", "Component Type");
		}
	}
}
