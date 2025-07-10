using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Environment
{
	sealed class UserResponseDialogTest : NUnit.Framework.TestCase
	{
		public void TestGetUserResponse()
		{
			const string Message = "Please enter a reason over 20 characters long for this change:";
			const string InvalidReason = "This is invalid.";
			const string ValidReason = "Customs had confirmed the status and the current job is in an unworkable state.";
			using (var dialog = new UserResponseDialog(Message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
			{
				dialog.Show();
				AssertEquals("OK Enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseTextBox.Text = InvalidReason;
				AssertEquals("OK Enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseTextBox.Text = ValidReason;
				AssertEquals("OK Enabled", true, dialog.Button1.Enabled);
				AssertEquals("Accept Button on Form", dialog.Button1, dialog.AcceptButton);
				dialog.Button1.PerformClick();
				AssertEquals("Dialog Result", DialogResult.OK, dialog.DialogResult);
			}
		}

		public void TestGetUserResponseWithButtonText()
		{
			const string Message = "Please enter a reason over 20 characters long for this change:";
			const string InvalidReason = "This is invalid.";
			const string ValidReason = "Customs had confirmed the status and the current job is in an unworkable state.";
			using (var dialog = new UserResponseDialog(Message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, "Send"))
			{
				dialog.Show();
				AssertEquals("Button1 text", "Send", dialog.Button1.Text);
				AssertEquals("Send button enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseTextBox.Text = InvalidReason;
				AssertEquals("Send button enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseTextBox.Text = ValidReason;
				AssertEquals("Send button enabled", true, dialog.Button1.Enabled);
				AssertEquals("Accept Button on Form", dialog.Button1, dialog.AcceptButton);
				dialog.Button1.PerformClick();
				AssertEquals("Dialog Result", DialogResult.OK, dialog.DialogResult);
			}
		}

		public void TestGetUserResponseWithAnswerList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("A", "Answer A");
			list.AddPair("B", "Answer B");

			const string message = "Please enter a reason for this change:";
			const string invalidReason = "C";
			const string validReason = "A";

			using (var dialog = new UserResponseDialog(message, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, list))
			{
				dialog.MaximumResponseLength = dialog.MaximumResponseLength = 1;
				dialog.Show();
				AssertEquals("OK Enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseDropEdit.Text = invalidReason;
				Application.DoEvents();
				UserIdleWorker.Flush();
				AssertEquals("OK Enabled", false, dialog.Button1.Enabled);
				dialog.UserResponseDropEdit.Text = validReason;
				Application.DoEvents();
				UserIdleWorker.Flush();
				AssertEquals("OK Enabled", true, dialog.Button1.Enabled);
				AssertEquals("Accept Button on Form", dialog.Button1, dialog.AcceptButton);
				dialog.Button1.PerformClick();
				AssertEquals("Dialog Result", DialogResult.OK, dialog.DialogResult);
			}
		}

		public void TestUpdateHeightWidthSettings_TextBoxVerticalLocationIsCorrect()
		{
			var multiLineMessage = @"This is a message with multiple lines.
This is the 1st line.
This is the 2nd line.
This is the 3rd line.
This is the 4th line.
This is the 5th line.
This is the 6th line.
This is the 7th line.
Please choose one of the answers below.";

			var answerList = new CodeDescriptionPairList();
			answerList.AddPair("AS1", "Answer Number One");
			answerList.AddPair("AS2", "Answer Number Two");
			answerList.AddPair("AS3", "Answer Number Three");

			using (var dialog = new UserResponseDialog(multiLineMessage, "This Is The Caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, answerList, string.Empty))
			{
				dialog.MinimumResponseLength = 3;
				dialog.MaximumResponseLength = 3;
				dialog.UserResponseTextBoxCharactersCasing = CharacterCasing.Upper;
				dialog.UserResponseDropEditCharactersCasing = CharacterCasing.Normal;
				dialog.UserResponseDropEditOnlyShowCode = false;
				dialog.InternalOnLoad(EventArgs.Empty);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(4), dialog.TextBox.Location.Y);
			}
		}
	}
}
