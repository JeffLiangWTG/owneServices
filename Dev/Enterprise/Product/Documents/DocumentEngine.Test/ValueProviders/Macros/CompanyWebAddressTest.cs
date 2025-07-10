using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyWebAddress))]
	sealed class CompanyWebAddressTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<copmany web addres>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    webaddress       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< companyweb       address       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    web     address       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CompanyWebAddress>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_WebAddress, ValueProviderToTest.GetReplacement("<Company webAddress>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyWebAddress();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_WebAddress = "www.wisetechglobal.com";
		}
	}
}
