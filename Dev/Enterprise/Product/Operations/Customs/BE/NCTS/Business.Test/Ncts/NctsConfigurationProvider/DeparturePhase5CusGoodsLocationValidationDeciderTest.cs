using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class DeparturePhase5CusGoodsLocationValidationDeciderTest : TestCase
{
	public void TestIsRuleC0382Active()
	{
		AssertEquals(true, decider.IsRuleC0382Active);
	}

	public void TestIsRuleC0394Active()
	{
		AssertEquals(true, decider.IsRuleC0394Active);
	}

	public void TestIsRuleNR0013Active()
	{
		AssertEquals(true, decider.IsRuleNR0013Active);
	}

	public void TestIsRuleNR0023Active()
	{
		AssertEquals(false, decider.IsRuleNR0023Active);
	}

	public void TestIsRuleNR0050Active()
	{
		AssertEquals(false, decider.IsRuleNR0050Active);
	}

	public void TestIsRuleNR0063Active()
	{
		AssertEquals(false, decider.IsRuleNR0063Active);
	}

	public void TestIsRuleTR0061Active()
	{
		AssertEquals(true, decider.IsRuleTR0061Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		decider = new DeparturePhase5CusGoodsLocationValidationDecider();
	}
	DeparturePhase5CusGoodsLocationValidationDecider decider;
}
