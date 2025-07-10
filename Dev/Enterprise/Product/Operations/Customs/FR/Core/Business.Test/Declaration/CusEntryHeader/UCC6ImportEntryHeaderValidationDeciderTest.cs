using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportEntryHeaderValidationDecider))]
	sealed class UCC6ImportEntryHeaderValidationDeciderTest : EU.Business.Declaration.Testing.EntryHeaderValidationDeciderTest<UCC6ImportEntryHeaderValidationDecider>
	{
		public void TestIsRuleNAT_174Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_174Active);
		}

		public void TestIsRuleNAT_177Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_177Active);
		}

		public void TestIsRuleNAT_178Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_178Active);
		}

		public void TestIsRuleNAT_179Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_179Active);
		}

		public void TestIsRuleNAT_185Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_185Active);
		}

		public void TestIdRuleNAT_189Active()
		{
			var validationDecider = new UCC6ImportEntryHeaderValidationDecider();
			Assert(validationDecider.IsRuleNAT_189Active);
		}
	}
}
