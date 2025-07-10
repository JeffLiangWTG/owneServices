using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class DepartureCusTransportMeansPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB2101Active()
	{
		AssertEquals(true, validationDecider.IsRuleB2101Active);
	}

	public void TestIsRuleG0789_1Active()
	{
		AssertEquals(true, validationDecider.IsRuleG0789_1Active);
	}

	public void TestIsRuleTR0078Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0078Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new DepartureCusTransportMeansPhase5ValidationDecider();
	}

	DepartureCusTransportMeansPhase5ValidationDecider validationDecider;
}
