using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public class AsycudaPackedItemGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (var control = new AsycudaPackedItemGridControl())
			{
				var billsGrid = (ZGrid)control.Controls.Find("PackedItemGrid", true).First();
				var columns = billsGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertContainsExactElementsInExactOrder("gridPackedItems Column Names",
					new[] { "API_LineNo",
						"API_FormattedTariff",
						"API_GoodsDescription",
						"API_GrossWeight",
						"API_GrossWeightUQ",
						"API_PackStatus",
						"UNDGs+UNDGSubstanceManager+Value",
						"UNDGs+UNDGClassManager+Value"
					}, columns.Select(x => x.ColumnName));

				CombineAssertions("Dangerous Goods context menu visible", () =>
				{
					var billsPackedItemGrid = (ZGrid)control.Controls.Find("PackedItemGrid", true).First();
					AssertNotNull("Have Bills Packed Item Grid", billsPackedItemGrid);
					AssertNotNull("Bills Packed Item Grid have context menu", billsPackedItemGrid.ContextMenu);
					var dangerousGoodsMenuItem = billsPackedItemGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Dangerous Goods");
					AssertNotNull("have context menu with Text Dangerous Goods", dangerousGoodsMenuItem);
					AssertEquals("Dangerous Goods context menu is visible", true, dangerousGoodsMenuItem.Visible);
				});
			}
		}
	}
}
