using System.Linq;
using System.Windows.Forms;
using Enterprise.EConversation.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(AddInternalLogForm))]
	sealed class AddInternalLogFormTests : ZFormBasherTest
	{
		public void TestOkButtonIsDisabled()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			var manager = new SubscriberAutocompleteHelper(Factory, parent);
			using (var logForm = new AddInternalLogForm(manager))
			{
				logForm.Show();

				var acceptButton = logForm.Controls.Find("okButton", false).Single();
				Assert("The text is empty - can't send", !acceptButton.Enabled);

				var textbox = logForm.Controls.OfType<ZAutoCompleteTextBox>().Single();
				textbox.Text = "Here is some text";

				Assert("There is some text so enable the button", acceptButton.Enabled);

				textbox.Text = "  ";

				Assert("We deleted the text - should be disabled again (whitespace doesnt count)", !acceptButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			return new AddInternalLogForm(new SubscriberAutocompleteHelper(Factory, parent));
		}
	}
}
