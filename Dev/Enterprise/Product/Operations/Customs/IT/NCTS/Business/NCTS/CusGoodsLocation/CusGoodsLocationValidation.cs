using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class CusGoodsLocationValidation : EU.NCTS.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	protected override string RuleC0394Code => ValidationRuleCodeConstants.CN0394;

	protected override void CheckCGL_Qualifier()
	{
		base.CheckCGL_Qualifier();
		if (IsNCTS5)
		{
			CusGoodsLocationValidationHelper.ValidateCGL_Qualifier(GoodsLocation);
		}
	}

	protected override void CheckCGL_Type()
	{
		base.CheckCGL_Type();
		if (IsNCTS5)
		{
			CusGoodsLocationValidationHelper.ValidateCGL_Type(GoodsLocation);
		}
	}

	#region Implementation

	CusGoodsLocation GoodsLocation => (CusGoodsLocation)Parent;

	NctsHeader header => GoodsLocation?.Header as NctsHeader;

	bool IsNCTS5 => header?.IsPhase5 ?? false;

	#endregion
}
