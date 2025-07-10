using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetCategoriesForReport))]
	sealed class GetCategoriesForReportTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <GetCategoriesForReport(\"BSH\")>", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoriesForReport(\"BSH\")>", Passes.FirstPass));
			Assert("should match <GetCategoriesForReport(BSH)>", ValueProviderToTest.IsResponsibleForReplacing("<GetCategoriesForReport(BSH)>", Passes.FirstPass));
			Assert("should match < GetCategoriesForReport(BSH) >", ValueProviderToTest.IsResponsibleForReplacing("< GetCategoriesForReport(BSH) >", Passes.FirstPass));
			Assert("should match < GetCategoriesForReport ( BSH ) >", ValueProviderToTest.IsResponsibleForReplacing("< GetCategoriesForReport ( BSH ) >", Passes.FirstPass));
			Assert("should not match, arg is missing", !ValueProviderToTest.IsResponsibleForReplacing("<GetCategoriesForReport()>", Passes.FirstPass));
			Assert("should not match, Macro name is wrong", !ValueProviderToTest.IsResponsibleForReplacing("<GetCategories For Report(BSH) >", Passes.FirstPass));
			Assert("should not match, Macro name is wrong", !ValueProviderToTest.IsResponsibleForReplacing("<Get Categories For Report(BSH)>", Passes.SecondPass));
			Assert("should not match, Macro name is wrong", !ValueProviderToTest.IsResponsibleForReplacing("<   GetCategories For Report()  >", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AssertEquals("A01, A02, A03, A04, A05, A01_YED, A02_YED, A03_YED, A04_YED, A05_YED", ValueProviderToTest.GetReplacement("<GetCategoriesForReport(\"TT0\")>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<GetCategoriesForReport()>", Report));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		public void TestAfterGetReplacement_ComplianceReportsSetupsCNNotCopyCategoriesToComplianceReportsSetupsUserDefined()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var list = new ComplianceReportTypeCollection();
			ComplianceReportType complianceReportType1 = list.AddNew();
			complianceReportType1.ReportType = "EFG";

			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Precondition", AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value.Count, 1);
			AssertEquals("Precondition", AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.Count, 8);
			ValueProviderToTest.GetReplacement("<GetCategoriesForReport(\"TT0\")>", Report);
			AssertEquals(AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value.Count, 1);
			AssertNotEquals(AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsUserDefined.Value.Count, 9);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetCategoriesForReport();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
		}
	}
}
