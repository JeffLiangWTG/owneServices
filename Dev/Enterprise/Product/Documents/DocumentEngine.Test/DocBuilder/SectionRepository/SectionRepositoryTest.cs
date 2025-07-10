using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(SectionRepository))]
	sealed class SectionRepositoryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetDataContextFromTemplateWithDocumentCurrencySetAsAnMacro()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("DocumentCurrency Set To Macro", string.Empty,
@"{A}-[#Config]
{A}-[DataContext=ARInvoice]
{A}-[DocumentCurrency=<Currency.Code>]
{A}-[Name=DocumentCurrency Set To Macro]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			var repository = new SectionRepository(excelTemplate);
			AssertEquals("The DataContext should be Test", "ARInvoice", repository.DataContext);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplateIsCustomisable()
		{
			var repository = new SectionRepository(CustomisableSectionTest);
			AssertEquals("Precondition: repository.AllSections.Count", 6, repository.AllSections.Count);
			AssertEquals("repository.IsCustomisable", true, repository.IsTemplateCustomisable);

			repository = new SectionRepository(new ExcelTemplateForUnitTesting("TestGroupBySections.xls", TestFilesSubFolder.DocumentTestFiles));
			AssertEquals("Precondition: repository.AllSections.Count", 2, repository.AllSections.Count);
			AssertEquals("repository.IsCustomisable", false, repository.IsTemplateCustomisable);
		}

		public void TestExposedConfigSectionProperties()
		{
			var template = new SectionRepository(CustomisableSectionTest);
			AssertEquals("template.TemplateName", "CustomisableSectionTest", template.TemplateName);
			AssertEquals("template.Version", "4.00", template.Version);
			AssertEquals("template.PageStyle", "LetterPortrait", template.PageStyle);
			AssertEquals("template.EmailSubject", "Customisable Section Test", template.EmailSubject);
			AssertEquals("template.DataContext", ".DummyBODocSupportable", template.DataContext);
			AssertEquals("template.DataSource", "DATA:Raindrops keep falling on my head", template.DataSource);
		}

		public void TestAllSections()
		{
			var template = new SectionRepository(CustomisableSectionTest);
			AssertEquals("template.AllSections", typeof(TemplateSectionCollection), template.AllSections.GetType());
			AssertEquals("template.AllSections.Count", 6, template.AllSections.Count);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new SectionRepository(null); });
		}

		protected override BusinessObject GetNewBusinessObject() => new SectionRepository(CustomisableSectionTest);

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ExcelTemplateForUnitTesting customisableSectionTest;
		ExcelTemplateForUnitTesting CustomisableSectionTest
		{
			get
			{
				if (customisableSectionTest == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls", "CustomisableSectionTest.xls");
					customisableSectionTest = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", Path.GetFullPath(tempFileName));
				}
				return customisableSectionTest;
			}
		}
	}
}
