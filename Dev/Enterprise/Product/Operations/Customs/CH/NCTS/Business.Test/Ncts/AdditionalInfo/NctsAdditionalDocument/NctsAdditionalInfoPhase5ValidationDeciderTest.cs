using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsAdditionalInfoPhase5ValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleC0015Active() => AssertEquals(true, validationDecider.IsRuleC0015Active);

	public void TestIsRuleR0023Active() => AssertEquals(true, validationDecider.IsRuleR0023Active);

	public void TestIsRuleE1104_1Active() => AssertEquals(false, validationDecider.IsRuleE1104_1Active);

	public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

	public void TestIsRuleR3060Active() => AssertEquals(true, validationDecider.IsRuleR3060Active);

	public void TestIsRuleR3061Active() => AssertEquals(true, validationDecider.IsRuleR3061Active);

	public void TestIsRuleTR0031Active() => AssertEquals(true, validationDecider.IsRuleTR0031Active);

	public void TestIsRuleTR0032Active() => AssertEquals(false, validationDecider.IsRuleTR0032Active);

	public void TestIsRuleTR0033Active() => AssertEquals(false, validationDecider.IsRuleTR0033Active);

	public void TestIsRuleTR0062Active() => AssertEquals(true, validationDecider.IsRuleTR0062Active);

	public void TestIsRuleE1301Active() => AssertEquals(false, validationDecider.IsRuleE1301Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsAdditionalInfoPhase5ValidationDecider();
	}

	NctsAdditionalInfoPhase5ValidationDecider validationDecider;
}
