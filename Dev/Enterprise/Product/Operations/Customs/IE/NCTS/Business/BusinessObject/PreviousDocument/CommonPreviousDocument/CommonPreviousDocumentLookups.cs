using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CommonPreviousDocumentLookups : EU.NCTS.Business.CommonPreviousDocumentLookups
	{
		public CommonPreviousDocumentLookups(CommonPreviousDocument previousDocument) : base(previousDocument)
		{
		}

		bool IsInTransitionPeriod => Parent.Parent switch
		{
			NctsHeader header => header.IsInPhase5TransitionPeriod,
			NctsBill bill => bill.IsInPhase5TransitionPeriod,
			_ => false
		};

		protected override ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string levelAttributeValue, string dataGroupingCode)
		{
			return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS
				, includeParent
				, levelAttributeValue
				, dataGroupingCode
				, applyLevelAttributeToChild: IsInTransitionPeriod);
		}

		protected override string TypeCodeListCachedKey => base.TypeCodeListCachedKey + $"IsInTransitionPeriod_{IsInTransitionPeriod}";

		public new CommonPreviousDocument Parent => (CommonPreviousDocument)base.Parent;
	}
}
