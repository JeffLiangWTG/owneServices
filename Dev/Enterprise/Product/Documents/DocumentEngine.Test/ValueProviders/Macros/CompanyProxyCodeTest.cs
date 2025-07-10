using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyProxyCode))]
	sealed class CompanyProxyCodeTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < CompanyProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyProxyCode >", Passes.FirstPass));
			Assert("should match < Company ProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("< Company ProxyCode >", Passes.FirstPass));
			Assert("should match < CompanyProxy Code >", ValueProviderToTest.IsResponsibleForReplacing("< CompanyProxy Code >", Passes.FirstPass));
			Assert("should match < Company Proxy Code >", ValueProviderToTest.IsResponsibleForReplacing("< Company Proxy Code >", Passes.FirstPass));
			Assert("should match < Company ProxyCode>", ValueProviderToTest.IsResponsibleForReplacing("< Company ProxyCode>", Passes.FirstPass));
			Assert("should match <Company ProxyCode >", ValueProviderToTest.IsResponsibleForReplacing("<Company ProxyCode >", Passes.FirstPass));
			Assert("should match < CompanyProxy Code>", ValueProviderToTest.IsResponsibleForReplacing("< CompanyProxy Code>", Passes.FirstPass));
			Assert("should match <CompanyProxy Code >", ValueProviderToTest.IsResponsibleForReplacing("<CompanyProxy Code >", Passes.FirstPass));
			Assert("should match <Company Proxy Code>", ValueProviderToTest.IsResponsibleForReplacing("<Company Proxy Code>", Passes.FirstPass));
			Assert("should match <CompanyProxyCode>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyProxyCode>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.OH_Code, ValueProviderToTest.GetReplacement("<CompanyProxyCode>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyProxyCode();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_Code = "WTG";
		}
	}
}
