using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.EndpointManagement.Module
{
	public class EdiTrustedSystemFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Product", EdiTrustedSystemSchema.ETS_Product, EdiTrustedSystemLookups.GetProductTypeList());
			filters.AddTextFilter("System ID", EdiTrustedSystemSchema.ETS_SystemID);
			filters.AddTextFilter("Description", EdiTrustedSystemSchema.ETS_Description);
			filters.AddTextFilter("System #", EdiTrustedSystemSchema.ETS_SystemNumber);
			return filters;
		}
	}
}
