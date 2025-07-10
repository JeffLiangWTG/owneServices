using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business;

public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	protected override bool IsQualifierInvalidForType(string type, string qualifier)
		=> !(type == CusGoodsLocationTypeList.Codes.AuthorizedPlace && qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber) &&
			base.IsQualifierInvalidForType(type, qualifier);
}
