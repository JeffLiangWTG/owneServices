using CargoWise.EntityFramework.Testing;
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
	[TestedType(typeof(ReportName))]
	sealed class ReportNameTest : ValueProviderTest
	{
		public void TestReplacementWithMacrosInTheReportName()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Germany);

			PrepareRenderer();

			((IReportForUnitTesting)Report).fName = "<CompanyCountry> My Title";
			AssertEquals("Germany My Title", ValueProviderToTest.GetReplacement("<ReportName>", Report));

			((IReportForUnitTesting)Report).fName = "<Upper(\"<CompanyCountry>\")> My Title";
			AssertEquals("GERMANY My Title", ValueProviderToTest.GetReplacement("<ReportName>", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <Reprot Name>", !ValueProviderToTest.IsResponsibleForReplacing("<Reprot Name>", Passes.FirstPass));
			Assert("should match < report      name       >", ValueProviderToTest.IsResponsibleForReplacing("< report      name       >", Passes.FirstPass));
			Assert("should match <ReportName>", ValueProviderToTest.IsResponsibleForReplacing("<ReportName>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals("TestTemplate", ValueProviderToTest.GetReplacement("<ReportName>", Report));
		}

		public void TestTranslation_Report()
		{
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put("ReportTitle|The Title", new ResourceStringData("", "Der Titel"));
				resourceStrings.Put("ReportTitle|{0} My Title", new ResourceStringData("", "{0} Mein Titel"));
				PrepareRenderer();
				((IReportForUnitTesting)Report).fName = "The Title";
				AssertEquals("Der Titel", ValueProviderToTest.GetReplacement("<ReportName>", Report));
				((IReportForUnitTesting)Report).fName = "<CompanyCountry> My Title";
				AssertEquals("Australia Mein Titel", ValueProviderToTest.GetReplacement("<ReportName>", Report));
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTranslation_Document()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			var dataSource = Factory.New<DummyBusinessObject>();
			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (var documentPack = new DocumentPack())
			using (var document = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put("ReportName|The Title", new ResourceStringData("", "Der Titel"));
				resourceStrings.Put("ReportName|{0} My Title", new ResourceStringData("", "{0} Mein Titel"));
				PrepareRenderer();
				((IReportForUnitTesting)document).fName = "The Title";
				AssertEquals("Der Titel", ValueProviderToTest.GetReplacement("<ReportName>", document));
				((IReportForUnitTesting)document).fName = "<CompanyCountry> My Title";
				AssertEquals("Australia Mein Titel", ValueProviderToTest.GetReplacement("<ReportName>", document));
			}
		}

		public void TestShouldNotStackOverflowWhenReportNameIsCallingItself()
		{
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportName>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<Report  Name>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("ABC<ReportName>");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportName>ABC");
			AssertShouldNotStackOverflowWhenReportNameIsCallingItself("<ReportName><ReportName>");
		}

		void AssertShouldNotStackOverflowWhenReportNameIsCallingItself(string reportNameContent)
		{
			Report.Name = reportNameContent;
			ValueProviderToTest.GetReplacement("<ReportName>", Report);

			AssertMultilineASCIIEquals("", @"
Severity: [Error (without error report)] Message: [The macro <ReportName> cannot be used in the 'Name=' field of the config area of the document.] Cell: [N/A]
".Trim(), Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

			Report.ErrorManager.ClearErrors();
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ReportName();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			((IReportForUnitTesting)Report).fName = "Cover Sheet";
		}
	}
}
