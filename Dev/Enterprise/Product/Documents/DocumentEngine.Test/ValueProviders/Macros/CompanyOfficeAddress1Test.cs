using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyOfficeAddress1))]
	sealed class CompanyOfficeAddress1Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyOfficeAddress1 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyOfficeAddress1 >", Passes.FirstPass));
			Assert("should match < Company Address 1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Office Address 1 >", Passes.FirstPass));
			Assert("should match < Company Address1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Office Address1 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 1>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyOfficeAddress 1>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1 = "";
			AssertEquals("Empty address 1", "", ValueProviderToTest.GetReplacement("<CompanyOfficeAddress1>", Report));

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1 = "OfficeAddress1";
			AssertEquals("Office OA_Address1 should be returned if it exists", "OfficeAddress1", ValueProviderToTest.GetReplacement("<CompanyOfficeAddress1>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyOfficeAddress1();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address1 = "Office Address Line 1";
		}
	}
}
