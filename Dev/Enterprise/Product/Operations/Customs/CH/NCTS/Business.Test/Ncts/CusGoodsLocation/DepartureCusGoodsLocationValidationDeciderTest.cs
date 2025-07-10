using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class DepartureCusGoodsLocationValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleC0382Active() => AssertEquals(true, validationDecider.IsRuleC0382Active);

	public void TestIsRuleC0394Active() => AssertEquals(false, validationDecider.IsRuleC0394Active);

	public void TestIsRuleNR0013Active() => AssertEquals(false, validationDecider.IsRuleNR0013Active);

	public void TestIsRuleNR0023Active() => AssertEquals(false, validationDecider.IsRuleNR0023Active);

	public void TestIsRuleNR0050ActiveActive() => AssertEquals(false, validationDecider.IsRuleNR0050Active);

	public void TestIsRuleNR0063Active() => AssertEquals(false, validationDecider.IsRuleNR0063Active);

	public void TestIsRuleTR0061Active() => AssertEquals(true, validationDecider.IsRuleTR0061Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new DepartureCusGoodsLocationValidationDecider();
	}

	DepartureCusGoodsLocationValidationDecider validationDecider;
}
