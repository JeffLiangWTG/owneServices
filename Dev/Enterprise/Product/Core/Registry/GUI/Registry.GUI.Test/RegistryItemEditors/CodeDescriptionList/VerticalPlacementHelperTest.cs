using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class VerticalPlacementHelperTest : TestCase
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new VerticalPlacementHelper(null));

			AssertNoExceptionThrown(() => new VerticalPlacementHelper(new Mock<IVerticalPlacementClient>().Object));
		}

		[ExpectNoExceptions]
		public void TestAdjustVerticalPlacement()
		{
			if (ControlDpiScalingHelper.ScaleToCurrentDpiY(100) != 100)
			{
				return; // M.K - no idea how those magic numbers are calculated
			}

			using (var form = new Form())
			using (var grid = new ZGrid())
			{
				grid.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
				grid.Dock = DockStyle.Top;
				form.Controls.Add(grid);

				var verticalPlacementClient = new Mock<IVerticalPlacementClient>();

				verticalPlacementClient.Setup(m => m.Grids).Returns(new[] { grid });
				verticalPlacementClient.Setup(m => m.GridMaxHeight).Returns(300);

				verticalPlacementClient.Setup(m => m.SetGridHeight(It.Is<int>(p => p == 80)));
				verticalPlacementClient.Setup(m => m.SetAdditionalControlsPosition(It.Is<int>(p => p == grid.Height + 12)));

				form.Show();

				var helper = new VerticalPlacementHelper(verticalPlacementClient.Object);
				helper.AdjustVerticalPlacement();

				verticalPlacementClient.Verify(m => m.Grids, Times.AtLeastOnce);
				verticalPlacementClient.Verify(m => m.GridMaxHeight, Times.AtLeastOnce);

				verticalPlacementClient.Verify(m => m.SetGridHeight(It.Is<int>(p => p == 80)), Times.Once);
				verticalPlacementClient.Verify(m => m.SetAdditionalControlsPosition(It.Is<int>(p => p == grid.Height + 12)), Times.Once);

				DataTable table = new DataTable();
				table.Columns.Add(new DataColumn("data", typeof(string)));
				table.Rows.Add("row1");
				table.Rows.Add("row2");
				table.Rows.Add("row3");
				table.Rows.Add("row4");
				table.Rows.Add("row5");

				grid.DataSource = table;

				verticalPlacementClient.Setup(m => m.Grids).Returns(new[] { grid });
				verticalPlacementClient.Setup(m => m.GridMaxHeight).Returns(300);

				verticalPlacementClient.Setup(m => m.SetGridHeight(It.Is<int>(p => p == 153))); //magic height for 6 rows (5 with data plus 1 additional one)
				verticalPlacementClient.Setup(m => m.SetAdditionalControlsPosition(It.Is<int>(p => p == grid.Height + 12)));

				form.Show();

				helper = new VerticalPlacementHelper(verticalPlacementClient.Object);
				helper.AdjustVerticalPlacement();

				verticalPlacementClient.Verify(m => m.Grids, Times.AtLeastOnce);
				verticalPlacementClient.Verify(m => m.GridMaxHeight, Times.AtLeastOnce);

				verticalPlacementClient.Verify(m => m.SetGridHeight(It.Is<int>(p => p == 153)), Times.Once);
				verticalPlacementClient.Verify(m => m.SetAdditionalControlsPosition(It.Is<int>(p => p == grid.Height + 12)), Times.Exactly(2));
			}
		}
	}
}
