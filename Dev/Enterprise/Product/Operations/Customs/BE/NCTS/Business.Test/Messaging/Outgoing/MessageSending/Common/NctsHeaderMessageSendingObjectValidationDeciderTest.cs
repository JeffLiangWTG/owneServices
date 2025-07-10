using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class NctsHeaderMessageSendingObjectValidationDeciderTest : TestCase
	{
		public void TestIsRuleC0220Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0220Active);
		}

		public void TestIsRuleC0315Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0315Active);
		}

		public void TestIsRuleTR0020Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0020Active);
		}

		public void TestIsRuleTR0021Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0021Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsHeaderMessageSendingObjectValidationDecider();
		}

		EU.NCTS.Business.INctsHeaderMessageSendingObjectValidationDecider validationDecider;
	}
}
