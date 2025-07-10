using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public sealed class SWControlCollection : CusSupportingInfoCollection<SWControl>
{
	public SWControlCollection(BusinessObject parent)
		: base(parent, CusSupportingInfoTypeList.Codes.SingleWindowControl)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("5861D293-601E-4C74-A1A9-7827645974DE", "The maximum number of {0} {1} has been exceeded.", maxAllowed, CusSupportingInfoTypeList.Descriptions.SingleWindowControl));
	}
}
