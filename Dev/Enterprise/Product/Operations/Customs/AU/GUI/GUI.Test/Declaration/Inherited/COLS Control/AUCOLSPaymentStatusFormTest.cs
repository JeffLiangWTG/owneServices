using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUCOLSPaymentStatusForm))]
	sealed class AUCOLSPaymentStatusFormTest : ZFormBasherTest
	{
		public void TestClientAccountNumberTextBox()
		{
			using (var form = new AUCOLSPaymentStatusForm())
			{
				AssertEquals("Max length", 12, form.ClientAccountNumberextBox.MaxLength);
			}
		}

		public void TestSendButton()
		{
			using (var form = new AUCOLSPaymentStatusForm())
			{
				form.ClientAccountNumberextBox.Text = "A";
				AssertEquals("Send button is enabled", true, form.SendButton.Enabled);

				form.ClientAccountNumberextBox.Text = string.Empty;
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new AUCOLSPaymentStatusForm();
		}
	}
}
