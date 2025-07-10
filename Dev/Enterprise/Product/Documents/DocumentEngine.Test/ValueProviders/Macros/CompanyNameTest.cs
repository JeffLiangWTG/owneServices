using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyName))]
	sealed class CompanyNameTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany Name>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany Name>", Passes.FirstPass));
			Assert("should match < company    name       >", ValueProviderToTest.IsResponsibleForReplacing("< company    name       >", Passes.FirstPass));
			Assert("should match <CompanyName>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, ValueProviderToTest.GetReplacement("<Company Name>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_Name = "WiseTech Global";
		}
	}
}
