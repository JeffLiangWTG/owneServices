using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	protected override void CheckCGL_AdditionalIdentifier()
	{
		base.CheckCGL_AdditionalIdentifier();

		CheckPlaceCode(GoodsLocation);
	}

	protected override void CheckCGL_Qualifier()
	{
		base.CheckCGL_Qualifier();
		if (IsUcc6AndExport)
		{
			CusGoodsLocationValidationHelper.ValidateCGL_Qualifier(GoodsLocation);
		}
	}

	protected override void CheckCGL_Type()
	{
		base.CheckCGL_Type();
		if (IsUcc6AndExport)
		{
			CusGoodsLocationValidationHelper.ValidateCGL_Type(GoodsLocation);
		}
	}

	protected override IDictionary<string, (ZString ruleCode, ZPropertyInfo requiredInfo)> GetCustomsOfficeRequirementRule()
	{
		var result = base.GetCustomsOfficeRequirementRule();
		result[CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier] = (ValidationRuleCodeConstants.CN0394, Parent.CGL_CustomsOfficeInfo);
		return result;
	}

	#region Implementation

	void CheckPlaceCode(CusGoodsLocation parent)
	{
		if (parent.IsPlaceCodeAvailable)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CGL_AdditionalIdentifierInfo);
		}
	}

	bool IsUcc6AndExport => GoodsLocation.Parent is JobDeclaration declaration && declaration.IsUCC6AndIsExport;

	CusGoodsLocation GoodsLocation => (CusGoodsLocation)Parent;

	#endregion
}
