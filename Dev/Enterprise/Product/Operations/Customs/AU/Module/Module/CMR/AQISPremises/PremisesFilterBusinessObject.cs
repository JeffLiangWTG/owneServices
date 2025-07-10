using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class PremisesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Premise ID", CMRAqisPremisesSchema.QP_AQISPremisesIdentifier);
			filters.AddTextFilter("Premise Name", CMRAqisPremisesSchema.QP_AQISPremisesName);
			filters.AddNkFilter("UNLOCO", CMRAqisPremisesSchema.QP_AQISPremisesPortCode, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory));

			return filters;
		}
	}
}
