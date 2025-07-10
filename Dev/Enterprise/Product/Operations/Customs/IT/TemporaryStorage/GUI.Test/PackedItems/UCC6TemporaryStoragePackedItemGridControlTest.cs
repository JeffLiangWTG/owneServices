using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePackedItemGridControlTest : TestCaseWithFactory
{
	public void TestGridColumns()
	{
		using var form = new ZForm(Factory.New<TemporaryStorageHeader>());
		using var control = new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemGridControl();

		form.Controls.Add(control);
		form.Show();

		var grid = (ZGrid)control.Controls.Find("PackedItemGrid", true).First();
		var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
		AssertContainsExactElementsInExactOrder("gridPackedItems Column Names",
				[
					"API_LineNo",
					"API_FormattedTariff",
					"API_GoodsDescription",
					"API_GrossWeight",
					"API_GrossWeightUQ",
					"API_NetWeight",
					"API_NetWeightUQ",
					"API_CustomsQty2",
					"API_CustomsUQ2",
					"API_ChemicalSubstanceCode",
					"RegistrationNo",
					"ReleaseDate"
			], columns.Select(x => x.ColumnName));
	}
}
