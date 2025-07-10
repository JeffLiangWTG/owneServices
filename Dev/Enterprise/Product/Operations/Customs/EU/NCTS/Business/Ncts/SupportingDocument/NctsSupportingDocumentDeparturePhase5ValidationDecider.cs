namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsSupportingDocumentDeparturePhase5ValidationDecider : INctsSupportingDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleE1301Active => false;

	public bool IsRuleG0321Active => false;

	public bool IsRuleNR0006Active => false;

	public bool IsRuleRP30Active => false;
}
