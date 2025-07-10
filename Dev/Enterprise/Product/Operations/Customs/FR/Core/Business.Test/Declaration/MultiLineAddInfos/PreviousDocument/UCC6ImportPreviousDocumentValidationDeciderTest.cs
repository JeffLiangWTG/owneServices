using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class UCC6ImportPreviousDocumentValidationDeciderTest : TestCase
	{
		public void TestIsRuleNAT_088Active()
		{
			AssertEquals(true, validationDecider.IsRuleNAT_088Active);
		}

		public void TestIsRuleNAT_130Active()
		{
			AssertEquals(true, validationDecider.IsRuleNAT_130Active);
		}

		public void TestISRuleNAT_259Active()
		{
			AssertEquals(true, validationDecider.ISRuleNAT_259Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new UCC6ImportPreviousDocumentValidationDecider();
		}

		UCC6ImportPreviousDocumentValidationDecider validationDecider;
	}
}
