using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class ArrivalPhase5CusGoodsLocationValidationDeciderTest : TestCaseWithFactory
	{
		public void TestIsRuleC0382Active() => AssertEquals(true, validationDecider.IsRuleC0382Active);

		public void TestIsRuleNR0011Active() => AssertEquals(false, validationDecider.IsRuleNR0011Active);

		public void TestIsRuleNR0012Active() => AssertEquals(false, validationDecider.IsRuleNR0012Active);

		public void TestIsRuleNR0013Active() => AssertEquals(false, validationDecider.IsRuleNR0013Active);

		public void TestIsRuleNR0075Active() => AssertEquals(false, validationDecider.IsRuleNR0075Active);

		public void TestIsRuleTR0061Active() => AssertEquals(true, validationDecider.IsRuleTR0061Active);

		public void TestIsRuleTR0069Active() => AssertEquals(false, validationDecider.IsRuleTR0069Active);

		public void TestIsRuleC0394Active() => AssertEquals(true, validationDecider.IsRuleC0394Active);

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new ArrivalPhase5CusGoodsLocationValidationDecider();
		}

		ArrivalPhase5CusGoodsLocationValidationDecider validationDecider;
	}
}
