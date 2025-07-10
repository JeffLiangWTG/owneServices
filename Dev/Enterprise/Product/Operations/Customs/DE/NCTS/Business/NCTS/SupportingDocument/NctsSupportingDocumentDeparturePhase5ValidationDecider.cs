using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsSupportingDocumentDeparturePhase5ValidationDecider : INctsSupportingDocumentDeparturePhase5ValidationDecider
	{
		public bool IsRuleE1301Active => false;

		public bool IsRuleG0321Active => false;

		public bool IsRuleNR0006Active => true;

		public bool IsRuleRP30Active => false;
	}
}
