using Enterprise.ZArchitecture.Business;

namespace Enterprise.CRM.Module
{
	public class CrmOpportunityFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}
	}
}
