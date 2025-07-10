using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyOfficeAddress2))]
	sealed class CompanyOfficeAddress2Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyOfficeAddress2 >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyOfficeAddress2 >", Passes.FirstPass));
			Assert("should match < Company Address 1 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Office Address 2 >", Passes.FirstPass));
			Assert("should match < Company Address2 >", ValueProviderToTest.IsResponsibleForReplacing("< Company Office Address2 >", Passes.FirstPass));
			Assert("should match < CompanyAddress 1>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyOfficeAddress 2>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2 = "";
			AssertEquals("Empty address 2", "", ValueProviderToTest.GetReplacement("<CompanyOfficeAddress2>", Report));

			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2 = "OfficeAddress2";
			AssertEquals("Office OA_Address2 should be returned if it exists", "OfficeAddress2", ValueProviderToTest.GetReplacement("<CompanyOfficeAddress2>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyOfficeAddress2();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.OA_Address2 = "Office Address Line 2";
		}
	}
}
