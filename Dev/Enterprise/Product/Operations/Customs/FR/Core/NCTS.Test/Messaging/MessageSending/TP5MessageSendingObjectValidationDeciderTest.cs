using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	sealed class TP5MessageSendingObjectValidationDeciderTest : TestCase
	{
		public void TestIsRuleC0220Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0220Active);
		}

		public void TestIsRuleC0315Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0315Active);
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
			validationDecider = new TP5MessageSendingObjectValidationDecider();
		}

		INctsHeaderMessageSendingObjectValidationDecider validationDecider;
	}
}
