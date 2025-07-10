using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleConfiguration))]
sealed class AfterCHNT015V4PeriodValidationRuleConfigurationTest : BaseValidationRuleConfigurationTest
{
	public override void TestIsRuleC0001_2Active() => AssertEquals(false, configuration.IsRuleC0001_2Active);

	protected override bool IsCHNT015V4Active => false;
}

[TestedType(typeof(ValidationRuleConfiguration))]
sealed class InCHNT015V4PeriodValidationRuleConfigurationTest : BaseValidationRuleConfigurationTest
{
	public override void TestIsRuleC0001_2Active() => AssertEquals(true, configuration.IsRuleC0001_2Active);
	protected override bool IsCHNT015V4Active => true;
}

abstract class BaseValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
{
	public override void TestIsRuleB1811Active() => AssertEquals(false, configuration.IsRuleB1811Active);

	public override void TestIsRuleC0030Active() => AssertEquals(false, configuration.IsRuleC0030Active);

	public override void TestIsRuleC0111Active() => AssertEquals(false, configuration.IsRuleC0111Active);

	public override void TestIsRuleC0186Active() => AssertEquals(false, configuration.IsRuleC0186Active);

	public override void TestIsRuleC0236Active() => AssertEquals(false, configuration.IsRuleC0236Active);

	public override void TestIsRuleR0850Active() => AssertEquals(false, configuration.IsRuleR0850Active);

	public override void TestIsRuleTR0007Active() => AssertEquals(false, configuration.IsRuleTR0007Active);

	public override void TestIsRuleTR0067Active() => AssertEquals(false, configuration.IsRuleTR0067Active);

	public override void TestIsRuleTR0073Active() => AssertEquals(false, configuration.IsRuleTR0073Active);

	public override void TestIsRuleTR0074Active() => AssertEquals(false, configuration.IsRuleTR0074Active);

	public override void TestIsRuleTR0075Active() => AssertEquals(false, configuration.IsRuleTR0075Active);

	public override void TestIsRuleTR0084Active() => AssertEquals(false, configuration.IsRuleTR0084Active);

	public override void TestGetNewMessages()
	{
		AssertType<ValidationRuleMessages>(configuration.Messages);
	}

	protected abstract bool IsCHNT015V4Active { get; }

	protected override void SetUp()
	{
		base.SetUp();
		nctsPeriodHelper = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, IsCHNT015V4Active);
	}

	protected override void TearDown()
	{
		base.TearDown();
		nctsPeriodHelper?.Dispose();
	}

	IDisposable nctsPeriodHelper;
}
