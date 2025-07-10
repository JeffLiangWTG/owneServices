namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsDepartureMovementHeaderPhase4ValidationDecider : INctsDepartureMovementHeaderPhase4ValidationDecider
	{
		public bool IsRuleB1858Active => false;

		public bool IsRuleC010Active => true;

		public bool IsRuleC035Active => true;

		public bool IsRuleC0191Active => true;

		public bool IsRuleC0839_1Active => false;

		public bool IsRuleC191Active => true;

		public bool IsRuleC547Active => true;

		public bool IsRuleC589Active => true;

		public bool IsRuleC599Active => true;

		public bool IsRuleR902Active => true;

		public bool IsRuleR903Active => true;

		public bool IsRuleR911Active => true;

		public bool IsRuleC011Active => true;

		public bool IsRuleC531Active => true;

		public bool IsRuleR0909Active => true;

		public bool IsRuleTR9090Active => true;

		public bool IsRuleTR9095Active => true;

		public bool IsInBondEntryTypeListValidationActive => true;
	}
}
