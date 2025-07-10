using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

public class NctsPreviousDocumentDeparturePhase5ValidationDecider : INctsPreviousDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleG0321Active => false;

	public bool IsRuleC0298Active => false;

	public bool IsRuleNR0008Active => true;

	public bool IsRuleNR0046Active => false;

	public bool IsRuleG0058_1Active => false;

	public bool IsRuleTR0030_1Active => false;

	public bool IsRuleNR0066Active => false;
}
