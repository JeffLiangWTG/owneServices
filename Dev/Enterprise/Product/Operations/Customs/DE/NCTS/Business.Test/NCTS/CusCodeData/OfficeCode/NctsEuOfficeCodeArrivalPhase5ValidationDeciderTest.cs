using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsEuOfficeCodeArrivalPhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleR0006Active()
		{
			AssertEquals(false, validationDecider.IsRuleR0006Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsEuOfficeCodeArrivalPhase5ValidationDecider();
		}

		NctsEuOfficeCodeArrivalPhase5ValidationDecider validationDecider;
	}
}
