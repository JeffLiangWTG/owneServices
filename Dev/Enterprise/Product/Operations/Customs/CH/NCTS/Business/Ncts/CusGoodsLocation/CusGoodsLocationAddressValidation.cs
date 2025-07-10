using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

sealed class CusGoodsLocationAddressValidation : EU.NCTS.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent)
		: base(parent)
	{
	}

	new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

	NctsHeader Header => Parent.GoodsLocation?.Header as NctsHeader;

	protected override bool ApplyC0065Rule => Header?.IsArrivalMovement ?? true;

	protected override void CheckE2_GovRegNum()
	{
		base.CheckE2_GovRegNum();

		if (Parent.IdentificationHolderPK.IsValid && !Parent.E2_GovRegNum.IsEmpty)
		{
			if (!Parent.Lookups.GetCusAuthorisationHeaderCollection().Find(new ZQuery(CusPermitHeaderSchema.CPH_Number, Parent.E2_GovRegNum)).Any())
			{
				Parent.E2_GovRegNumInfo.AddMessageError(Res.GetString("1F813BF0-E9FE-4A70-A2FD-62B909DBC92C", "Approved Place Authorization Number is not valid."));
			}
		}

		if ((Parent.GoodsLocation?.IsForActivationSending ?? false) && Parent.AuthorisationNumber.IsEmpty)
		{
			Parent.E2_GovRegNumInfo.AddError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Res.GetString("FF896285-7CB0-4401-A1BF-948E6CCA1B33", "Location: Authorization No."), Parent.GoodsLocation?.CGL_Qualifier.ToUpperInvariant(), ValidationRuleCodeConstants.Codes.C0065));
		}
	}
}
