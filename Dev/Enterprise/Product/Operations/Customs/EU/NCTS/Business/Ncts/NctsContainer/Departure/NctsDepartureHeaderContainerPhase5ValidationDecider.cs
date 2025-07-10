namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsDepartureHeaderContainerPhase5ValidationDecider : INctsDepartureHeaderContainerPhase5ValidationDecider
{
	public bool IsRuleC0055Active => true;

	public bool IsRuleN0003Active => false;

	public bool IsRuleR0448Active => true;

	public bool IsRuleTR0043Active => true;

	public bool IsRuleTR0044Active => true;

	public bool IsRuleTR0045Active => true;

	public bool IsRuleTR0046Active => true;

	public bool IsRuleTR0095Active => true;
}
