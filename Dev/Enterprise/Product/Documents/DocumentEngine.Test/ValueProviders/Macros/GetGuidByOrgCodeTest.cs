using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetGuidByOrgCode))]
	sealed class GetGuidByOrgCodeTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider() => new GetGuidByOrgCode();

		void AssertMacroGetGuidByOrgCode(object expected, string macro)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expecting macro to translate", expected, new MacroTranslator(Report).GetValue(macro, Passes.FirstPass));
				Assert("Expected no errors but" + string.Join(System.Environment.NewLine, Report.ErrorManager.ToString()), !Report.ErrorManager.HasErrors);
			});
		}
		public void TestSyntax()
		{
			CombineAssertions(() =>
			{
				Assert("Macro needs to be inside angled brackets", !ValueProviderToTest.IsResponsibleForReplacing(@"GetGuidByOrgCode(""ABCDE"")", Passes.FirstPass));
				Assert("Org Code needs tp be inside parentheses", !ValueProviderToTest.IsResponsibleForReplacing(@"<GetGuidByOrgCode ""ABCDE"">", Passes.FirstPass));
				Assert("Should Pass", ValueProviderToTest.IsResponsibleForReplacing("<GetGuidByOrgCode(\"      ABCDE      \"))>", Passes.FirstPass));
				Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetGuidByOrgCode(\"ABCDE\")>", Passes.FirstPass));
				Assert("Should Pass", ValueProviderToTest.IsResponsibleForReplacing("<GetGuidByOrgCode(ABCDE)>", Passes.FirstPass));
			});
		}

		public void TestGetGuidByOrgCode()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ABCDE";

			Factory.Save();

			AssertMacroGetGuidByOrgCode(organization.PK.ToString(), "<GetGuidByOrgCode(\"ABCDE\")>");
		}

		public void TestOrganizationNotFound()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ABCDE";

			Factory.Save();

			organization.Delete();
			Factory.Save();

			new MacroTranslator(Report).GetValue("<GetGuidByOrgCode(\"ABCDE\")>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Organization not found:", Report.ErrorManager.ToString());
		}

		public void TestOrganizationWithoutQuotationMarks()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ABCDE";

			Factory.Save();

			new MacroTranslator(Report).GetValue("<GetGuidByOrgCode(ABCDE)>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Organization Code must use quotation marks", Report.ErrorManager.ToString());
		}

		public void TestEmptyOrganizationCode()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_Code = "ABCDE";

			Factory.Save();

			new MacroTranslator(Report).GetValue("<GetGuidByOrgCode()>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertContains("Organization Code cannot be empty", Report.ErrorManager.ToString());
		}
	}
}
