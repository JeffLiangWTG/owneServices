using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class MenuCustomizationTemplateCopierTest : TestCaseWithFactory
	{
		public void TestCopyTemplateForChineseSystemDocumentElements()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document", @"{A}-[Hello World]");

			var template = helper.CreateTemplate(Factory, "System Document Elements [CHT]");
			var dummy = Factory.New<DummyBODocSupportable>();
			var customization = DocumentMenuCustomisation.New(dummy, null);
			var copier = new MenuCustomizationTemplateCopier(customization);

			using (var excelInterface = new ExcelInterface())
			{
				var copiedTemplate = copier.CopyTemplate(template);
				excelInterface.LoadExcelFile(copiedTemplate.SO_Template);

				AssertEquals("excelInterface.WorkSheets[0].ToString()", @"{A}-[Hello World]", excelInterface.WorkSheets[0].ToString());

				var copiedTemplate2 = copier.CopyTemplate(template);
				excelInterface.LoadExcelFile(copiedTemplate2.SO_Template);

				AssertEquals("excelInterface.WorkSheets[0].ToString()", @"{A}-[Hello World]", excelInterface.WorkSheets[0].ToString());
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestCopyTemplateForEnglishSystemDocumentElements()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document", @"{A}-[Hello World]");

			var template = helper.CreateTemplate(Factory, "System Document Elements");
			var dummy = Factory.New<DummyBODocSupportable>();
			var customization = DocumentMenuCustomisation.New(dummy, null);
			var copier = new MenuCustomizationTemplateCopier(customization);

			using (var excelInterface = new ExcelInterface())
			{
				var copiedTemplate = copier.CopyTemplate(template);
				excelInterface.LoadExcelFile(copiedTemplate.SO_Template);

				AssertEquals("excelInterface.WorkSheets[0].ToString()", @"{A}-[Hello World]", excelInterface.WorkSheets[0].ToString());
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestCopyTemplate()
		{
			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document", @"{A}-[Hello World]");

			var template = helper.CreateTemplate(Factory, "Test");
			var dummy = Factory.New<DummyBODocSupportable>();
			var customization = DocumentMenuCustomisation.New(dummy, null);
			var copier = new MenuCustomizationTemplateCopier(customization);

			using (var excelInterface = new ExcelInterface())
			{
				var copiedTemplate = copier.CopyTemplate(template);
				excelInterface.LoadExcelFile(copiedTemplate.SO_Template);

				AssertEquals("excelInterface.WorkSheets[0].ToString()", @"{A}-[Hello World]", excelInterface.WorkSheets[0].ToString());
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}
	}
}
