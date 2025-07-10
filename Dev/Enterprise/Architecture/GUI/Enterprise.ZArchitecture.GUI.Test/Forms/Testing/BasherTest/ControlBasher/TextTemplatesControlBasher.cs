using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TextTemplatesControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			ZTextBoxBaseContextMenuManager contextMenuManager;
			bool multiline;
			if (control is ZTextBox)
			{
				contextMenuManager = ((ZTextBox)control).contextMenuManager;
				multiline = ((ZTextBox)control).Multiline;
			}
			else if (control is ZRichTextBox)
			{
				multiline = ((ZRichTextBox)control).RichEdit.Multiline;
				contextMenuManager = ((ZRichTextBox)control).contextMenuManager;
			}
			else
			{
				throw new Exception("TextTemplatesControlBasher expecting a ZTextBox or ZRichTextBox");
			}
			if (multiline)
			{
				contextMenuManager.InitializeContextMenu();
				contextMenuManager.textBox.ContextMenuStrip.Show();
				if (contextMenuManager.textBox.ContextMenuStrip.Items[0].Visible)
				{
					((ToolStripMenuItem)contextMenuManager.textBox.ContextMenuStrip.Items[0]).DropDown.Show();
					((ToolStripMenuItem)contextMenuManager.textBox.ContextMenuStrip.Items[0]).DropDown.Hide();
				}
				contextMenuManager.textBox.ContextMenuStrip.Hide();
			}
		}
	}
}
