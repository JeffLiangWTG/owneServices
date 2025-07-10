using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class SectionPreviewControllerTest : TestCaseWithFactory
	{
		public void TestBindAndChangeZoom()
		{
			var manager = CreateSectionPreviewManagerToTest("Section 1", "Test");
			var view = new DummySectionPreviewView();

			using (var controller = new SectionPreviewController())
			{
				controller.Bind(manager, view);
				AssertEquals("LastZoom", 100, view.LastZoom);

				manager.Zoom = 200;
				AssertEquals("LastZoom", 200, view.LastZoom);
			}

			manager.Zoom = 300;
			AssertEquals("LastZoom should not change as controller has been disposed.", 200, view.LastZoom);
		}

		public void TestBindAndChangeLanguage()
		{
			var manager = CreateSectionPreviewManagerToTest("Section 1", "Test");
			var view = new DummySectionPreviewView();

			using (var controller = new SectionPreviewController())
			{
				controller.Bind(manager, view);
				AssertEquals("DocumentTitle", manager.DocumentTitle, view.LastDocumentTitle);
				Assert("DocDataProviders", manager.DocDataProviders.SequenceEqual(view.LastDocDataProviders));
				AssertEquals("SystemExcelTemplate", manager.SystemExcelTemplate, view.LastSystemExcelTemplate);
				AssertEquals("CustomizedExcelTemplate", manager.CustomizedExcelTemplate, view.LastCustomizedExcelTemplate);
				view.Reset();

				manager.Language = Enterprise.Core.SharedConstants.Languages.Dutch;
				AssertEquals("DocumentTitle", manager.DocumentTitle, view.LastDocumentTitle);
				Assert("DocDataProviders", manager.DocDataProviders.SequenceEqual(view.LastDocDataProviders));
				AssertEquals("SystemExcelTemplate", manager.SystemExcelTemplate, view.LastSystemExcelTemplate);
				AssertEquals("CustomizedExcelTemplate", manager.CustomizedExcelTemplate, view.LastCustomizedExcelTemplate);
				view.Reset();

				manager.Language = Enterprise.Core.SharedConstants.Languages.French;
				AssertEquals("DocumentTitle", manager.DocumentTitle, view.LastDocumentTitle);
				Assert("DocDataProviders", manager.DocDataProviders.SequenceEqual(view.LastDocDataProviders));
				AssertEquals("SystemExcelTemplate", manager.SystemExcelTemplate, view.LastSystemExcelTemplate);
				AssertEquals("CustomizedExcelTemplate", manager.CustomizedExcelTemplate, view.LastCustomizedExcelTemplate);
				view.Reset();
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
			SetupDutchSystemTemplate();
			SetupFrenchSystemTemplate();
			SetupFrenchCustimizedTemplate();
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

		void SetupDutchSystemTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=System Document Elements [DCH]]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "System Document Elements [DCH]");
		}

		void SetupFrenchSystemTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=System Document Elements [FRN]]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "System Document Elements [FRN]");
		}

		void SetupFrenchCustimizedTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [FRN]]
{A}-[#ConfigurableSection:GEN:Test, Section 1]
{B}-[This is section 1. (Customized)]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "Customized Document Elements [FRN]");
		}

		class DummySectionPreviewView : ISectionPreviewView
		{
			internal string LastDocumentTitle;
			internal IBODocDataProvider[] LastDocDataProviders;
			internal ExcelTemplate LastSystemExcelTemplate;
			internal ExcelTemplate LastCustomizedExcelTemplate;
			internal int LastZoom;

			public void UpdateSectionViews(string documentTitle, IBODocDataProvider[] docDataProviders, ExcelTemplate systemExcelTemplate, ExcelTemplate customizedExcelTemplate, ContactType contactType, DocumentDirection documentDirection)
			{
				LastDocumentTitle = documentTitle;
				LastDocDataProviders = docDataProviders;
				LastSystemExcelTemplate = systemExcelTemplate;
				LastCustomizedExcelTemplate = customizedExcelTemplate;
			}

			public void Reset()
			{
				LastDocumentTitle = null;
				LastDocDataProviders = null;
				LastSystemExcelTemplate = null;
				LastCustomizedExcelTemplate = null;
			}

			#region ISectionPreviewView Members

			public void UpdateZoom(int zoom)
			{
				LastZoom = zoom;
			}

			#endregion
		}

		#endregion
	}
}
