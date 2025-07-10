using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DeparturePhase5CusGoodsLocationValidationDecider))]
	public class DeparturePhase5CusGoodsLocationValidationDeciderTest : TestCaseWithFactory
	{
		public void TestIsRuleC0382Active() => AssertEquals(true, validationDecider.IsRuleC0382Active);

		public void TestIsRuleC0394Active() => AssertEquals(true, validationDecider.IsRuleC0394Active);

		public void TestIsRuleNR0013Active() => AssertEquals(false, validationDecider.IsRuleNR0013Active);

		public void TestIsRuleNR0023Active() => AssertEquals(false, validationDecider.IsRuleNR0023Active);

		public void TestIsRuleNR0050ActiveActive() => AssertEquals(false, validationDecider.IsRuleNR0050Active);

		public void TestIsRuleNR0063Active() => AssertEquals(false, validationDecider.IsRuleNR0063Active);

		public void TestIsRuleTR0061Active() => AssertEquals(false, validationDecider.IsRuleTR0061Active);

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new DeparturePhase5CusGoodsLocationValidationDecider();
		}

		DeparturePhase5CusGoodsLocationValidationDecider validationDecider;
	}
}
