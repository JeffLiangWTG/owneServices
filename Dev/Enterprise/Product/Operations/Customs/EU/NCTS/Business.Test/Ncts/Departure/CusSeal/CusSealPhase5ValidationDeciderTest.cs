using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class CusSealPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleN0003Active()
	{
		AssertEquals(false, validationDecider.IsRuleN0003Active);
	}

	public void TestIsRuleTR0045Active()
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
