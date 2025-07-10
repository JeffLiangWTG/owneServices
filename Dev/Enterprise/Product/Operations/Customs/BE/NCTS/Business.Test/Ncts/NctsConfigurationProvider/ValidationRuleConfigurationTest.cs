using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleConfiguration))]
sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
{
	public override void TestIsRuleNR0010Active() => AssertEquals(true, configuration.IsRuleNR0010Active);

	public override void TestIsRuleNR0053Active() => AssertEquals(true, configuration.IsRuleNR0053Active);

	public override void TestIsRuleNR0054Active() => AssertEquals(true, configuration.IsRuleNR0054Active);

	public override void TestIsRuleR0850Active() => AssertEquals(false, configuration.IsRuleR0850Active);

	public override void TestIsRuleR0850_1Active() => AssertEquals(true, configuration.IsRuleR0850_1Active);

	public override void TestIsRuleTR0052Active() => AssertEquals(true, configuration.IsRuleTR0052Active);
}
