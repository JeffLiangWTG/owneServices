using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class EnRouteIncidentValidationDeciderTest : TestCase
{
	public void TestIsRuleC0240_1Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0240_1Active);
	}

	public void TestIsIsRuleTR0010Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0010Active);
	}

	public void TestIsIsRuleTR0012Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0012Active);
	}

	public void TestIsIsRuleTR0013Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0013Active);
	}

	public void TestIsIsRuleTR0014Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0014Active);
	}

	public void TestIsIsRuleTR0015Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0015Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new EnRouteIncidentValidationDecider();
	}

	EnRouteIncidentValidationDecider validationDecider;
}
