using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public sealed class DfiaExportItemDetailCollection : CusSupportingInfoCollection<DfiaExportItemDetail>
{
	public DfiaExportItemDetailCollection(BusinessObject parent)
		: base(parent, CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization)
	{
		const int maxAllowed = 9999;
		this.EnableMaxCountValidationWithMessageError(maxAllowed, warnAtHalfway: false, Res.GetString("CB9737A0-9F11-41F9-B146-02C81E29A32F", "The maximum number of {0} {1} has been exceeded.", maxAllowed, CusSupportingInfoTypeList.Descriptions.DutyFreeImportAuthorization));
	}
}
