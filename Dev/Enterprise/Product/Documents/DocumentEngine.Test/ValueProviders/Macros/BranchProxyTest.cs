using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(BranchProxy))]
	sealed class BranchProxyTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < BranchProxy >", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy >", Passes.FirstPass));
			Assert("should match < Branch Proxy>", ValueProviderToTest.IsResponsibleForReplacing("< Branch Proxy>", Passes.FirstPass));
			Assert("should match < Branch Proxy >", ValueProviderToTest.IsResponsibleForReplacing("< Branch Proxy >", Passes.FirstPass));
			Assert("should match < BranchProxy>", ValueProviderToTest.IsResponsibleForReplacing("< BranchProxy>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert("Prerequisite: GlbBranch.CurrentBranch shouldn't be empty", !GlbBranch.CurrentBranch.IsNull);
			AssertEquals(GlbBranch.CurrentBranch.GB_OH_OrgProxy, ValueProviderToTest.GetReplacement("<BranchProxy>", Report));
		}

		public void TestReplacementWhenGlbBranchProxyIsEmpty()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("Should be the CurrentCompanies OrgProxy", GlbCompany.CurrentCompany.OrgProxy.PK, ValueProviderToTest.GetReplacement("<BranchProxy>", Report));
		}

		public void TestReplacementWhenGLBOrgProxyIsEmpty()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;

			AssertEquals("Should be the Empty", string.Empty, ValueProviderToTest.GetReplacement("<BranchProxy>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new BranchProxy();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
		}
	}
}
