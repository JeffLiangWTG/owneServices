using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderPhase4ValidationDeciderTest : TestCase
	{
		public void TestIsRuleB1858Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1858Active);
		}

		public void TestIsRuleC010Active()
		{
			AssertEquals(true, validationDecider.IsRuleC011Active);
		}

		public void TestIsRuleC0191Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0191Active);
		}

		public void TestIsRuleC035Active()
		{
			AssertEquals(true, validationDecider.IsRuleC035Active);
		}

		public void TestIsRuleC191Active()
		{
			AssertEquals(true, validationDecider.IsRuleC191Active);
		}

		public void TestIsRuleC547Active()
		{
			AssertEquals(true, validationDecider.IsRuleC547Active);
		}

		public void TestIsRuleC589Active()
		{
			AssertEquals(true, validationDecider.IsRuleC589Active);
		}

		public void TestIsRuleC599Active()
		{
			AssertEquals(true, validationDecider.IsRuleC599Active);
		}

		public void TestIsRuleR0909Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0909Active);
		}

		public void TestIsRuleR902Active()
		{
			AssertEquals(true, validationDecider.IsRuleR902Active);
		}

		public void TestIsRuleR903Active()
		{
			AssertEquals(true, validationDecider.IsRuleR903Active);
		}

		public void TestIsRuleR911Active()
		{
			AssertEquals(true, validationDecider.IsRuleR911Active);
		}

		public void TestIsRuleC011Active()
		{
			AssertEquals(true, validationDecider.IsRuleC011Active);
		}

		public void TestIsRuleC531Active()
		{
			AssertEquals(true, validationDecider.IsRuleC531Active);
		}

		public void TestIsRuleTR9090Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR9090Active);
		}

		public void TestIsRuleTR9095Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR9095Active);
		}

		public void TestIsInBondEntryTypeListValidationActive()
		{
			AssertEquals(true, validationDecider.IsInBondEntryTypeListValidationActive);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsDepartureMovementHeaderPhase4ValidationDecider();
		}

		NctsDepartureMovementHeaderPhase4ValidationDecider validationDecider;
	}
}
