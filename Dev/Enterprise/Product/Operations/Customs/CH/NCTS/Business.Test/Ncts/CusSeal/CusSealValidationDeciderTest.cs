using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class CusSealValidationDeciderTest : TestCase
{
	public void TestIsRuleNR0029Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0029Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new CusSealValidationDecider();
	}

	CusSealValidationDecider validationDecider;
}
