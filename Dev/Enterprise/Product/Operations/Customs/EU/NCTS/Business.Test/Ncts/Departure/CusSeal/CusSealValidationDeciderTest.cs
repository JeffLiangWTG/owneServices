using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class CusSealValidationDeciderTest : TestCase
{
	public void TestIsRuleNR0029Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0029Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new CusSealValidationDecider();
	}

	CusSealValidationDecider validationDecider;
}
