using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CommunicationStatusCodeDescriptionPairListProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetCodeDescriptionPairList();
		}
	}
}
