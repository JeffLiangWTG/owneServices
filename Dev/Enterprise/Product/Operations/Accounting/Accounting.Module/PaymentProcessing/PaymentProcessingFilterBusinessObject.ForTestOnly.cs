#if DEBUG
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class PaymentProcessingFilterBusinessObject
	{
		public string OrganisationFilterName_ForTestOnly => OrganisationFilterName;

		public string OrganisationAndAddressFilterName_ForTestOnly => OrganisationAndAddressFilterName;

		public ModuleFilterCollection GetModuleFiltersCore_ForTestOnly() => GetModuleFiltersCore();
	}
}

#endif
