using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsPreviousDocumentDeparturePhase5ValidationDecider : INctsPreviousDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleC0298Active => true;

	public bool IsRuleG0058_1Active => true;

	public bool IsRuleG0321Active => false;

	public bool IsRuleNR0008Active => false;

	public bool IsRuleNR0046Active => true;

	public bool IsRuleNR0066Active => true;

	public bool IsRuleTR0030_1Active => false;
}
