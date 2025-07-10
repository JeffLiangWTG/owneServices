using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaImportItemDetailCollection : CusSupportingInfoCollection<DfiaImportItemDetail>
{
	public DfiaImportItemDetailCollection(BusinessObject parent)
		: base(parent, CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("40E8524C-D8CF-4A4B-A65B-CE00A2645552", "The maximum number of {0} {1} has been exceeded.", maxAllowed, CusSupportingInfoTypeList.Descriptions.DutyFreeImportAuthorization));
	}
}
