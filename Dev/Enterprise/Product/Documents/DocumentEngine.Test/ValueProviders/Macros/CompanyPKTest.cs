using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyPK))]
	sealed class CompanyPKTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <copmany PK>", !ValueProviderToTest.IsResponsibleForReplacing("<copmany PK>", Passes.FirstPass));
			Assert("should match < company    pk       >", ValueProviderToTest.IsResponsibleForReplacing("< company    pk       >", Passes.FirstPass));
			Assert("should match <CompanyPK>", ValueProviderToTest.IsResponsibleForReplacing("<CompanyPK>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, ValueProviderToTest.GetReplacement("<Company PK>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyPK();
		}
	}
}
