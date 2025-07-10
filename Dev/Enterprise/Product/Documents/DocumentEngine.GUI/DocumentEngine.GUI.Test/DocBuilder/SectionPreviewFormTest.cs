using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocBuilder.Testing
{
	[TestedType(typeof(SectionPreviewForm))]
	sealed class SectionPreviewFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestEmptySectionPreviewForm()
		{
			var manager = new SectionPreviewManager(Factory, "Blah1", "Blah2", "Blah3", ContactType.All, DocumentDirection.ANY);
			using (var form = new SectionPreviewForm(manager))
			{
			}
		}

		public void TestSectionWithTotalMacro()
		{
			DocumentEngineTestHelper.ClearTemplates();

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#ConfigurableSection:GEN:Test, Section with Total Macro]
{B}-[<Total Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var template = helper.CreateTemplate(Factory, "System Document Elements");
			template.SO_IsSystemDefined = true;

			var manager = CreateSectionPreviewManagerToTest("Section with Total Macro", "Test");

			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (var form = new SectionPreviewForm(manager))
			{
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		SectionPreviewManager CreateSectionPreviewManagerToTest(ZString sectionName, ZString category)
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var dataContext = new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob));
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var docDataProviders = dummy.DocumentSupporter.GetBODocDataProviders(dataContext, documentCommand);

			return new SectionPreviewManager(Factory, sectionName, category, "Test Document", ContactType.Find(documentCommand.SU_ContactType), documentCommand.DocumentDirection, docDataProviders);
		}

		protected override Form GetFormToBashCore()
		{
			var manager = CreateSectionPreviewManagerToTest("Default Page Footer for All Documents", "Footer");
			return new SectionPreviewForm(manager);
		}
	}
}
