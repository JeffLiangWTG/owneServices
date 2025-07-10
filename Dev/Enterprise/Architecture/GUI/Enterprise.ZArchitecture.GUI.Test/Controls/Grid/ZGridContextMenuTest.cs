#if !WINZOR
using System.Windows.Forms;
using Enterprise.Core.Forms.Internal.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridContextMenuTest : MenuItemClickTrackerTest
	{
		protected override Control GetMenuOwnerControl()
		{
			return grid;
		}

		protected override Menu GetMenuToTest()
		{
			return grid.ContextMenu;
		}

		protected override void SetUp()
		{
			form = new Form();
			grid = new ZGrid();
			form.Controls.Add(grid);
			base.SetUp();
		}

		protected override void TearDown()
		{
			form.Dispose();
			grid.Dispose();
			base.TearDown();
		}

		Form form;
		ZGrid grid;
	}
}
#endif
