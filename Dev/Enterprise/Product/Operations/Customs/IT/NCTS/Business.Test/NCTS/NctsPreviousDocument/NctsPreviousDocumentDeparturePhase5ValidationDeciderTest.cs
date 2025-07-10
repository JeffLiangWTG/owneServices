using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPreviousDocumentDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

	public void TestIsRuleC0298Active() => AssertEquals(true, validationDecider.IsRuleC0298Active);

	public void TestIsRuleNR0008Active() => AssertEquals(false, validationDecider.IsRuleNR0008Active);

	public void TestIsRuleNR0046Active() => AssertEquals(true, validationDecider.IsRuleNR0046Active);

	public void TestIsRuleG0058_1Active() => AssertEquals(true, validationDecider.IsRuleG0058_1Active);

	public void TestIsRuleTR0030_1Active() => AssertEquals(false, validationDecider.IsRuleTR0030_1Active);

	public void TestIsRuleNR0066Active() => AssertEquals(true, validationDecider.IsRuleNR0066Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	NctsPreviousDocumentDeparturePhase5ValidationDecider validationDecider;
}
