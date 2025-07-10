using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportsUcc6GridUserControlTest : TestCaseWithFactory
{
	public void TestReportsGridFieldsLayoutUserControl() => AssertNotNull(reportFieldsLayoutUserControl);

	public void TestIReportsGridUserControlMembers()
	{
		IReportsGridUserControl iControl = userControl;
		AssertSame("ReportsGrid", userControl.ReportsGrid, iControl.ReportsGrid);
	}

	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);

	public void TestAvailableColumns() => AssertSequencesEqual("Columns",
		new[] {
			CusExitReport.Schema.CER_CXC_Consignment, CusExitReport.Schema.CER_Calc_Discrepancies, CusExitReport.Schema.CER_TransportMode,
			CusExitReport.Schema.CER_TransportType, CusExitReport.Schema.CER_TransportID, CusExitReport.Schema.CER_RN_NKTransportNationality,
			CusExitReport.Schema.CER_Location, CusExitReport.Schema.CER_OfficeOfExit, CusExitReport.Schema.CER_Status, nameof(CusExitReport.StatusDescription),
			CusExitReport.Schema.CER_MessageStatus, nameof(CusExitReport.MessageStatusDescription)
		},
		reportsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

	public void TestColumn_CER_Type() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Type);
		AssertNull(columnInfo);
	});

	public void TestColumn_CER_TransportMode() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportMode);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97), columnInfo.Width);
	});

	public void TestColumn_CER_TransportType() =>  CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportType);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85), columnInfo.Width);
	});

	public void TestColumn_CER_TransportID() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportID);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
	});

	public void TestColumn_CER_RN_NKTransportNationality() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_RN_NKTransportNationality);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108), columnInfo.Width);
	});

	public void TestColumn_CER_Location() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Location);
		AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
	});

	public void TestColumn_CER_OfficeOfExit() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_OfficeOfExit);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnInfo.Width);
	});

	public void TestColumn_CER_Status() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Status);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53), columnInfo.Width);
	});

	public void TestStatusDescription() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.StatusDescription));
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
	});

	public void TestColumn_CER_MessageStatus() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_MessageStatus);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103), columnInfo.Width);
	});

	public void TestMessageStatusDescription() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.MessageStatusDescription));
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
	});

	public void TestColumn_CER_CXC_Consignment() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_CXC_Consignment);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168), columnInfo.Width);
	});

	public void TestColumn_CER_Calc_Discrepancies() => CombineAssertions(() =>
	{
		var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_Discrepancies);
		AssertType<ZCheckBoxColumnStyleInfo>("columnInfo", columnInfo);
		AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83), columnInfo.Width);
	});

	public void TestReportsGridDock() => AssertEquals("Should be Fill", DockStyle.Fill, reportsGrid.Dock);

	public void TestBottomGroupBox() => CombineAssertions(() =>
	{
		var bottomGroupBox = userControl.BottomGroupBox;
		AssertEquals("Dock should be Bottom", DockStyle.Bottom, bottomGroupBox.Dock);
		AssertEquals("Height", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), bottomGroupBox.Size.Height);
	});

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new ReportsUcc6GridUserControl();
		reportsGrid = userControl.ReportsGrid;
		reportFieldsLayoutUserControl = userControl.ReportsGridFieldsLayoutUserControl;
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	ReportsUcc6GridUserControl userControl;
	ZGrid reportsGrid;
	ReportsUcc6GridFieldsLayoutUserControl reportFieldsLayoutUserControl;
}
