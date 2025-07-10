using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ReportNameShort))]
	sealed class ReportNameShortTest : ValueProviderTest
	{
		public void TestReplacementWithMacrosInTheReportName()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Germany);

			PrepareRenderer();

			((IReportForUnitTesting)Report).fName = "Shipment Cartage Advice for <CompanyCountry>";
			AssertEquals("Cartage Advice for Germany", ValueProviderToTest.GetReplacement("<ReportNameShort>", Report));

			((IReportForUnitTesting)Report).fName = "Shipment Cartage Advice for <Upper(\"<CompanyCountry>\")>";
			AssertEquals("Cartage Advice for GERMANY", ValueProviderToTest.GetReplacement("<ReportNameShort>", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Reprot Name Short>", !ValueProviderToTest.IsResponsibleForReplacing("<Reprot Name Short>", Passes.FirstPass));
			Assert("should match < report      name       >", ValueProviderToTest.IsResponsibleForReplacing("< report      name         short >", Passes.FirstPass));
			Assert("should match <ReportNameShort>", ValueProviderToTest.IsResponsibleForReplacing("<ReportNameShort>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var template = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				var report = new Report(Pack, template, Guid.Empty, Core.Constants.DataContext.Consol, "Declaration Freaks");
				AssertEquals("Freaks", ValueProviderToTest.GetReplacement("<ReportNameShort>", report));

				report = new Report(Pack, template, Guid.Empty, Core.Constants.DataContext.Consol, "Consol Cartage Advice");
				AssertEquals("Cartage Advice", ValueProviderToTest.GetReplacement("<ReportNameShort>", report));
			}
		}

		public void TestTranslation_Report()
		{
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put("ReportTitle|Shipment Cartage Advice for {0}", new ResourceStringData("", "Karthago Beratung für {0}", "", "Versand Karthago Beratung für {0}", ""));
				PrepareRenderer();

				((IReportForUnitTesting)Report).fName = "Shipment Cartage Advice for <CompanyCountry>";
				AssertEquals("Karthago Beratung für Australia", ValueProviderToTest.GetReplacement("<ReportNameShort>", Report));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTranslation_Document()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			var dataSource = Factory.New<DummyBusinessObject>();
			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (var documentPack = new DocumentPack())
			using (var document = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put("ReportName|Shipment Cartage Advice for {0}", new ResourceStringData("", "Karthago Beratung für {0}", "", "Versand Karthago Beratung für {0}", ""));
				PrepareRenderer();

				((IReportForUnitTesting)document).fName = "Shipment Cartage Advice for <CompanyCountry>";
				AssertEquals("Karthago Beratung für Australia", ValueProviderToTest.GetReplacement("<ReportNameShort>", document));
			}
		}

		public void TestShouldNotStackOverflowWhenReportNameIsCallingItself()
		{
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportNameShort>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<Report  Name Short>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("ABC<ReportNameShort>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportNameShort>ABC");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportNameShort><ReportNameShort>");
		}

		protected override ValueProvider GetNewValueProvider() => new ReportNameShort();

		protected override void PrepareDataForExamplesEvaluate()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var template = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				Report = new Report(Pack, template, Guid.Empty, Core.Constants.DataContext.Consol, "Consol Cartage Advice");
			}
		}

		void AssertShouldNotStackOverflowWhenReportNameIsCallingItself(string reportNameContent)
		{
			Report.Name = reportNameContent;
			ValueProviderToTest.GetReplacement("<ReportNameShort>", Report);

			AssertMultilineASCIIEquals("", @"
Severity: [Error (without error report)] Message: [The macro <ReportNameShort> cannot be used in the 'Name=' field of the config area of the document.] Cell: [N/A]
".Trim(), Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

			Report.ErrorManager.ClearErrors();
		}
	}
}
