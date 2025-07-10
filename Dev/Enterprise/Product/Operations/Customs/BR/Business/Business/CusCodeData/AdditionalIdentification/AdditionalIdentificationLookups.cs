using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalIdentificationLookups : CusCodeDataLookups
	{
		public AdditionalIdentificationLookups(AdditionalIdentification parent) : base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection IssuingAgencyList => BRRefCusCodeListTypes.GetIssuingAgencyList(Factory);
	}
}
