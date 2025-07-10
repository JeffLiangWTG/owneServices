using System.Collections;
using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI.DocumentMenu;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class CustomisationToolStripMenusMakerTest : CustomisationMenusMakerTest<ToolStripItem>
	{
		protected override string GetText(ToolStripItem menuItem) => menuItem is ToolStripSeparator ? " " : menuItem.Text;

		protected override IList GetMenuItems(Form form) => form.MainMenuStrip.Items;

		protected override void PerformClick(ToolStripItem menuItem) => menuItem.PerformClick();

		protected override ZDocumentsMenuItemHelper<ToolStripItem> GetNewHelper() => new ZDocumentsToolStripMenuHelper();
	}
}
