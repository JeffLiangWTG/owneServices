using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleConfiguration))]
sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
{
	public override void TestIsRuleB1820_1Active() => AssertEquals(true, configuration.IsRuleB1820_1Active);

	public override void TestIsRuleB1820_2Active() => AssertEquals(true, configuration.IsRuleB1820_2Active);

	public override void TestIsRuleB1877_1Active() => AssertEquals(true, configuration.IsRuleB1877_1Active);

	public override void TestIsRuleB1896Active() => AssertEquals(true, configuration.IsRuleB1896Active);

	public override void TestIsRuleC0065Active()
	{
		AssertEquals(false, configuration.IsRuleC0065Active);
	}

	public override void TestIsRuleC0542_1Active()
	{
		AssertEquals(true, configuration.IsRuleC0542_1Active);
	}

	public override void TestIsRuleC0587_1Active()
	{
		AssertEquals(true, configuration.IsRuleC0587_1Active);
	}

	public override void TestIsRuleC0839Active()
	{
		AssertEquals(false, configuration.IsRuleC0839Active);
	}

	public override void TestIsRuleE1102Active()
	{
		AssertEquals(false, configuration.IsRuleE1102Active);
	}

	public override void TestIsRuleE1102_1Active()
	{
		AssertEquals(true, configuration.IsRuleE1102_1Active);
	}

	public override void TestIsRuleE1104_1Active()
	{
		AssertEquals(true, configuration.IsRuleE1104_1Active);
	}

	public override void TestIsRuleE1401_1Active()
	{
		AssertEquals(true, configuration.IsRuleE1401_1Active);
	}

	public override void TestIsRuleG0090Active() => AssertEquals(true, configuration.IsRuleG0090Active);

	public override void TestIsRuleG0123_1Active()
	{
		AssertEquals(true, configuration.IsRuleG0123_1Active);
	}

	public override void TestIsRuleNR0022Active()
	{
		AssertEquals(true, configuration.IsRuleNR0022Active);
	}

	public override void TestIsRuleNR0048Active()
	{
		AssertEquals(true, configuration.IsRuleNR0048Active);
	}

	public override void TestGetNewMessages()
	{
		AssertType<ValidationRuleMessages>(configuration.Messages);
	}
}
