using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(EntryHeaderValidationDeciderTestContext))]
	sealed class EntryHeaderValidationDeciderTestContextTest : TestCaseWithFactory
	{
		public void TestBaseRulesEnableAndDisable()
		{
			using var testContext = new EntryHeaderValidationDeciderTestContext(entryHeader);

			testContext.EnableRule(x => x.IsRuleNAT_174Active);
			AssertEquals("IsRuleNAT_174Active enabled", true, ValidationDecider.IsRuleNAT_174Active);

			testContext.DisableRule(x => x.IsRuleNAT_174Active);
			AssertEquals("IsRuleNAT_174Active disabled", false, ValidationDecider.IsRuleNAT_174Active);
		}

		IEntryHeaderValidationDecider ValidationDecider => (IEntryHeaderValidationDecider)entryHeader.Validation.ValidationDecider;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
