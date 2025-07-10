using System.Windows.Forms;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(HyperlinkAlertForm))]
	sealed class HyperlinkAlertFormTest : ZFormBasherTest
	{
		public void TestHyperlinkAlertForm()
		{
			var messages = new HyperlinkAlertBusinessObject(new ZString("This record is in use by other record and cannot be deleted."), new ZString("This record is in use by other records in the system and cannot be deleted."));
			using (var form = new HyperlinkAlertForm(messages))
			{
				form.Show();
				AssertNull(form.HyperlinkFormDetailMessageTextBox);
				AssertEquals("View Details", form.HyperlinkFormViewDetailLinkLabel.Text);
				form.HyperlinkFormViewDetailLinkLabel.OnLinkClicked_Exposed(null);
				Application.DoEvents();
				AssertNotNull(form.HyperlinkFormDetailMessageTextBox);
				AssertEquals(messages.MessageLabel, form.HyperlinkFormMessageLabel.Text);
				AssertEquals(messages.LongMessageText, form.HyperlinkFormDetailMessageTextBox.Text);
				AssertEquals("Hide Details", form.HyperlinkFormViewDetailLinkLabel.Text);

				form.HyperlinkFormViewDetailLinkLabel.OnLinkClicked_Exposed(null);
				Application.DoEvents();
				AssertNull(form.HyperlinkFormDetailMessageTextBox);
				AssertEquals("View Details", form.HyperlinkFormViewDetailLinkLabel.Text);

				form.HyperlinkFormViewDetailLinkLabel.OnLinkClicked_Exposed(null);
				Application.DoEvents();
				AssertNotNull(form.HyperlinkFormDetailMessageTextBox);
				AssertEquals(messages.MessageLabel, form.HyperlinkFormMessageLabel.Text);
				AssertEquals(messages.LongMessageText, form.HyperlinkFormDetailMessageTextBox.Text);
				AssertEquals("Hide Details", form.HyperlinkFormViewDetailLinkLabel.Text);

				form.HyperlinkFormOKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var messages = new HyperlinkAlertBusinessObject(new ZString("This record is in use by other record and cannot be deleted."), new ZString("This record is in use by other records in the system and cannot be deleted."));

			return new HyperlinkAlertForm(messages);
		}
		#endregion
	}
}
