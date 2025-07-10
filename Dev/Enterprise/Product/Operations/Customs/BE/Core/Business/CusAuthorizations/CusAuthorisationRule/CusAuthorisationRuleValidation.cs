using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
{
	public CusAuthorisationRuleValidation(Customs.Business.CusAuthorisationRule parent) : base(parent)
	{
	}

	protected override void CheckCPR_ValueFromIsValidCode()
	{
		ListValidation.MessageErrorIfInvalidCode(Parent.CPR_ValueFromInfo);
	}
}
