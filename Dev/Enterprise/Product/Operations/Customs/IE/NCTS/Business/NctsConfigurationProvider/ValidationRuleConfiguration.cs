namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
	{
		protected override bool IsRuleB1822ActiveCore() => true;

		protected override bool IsRuleB1896ActiveCore() => true;

		protected override bool IsRuleC0587_1ActiveCore() => true;

		protected override bool IsRuleR0520ActiveCore() => true;
	}
}
