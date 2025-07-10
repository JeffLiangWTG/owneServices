using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyNameFromPK))]
	sealed class CompanyNameFromPKTest : ValueProviderWithLoadControlFactoryTest<CompanyNameFromPK>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany Name>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany Name>", Passes.FirstPass));
			Assert("should not match < company    name       >", !ValueProviderToTest.IsResponsibleForReplacing("< company    name       >", Passes.FirstPass));
			Assert("should not match < company    name       from >", !ValueProviderToTest.IsResponsibleForReplacing("< company    name       from >", Passes.FirstPass));
			Assert("should not match < company    name    from   pk   >", !ValueProviderToTest.IsResponsibleForReplacing("< company    name    from   pk   >", Passes.FirstPass));
			Assert("should match < company    name    from   pk   (\"e3735721-ca6b-4597-acc4-f4a3b309d790\")>", ValueProviderToTest.IsResponsibleForReplacing("< company    name    from   pk   (\"e3735721-ca6b-4597-acc4-f4a3b309d790\")>", Passes.FirstPass));
			Assert("should match < company    name    from   pk   (<CompanyPK>)>", ValueProviderToTest.IsResponsibleForReplacing("< company    name    from   pk   (<CompanyPK>)>", Passes.FirstPass));
			Assert("should match <CompanyNameFromPK(\"e3735721-ca6b-4597-acc4-f4a3b309d790\")>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyNameFromPK(\"e3735721-ca6b-4597-acc4-f4a3b309d790\")>", Passes.FirstPass));
			Assert("should match <CompanyNameFromPK(<CompanyPK>)>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyNameFromPK(<CompanyPK>)>", Passes.FirstPass));
			Assert("should match <CompanyNameFromPK()>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyNameFromPK()>", Passes.FirstPass));
		}

		public override void TestReplacement()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Blah";
			Factory.Save();
			AssertEquals("Blah", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", company.PK), Report));
			AssertEquals("Blah", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK(\"{0}\")>", company.PK), Report));
			AssertEquals("", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", ZGuid.NewZGuid()), Report));
			AssertEquals("", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK(\"{0}\")>", ZGuid.NewZGuid()), Report));
			AssertEquals("", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", ZGuid.Empty), Report));
			AssertEquals("", ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", ""), Report));
		}

		public void TestNoErrorsWithEmptyCompanyGuid()
		{
			var companyName = ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", ZGuid.Empty), Report);
			Assert("should no errors", !Report.ErrorManager.HasErrors);
			companyName = ValueProviderToTest.GetReplacement(string.Format("<CompanyNameFromPK({0})>", ""), Report);
			Assert("should no errors", !Report.ErrorManager.HasErrors);
		}
	}
}
