namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
	{
		protected override bool IsRuleC0065ActiveCore() => false;

		protected override bool IsRuleC0505ActiveCore() => false;

		protected override bool IsRuleC0587_1ActiveCore() => true;

		protected override bool IsRuleC0587_2ActiveCore() => true;

		protected override bool IsRuleC0839ActiveCore() => false;

		protected override bool IsRuleE1102ActiveCore() => false;

		protected override bool IsRuleNR0002ActiveCore() => true;

		protected override bool IsRuleR0350ActiveCore() => false;

		protected override bool IsRuleR0850ActiveCore() => false;

		protected override bool IsRuleTR0046ActiveCore() => false;

		protected override bool IsRuleTR0055ActiveCore() => true;

		protected override bool IsRuleTR0056ActiveCore() => true;
	}
}
