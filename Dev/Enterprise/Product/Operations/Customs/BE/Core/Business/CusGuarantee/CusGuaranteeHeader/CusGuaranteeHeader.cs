using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class CusGuaranteeHeader : EU.Business.CusGuaranteeHeader, Integration.Customs.BE.ICusGuaranteeHeader
{
	public CusGuaranteeHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGuaranteeHeaderValidation Validation => (CusGuaranteeHeaderValidation)base.Validation;

	public new GuaranteeCountrySpecificInstruction CountrySpecificInstruction => (GuaranteeCountrySpecificInstruction)base.CountrySpecificInstruction;

	protected override CusPermitHeaderValidation GetNewValidation() => new CusGuaranteeHeaderValidation(this);

	protected override CusGuaranteeRuleCollection CreateNewCusGuaranteeRulesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this);

	protected override CusGuaranteeRuleCollection CreateNewAdditionalAccessCodesCore() => new CusGuaranteeRuleCollection<CusGuaranteeRule>(this, PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber);
}
