namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsArrivalCargoDescPhase5ValidationDecider : INctsArrivalCargoDescPhase5ValidationDecider
	{
		public bool IsRuleE1109_1Active => false;

		public bool IsRuleNR0004Active => true;

		public bool IsRuleNR0029Active => false;

		public bool IsRuleNR0055Active => false;

		public bool IsRuleNR0058Active => false;

		public bool IsRuleNR0059Active => false;

		public bool IsRuleNR0060Active => false;
	}
}
