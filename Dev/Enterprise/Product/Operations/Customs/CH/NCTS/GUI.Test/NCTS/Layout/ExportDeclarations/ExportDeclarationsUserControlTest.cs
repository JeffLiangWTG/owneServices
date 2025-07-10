using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.GUI.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

class ExportDeclarationsUserControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using (var control = new ExportDeclarationsUserControl())
		{ 
			var gridColumnStyles = control.ExportDeclarationsModuleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
			UserControlTestHelper.AssertColumnStyles<ZCalcEditColumnStyleInfo>(gridColumnStyles, RelatedExportEntryHeaderGenPivot.Schema.XX_Sequence, 0, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.ShipmentType), 1, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.EntryNumber), 2, width: 160);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.JobNumber), 3, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.ReferenceNumber), 4, width: 160);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.EntryStatus), 5, width: 80);
			UserControlTestHelper.AssertColumnStyles<ZTextBoxColumnStyleInfo>(gridColumnStyles, nameof(RelatedExportEntryHeaderGenPivot.EntryStatusDescription), 6, width: 160);
		}
	}

	public void TestButtons() => CombineAssertions(() =>
	{
		using (var control = new ExportDeclarationsUserControl())
		{
			var grid = control.ExportDeclarationsModuleButtonGrid;
			AssertEquals("ShowAttachButton", true, grid.ShowAttachButton);
			AssertEquals("ShowDetachButton", true, grid.ShowDetachButton);
			AssertEquals("ShowNewButton", false, grid.ShowNewButton);
			AssertEquals("ShowEditButton", false, grid.ShowEditButton);
		}
	});
}
