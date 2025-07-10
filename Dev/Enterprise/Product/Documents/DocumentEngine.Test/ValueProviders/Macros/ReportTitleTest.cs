using System;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ReportTitle))]
	sealed class ReportTitleTest : ValueProviderTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestGetReplacement_ShouldDealWithInvalidMacro()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var testReport = new Report(new DocumentPack(Factory.New<StmMenuItem>()), excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				ValueProviderToTest.GetReplacement("", testReport);
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should match <Report Title>", ValueProviderToTest.IsResponsibleForReplacing("<Report Title>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<Title Of Report>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<The report title>", Passes.FirstPass));
			Assert("should match <ReportTitle>", ValueProviderToTest.IsResponsibleForReplacing("<ReportTitle>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			using (var rpt = new Report(new DocumentPack(), MultipleTemplatesWithOptionalColumnsTemplate))
			{
				rpt.PrepareForRender();
				rpt.ColumnHeadingManager.CurrentConfiguration.Clear();
				rpt.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				AssertEquals("The title should be Sheet Number 1", "Sheet Number 1", ValueProviderToTest.GetReplacement("<ReportTitle>", rpt));
			}
		}

		public void TestWorkSheetCurrentlyBeingProcessedIsNull()
		{
			using (var rpt = new Report(new DocumentPack(), null))
			{
				AssertEquals("Test integrity check - WorkSheetCurrentlyBeingProcessed must be null for test to be valid", null, rpt.WorkSheetCurrentlyBeingProcessed);
				AssertEquals("The title should return null", null, ValueProviderToTest.GetReplacement("<ReportTitle>", rpt));
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report = new Report(new DocumentPack(), MultipleTemplatesWithOptionalColumnsTemplate);
			Report.PrepareForRender();
			Report.ColumnHeadingManager.CurrentConfiguration.Clear();
			Report.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
			Report.ColumnHeadingManager.CurrentConfiguration.Worksheets[Report.WorkSheetCurrentlyBeingProcessed.SheetName].Title = "Sales Trade Profile";
		}

		protected override ValueProvider GetNewValueProvider() => new ReportTitle();

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting multipleTemplatesWithOptionalColumnsTemplate;
		ExcelTemplateForUnitTesting MultipleTemplatesWithOptionalColumnsTemplate
		{
			get
			{
				if (multipleTemplatesWithOptionalColumnsTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.MultipleTemplatesWithOptionalColumns.xls", "MultipleTemplatesWithOptionalColumns.xls");
					multipleTemplatesWithOptionalColumnsTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalColumns.xls", Path.GetFullPath(tempFileName));
				}
				return multipleTemplatesWithOptionalColumnsTemplate;
			}
		}
	}
}
