namespace Enterprise.Customs.EU.NCTS.Business;

public sealed class CusAuthorizationUsagePhase5ValidationDecider : ICusAuthorizationUsagePhase5ValidationDecider
{
	public bool IsRuleG0114Active => true;

	public bool IsRuleTR0005Active => true;
}
