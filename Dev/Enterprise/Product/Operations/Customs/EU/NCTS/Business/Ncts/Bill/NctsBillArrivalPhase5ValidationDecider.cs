namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsBillArrivalPhase5ValidationDecider : INctsBillArrivalPhase5ValidationDecider
	{
		public bool IsRuleB1964Active => false;
		public bool IsRuleC0909Active => true;
		public bool IsRuleNR0062Active => false;
	}
}
