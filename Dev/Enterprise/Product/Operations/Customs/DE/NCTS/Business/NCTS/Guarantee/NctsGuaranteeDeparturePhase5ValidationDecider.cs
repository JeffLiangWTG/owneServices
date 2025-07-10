using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsGuaranteeDeparturePhase5ValidationDecider : INctsGuaranteeDeparturePhase5ValidationDecider, IRuleTR0089Decider
	{
		public bool IsRuleB1898_1ActiveForPW_RX_NKCurrency => false;

		public bool IsRuleB2101Active => false;

		public bool IsRuleC0085Active => false;

		public bool IsRuleC0085_1Active => false;

		public bool IsRuleC0085_2Active => false;

		public bool IsRuleC0086Active => true;

		public bool IsRuleC0086_1Active => false;

		public bool IsRuleC0130Active => false;

		public bool IsRuleNR0005Active => false;

		public bool IsRuleNR0014Active => false;

		public bool IsRuleR0318Active => false;

		public bool IsRuleNR0064Active => true;

		public bool IsRuleNR0065Active => true;

		public bool IsRuleR0900Active => false;

		public bool IsRuleR0900_1Active => false;

		public bool IsRuleR0900_2Active => false;

		public bool IsRuleR0900_3Active => false;

		public bool IsRuleTR0019Active => true;

		public bool IsRuleTR0065Active => false;

		public bool IsRuleTR0093Active => true;

		public bool IsRuleTR0096Active => true;

		bool IRuleTR0089Decider.IsActive => true;
	}
}
