using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

class CusAuthorizationUsagePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleG0114Active() => AssertEquals(expected: true, validationDecider.IsRuleG0114Active);

	public void TestIsRuleTR0005Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0005Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new CusAuthorizationUsagePhase5ValidationDecider();
	}

	CusAuthorizationUsagePhase5ValidationDecider validationDecider;
}
