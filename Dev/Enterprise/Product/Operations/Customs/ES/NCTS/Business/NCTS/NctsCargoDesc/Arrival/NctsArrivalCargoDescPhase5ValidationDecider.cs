using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsArrivalCargoDescPhase5ValidationDecider : INctsArrivalCargoDescPhase5ValidationDecider
	{
		public bool IsRuleE1109_1Active => false;

		public bool IsRuleNR0004Active => false;

		public bool IsRuleNR0029Active => false;

		public bool IsRuleNR0055Active => true;

		public bool IsRuleNR0058Active => true;

		public bool IsRuleNR0059Active => true;

		public bool IsRuleNR0060Active => true;
	}
}
