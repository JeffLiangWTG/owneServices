namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsPackageDeparturePhase5ValidationDecider : EU.NCTS.Business.INctsPackageDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1819Active => false;
		public bool IsRuleB1919Active => false;
		public bool IsRuleC0060Active => false;
		public bool IsRuleC0060_1Active => false;
		public bool IsRuleC0060_2Active => false;
		public bool IsRuleC0060_3Active => false;
		public bool IsRuleC0670Active => true;
		public bool IsRuleE1111Active => true;
		public bool IsRuleNR0003Active => false;
		public bool IsRuleNR0027Active => false;
		public bool IsRuleR0219Active => false;
		public bool IsRuleR0220Active => false;
		public bool IsRuleR0364_1Active => false;
		public bool IsRuleR0364_2Active => false;
		public bool IsRuleR0364_3Active => true;
		public bool IsRuleTR0066Active => true;
		public bool IsRuleTR0083Active => true;
	}
}
