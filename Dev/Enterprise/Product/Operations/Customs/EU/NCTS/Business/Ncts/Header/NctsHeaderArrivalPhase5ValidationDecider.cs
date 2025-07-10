namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsHeaderArrivalPhase5ValidationDecider : INctsHeaderArrivalPhase5ValidationDecider
	{
		public bool IsRuleNR0015Active => false;

		public bool IsRuleTR0035Active => true;

		public bool IsRuleTR0047Active => true;
	}
}
