using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GlobalChargeCodeOrganizationFilterBusinessObject : GlobalChargeCodeFilterBusinessObject
	{
		public GlobalChargeCodeOrganizationFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var orgFilter = filters.AddGuidFilter("Organization", ModuleIDs.Organisation, AccGlobalChargeCodeMapSchema.YG_OH, Headers);
			orgFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|Organization", "Organization");

			return filters;
		}

		public virtual OrgHeaderCollection Headers
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}
	}
}
