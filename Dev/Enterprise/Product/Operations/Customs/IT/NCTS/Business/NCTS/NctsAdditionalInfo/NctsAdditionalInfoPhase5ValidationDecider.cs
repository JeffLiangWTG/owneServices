using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsAdditionalInfoPhase5ValidationDecider : INctsAdditionalInfoPhase5ValidationDecider
{
	public bool IsRuleC0015Active => true;

	public bool IsRuleE1104_1Active => true;

	public bool IsRuleE1301Active => true;

	public bool IsRuleR0023Active => true;

	public bool IsRuleG0321Active => false;

	public bool IsRuleR3060Active => true;

	public bool IsRuleR3061Active => true;

	public bool IsRuleTR0031Active => true;

	public bool IsRuleTR0032Active => true;

	public bool IsRuleTR0033Active => true;

	public bool IsRuleTR0062Active => false;
}
