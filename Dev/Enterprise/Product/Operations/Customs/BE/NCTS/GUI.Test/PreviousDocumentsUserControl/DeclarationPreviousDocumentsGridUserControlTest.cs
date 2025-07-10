using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.NCTS.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.Business.AutoCusSupportingInfo.Schema;

namespace Enterprise.Customs.BE.NCTS.GUI.Testing;

sealed class DeclarationPreviousDocumentsGridUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		AssertEquals(nameof(NctsHeader.PreviousDocuments), previousDocumentsGrid.GetBindingMember());
	}

	public void TestAvailableColumns()
	{
		AssertSequencesEqual("Columns", new[] { CSI_Code, CSI_ReferenceNumber, CSI_ReferenceNumber2 },
				previousDocumentsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new DeclarationPreviousDocumentsGridUserControl();
		previousDocumentsGrid = (ZGrid)userControl.Controls.Find("PreviousDocumentsGrid", true).First();
	}
	DeclarationPreviousDocumentsGridUserControl userControl;
	ZGrid previousDocumentsGrid;

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}
}
