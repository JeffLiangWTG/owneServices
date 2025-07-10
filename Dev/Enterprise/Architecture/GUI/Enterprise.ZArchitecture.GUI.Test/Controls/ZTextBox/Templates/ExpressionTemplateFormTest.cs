using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ExpressionTemplateForm))]
	sealed class ExpressionTemplateFormTest : ZFormBasherTest
	{
		public void TestPreviewButtonHidden()
		{
			using (var form = GetFormForTest())
			{
				Assert(!form.previewButton.Visible);
			}
		}

		public void TestFormTitle()
		{
			using (var form = GetFormForTest())
			{
				AssertEquals("Expression Template", form.FormCaption);
			}
		}

		ExpressionTemplateForm GetFormForTest()
		{
			return new ExpressionTemplateForm(Factory.New<ExpressionNoteTemplate>(), new BusinessObject[] { Factory.New<DummyBusinessObject>() }, false, null);
		}

		protected override Form GetFormToBashCore()
		{
			return GetFormForTest();
		}
	}
}
