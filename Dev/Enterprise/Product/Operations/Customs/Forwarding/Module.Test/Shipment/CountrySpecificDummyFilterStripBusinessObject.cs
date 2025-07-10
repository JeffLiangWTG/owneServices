using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	abstract class CountrySpecificDummyFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			{
				var filtersProvider = new ForwardingShipmentModuleCustomsFiltersProvider(Factory);
				filtersProvider.AddFilters(result);
			}
			return result;
		}

		protected abstract string Country { get; }
	}
}
