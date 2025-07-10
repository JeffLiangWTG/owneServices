using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

sealed class NctsPackageArrivalPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1919Active() => AssertEquals(expected: false, validationDecider.IsRuleB1919Active);

	public void TestIsRuleC0670Active() => AssertEquals(expected: true, validationDecider.IsRuleC0670Active);

	public void TestIsRuleNR0029Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0029Active);

	public void TestIsRuleNR0061Active() => AssertEquals(expected: true, validationDecider.IsRuleNR0061Active);

	public void TestIsRuleR0220Active() => AssertEquals(expected: false, validationDecider.IsRuleR0220Active);

	public void TestIsRuleTR0097Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0097Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsPackageArrivalPhase5ValidationDecider();
	}

	NctsPackageArrivalPhase5ValidationDecider validationDecider;
}
