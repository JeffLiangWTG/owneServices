using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FieldNameCaseValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var textField1 = new TextField(Factory) { DisplayName = "Text Field", TemplateName = "Template 1" };
			Assert(Validator.IsValid(textField1));

			var textField2 = new TextField(Factory) { DisplayName = "TextField", TemplateName = "Template 1" };
			Assert(!Validator.IsValid(textField2));
		}

		public void TestGetErrorMessage()
		{
			var textField1 = new TextField(Factory) { DisplayName = "Text Field", TemplateName = "Template 1" };
			AssertNullOrEmpty(Validator.GetErrorMessage(textField1));

			var textField2 = new TextField(Factory) { DisplayName = "TextField", TemplateName = "Template 1" };
			AssertEquals(@"TextField is duplicate with field(s) below (same name in different case):
'Textfield' in template 'Template 1'
'TexTField' in template 'Template 3'", Validator.GetErrorMessage(textField2));
		}

		FieldNameCaseValidator Validator
		{
			get
			{
				var testField1 = new TextField(Factory) { DisplayName = "Textfield", TemplateName = "Template 1" };
				var testField2 = new TextField(Factory) { DisplayName = "TextField", TemplateName = "Template 2" };
				var testField3 = new TextField(Factory) { DisplayName = "TexTField", TemplateName = "Template 3" };

				var note = Factory.New<DocumentNote>();
				var dummyBo = Factory.New<DummyBODocSupportable>();
				((IDocumentNote)note).MainBusinessObject = dummyBo;
				note.UserDefinedFieldList.Add(testField1);
				note.UserDefinedFieldList.Add(testField2);
				note.UserDefinedFieldList.Add(testField3);
				return new FieldNameCaseValidator(note);
			}
		}
	}
}
