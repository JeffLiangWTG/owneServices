namespace Enterprise.Customs.IT.NCTS.Business;

sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
{
	protected override bool IsRuleB1820_1ActiveCore() => true;

	protected override bool IsRuleB1820_2ActiveCore() => true;

	protected override bool IsRuleB1877_1ActiveCore() => true;

	protected override bool IsRuleB1896ActiveCore() => true;

	protected override bool IsRuleC0065ActiveCore() => false;

	protected override bool IsRuleC0542_1ActiveCore() => true;

	protected override bool IsRuleC0587_1ActiveCore() => true;

	protected override bool IsRuleC0839ActiveCore() => false;

	protected override bool IsRuleE1102ActiveCore() => false;

	protected override bool IsRuleE1102_1ActiveCore() => true;

	protected override bool IsRuleE1104_1ActiveCore() => true;

	protected override bool IsRuleE1401_1ActiveCore() => true;

	protected override bool IsRuleG0090ActiveCore() => true;

	protected override bool IsRuleG0123_1ActiveCore() => true;

	protected override bool IsRuleNR0022ActiveCore() => true;

	protected override bool IsRuleNR0048ActiveCore() => true;

	protected override EU.NCTS.Business.ValidationRuleMessages GetNewMessagesCore() => new ValidationRuleMessages();
}
