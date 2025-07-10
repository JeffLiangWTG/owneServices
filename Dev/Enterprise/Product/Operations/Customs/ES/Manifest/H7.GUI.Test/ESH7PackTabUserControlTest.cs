using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	public class ESH7PackTabUserControlTest : TestCaseWithFactory
	{
		readonly string[] expectedColumns = new[] { "APA_PackQty", "APA_PackUQ",
													"APA_GoodsDescription",
													"APA_MarksAndNumbers",
													"APA_Weight", "APA_WeightUQ",
													"APA_Volume", "APA_VolumeUQ" };

		readonly string[] expectedReadOnlyColumns = System.Array.Empty<string>();

		public void TestSetVisibility()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			using (var form = new ZForm(manifest))
			using (var control = new ESH7PackTabUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("number of columns is the same", expectedColumns, grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
					EUH7GUITestHelper.AssertGridLayout(grid, expectedColumns);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, expectedReadOnlyColumns);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, expectedColumns.Except(expectedReadOnlyColumns).ToArray());
				});
			}

			using (var form = new ZForm(bill))
			using (var control = new ESH7PackTabUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PacksGrid");

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("number of columns is the same", expectedColumns, grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
					EUH7GUITestHelper.AssertGridLayout(grid, expectedColumns);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, expectedReadOnlyColumns);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, expectedColumns.Except(expectedReadOnlyColumns).ToArray());
				});
			}
		}
	}
}
