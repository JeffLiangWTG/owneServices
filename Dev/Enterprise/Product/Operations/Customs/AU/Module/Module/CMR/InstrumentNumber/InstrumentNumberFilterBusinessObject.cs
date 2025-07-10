using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class InstrumentNumberFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Instrument Type", CMRInstrumentSchema.IN_Type);
			filters.AddTextFilter("Instrument Number", CMRInstrumentSchema.IN_Number);
			filters.AddTextFilter("Concessional Item Number", CMRInstrumentSchema.IN_ConcessionalItemNumber);
			return filters;
		}
	}
}
