namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
{
	protected override bool IsRuleB1811ActiveCore() => false;

	protected override bool IsRuleC0001_2ActiveCore() => CH.Business.FuncsHelper.IsCHNT015V4Active;

	protected override bool IsRuleC0030ActiveCore() => false;

	protected override bool IsRuleC0111ActiveCore() => false;

	protected override bool IsRuleC0186ActiveCore() => false;

	protected override bool IsRuleC0236ActiveCore() => false;

	protected override bool IsRuleR0850ActiveCore() => false;

	protected override bool IsRuleTR0007ActiveCore() => false;

	protected override bool IsRuleTR0067ActiveCore() => false;

	protected override bool IsRuleTR0073ActiveCore() => false;

	protected override bool IsRuleTR0074ActiveCore() => false;

	protected override bool IsRuleTR0075ActiveCore() => false;

	protected override bool IsRuleTR0084ActiveCore() => false;

	protected override EU.NCTS.Business.ValidationRuleMessages GetNewMessagesCore() => new ValidationRuleMessages();
}
