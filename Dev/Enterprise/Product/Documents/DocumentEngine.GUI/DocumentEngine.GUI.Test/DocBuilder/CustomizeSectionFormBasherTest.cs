using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder.SectionEditing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocBuilder.Testing
{
	[TestedType(typeof(CustomizeSectionForm))]
	sealed class CustomizeSectionFormBasherTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestCopySection_WhenNoDocumentSheetFound()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("NOT Document!!1",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, My Section]
{B}-[My Section]
{A}-[#EndOfReport]");

			helper.CreateTemplate(Factory, "Customized Document Elements [FR-FR]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.Constants.Languages.French);

			using (var form = new CustomizeSectionForm(manager))
			{
				form.Show();
				Assert(form.sectionsGrid.ListManager.Count > 0);

				form.sectionsGrid.Select(0);
				form.copyButton.PerformClick();

				AssertEquals("Error message should be displayed", @"Could not find worksheet named [Document] in template [Customized Document Elements [FR-FR]].
Please verify this template before proceeding.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCopySection_WhenNoItemSelected()
		{
			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.Constants.Languages.French);

			using (var form = new CustomizeSectionForm(manager))
			{
				form.Show();
				while (form.sectionsGrid.ListManager.Count > 0)
				{
					form.sectionsGrid.ListManager.RemoveAt(0);
				}
				Assert(form.sectionsGrid.ListManager.Count == 0);

				form.copyButton.PerformClick();

				AssertEquals("Error message should be displayed", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCopySectionWithTooManyRowsInCustomizedDocuments()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly))
			{
				var template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = "GenericFreightJob";
				template.SO_Name = "Customized Document Elements [FR-FR]";
				var tempFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.Customized Document Elements System Empty With Too Many Rows.xls", "Customized Document Elements System Empty With Too Many Rows.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements System Empty With Too Many Rows.xls", Path.GetFullPath(tempFileName));
				template.SO_Template = excelTemplate.GetAsByteArray();
				Factory.Save();

				var manager = new CustomizeSectionManager(Factory, Core.SharedConstants.Languages.French);

				using (var form = new CustomizeSectionForm(manager))
				{
					form.Show();
					Assert(form.sectionsGrid.ListManager.Count > 0);

					form.sectionsGrid.Select(0);
					form.copyButton.PerformClick();

					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Too many rows (65539): The maximum number of rows supported by this file format is 65536.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			var manager = new CustomizeSectionManager(Factory, Core.Constants.Languages.English);
			return new CustomizeSectionForm(manager);
		}
	}
}


