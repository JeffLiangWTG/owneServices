using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetCategoryDescForReport))]
	sealed class GetCategoryDescForReportTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(\"A01\",\"BSH\")>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(A01,BSH)>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(A01,\"BSH\")>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(\"A01\",BSH)>", Passes.FirstPass));
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(  \"A01\" , \"BSH\" )>", Passes.FirstPass));

			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match, one arg is missing", !ValueProviderToTest.IsResponsibleForReplacing("<GetCategoryDescForReport(\"BSH\")>", Passes.FirstPass));
			Assert("should not match , Macro name is wrong", !ValueProviderToTest.IsResponsibleForReplacing("<GetCategory Desc For Report(A01,BSH)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AssertEquals("Balance Sheet1", ValueProviderToTest.GetReplacement("<GetCategoryDescForReport(\"A01\",\"TT0\")>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<GetCategoryDescForReport()>", Report));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetCategoryDescForReport();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
		}
	}
}
