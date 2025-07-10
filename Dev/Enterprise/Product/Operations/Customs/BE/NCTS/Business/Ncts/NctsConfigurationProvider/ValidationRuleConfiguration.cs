
namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
{
	protected override bool IsRuleNR0010ActiveCore() => true;

	protected override bool IsRuleNR0053ActiveCore() => true;

	protected override bool IsRuleNR0054ActiveCore() => true;

	protected override bool IsRuleR0850ActiveCore() => false;

	protected override bool IsRuleR0850_1ActiveCore() => true;

	protected override bool IsRuleTR0052ActiveCore() => true;
}
