using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BranchProxyCode))]
	sealed class BranchProxyCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < BranchProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxyCode >", Passes.FirstPass));
			Assert("should match < Branch ProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("< Branch ProxyCode >", Passes.FirstPass));
			Assert("should match < BranchProxy Code >", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy Code >", Passes.FirstPass));
			Assert("should match < Branch Proxy Code >", ValueProviderToTest.IsResponsibleForReplacing("< Branch Proxy Code >", Passes.FirstPass));
			Assert("should match < Branch ProxyCode>", ValueProviderToTest.IsResponsibleForReplacing("< Branch ProxyCode>", Passes.FirstPass));
			Assert("should match <Branch ProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("<Branch ProxyCode >", Passes.FirstPass));
			Assert("should match < BranchProxy Code>", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy Code>", Passes.FirstPass));
			Assert("should match <BranchProxy Code >", ValueProviderToTest.IsResponsibleForReplacing("<BranchProxy Code >", Passes.FirstPass));
			Assert("should match <Branch Proxy Code>", ValueProviderToTest.IsResponsibleForReplacing("<Branch Proxy Code>", Passes.FirstPass));
			Assert("should match <BranchProxyCode>", ValueProviderToTest.IsResponsibleForReplacing("<BranchProxyCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert("Prerequisite: GlbBranch.CurrentBranch shouldn't be empty", !GlbBranch.CurrentBranch.IsNull);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.OH_Code, ValueProviderToTest.GetReplacement("<BranchProxyCode>", Report));
		}

		public void TestReplacementWhenGlbBranchProxyIsEmpty()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("Should be the CurrentCompanies OrgProxy", GlbCompany.CurrentCompany.OrgProxy.OH_Code, ValueProviderToTest.GetReplacement("<BranchProxyCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BranchProxyCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
		}
	}
}
