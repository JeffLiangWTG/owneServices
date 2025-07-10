using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZRichTextBoxTemplatesFactoryTest : TestCaseWithDummy
	{
		public void TestTemplatesMenuLayoutWithManyTemplates()
		{
			Dummy.Z0_Description = @"\\abc";

			using (var form = new RichTextBoxFormForTest(Dummy, "Z0_VarBinaryMax"))
			{
				form.RichTextBox.contextMenuManager.InitializeContextMenu();
				var template = form.RichTextBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_Description = "Test desc";
				template.S8_TemplateText = "random text";
				template.Factory.Save();

				var templates = form.RichTextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals(1, templates.Length);
			}
		}

		class RichTextBoxFormForTest : ZForm
		{
			public RichTextBoxFormForTest(BusinessObject bzo, string bindingMember)
				: base(bzo)
			{
				RichTextBox = new ZRichTextBox();
				BindingSource.SetBindingMember(RichTextBox, bindingMember);
				Controls.Add(RichTextBox);
				Show();
			}

			public readonly ZRichTextBox RichTextBox;
		}
	}
}
