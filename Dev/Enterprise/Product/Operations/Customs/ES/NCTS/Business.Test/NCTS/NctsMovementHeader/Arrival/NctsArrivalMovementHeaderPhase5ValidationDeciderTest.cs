using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class NctsArrivalMovementHeaderPhase5ValidationDeciderTest : TestCase
	{
		public void TestIsRuleB1858Active()
		{
			AssertEquals(expected: false, validationDecider.IsRuleB1858Active);
		}

		public void TestIsRuleC0191Active() => AssertEquals(expected: true, validationDecider.IsRuleC0191Active);

		public void TestIsRuleNR0009Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0009Active);

		public void TestIsRuleNR0026Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0026Active);

		public void TestIsRuleNR0028Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0028Active);

		public void TestIsRuleNR0076Active() => AssertEquals(expected: true, validationDecider.IsRuleNR0076Active);

		public void TestIsRuleTR0022Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0022Active);

		public void TestIsRuleTR0034Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0034Active);

		public void TestIsRuleTR0042Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0042Active);

		public void TestIsRuleTR0063Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0063Active);

		public void TestIsRuleTR0071Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0071Active);

		public void TestIsRuleTR0072Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0072Active);

		public void TestIsRuleTR0091Active()
		{
			AssertEquals(expected: true, validationDecider.IsRuleTR0091Active);
		}

		public void TestIsRuleTR0098Active() => AssertEquals(expected: false, validationDecider.IsRuleTR0098Active);

		public void TestIsInBondEntryTypeListValidationActive()
		{
			AssertEquals(expected: true, validationDecider.IsInBondEntryTypeListValidationActive);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new NctsArrivalMovementHeaderPhase5ValidationDecider();
		}

		NctsArrivalMovementHeaderPhase5ValidationDecider validationDecider;
	}
}
