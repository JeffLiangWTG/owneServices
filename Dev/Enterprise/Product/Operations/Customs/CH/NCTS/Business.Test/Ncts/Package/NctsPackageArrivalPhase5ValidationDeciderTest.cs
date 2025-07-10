using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageArrivalPhase5ValidationDecider))]
sealed class NctsPackageArrivalPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1919Active() => AssertEquals(false, validationDecider.IsRuleB1919Active);

	public void TestIsRuleC0670Active() => AssertEquals(true, validationDecider.IsRuleC0670Active);

	public void TestIsRuleNR0029Active() => AssertEquals(true, validationDecider.IsRuleNR0029Active);

	public void TestIsRuleNR0061Active() => AssertEquals(false, validationDecider.IsRuleNR0061Active);

	public void TestIsRuleR0220Active() => AssertEquals(false, validationDecider.IsRuleR0220Active);

	public void TestIsRuleTR0097Active() => AssertEquals(false, validationDecider.IsRuleTR0097Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsPackageArrivalPhase5ValidationDecider();
	}

	INctsPackageArrivalPhase5ValidationDecider validationDecider;
}
