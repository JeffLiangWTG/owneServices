namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsSupportingDocumentDeparturePhase5ValidationDecider : EU.NCTS.Business.INctsSupportingDocumentDeparturePhase5ValidationDecider
{
	public bool IsRuleE1301Active => true;

	public bool IsRuleG0321Active => false;

	public bool IsRuleNR0006Active => false;

	public bool IsRuleRP30Active => false;
}
