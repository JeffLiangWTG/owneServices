using System.Windows.Forms;
using CargoWise.Interop;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class MultiLineTextBoxGridHelperTest : TestCase
	{
		public void TestSetupTextBoxAsMultiline()
		{
			using (var textBox = new TextBox())
			{
				MultiLineTextBoxGridHelper.SetupTextBoxAsMultiline(textBox);
				Assert("Should be multiline", textBox.Multiline);
				AssertEquals("ScrollBars", ScrollBars.Both, textBox.ScrollBars);
				Assert("Should accept return", textBox.AcceptsReturn);
				AssertEquals(Control.DefaultFont.FontFamily.Name, textBox.Font.FontFamily.Name);
				AssertEquals(Control.DefaultFont.Size, textBox.Font.Size);
			}
		}

#if !WINZOR

		public void TestShouldProcessCmdKey()
		{
			var message = new Message();
			message.Msg = WindowsMessage.WM_KEYDOWN;
			var keyData = Keys.Enter;
			Assert("Should be processed", MultiLineTextBoxGridHelper.ShouldProcessCmdKey(ref message, keyData));

			keyData = Keys.End;
			Assert("not an Enter key", !MultiLineTextBoxGridHelper.ShouldProcessCmdKey(ref message, keyData));

			keyData = Keys.Enter;
			message.Msg = WindowsMessage.WM_LBUTTONDOWN;
			Assert("not a key down windows message", !MultiLineTextBoxGridHelper.ShouldProcessCmdKey(ref message, keyData));
		}

		public void TestProcessCmdKey()
		{
			var message = new Message();
			message.Msg = WindowsMessage.WM_KEYDOWN;
			var keyData = Keys.Enter;
			using (var textBox = new DataGridTextBox())
			{
				textBox.Text = "First LineSecond Line";
				textBox.IsInEditOrNavigateMode = true;
				textBox.SelectionStart = 10;
				textBox.SelectionLength = 0;

				MultiLineTextBoxGridHelper.ProcessCmdKey(textBox, ref message, keyData);
				AssertEquals("First Line" + System.Environment.NewLine + "Second Line", textBox.Text);
				AssertEquals(12, textBox.SelectionStart);
				AssertEquals(false, textBox.IsInEditOrNavigateMode);
			}
		}

#endif
	}
}
