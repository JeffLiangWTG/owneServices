using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal class RichTextBoxSelectionPreserver : IDisposable
	{
		public RichTextBoxSelectionPreserver(RichTextBox richEdit)
		{
			this.RichEdit = richEdit;
			SelectionStart = richEdit.SelectionStart;
			SelectionLength = richEdit.SelectionLength;
		}

		void IDisposable.Dispose()
		{
			if (SelectionLength != -1)
			{
				RichEdit.SelectionLength = SelectionLength;
			}
			if (SelectionStart != -1)
			{
				RichEdit.SelectionStart = SelectionStart;
			}
		}

		readonly RichTextBox RichEdit;
		readonly int SelectionStart;
		readonly int SelectionLength;
	}
}
