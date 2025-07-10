namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
	{
		protected override bool IsRuleB1811ActiveCore() => false;

		protected override bool IsRuleB1820_1ActiveCore() => true;

		protected override bool IsRuleB1848_1ActiveCore() => true;

		protected override bool IsRuleC0030ActiveCore() => false;

		protected override bool IsRuleC0186ActiveCore() => false;

		protected override bool IsRuleC0337ActiveCore() => false;

		protected override bool IsRuleC0382ActiveCore() => false;

		protected override bool IsRuleC0394ActiveCore() => false;

		protected override bool IsRuleC0505ActiveCore() => false;

		protected override bool IsRuleC0542ActiveCore() => false;

		protected override bool IsRuleC0839ActiveCore() => false;

		protected override bool IsRuleE1102ActiveCore() => false;

		protected override bool IsRuleE1401_1ActiveCore() => true;

		protected override bool IsRuleNR0002ActiveCore() => true;

		protected override bool IsRuleR0076ActiveCore() => true;

		protected override bool IsRuleR0350ActiveCore() => false;

		protected override bool IsRuleR0850ActiveCore() => false;

		protected override bool IsRuleR0850_1ActiveCore() => true;

		protected override bool IsRuleTR0052ActiveCore() => true;

		protected override bool IsRuleTR0084ActiveCore() => false;
	}
}
