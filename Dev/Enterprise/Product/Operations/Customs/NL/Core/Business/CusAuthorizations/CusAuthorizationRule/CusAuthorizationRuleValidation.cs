namespace Enterprise.Customs.NL.Business;

public class CusAuthorisationRuleValidation : Customs.Business.CusAuthorisationRuleValidation
{
	public CusAuthorisationRuleValidation(Customs.Business.CusAuthorisationRule parent) : base(parent)
	{
	}

	protected override void CheckCPR_ValueFrom()
	{
	}
}
