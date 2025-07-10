using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class ArrivalCusTransportMeansValidationDecider : IArrivalCusTransportMeansPhase5ValidationDecider
	{
		public bool IsRuleNR0081Active => true;

		public bool IsRuleNR0082Active => true;
	}
}
