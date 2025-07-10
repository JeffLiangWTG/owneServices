using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Core.Environment
{
	sealed class UserConfirmationDialogTest : TestCase
	{
		public void TestNoOverlapForLargeMessage()
		{
			var message = "";
			for (var i = 1; i <= 10; ++i)
			{
				message = message + "New Line\r\n";
				var confirmation = "yes";

				using (var dialog = new UserConfirmationDialog(message, "caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
				{
					dialog.ExpectedString = confirmation;
					dialog.ConfirmationPromptLabelText = "Please type the following to continue: " + confirmation;
					dialog.Show();
					Application.DoEvents();
					ZFormModaliser.ShowDialogWithoutDispose(dialog);
					Assert($"At line {i}, ConfirmationPromptLabel ({dialog.ConfirmationPromptLabel.Location}, {dialog.ConfirmationPromptLabel.Size}) is below DialogTextField ({((IDynamicSizedDialog)dialog).DialogTextField.Location}, {((IDynamicSizedDialog)dialog).DialogTextField.Size})"
						, dialog.ConfirmationPromptLabel.Top > ((IDynamicSizedDialog)dialog).DialogTextField.Bottom);
					Assert($"At line {i}, ConfirmationStringTextBox ({dialog.ConfirmationStringTextBox.Location}, {dialog.ConfirmationStringTextBox.Size}) is below DialogTextField ({((IDynamicSizedDialog)dialog).DialogTextField.Location}, {((IDynamicSizedDialog)dialog).DialogTextField.Size})"
						, dialog.ConfirmationStringTextBox.Top > ((IDynamicSizedDialog)dialog).DialogTextField.Bottom);
					Assert($"At line {i}, Button1 ({dialog.Button1.Location}, {dialog.Button1.Size}) is below ConfirmationPromptLabel ({dialog.ConfirmationPromptLabel.Location}, {dialog.ConfirmationPromptLabel.Size})"
						, dialog.Button1.Top > dialog.ConfirmationPromptLabel.Bottom);
					Assert($"At line {i}, Button2 ({dialog.Button2.Location}, {dialog.Button2.Size}) is below ConfirmationPromptLabel ({dialog.ConfirmationPromptLabel.Location}, {dialog.ConfirmationPromptLabel.Size})"
						, dialog.Button2.Top > dialog.ConfirmationPromptLabel.Bottom);
					Assert($"At line {i}, Button1 ({dialog.Button1.Location}, {dialog.Button1.Size}) is in bounds ({dialog.ClientSize})"
						, dialog.Button1.Bottom < dialog.ClientSize.Height);
					Assert($"At line {i}, Button2 ({dialog.Button1.Location}, {dialog.Button2.Size}) is in bounds ({dialog.ClientSize})"
						, dialog.Button2.Bottom < dialog.ClientSize.Height);
				}
			}
		}

		public void TestExpectedMessageAlwaysFits()
		{
			var message = "These objects will be cancelled: Document Type, Document Type";
			var confirmation = "yes";

			using (var dialog = new UserConfirmationDialog(message, "Purge Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Please type the following to continue: " + confirmation;
				ZFormModaliser.ShowDialogWithoutDispose(dialog);
				var maxWidth = CachedScreenInfo.Instance.PrimaryScreenInfo.Width;
				var expectedTextSize = dialog.TextBox.CreateGraphics().MeasureString(dialog.ConfirmationPromptLabelText, dialog.ConfirmationPromptLabel.Font, maxWidth);

				dialog.UpdateHeightWidthSettings();
				Assert("Dialog should not be too wide - Currently - " + dialog.Width, dialog.Width < maxWidth);
				Assert(dialog.ConfirmationPromptLabel.Left > 0);
			}
		}

		public void TestOKEnabled()
		{
			using (var dialog = new UserConfirmationDialog(MessageToConfirm, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
			{
				dialog.ExpectedString = ConfirmationString;
				dialog.Show();
				AssertEquals("OK Enabled", false, dialog.Button1.Enabled);
				dialog.ConfirmationStringTextBox.Text = ConfirmationString;
				AssertEquals("OK Enabled", true, dialog.Button1.Enabled);
				AssertEquals("Accept Button on Form", dialog.Button1, dialog.AcceptButton);
				dialog.Button1.PerformClick();
				AssertEquals("Dialog Result", DialogResult.OK, dialog.DialogResult);
			}
		}

		public void TestSmallerText()
		{
			var message = "Do you really want to purge data from this database?";
			var confirmation = "YesIWantToPurgeDataFromThisDatabase";

			using (var dialog = new UserConfirmationDialog(message, "Purge Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Please type the following to continue: " + confirmation;
				ZFormModaliser.ShowDialogWithoutDispose(dialog);
				var maxWidth = CachedScreenInfo.Instance.PrimaryScreenInfo.Width;
				var expectedTextSize = dialog.TextBox.CreateGraphics().MeasureString(dialog.ConfirmationPromptLabelText, dialog.ConfirmationPromptLabel.Font, maxWidth);
				Assert("User confirmation entry box should resize to fit a large string - Actual: " + dialog.Width.ToString() + " Which should be more than: " + Utilities.Round(new decimal(expectedTextSize.Width), 0).ToString(),
						dialog.ConfirmationPromptLabel.Right < dialog.Width);
				Assert("Dialog should not be too wide - Currently - " + dialog.Width, dialog.Width < maxWidth);
			}
		}

		public void TestLargeText()
		{
			var message = "This is a really really long message that will hopefully not cause the confirmation dialog to take up both my monitors because it looks really crappy. Cuckoo squeakers are really great because they swim around everywhere and have spots and live upside down sometimes... BUT NOT OTHER TIMES!!!! \r\nThis is another line because I don't think it handles multiline messages and just continues along its merry way.";
			var confirmation = "I AGREE THAT CUCKOO SQUEAKERS ARE REALLY GREAT";
			using (var dialog = new UserConfirmationDialog(message, "Do you like Cuckoo Squeakers?", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Watch out for Jackalopes - they appear when you least expect them. Quite often they are hungry and dangerous. Something about cuckoo squeakers and type: " + confirmation;
				dialog.Show();
				var maxWidth = CachedScreenInfo.Instance.PrimaryScreenInfo.Width;
				var expectedTextSize = TextRenderer.MeasureText(dialog.TextBox.CreateGraphics(), dialog.ExpectedStringLabel.Text, dialog.ExpectedStringLabel.Font, new Size(maxWidth, 0), TextFormatFlags.NoClipping);

				Assert("User confirmation entry box should resize to fit a large string", dialog.ConfirmationStringTextBox.Width == expectedTextSize.Width);
				Assert("Dialog should not be too wide - Currently - " + dialog.Width, dialog.Width < maxWidth);
			}
		}

		public void TestDifferentConfirmationMessageLayout()
		{
			var message = "Do you really want to purge data from this database?";
			var confirmation = "Yes";

			using (var dialog = new UserConfirmationDialog(message, "Test", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, ConfirmationMessageLayout.AllInOneLine))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Please type: " + confirmation;
				dialog.Show();

				var textBoxVerticalMiddleAlignmentAdjustment = (dialog.ConfirmationStringTextBox.Height - dialog.ExpectedStringLabel.Height) / 2;

				AssertEquals("Prompt label and expected text label are in same line", dialog.ConfirmationPromptLabel.Location.Y, dialog.ExpectedStringLabel.Location.Y);
				AssertEquals("Prompt label and confirmation textbox are in same line", dialog.ConfirmationPromptLabel.Location.Y, dialog.ConfirmationStringTextBox.Location.Y + textBoxVerticalMiddleAlignmentAdjustment);
			}

			using (var dialog = new UserConfirmationDialog(message, "Test", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, ConfirmationMessageLayout.LineBreakAfterConfirmationPrompt))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Please type: " + confirmation;
				dialog.Show();

				var textBoxVerticalMiddleAlignmentAdjustment = (dialog.ConfirmationStringTextBox.Height - dialog.ExpectedStringLabel.Height) / 2;

				AssertNotEquals("Prompt label and expected text label are in different line", dialog.ConfirmationPromptLabel.Location.Y, dialog.ExpectedStringLabel.Location.Y);
				AssertEquals("Prompt label and expected text label are both left aligned", dialog.ConfirmationPromptLabel.Location.X, dialog.ExpectedStringLabel.Location.X);
				AssertEquals("Expected text label and confirmation textbox are in same line", dialog.ExpectedStringLabel.Location.Y, dialog.ConfirmationStringTextBox.Location.Y + textBoxVerticalMiddleAlignmentAdjustment);
			}

			using (var dialog = new UserConfirmationDialog(message, "Test", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, ConfirmationMessageLayout.LineBreakAfterEachPart))
			{
				dialog.ExpectedString = confirmation;
				dialog.ConfirmationPromptLabelText = "Please type: " + confirmation;
				dialog.Show();

				var textBoxVerticalMiddleAlignmentAdjustment = (dialog.ConfirmationStringTextBox.Height - dialog.ExpectedStringLabel.Height) / 2;

				AssertNotEquals("Prompt label and expected text label are in different line", dialog.ConfirmationPromptLabel.Location.Y, dialog.ExpectedStringLabel.Location.Y);
				AssertEquals("Prompt label and expected text label are both left aligned", dialog.ConfirmationPromptLabel.Location.X, dialog.ExpectedStringLabel.Location.X);
				AssertNotEquals("Expected text label and confirmation textbox are in different line", dialog.ExpectedStringLabel.Location.Y, dialog.ConfirmationStringTextBox.Location.Y + textBoxVerticalMiddleAlignmentAdjustment);
				AssertEquals("Prompt label and confirmation textbox are both left aligned", dialog.ConfirmationPromptLabel.Location.X, dialog.ConfirmationStringTextBox.Location.X);
			}
		}

		public void TestMessageWidthAlwaysFitsIfMessageIsLongerThanPrompt()
		{
			var confirmationString = "Yes";
			var message = "Long long long long message";
			var prompt = "type yes";
			var originalWidthForTextBox = 0;

			using (var dialog = GetUserConfirmationDialog(message, prompt, confirmationString, ConfirmationMessageLayout.LineBreakAfterEachPart))
			{
				ZFormModaliser.ShowDialogWithoutDispose(dialog);
				dialog.UpdateHeightWidthSettings();

				var messageWidth = TextRenderer.MeasureText(message, dialog.TextBox.Font).Width;
				Assert(dialog.TextBox.Width >= messageWidth);

				originalWidthForTextBox = dialog.TextBox.Height;
			}

			message = string.Empty;
			for (var i = 0; i < 620; i++)
			{
				message += 0 + " ";
			}

			using (var dialog = GetUserConfirmationDialog(message, prompt, confirmationString, ConfirmationMessageLayout.LineBreakAfterEachPart))
			{
				ZFormModaliser.ShowDialogWithoutDispose(dialog);
				dialog.UpdateHeightWidthSettings();

				var messageSize = TextRenderer.MeasureText(dialog.TextBox.CreateGraphics(), message, dialog.TextBox.Font, ControlDpiScalingHelper.NewScaledSize(dialog.MaxWidth, 0, false), TextFormatFlags.WordBreak);
				Assert(dialog.TextBox.Width >= messageSize.Width);
				AssertEquals(dialog.TextBox.Height, messageSize.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(15));
				Assert(dialog.TextBox.Height > originalWidthForTextBox);
			}
		}

		public void TestDialogWidthFitsAllCases()
		{
			var layoutToTest = new[]
			{
					ConfirmationMessageLayout.AllInOneLine,
					ConfirmationMessageLayout.LineBreakAfterConfirmationPrompt,
					ConfirmationMessageLayout.LineBreakAfterEachPart
				};

			string[] promptToTest, confirmationStringToTest;

			var shortString = "very short sting";
			var longString = "This Criticality CR3 means 'SINGLE FUNCTION NOT WORKING WITH NO MANUAL WORK AROUND'. But we will append the message so that it will require a line break after measured with length 600.";

			var messagesToTest = promptToTest = confirmationStringToTest = new[] { shortString, longString };

			foreach (var layout in layoutToTest)
			{
				foreach (var message in messagesToTest)
				{
					foreach (var prompt in promptToTest)
					{
						foreach (var confirmationString in confirmationStringToTest)
						{
							AssertDialogWidthFitsAllCases(message, prompt, confirmationString, layout);
						}
					}
				}
			}
		}

		void AssertDialogWidthFitsAllCases(string message, string prompt, string confirmationString, ConfirmationMessageLayout layout)
		{
			using (var dialog = GetUserConfirmationDialog(message, prompt, confirmationString, layout))
			{
				ZFormModaliser.ShowDialogWithoutDispose(dialog);

				var originalWidth = dialog.Width;
				dialog.UpdateHeightWidthSettings();
				var realMaxWidth = dialog.GetRealMaxWidth() + dialog.TextBox.Left * 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(dialog.padding);

				if (originalWidth < realMaxWidth && realMaxWidth < CachedScreenInfo.Instance.PrimaryScreenInfo.Width)
				{
					AssertEquals(dialog.Width, realMaxWidth);
				}
				else
				{
					AssertEquals(dialog.Width, CachedScreenInfo.Instance.PrimaryScreenInfo.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(1));
				}
			}
		}

		UserConfirmationDialog GetUserConfirmationDialog(string message, string prompt, string confirmationString, ConfirmationMessageLayout layout)
		{
			var dialog = new UserConfirmationDialog(message, "Caption", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, layout)
			{
				ExpectedString = confirmationString,
				ConfirmationPromptLabelText = prompt
			};
			return dialog;
		}

		// this test times-out on a few computers from some reason
		//public void TestChangeInputCase()
		//{
		//    string message = "Do you really want to purge data from this database?";
		//    string confirmation = "YesIWantToPurgeDataFromThisDatabase";

		//    using (UserConfirmationDialog dialog = new UserConfirmationDialog(message, "Purge Data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
		//    {
		//        dialog.ExpectedString = confirmation;
		//        dialog.ConfirmationPromptLabelText = "Please type the following to continue: " + confirmation;
		//        dialog.Show();
		//        dialog.ConfirmationStringTextBox.Focus();

		//        SendKeys.Send("y");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send("-");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y-", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send(" ");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y- ", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send("b");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y- B", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send("{BS}");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y- ", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send("{BS}");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y-", dialog.ConfirmationStringTextBox.Text);

		//        SendKeys.Send("A");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("Y-a", dialog.ConfirmationStringTextBox.Text);

		//        dialog.ConfirmationStringTextBox.Text = "YesIW";
		//        dialog.ConfirmationStringTextBox.SelectionStart = 3;
		//        SendKeys.Send("i");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("YesIIW", dialog.ConfirmationStringTextBox.Text);

		//        dialog.ConfirmationStringTextBox.Select(1, 3);
		//        SendKeys.Send("a");
		//        SendKeys.Flush();
		//        Application.DoEvents();
		//        AssertEquals("YaIW", dialog.ConfirmationStringTextBox.Text);

		//        dialog.Close();
		//    }
		//}

		public void TestText()
		{
			using (var dialog = new UserConfirmationDialog(MessageToConfirm, "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1))
			{
				AssertEquals("Text", "Confirm", dialog.Text);
			}
		}

		const string ConfirmationString = "YES";
		const string MessageToConfirm = "Editing the job will force an ammendment to Customs.  This exception will reset the 24 hour clock.  Please type the expected string text below in the text box and press OK to send this message";
	}
}
