using System.Collections;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class ZDocumentToolStripMenuItemTest : ZDocumentMenuTest<ZDocumentToolStripMenuItem, ToolStripItem>
	{
		protected override ToolStripItem GetMenuItem(object childMenuItem)
		{
			if (childMenuItem is ToolStripSeparator)
			{
				return null;
			}

			return base.GetMenuItem(childMenuItem);
		}

		protected override ToolStripItem FindByText(ZDocumentToolStripMenuItem documentMenuItem, string text)
		{
			var menuItem = documentMenuItem.DropDownItems.Find(text, true);
			if (menuItem.Length > 0)
			{
				return menuItem[0];
			}

			return null;
		}

		protected override void PerformClick(ToolStripItem documentMenuItem)
		{
			documentMenuItem.PerformClick();
		}

		protected override void AddToMainMenu(ZForm form, ZDocumentToolStripMenuItem docMenuItem)
		{
			form.MainMenuStrip.Items.Add(docMenuItem);
		}

		protected override string GetText(ToolStripItem menuItem)
		{
			return menuItem.Text;
		}

		protected override bool GetEnabled(ToolStripItem menuItem)
		{
			return menuItem.Enabled;
		}

		protected override string GetShortCut(ToolStripItem menuItem)
		{
			return ((ToolStripMenuItem)menuItem).ShortcutKeys.ToString();
		}

		protected override IList GetItems(ToolStripItem menuItem)
		{
			return ((ToolStripMenuItem)menuItem).DropDownItems;
		}

		protected override ZDocumentsMenuItemHelper<ToolStripItem> HelperForTesting(ZDocumentToolStripMenuItem menuItem)
		{
			return menuItem.HelperForTesting;
		}

		protected override void ResetIsRebuildMenuItemsCalled(ZDocumentToolStripMenuItem menuItem)
		{
			menuItem.HelperForTesting.IsRebuildMenuItemsCalled = false;
		}

		protected override IList GetMainMenuCollection(ZForm form)
		{
			return form.MainMenuStrip.Items;
		}
	}
}
