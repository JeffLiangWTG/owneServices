namespace Enterprise.Customs.EU.NCTS.Business;

public class DepartureCusTransportMeansPhase5ValidationDecider : IDepartureCusTransportMeansPhase5ValidationDecider
{
	public bool IsRuleB2101Active => false;
	public bool IsRuleG0789_1Active => false;
	public bool IsRuleTR0078Active => false;
}
