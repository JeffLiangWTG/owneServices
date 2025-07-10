
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business;

public class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(EU.Business.CusGoodsLocation parent) : base(parent)
	{
	}

	protected override void CheckCGL_AdditionalIdentifier()
	{
		base.CheckCGL_AdditionalIdentifier();
		if (((CusGoodsLocation)Parent).IsInAuthorisationMode)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CGL_AdditionalIdentifierInfo);
			if (Parent.CGL_AdditionalIdentifier.Split(',').Length > 1)
			{
				Parent.CGL_AdditionalIdentifierInfo.AddMessageError(Res.GetString("B67D3D16-D98A-47F3-BBA2-01F5B55EA4EB", "You cannot use comma's in the house number"));
			}
		}
	}
}
