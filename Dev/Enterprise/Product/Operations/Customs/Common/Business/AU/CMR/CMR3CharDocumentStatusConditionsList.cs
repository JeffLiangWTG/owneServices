using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public class CMR3CharDocumentStatusConditionsList : CodeDescriptionPairList
	{
		public CMR3CharDocumentStatusConditionsList()
		{
			Add(CMR3CharDocumentStatusConditions.Embargoed);
			Add(CMR3CharDocumentStatusConditions.Expired);
			Add(CMR3CharDocumentStatusConditions.Suspended);
			Add(CMR3CharDocumentStatusConditions.Validation);
		}
	}
}
