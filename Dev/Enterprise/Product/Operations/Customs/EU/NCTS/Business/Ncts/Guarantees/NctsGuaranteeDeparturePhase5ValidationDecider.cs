namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsGuaranteeDeparturePhase5ValidationDecider : INctsGuaranteeDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1898_1ActiveForPW_RX_NKCurrency => false;

		public bool IsRuleB2101Active => false;

		public bool IsRuleC0085Active => true;

		public bool IsRuleC0085_1Active => false;

		public bool IsRuleC0085_2Active => false;

		public bool IsRuleC0086Active => true;

		public bool IsRuleC0086_1Active => false;

		public bool IsRuleC0130Active => true;

		public bool IsRuleNR0005Active => false;

		public bool IsRuleNR0014Active => false;

		public bool IsRuleNR0064Active => false;

		public bool IsRuleNR0065Active => false;

		public bool IsRuleR0318Active => true;

		public bool IsRuleR0900Active => true;

		public bool IsRuleR0900_1Active => false;

		public bool IsRuleR0900_2Active => false;

		public bool IsRuleR0900_3Active => false;

		public bool IsRuleTR0019Active => true;

		public bool IsRuleTR0065Active => false;

		public bool IsRuleTR0093Active => false;

		public bool IsRuleTR0096Active => true;
	}

	public interface IRuleNR0067Decider
	{
		bool IsActive { get; }
	}

	public interface IRuleTR0089Decider
	{
		bool IsActive { get; }
	}
}
