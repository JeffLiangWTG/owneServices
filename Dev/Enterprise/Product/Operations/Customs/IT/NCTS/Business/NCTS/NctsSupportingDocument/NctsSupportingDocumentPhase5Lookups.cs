using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsSupportingDocumentPhase5Lookups(EU.NCTS.Business.NctsSupportingDocument parent) : EU.NCTS.Business.NctsSupportingDocumentPhase5Lookups(parent)
{
	protected override ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string dataGroupingCode)
	{
		return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, RefCusCodeListType.Code.SupportingDocumentOfNCTS
				, includeParent
				, CusSupportingInfoHelper.GetLevelAttributeValue(Parent)
				, dataGroupingCode
				, applyLevelAttributeToChild: false);
	}
}
