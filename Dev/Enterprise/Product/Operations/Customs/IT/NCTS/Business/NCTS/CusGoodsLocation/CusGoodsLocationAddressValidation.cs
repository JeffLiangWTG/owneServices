using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class CusGoodsLocationAddressValidation : EU.NCTS.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
	{
	}

	protected override string RuleC0394Code => ValidationRuleCodeConstants.CN0394;

	protected override void CheckE2_Email()
	{
		if (!IsNCTSPhase5)
		{
			base.CheckE2_Email();
			return;
		}
		CusGoodsLocationAddressValidationHelper.ValidateE2_Email(GoodsLocationAddress);
	}

	protected override void CheckE2_RN_NKCountryCode()
	{
		base.CheckE2_RN_NKCountryCode();
		ListValidation.MessageErrorIfInvalidCode(Parent.E2_RN_NKCountryCodeInfo);
	}

	protected override void CheckE2_Contact()
	{
		base.CheckE2_Contact();
		if (IsNCTSPhase5)
		{
			CusGoodsLocationAddressValidationHelper.ValidateE2_Contact(GoodsLocationAddress);
		}
	}

	protected override void CheckE2_Phone()
	{
		base.CheckE2_Phone();
		if (IsNCTSPhase5)
		{
			CusGoodsLocationAddressValidationHelper.ValidateE2_Phone(GoodsLocationAddress);
		}
	}

	#region Implementation

	CusGoodsLocationAddress GoodsLocationAddress => (CusGoodsLocationAddress)Parent;

	CusGoodsLocation GoodsLocation => GoodsLocationAddress.GoodsLocation;

	NctsHeader Header => GoodsLocation?.Header as NctsHeader;

	bool IsNCTSPhase5 => Header?.IsPhase5 ?? false;

	#endregion
}
