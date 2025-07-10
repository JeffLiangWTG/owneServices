using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyReg2))]
	sealed class CompanyReg2Test : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyReg2();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<copmany reg 2>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    reg2       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< companyreg    2       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    reg      2       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CompanyReg2>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_BusinessRegNo2, ValueProviderToTest.GetReplacement("<Company reg2>", Report));
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_BusinessRegNo2 = "C065894724";
		}
	}
}
