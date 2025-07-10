using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusSealPhase5ValidationDeciderTest : TestCase
{
	public void IsRuleNR0029Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0029Active);
	}

	public void TestIsRuleN0003Active()
	{
		AssertEquals(true, validationDecider.IsRuleN0003Active);
	}

	public void IsRuleTR0045Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0045Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new CusSealPhase5ValidationDecider();
	}

	CusSealPhase5ValidationDecider validationDecider;
}
