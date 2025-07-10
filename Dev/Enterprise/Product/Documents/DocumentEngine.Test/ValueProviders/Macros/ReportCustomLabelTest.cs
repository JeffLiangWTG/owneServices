using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ReportCustomLabel))]
	sealed class ReportCustomLabelTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <ReportCustomLabel(x,x)>", ValueProviderToTest.IsResponsibleForReplacing("<ReportCustomLabel(x,x)>", Passes.FirstPass));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetConfigOrganisation()
		{
			var valueProvider = new TestReportCustomLabel();
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleReportUsingBizObj.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate, "x", ContactType.All, false))
			{
				var configOrg = valueProvider.GetConfigOrganisation(testReport);
				AssertEquals("Should be the currently logged in organisation", configOrg.PK, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			}
		}

		protected override ValueProvider GetNewValueProvider() => new ReportCustomLabel();

		protected override void PrepareDataForExamplesEvaluate()
		{
			var org = GlbCompany.CurrentCompany.OrgProxy;
			var customLabel = org.CustomLabels.AddNew();
			customLabel.OT_FieldName = "CustomText1";
			customLabel.OT_Caption = "PalletValue";
			customLabel.OT_Type = OrgConstants.CustomLabelType.Report;
		}

		sealed class TestReportCustomLabel : ReportCustomLabel
		{
			internal new OrgHeader GetConfigOrganisation(Report report) => base.GetConfigOrganisation(report);
		}
	}
}
