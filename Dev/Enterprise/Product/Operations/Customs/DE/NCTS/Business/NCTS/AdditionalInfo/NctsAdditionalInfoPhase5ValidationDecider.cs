using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsAdditionalInfoPhase5ValidationDecider : INctsAdditionalInfoPhase5ValidationDecider
{
	public bool IsRuleC0015Active => false;

	public bool IsRuleE1104_1Active => false;

	public bool IsRuleE1301Active => false;

	public bool IsRuleR0023Active => false;

	public bool IsRuleG0321Active => false;

	public bool IsRuleR3060Active => true;

	public bool IsRuleR3061Active => false;

	public bool IsRuleTR0031Active => true;

	public bool IsRuleTR0032Active => true;

	public bool IsRuleTR0033Active => true;

	public bool IsRuleTR0062Active => false;
}
