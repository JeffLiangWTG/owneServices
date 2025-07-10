using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureSetFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Feature Set Name", FeatureControlSetSchema.FCS_ProductName);
			return filters;
		}
	}
}
