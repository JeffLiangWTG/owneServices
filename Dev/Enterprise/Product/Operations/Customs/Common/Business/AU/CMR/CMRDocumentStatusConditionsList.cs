using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMRDocumentStatusConditionsList : CodeDescriptionPairList
	{
		public CMRDocumentStatusConditionsList()
		{
			Add(CMRDocumentStatusConditions.Embargoed);
			Add(CMRDocumentStatusConditions.Expired);
			Add(CMRDocumentStatusConditions.Suspended);
			Add(CMRDocumentStatusConditions.Validation);
		}
	}
}
