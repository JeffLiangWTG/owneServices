using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	public class CusSCADepotContainerFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Container Number", CusSCADepotContainerSchema.CJ_ContainerNumber);
			filters.AddTextFilter("Voyage", CusSCADepotContainerSchema.CJ_Voyage);

			return filters;
		}
	}
}
