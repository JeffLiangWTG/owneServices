using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(EntryLineValidationDeciderTestContext))]
	sealed class EntryLineValidationDeciderTestContextTest : TestCaseWithFactory
	{
		public void TestBaseRulesEnableAndDisable()
		{
			using var testContext = new EntryLineValidationDeciderTestContext(entryLine);

			testContext.EnableRule(x => x.IsRuleNAT_175Active);
			AssertEquals("IsRuleNAT_175Active enabled", true, ValidationDecider.IsRuleNAT_175Active);

			testContext.DisableRule(x => x.IsRuleNAT_175Active);
			AssertEquals("IsRuleNAT_175Active disabled", false, ValidationDecider.IsRuleNAT_175Active);
		}

		IEntryLineValidationDecider ValidationDecider => (IEntryLineValidationDecider)entryLine.Validation.ValidationDecider;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		}

		JobDeclaration declaration;
		CusEntryLine entryLine;
	}
}
