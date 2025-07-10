using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsDepartureHeaderContainerPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleC0055Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0055Active);
	}

	public void TestIsRuleN0003Active()
	{
		AssertEquals(false, validationDecider.IsRuleN0003Active);
	}

	public void TestIsRuleR0448Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0448Active);
	}

	public void TestIsRuleTR0043Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0043Active);
	}

	public void TestIsRuleTR0044Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0044Active);
	}

	public void TestIsRuleTR0045Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0045Active);
	}

	public void TestIsRuleTR0046Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0046Active);
	}

	public void TestIsRuleTR0095Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0095Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsDepartureHeaderContainerPhase5ValidationDecider();
	}

	NctsDepartureHeaderContainerPhase5ValidationDecider validationDecider;
}
