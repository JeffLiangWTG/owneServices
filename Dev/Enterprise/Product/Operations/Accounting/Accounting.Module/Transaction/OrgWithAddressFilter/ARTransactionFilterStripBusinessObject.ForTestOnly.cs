#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class ARTransactionFilterStripBusinessObject
	{
		public ModuleFilter GetModuleFilterThatOverridesAllOtherFilters_ForTestOnly()
		{
			return GetModuleFilterThatOverridesAllOtherFilters();
		}

		public ZQuery GetAROrganizationAndAddressFilter_ForTestOnly(ZGuid orgPK, ZGuid addressPK)
		{
			return GetAROrganizationAndAddressFilter(orgPK, addressPK);
		}
	}
}

#endif
