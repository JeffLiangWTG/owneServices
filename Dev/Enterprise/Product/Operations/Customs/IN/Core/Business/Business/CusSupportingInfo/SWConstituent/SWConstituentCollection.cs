using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class SWConstituentCollection : CusSupportingInfoCollection<SWConstituent>
{
	public SWConstituentCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.SingleWindowConstituent)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("89CBC596-8FE4-4FF8-B5B7-C9207F66DB9D", "The maximum number of {0} Single Window Constituents has been exceeded.", maxAllowed));
	}
}

