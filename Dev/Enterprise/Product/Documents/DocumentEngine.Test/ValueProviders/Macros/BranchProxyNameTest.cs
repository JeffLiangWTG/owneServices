using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BranchProxyName))]
	sealed class BranchProxyNameTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < BranchProxyName >", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxyName >", Passes.FirstPass));
			Assert("should match < Branch ProxyName >", ValueProviderToTest.IsResponsibleForReplacing("< Branch ProxyName >", Passes.FirstPass));
			Assert("should match < BranchProxy Name >", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy Name >", Passes.FirstPass));
			Assert("should match < Branch Proxy Name >", ValueProviderToTest.IsResponsibleForReplacing("< Branch Proxy Name >", Passes.FirstPass));
			Assert("should match < Branch ProxyName>", ValueProviderToTest.IsResponsibleForReplacing("< Branch ProxyName>", Passes.FirstPass));
			Assert("should match <Branch ProxyName >", ValueProviderToTest.IsResponsibleForReplacing("<Branch ProxyName >", Passes.FirstPass));
			Assert("should match < BranchProxy Name>", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy Name>", Passes.FirstPass));
			Assert("should match <BranchProxy Name >", ValueProviderToTest.IsResponsibleForReplacing("<BranchProxy Name >", Passes.FirstPass));
			Assert("should match <Branch Proxy Name>", ValueProviderToTest.IsResponsibleForReplacing("<Branch Proxy Name>", Passes.FirstPass));
			Assert("should match <BranchProxyName>", ValueProviderToTest.IsResponsibleForReplacing("<BranchProxyName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert("Prerequisite: GlbBranch.CurrentBranch shouldn't be empty", !GlbBranch.CurrentBranch.IsNull);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.OH_FullName, ValueProviderToTest.GetReplacement("<BranchProxyName>", Report));
		}

		public void TestReplacementWhenGlbBranchProxyIsEmpty()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("Should be the CurrentCompanies OrgProxy", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, ValueProviderToTest.GetReplacement("<BranchProxyName>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BranchProxyName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
		}

		public void TestReplacement_NullReferenceException()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<BranchProxyName>", Report));
		}
	}
}
