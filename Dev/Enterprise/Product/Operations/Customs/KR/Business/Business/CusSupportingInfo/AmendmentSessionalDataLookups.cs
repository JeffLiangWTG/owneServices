using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class AmendmentSessionalDataLookups : Customs.Business.CusSupportingInfoLookups
	{
		public AmendmentSessionalDataLookups(AmendmentSessionalData parent) : base(parent)
		{
		}

		public CodeDescriptionPairList DutyPenaltyCauseList => Factory.GetCachedValue<TaxPenaltyTypeCodeList>();
		public CodeDescriptionPairList TaxPenaltyCauseList => Factory.GetCachedValue<TaxPenaltyTypeCodeList>();
	}
}
