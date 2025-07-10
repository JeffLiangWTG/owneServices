using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class DepartureCusTransportMeansPhase5ValidationDecider : IDepartureCusTransportMeansPhase5ValidationDecider
{
	public bool IsRuleB2101Active => true;
	public bool IsRuleG0789_1Active => true;
	public bool IsRuleTR0078Active => true;
}
