using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CellNotificationSuspenderTest : TestCase
	{
		public void TestCellNotificationSuspender()
		{
			var grid1 = new ZGrid();
			var grid2 = new ZGrid();

			using (var form = new Form())
			{
				form.Controls.Add(grid1);
				var box = new GroupBox();
				form.Controls.Add(box);
				box.Controls.Add(grid2);
				using (var suspender = new CellNotificationSuspender(form))
				{
					AssertEquals(1, grid1.suspendCellNotificationCount);
					AssertEquals(1, grid2.suspendCellNotificationCount);
				}
				AssertEquals(0, grid1.suspendCellNotificationCount);
				AssertEquals(0, grid2.suspendCellNotificationCount);
			}
		}
	}
}
