using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportEntryLineValidationDecider))]
	sealed class UCC6ImportEntryLineValidationDeciderTest : EU.Business.Declaration.Testing.EntryLineValidationDeciderTest<UCC6ImportEntryLineValidationDecider>
	{
		public void TestIsRuleNAT_175Active()
		{
			var validationDecider = new UCC6ImportEntryLineValidationDecider();
			Assert(validationDecider.IsRuleNAT_175Active);
		}

		public void TestIsRuleNAT_184Active()
		{
			var validationDecider = new UCC6ImportEntryLineValidationDecider();
			Assert(validationDecider.IsRuleNAT_184Active);
		}

		public void TestIdRuleNAT_188Active()
		{
			var validationDecider = new UCC6ImportEntryLineValidationDecider();
			Assert(validationDecider.IsRuleNAT_188Active);
		}
	}
}
