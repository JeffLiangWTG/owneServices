using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(MessageInstructionForm))]
	sealed class MessageInstructionFormBasherTest : ZFormBasherTest
	{
		public void TestMessageInstructionForm()
		{
			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, false, string.Empty, string.Empty)))
			{
				form.Show();
				var isContinueWithAWaitingForResponseCheckBox = form.Controls.Find("IsContinueWithAWaitingForResponseCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithAWaitingForResponseCheckBox invisible", !isContinueWithAWaitingForResponseCheckBox.Visible);
				var isContinueWithValidationErrorsCheckBox = form.Controls.Find("IsContinueWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithValidationErrorsCheckBox invisible", !isContinueWithValidationErrorsCheckBox.Visible);
				var isContinueWithAdditionalWarningsCheckBox = form.Controls.Find("IsContinueWithAdditionalWarningsCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithAdditionalWarningsCheckBox invisible", !isContinueWithAdditionalWarningsCheckBox.Visible);
				var additionalWarningsTextBox = form.Controls.Find("AdditionalWarningsTextBox", true)[0] as ZTextBox;
				Assert("AdditionalWarningsTextBox invisible", !additionalWarningsTextBox.Visible);
			}

			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, notifStr, warningStr)))
			{
				form.Show();
				var isContinueWithAWaitingForResponseCheckBox = form.Controls.Find("IsContinueWithAWaitingForResponseCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithAWaitingForResponseCheckBox visible", isContinueWithAWaitingForResponseCheckBox.Visible);
				var isContinueWithValidationErrorsCheckBox = form.Controls.Find("IsContinueWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithValidationErrorsCheckBox visible", isContinueWithValidationErrorsCheckBox.Visible);
				var isContinueWithAdditionalWarningsCheckBox = form.Controls.Find("IsContinueWithAdditionalWarningsCheckBox", true)[0] as ZCheckBox;
				Assert("IsContinueWithAdditionalWarningsCheckBox visible", isContinueWithAdditionalWarningsCheckBox.Visible);
				var additionalWarningsTextBox = form.Controls.Find("AdditionalWarningsTextBox", true)[0] as ZTextBox;
				Assert("AdditionalWarningsTextBox visible", additionalWarningsTextBox.Visible);
				AssertEquals("AdditionalWarningsTextBox Text", warningStr, additionalWarningsTextBox.Text);
				var notificatoinsTextBox = form.Controls.Find("NotificatoinsTextBox", true)[0] as ZTextBox;
				Assert("Does not contain \\n", !Regex.IsMatch(notificatoinsTextBox.Text, "(?<!\r)\n"));
				AssertEquals("NotificatoinsTextBox Text", notifStr, notificatoinsTextBox.Text);
			}

			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, notifStr, warningStr, null, false, false)))
			{
				form.Show();
				var securityNotAllowedLabel = form.Controls.Find("SecurityNotAllowedLabel", true)[0] as ZLabel;
				Assert("SecurityNotAllowedLabel visible", securityNotAllowedLabel.Visible);
			}

			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, notifStr, warningStr, null, false, true)))
			{
				form.Show();
				var securityNotAllowedLabel = form.Controls.Find("SecurityNotAllowedLabel", true)[0] as ZLabel;
				Assert("SecurityNotAllowedLabel invisible", !securityNotAllowedLabel.Visible);
			}
		}

		public void TestSendButton_Click()
		{
			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, string.Empty, string.Empty)))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				var isContinueWithAWaitingForResponseCheckBox = form.Controls.Find("IsContinueWithAWaitingForResponseCheckBox", true)[0] as ZCheckBox;
				isContinueWithAWaitingForResponseCheckBox.Checked = true;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(false, form.Visible);
			}
		}

		public void TestzCancelButton_Click()
		{
			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, string.Empty, string.Empty)))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				var zCancelButton = form.Controls.Find("zCancelButton", true)[0] as ZButton;
				AssertEquals(true, form.Visible);
				zCancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
				AssertEquals(false, form.Visible);
			}
		}

		public void TestUpdateSendButtonEnabled()
		{
			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, string.Empty, string.Empty)))
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				Assert("Not Enabled", !sendButton.Enabled);
				var isContinueWithAWaitingForResponseCheckBox = form.Controls.Find("IsContinueWithAWaitingForResponseCheckBox", true)[0] as ZCheckBox;
				isContinueWithAWaitingForResponseCheckBox.Checked = true;
				Assert("Enabled", sendButton.Enabled);
			}
			using (var form = new MessageInstructionForm(new MessageInstruction(Factory, true, notifStr, warningStr)))
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				Assert("Not Enabled", !sendButton.Enabled);
				var isContinueWithAWaitingForResponseCheckBox = form.Controls.Find("IsContinueWithAWaitingForResponseCheckBox", true)[0] as ZCheckBox;
				isContinueWithAWaitingForResponseCheckBox.Checked = true;
				Assert("Not Enabled", !sendButton.Enabled);
				var isContinueWithValidationErrorsCheckBox = form.Controls.Find("IsContinueWithValidationErrorsCheckBox", true)[0] as ZCheckBox;
				isContinueWithValidationErrorsCheckBox.Checked = true;
				Assert("Not Enabled", !sendButton.Enabled);
				var isContinueWithAdditionalWarningsCheckBox = form.Controls.Find("IsContinueWithAdditionalWarningsCheckBox", true)[0] as ZCheckBox;
				isContinueWithAdditionalWarningsCheckBox.Checked = true;
				Assert("Enabled", sendButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore() => new MessageInstructionForm(new MessageInstruction(Factory, false, string.Empty, string.Empty));

		protected override void SetUp()
		{
			base.SetUp();
			Factory.NewWithValidTestData<JobDeclaration>();
			notifStr = "Notif String\\nNotif String";
			warningStr = "Warning String";
			Factory.Save();
		}
		ZString notifStr;
		ZString warningStr;
	}
}
