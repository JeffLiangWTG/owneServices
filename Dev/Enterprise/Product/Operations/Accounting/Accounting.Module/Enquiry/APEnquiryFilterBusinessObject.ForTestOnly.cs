#if DEBUG

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public partial class APEnquiryFilterBusinessObject
	{
		public OrgHeaderCollection OrganisationList_ForTestOnly => OrganisationList;

		public bool ModuleFiltersAreCreated_ForTestOnly
		{
			get { return ModuleFiltersAreCreated; }
			set { ModuleFiltersAreCreated = value; }
		}

		public ModuleGuidFilter FOrganisationFilter_ForTestOnly
		{
			get { return fOrganisationFilter; }
			set { fOrganisationFilter = value; }
		}

		public CodeDescriptionPairList PaymentStatusList_ForTestOnly => PaymentStatusList;
	}
}

#endif
