using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CompanyReg1))]
	sealed class CompanyReg1Test : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<copmany reg 1>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    reg1       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< companyreg    1       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< company    reg      1       >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CompanyReg1>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_BusinessRegNo, ValueProviderToTest.GetReplacement("<Company reg1>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CompanyReg1();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "41 065 894 724";
		}
	}
}
