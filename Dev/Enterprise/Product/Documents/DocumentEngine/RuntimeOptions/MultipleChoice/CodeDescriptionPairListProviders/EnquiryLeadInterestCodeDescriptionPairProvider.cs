using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class EnquiryLeadInterestCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Value.GetActiveCodeDescriptionPairList();
		}
	}
}
