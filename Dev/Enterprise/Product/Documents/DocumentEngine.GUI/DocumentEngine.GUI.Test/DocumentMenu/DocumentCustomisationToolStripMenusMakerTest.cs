using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class DocumentCustomisationToolStripMenusMakerTest : DocumentCustomisationMenusMakerTest<ToolStripItem, DocumentCustomisationToolStripMenusMaker>
	{
		protected override IList GetMenuItems(Form form)
		{
			return form.MainMenuStrip.Items;
		}

		protected override DocumentCustomisationToolStripMenusMaker GetNewDocumentCustomisationMenusMaker(Form form, IDocumentSupportable documentSupportable)
		{
			return new DocumentCustomisationToolStripMenusMaker(form, documentSupportable, null, new ZDocumentsToolStripMenuHelper());
		}

		protected override void PerformClick(ToolStripItem menuItem)
		{
			menuItem.PerformClick();
		}

		protected override string GetText(ToolStripItem menuItem)
		{
			var separator = menuItem as ToolStripSeparator;
			if (separator != null)
			{
				return " ";
			}

			return menuItem.Text;
		}

		protected override string[] GetMenuAsString(ToolStripItem menuItem)
		{
			var toolStripMenuItem = menuItem as ToolStripMenuItem;
			if (toolStripMenuItem != null)
			{
				return new string[] { toolStripMenuItem.Text, toolStripMenuItem.Checked.ToString() };
			}

			return new string[] { "-", "false" };
		}
	}
}
