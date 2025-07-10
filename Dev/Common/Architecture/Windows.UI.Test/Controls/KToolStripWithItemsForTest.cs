using System.Windows.Forms;

namespace CargoWise.Windows.UI.Testing
{
	sealed class KToolStripWithItemsForTest : KToolStrip
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		public KToolStripWithItemsForTest()
		{
			for (int i = 0; i < 5; i++)
			{
				var toolStripMenuItem = new ToolStripMenuItem(i.ToString());
				toolStripMenuItem.DropDown = new KDropDownForTest();

				this.Items.Add(toolStripMenuItem);
			}
		}
	}
}
