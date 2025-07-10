using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsArrivalHeaderContainerPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleNR0029Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0029Active);
	}

	public void TestIsRuleTR0043Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0043Active);
	}

	public void TestIsRuleTR0044Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0044Active);
	}

	public void TestIsRuleTR0045Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0045Active);
	}

	public void TestIsRuleTR0046Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0046Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsArrivalHeaderContainerPhase5ValidationDecider();
	}

	NctsArrivalHeaderContainerPhase5ValidationDecider validationDecider;
}
