using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(SectionPreviewManager))]
	sealed class SectionPreviewManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPreviewDocStripThatIsInCustomizedDocumentElementsOnly()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test, Section 3]
{B}-[This is customized section 3.]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "Customized Document Elements");

			Factory.Save();

			var manager = CreateSectionPreviewManagerToTest("Section 3", "Test");
			manager.Language = Enterprise.Core.SharedConstants.Languages.English;

			using (var excelInterface = new ExcelInterface(manager.CustomizedExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("CustomizedExcelTemplate",
@"{A}-[#Config]
{A}-[ContainsCustomisedSections=True]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is customized section 3.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestPreviewDifferentSectionsForSameTemplateWhenTemplateCacheIsOn()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements]
{A}-[#ConfigurableSection:GEN:Test, Section]
{B}-[This is customized section.]
{A}-[#EndOfReport]");

			var template = helper.CreateTemplate(Factory, "Customized Document Elements");

			Factory.Save();

			var manager1 = CreateSectionPreviewManagerToTest("Section 1", "Test1");
			manager1.Language = Enterprise.Core.SharedConstants.Languages.English;

			var manager2 = CreateSectionPreviewManagerToTest("Section 2", "Test2");
			manager2.Language = Enterprise.Core.SharedConstants.Languages.English;

			using (var excelInterface1 = new ExcelInterface(manager1.SystemExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("CustomizedExcelTemplate",
	@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is section 1.]
{A}-[#EndOfReport]",
					excelInterface1.WorkSheets[0].ToString());
			}

			using (var excelInterface2 = new ExcelInterface(manager2.SystemExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("CustomizedExcelTemplate",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is section 2.]
{A}-[#EndOfReport]",
					excelInterface2.WorkSheets[0].ToString());
			}
		}

		public void TestConstructor()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var dataContext = new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob));
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_ContactType = "CNE";
			documentCommand.SU_DocumentDirection = "ARV";

			var docDataProviders = dummy.DocumentSupporter.GetBODocDataProviders(dataContext, documentCommand);

			var manager = new SectionPreviewManager(Factory, "Section 1", "Test", "My Document", ContactType.Find(documentCommand.SU_ContactType), documentCommand.DocumentDirection, docDataProviders);
			AssertEquals("SectionName", "Section 1", manager.SectionName);
			AssertEquals("Category", "Test", manager.Category);
			AssertEquals("Language", ZString.Empty, manager.Language);
			AssertEquals("Zoom", 100, manager.Zoom);
			AssertEquals("DocumentTitle", "My Document", manager.DocumentTitle);
			AssertEquals("DocDataProviders", docDataProviders, manager.DocDataProviders);
			AssertEquals("SystemExcelTemplate", null, manager.SystemExcelTemplate);
			AssertEquals("CustomizedExcelTemplate", null, manager.CustomizedExcelTemplate);
			AssertEquals("ContactType", ContactType.Consignee, manager.ContactType);
			AssertEquals("DocumentDirection", DocumentDirection.ARV, manager.DocumentDirection);
		}

		public void TestLanguage()
		{
			var manager = CreateSectionPreviewManagerToTest("Section 1", "Test");
			AssertEquals("Pre-condition: Language", ZString.Empty, manager.Language);
			AssertNull("Pre-condition: SystemExcelTemplate", manager.SystemExcelTemplate);
			AssertNull("Pre-condition: CustomizedExcelTemplate", manager.CustomizedExcelTemplate);

			manager.Language = Enterprise.Core.SharedConstants.Languages.English;
			AssertEquals("Language", Enterprise.Core.SharedConstants.Languages.English, manager.Language);

			manager.Language = Enterprise.Core.SharedConstants.Languages.English;
			AssertNotNull("CustomizedExcelTemplate", manager.SystemExcelTemplate);
			using (var excelInterface = new ExcelInterface(manager.SystemExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("SystemExcelTemplate",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is section 1.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			AssertNull("CustomizedExcelTemplate", manager.CustomizedExcelTemplate);

			manager.Language = Enterprise.Core.SharedConstants.Languages.Dutch;
			AssertNotNull("CustomizedExcelTemplate", manager.SystemExcelTemplate);
			using (var excelInterface = new ExcelInterface(manager.SystemExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("SystemExcelTemplate",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[Dit is deel 1.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			AssertNull("CustomizedExcelTemplate", manager.CustomizedExcelTemplate);

			manager.Language = Enterprise.Core.SharedConstants.Languages.French;
			AssertNotNull("CustomizedExcelTemplate", manager.SystemExcelTemplate);
			using (var excelInterface = new ExcelInterface(manager.SystemExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("SystemExcelTemplate",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[C'est l'article 1.]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			AssertNotNull("CustomizedExcelTemplate", manager.CustomizedExcelTemplate);
			using (var excelInterface = new ExcelInterface(manager.CustomizedExcelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("CustomizedExcelTemplate",
@"{A}-[#Config]
{A}-[ContainsCustomisedSections=True]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[This is section 1. (Customized)]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		#region Implementation

		SectionPreviewManager CreateSectionPreviewManagerToTest(ZString sectionName, ZString category)
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var dataContext = new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob));
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var docDataProviders = dummy.DocumentSupporter.GetBODocDataProviders(dataContext, documentCommand);

			return new SectionPreviewManager(Factory, sectionName, category, "Test Document", ContactType.Find(documentCommand.SU_ContactType), documentCommand.DocumentDirection, docDataProviders);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DocumentEngineTestHelper.ClearTemplates();

			SetupEnglishSystemTemplate();
			SetupDutchTranslations();
			SetupFrenchTranslations();
			SetupFrenchCustimizedTemplate();
		}

		protected override void TearDown()
		{
			if (dutchResourceStrings != null)
			{
				dutchResourceStrings.Dispose();
			}
			if (frenchResourceStrings != null)
			{
				frenchResourceStrings.Dispose();
			}
			base.TearDown();
		}

		void SetupEnglishSystemTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, Section 1]
{B}-[This is section 1.]
{A}-[#ConfigurableSection:GEN:Test, Section 2]
{B}-[This is section 2.]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "System Document Elements");
		}

		void SetupDutchTranslations()
		{
			dutchResourceStrings = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Dutch).UseMockData();
			dutchResourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "This is section 1."), new ResourceStringData("", "Dit is deel 1."));
			dutchResourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "This is section 2."), new ResourceStringData("", "Dit is deel 2."));
		}

		void SetupFrenchTranslations()
		{
			frenchResourceStrings = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.French).UseMockData();
			frenchResourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "This is section 1."), new ResourceStringData("", "C'est l'article 1."));
			frenchResourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "This is section 2."), new ResourceStringData("", "C'est l'article 2."));
			frenchResourceStrings.Put(DocBuilderResourceStrings.GetKey(null, "This is section 1. (Customized)"), new ResourceStringData("", "C'est l'article 1. (sur mesure)"));
		}

		void SetupFrenchCustimizedTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [FR-FR]]
{A}-[#ConfigurableSection:GEN:Test, Section 1]
{B}-[This is section 1. (Customized)]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "Customized Document Elements [FR-FR]");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateSectionPreviewManagerToTest("Section 1", "Test");
		}

		IMockResourceStringCache dutchResourceStrings;
		IMockResourceStringCache frenchResourceStrings;

		#endregion
	}
}
