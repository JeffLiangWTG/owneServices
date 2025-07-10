using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class TextTemplatesTestHelper
	{
		public static StmNoteTemplate NewTemplate(ZTextBox textBox)
		{
			textBox.contextMenuManager.InitializeContextMenu();
			return textBox.contextMenuManager.TextTemplatesFactory.New();
		}

		public static void ApplyTemplate(ZTextBox textBox, string templateName)
		{
			textBox.contextMenuManager.InitializeContextMenu();
			textBox.ContextMenuStrip.Show();
			var templateMenu = (ToolStripMenuItem)textBox.ContextMenuStrip.Items[0];
			templateMenu.DropDown.Show();
			ToolStripItem item = null;
			for (var i = 0; i < templateMenu.DropDown.Items.Count; i++)
			{
				if (templateMenu.DropDown.Items[i].Text == templateName)
				{
					item = templateMenu.DropDown.Items[i];
					break;
				}
			}
			if (item == null)
			{
				throw new InvalidOperationException("Template '" + templateName + "' does not exist on the textBox '" + textBox.Name + "'");
			}
			item.PerformClick();
		}
	}
}
