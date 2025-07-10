using System.Windows.Forms;
using CargoWise.Interop;

namespace Enterprise.Core.Forms
{
	public static class MultiLineTextBoxGridHelper
	{
		public static void SetupTextBoxAsMultiline(TextBox textBox)
		{
			textBox.Multiline = true;
			textBox.ScrollBars = ScrollBars.Both;
			textBox.AcceptsReturn = true;
#if WINZOR
			textBox.ExpandOnEdit = true;
#endif
		}

#if !WINZOR

		public static bool ShouldProcessCmdKey(ref Message m, Keys keyData)
		{
			return (m.Msg == WindowsMessage.WM_KEYDOWN && keyData == Keys.Enter);
		}

		public static bool ProcessCmdKey(DataGridTextBox textBox, ref Message m, Keys keyData)
		{
			var oldSelectionStart = textBox.SelectionStart;
			var newText = textBox.Text.Substring(0, textBox.SelectionStart) + System.Environment.NewLine + textBox.Text.Substring(textBox.SelectionStart + textBox.SelectionLength);

			if (newText.Length <= textBox.MaxLength)
			{
				textBox.Text = newText;
				textBox.Select(oldSelectionStart + System.Environment.NewLine.Length, 0);
				textBox.IsInEditOrNavigateMode = false;
			}

			return false;
		}

#endif

	}
}
