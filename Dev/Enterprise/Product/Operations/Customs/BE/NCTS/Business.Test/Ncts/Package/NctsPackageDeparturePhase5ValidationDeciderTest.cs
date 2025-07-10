using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsPackageDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1819Active() => AssertEquals(expected: false, validationDecider.IsRuleB1819Active);

	public void TestIsRuleB1919Active() => AssertEquals(expected: false, validationDecider.IsRuleB1919Active);

	public void TestIsRuleC0060Active() => AssertEquals(expected: true, validationDecider.IsRuleC0060Active);

	public void TestIsRuleC0060_1Active() => AssertEquals(expected: false, validationDecider.IsRuleC0060_1Active);

	public void TestIsRuleC0060_2Active() => AssertEquals(expected: false, validationDecider.IsRuleC0060_2Active);

	public void TestIsRuleC0060_3Active() => AssertEquals(expected: false, validationDecider.IsRuleC0060_3Active);

	public void TestIsRuleC0670Active() => AssertEquals(expected: true, validationDecider.IsRuleC0670Active);

	public void TestIsRuleE1111Active() => AssertEquals(expected: true, validationDecider.IsRuleE1111Active);

	public void TestIsRuleNR0003Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0003Active);

	public void TestIsRuleNR0027Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0027Active);

	public void TestIsRuleR0219Active() => AssertEquals(expected: true, validationDecider.IsRuleR0219Active);

	public void TestIsRuleR0220Active() => AssertEquals(expected: false, validationDecider.IsRuleR0220Active);

	public void TestIsRuleR0364_1Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_1Active);

	public void TestIsRuleR0364_2Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_2Active);

	public void TestIsRuleR0364_3Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_3Active);

	public void TestIsRuleTR0066Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0066Active);

	public void TestIsRuleTR0083Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0083Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsPackageDeparturePhase5ValidationDecider();
	}

	INctsPackageDeparturePhase5ValidationDecider validationDecider;
}
