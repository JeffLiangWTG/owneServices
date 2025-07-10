using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
	[TestedType(typeof(FRH7PackTabUserControl))]
	public class FRH7PackTabUserControlTest : TestCaseWithFactory
	{
		public void TestSetVisibility()
		{
			var manifest = Factory.New<H7ManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new FRH7PackTabUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("number of columns is the same", expectedColumns, grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
					EUH7GUITestHelper.AssertGridLayout(grid, visibleColumns);
				});
			}
		}

		readonly string[] expectedColumns = new[] { "APA_PackQty", "APA_PackUQ",
													"APA_CommodityCode",
													"APA_GoodsDescription",
													"APA_MarksAndNumbers",
													"APA_Weight", "APA_WeightUQ",
													"APA_Volume", "APA_VolumeUQ",
													"LinePrice", "LinePriceCurrency",
													"APA_LineNo" };

		readonly string[] visibleColumns = new[] { "APA_PackQty", "APA_PackUQ",
													"APA_CommodityCode",
													"APA_GoodsDescription",
													"APA_MarksAndNumbers",
													"APA_Weight", "APA_WeightUQ",
													"APA_Volume", "APA_VolumeUQ" };
	}
}
