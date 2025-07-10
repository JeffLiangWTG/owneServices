using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	[TestedType(typeof(ZRichTextBoxPopupForm))]
	public sealed class ZRichTextBoxPopupFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ZRichTextBoxPopupForm(null);
		}

		public void TestPopupFormWillUseOwnerParentAsZParentForm()
		{
			using (var form = new ZForm())
			using (var box = new ZRichTextBox())
			{
				form.Text = "I am a Parent.";
				form.Controls.Add(box);
				form.Show();
				box.ShowPopupEditor();
				AssertEquals(box.ParentZForm, form);
				AssertEquals(box.ParentZForm.Text, form.Text);
				var popup = ZApplication.GetOpenForms().OfType<ZRichTextBoxPopupForm>().Single();
				AssertNotEquals(box.ParentZForm, popup);
				AssertNotEquals(box.ParentZForm.Text, popup.Text);
			}
		}
	}
}
