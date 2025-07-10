using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ConfirmSendForm))]
	class ConfirmSendFormTest : ZFormBasherTest
	{
		public void TestSize()
		{
			using (var form = new ConfirmSendForm())
			{
				form.Show();

				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 270, true), form.MinimumSize);
				AssertEquals("MaximumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 270, true), form.MaximumSize);
				Assert("MinimizeBox", !form.MinimizeBox);
			}
		}

		public void TestComponents()
		{
			using (var form = new ConfirmSendForm())
			{
				form.Show();

				AssertEquals("WarningMessageLabel.Text", expectedWaringMessage, form.WarningMessageLabel.Text);
				AssertEquals("OKButton.Caption", "OK", form.OKButton.CaptionResourceString.Caption);
				AssertEquals("CancelButton.Caption", "Cancel", form.ZCancelButton.CaptionResourceString.Caption);
				AssertEquals("ReasonTextBox.Caption", "Reason", form.ReasonTextBox.CaptionResourceString.Caption);
			}
		}

		public void TestSendButton_Click()
		{
			using (var form = new ConfirmSendForm())
			{
				form.Show();

				form.OKButton.PerformClick();
				AssertEquals("Notification", "Please enter reasons", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ReasonTextBox.Text = "Test Reason";
				form.OKButton.PerformClick();

				AssertEquals("Reason", "Test Reason", form.Reason);
			}
		}

		readonly string expectedWaringMessage = @"Please note that you have not received a response for the message that you have submitted previously.
The consequences of sending duplicate messages means that you will have possibly 2 declarations for the same job.
This will require that at least one of these needs to be manually canceled.
Do you still want to submit another message?

If you are sure you want to submit another message, please enter reasons: ";

		protected override Form GetFormToBashCore() => new ConfirmSendForm();
	}
}
