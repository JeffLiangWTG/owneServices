using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsHeaderMessageSendingObjectValidationDeciderTest : TestCase
{
	public void TestIsRuleC0220Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0220Active);
	}

	public void TestIsRuleC0315Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0315Active);
	}

	public void TestIsRuleTR0020Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0020Active);
	}

	public void TestIsRuleTR0021Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0021Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsHeaderMessageSendingObjectValidationDecider();
	}

	EU.NCTS.Business.INctsHeaderMessageSendingObjectValidationDecider validationDecider;
}
