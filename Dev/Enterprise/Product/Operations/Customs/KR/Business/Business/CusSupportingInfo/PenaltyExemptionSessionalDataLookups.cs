using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class PenaltyExemptionSessionalDataLookups : CusSupportingInfoLookups
	{
		public PenaltyExemptionSessionalDataLookups(PenaltyExemptionSessionalData parent) : base(parent)
		{
		}

		public CodeDescriptionPairList YNCodeList => Factory.GetCachedValue<YesNoList>();
	}
}
