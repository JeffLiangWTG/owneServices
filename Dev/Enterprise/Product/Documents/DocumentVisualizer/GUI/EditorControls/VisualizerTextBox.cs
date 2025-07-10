using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentVisualizer.GUI
{
	class VisualizerTextBox : ZTextBox
	{
		public VisualizerTextBox()
		{
			Hotkeys.RegisterHotKey(Keys.Alt | Keys.Enter, InsertNewline, ResString.GetMultilingualString("484d554e-76b8-458f-921b-46eca6bb9f3e", "Insert new line (when multi-line)"));
		}

		protected override void Select(bool directed, bool forward)
		{
			base.Select(directed, forward);
			SelectAll();
		}

		bool InsertNewline(object sender, Keys key)
		{
			if (Multiline)
			{
				var originalSelectionStart = SelectionStart;
				var selectionEnd = SelectionStart + SelectionLength;

				Text = Text.Substring(0, SelectionStart)
					+ System.Environment.NewLine
					+ Text.Substring(selectionEnd, Text.Length - selectionEnd);

				SelectionStart = originalSelectionStart + System.Environment.NewLine.Length;

				ScrollToCaret();
				return true;
			}

			return false;
		}
	}
}
