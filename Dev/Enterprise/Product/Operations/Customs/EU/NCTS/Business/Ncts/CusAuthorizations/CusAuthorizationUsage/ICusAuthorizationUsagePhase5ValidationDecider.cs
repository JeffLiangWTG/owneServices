namespace Enterprise.Customs.EU.NCTS.Business;

public interface ICusAuthorizationUsagePhase5ValidationDecider
{
	bool IsRuleG0114Active { get; }

	bool IsRuleTR0005Active { get; }
}
