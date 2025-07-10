using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackedItemGridControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using var form = new ZForm(Factory.New<TemporaryStorageHeader>());
			using var control = new UCC6TemporaryStoragePackedItemGridControl();

			form.Controls.Add(control);
			form.Show();

			var grid = (ZGrid)control.Controls.Find("PackedItemGrid", true)[0];
			var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
			AssertContainsExactElementsInExactOrder("gridPackedItems Column Names",
				[
				"API_LineNo",
				"API_FormattedTariff",
				"API_GoodsDescription",
				"API_GrossWeight",
				"API_GrossWeightUQ",
				"API_ChemicalSubstanceCode"
				],
				columns.Select(x => x.ColumnName));
		}
	}
}
