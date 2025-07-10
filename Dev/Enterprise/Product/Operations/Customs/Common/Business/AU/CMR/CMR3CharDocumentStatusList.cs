using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMR3CharDocumentStatusList : CodeDescriptionPairList
	{
		public CMR3CharDocumentStatusList()
		{
			Add(CMR3CharDocumentStatus.Cancelled);
			Add(CMR3CharDocumentStatus.Clear);
			Add(CMR3CharDocumentStatus.Error);
			Add(CMR3CharDocumentStatus.Rejected);
			Add(CMR3CharDocumentStatus.Revoked);
			Add(CMR3CharDocumentStatus.Withdrawn);
		}
	}
}
