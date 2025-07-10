using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class AQISProducerCodeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Producer Code", CMRAqisProducerSchema.QR_AQISProducerCode);
			filters.AddTextFilter("Producer Name", CMRAqisProducerSchema.QR_AQISProducerName);
			filters.AddFilter(new CountryModuleNkFilter(CMRAqisProducerSchema.QR_AQISProducerCountryCode, Factory));

			return filters;
		}
	}
}
