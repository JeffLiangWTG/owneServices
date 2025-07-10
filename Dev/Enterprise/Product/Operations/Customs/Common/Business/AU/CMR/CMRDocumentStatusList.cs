using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRDocumentStatusList : CodeDescriptionPairList
	{
		public CMRDocumentStatusList()
		{
			Add(CMRDocumentStatus.Cancelled);
			Add(CMRDocumentStatus.Clear);
			Add(CMRDocumentStatus.Error);
			Add(CMRDocumentStatus.Rejected);
			Add(CMRDocumentStatus.Revoked);
			Add(CMRDocumentStatus.Withdrawn);
		}
	}
}
