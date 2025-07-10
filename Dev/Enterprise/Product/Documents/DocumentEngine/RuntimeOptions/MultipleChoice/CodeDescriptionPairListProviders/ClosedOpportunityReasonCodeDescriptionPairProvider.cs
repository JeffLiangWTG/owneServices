using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ClosedOpportunityReasonCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.Value.GetCodeDescriptionPairList();
		}
	}
}
