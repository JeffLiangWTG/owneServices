using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsEuOfficeCodeArrivalPhase5ValidationDecider : INctsEuOfficeCodeArrivalPhase5ValidationDecider
	{
		public bool IsRuleR0006Active => false;
	}
}
