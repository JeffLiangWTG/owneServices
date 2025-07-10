using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CommissionStreamCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair(string.Empty, Res.GetString("084C87F2-14CB-4606-AADC-FA4685894C7D", "No Stream"));

			foreach (ICodeDescriptionBool item in OrganisationRegistry.Instance.CommissionAgreementStreams.Value)
			{
				list.AddPair(item.Code, item.Description);
			}

			return list;
		}
	}
}
