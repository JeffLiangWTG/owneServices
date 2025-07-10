using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(EU.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	protected override bool IsQualifierInvalidForType(string type, string qualifier) =>
		type switch
		{
			CusGoodsLocationTypeList.Codes.Other => !((ZString)qualifier).In(AuthorisedQualifierForOtherTypeList()),
			CusGoodsLocationTypeList.Codes.DesignatedLocation => qualifier is not(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier or CusGoodsLocationQualifierList.Codes.UnLocode),
			CusGoodsLocationTypeList.Codes.AuthorizedPlace or CusGoodsLocationTypeList.Codes.ApprovedPlace => qualifier is not CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier,
			_ => false,
		};
}
