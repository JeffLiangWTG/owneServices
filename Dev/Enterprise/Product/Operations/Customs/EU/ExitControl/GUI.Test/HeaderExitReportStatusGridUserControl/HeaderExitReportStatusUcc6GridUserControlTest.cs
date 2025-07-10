using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusExitHeader = Enterprise.Customs.EU.ExitControl.Business.CusExitHeader;
using CusExitReport = Enterprise.Customs.EU.ExitControl.Business.CusExitReport;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class HeaderExitReportStatusUcc6GridUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType() => AssertEquals(typeof(CusExitHeader), userControl.BindingSource.DataSourceType);

	public void TestReportsGrid() => CombineAssertions(() =>
	{
		AssertEquals("BindingMember", nameof(CusExitHeader.CusExitReportStatus), reportStatusGrid.GetBindingMember());
		AssertEquals("Dock", DockStyle.Fill, reportStatusGrid.Dock);
	});

	public void TestAvailableColumns() => AssertSequencesEqual("Columns",
		new[] {
			AutoCusExitReport.Schema.CER_Type,
			nameof(CusExitReport.TypeDescription),
			AutoCusExitReport.Schema.CER_CXC_Consignment,
			AutoCusExitReport.Schema.CER_OfficeOfExit,
			AutoCusExitReport.Schema.CER_Location,
			AutoCusExitReport.Schema.CER_Status,
			nameof(CusExitReport.StatusDescription),
			AutoCusExitReport.Schema.CER_MessageStatus,
			nameof(CusExitReport.MessageStatusDescription),
			CusExitReport.Schema.CER_Calc_Discrepancies,
		},
		reportStatusGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

	public void TestGridReadOnly() => CombineAssertions(() =>
	{
		var cusExitHeader = Factory.New<CusExitHeader>();

		using (var form = new ExitControlForm(cusExitHeader))
		{
			form.Show();

			var gridControl = form.FindSingle<HeaderExitReportStatusGridUserControl>(nameof(HeaderExitReportStatusGridUserControl));
			var reportsGrid = gridControl.ReportsGrid;
			AssertEquals(true, reportsGrid.GetReadOnly());

			foreach (ZGridColumnInfo column in reportsGrid.ColumnStyles)
			{
				AssertEquals($"{column.ColumnName} Column ReadOnly", true, column.IsReadOnly);
			}
		}
	});

	public void TestColumn_CER_Type() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(AutoCusExitReport.Schema.CER_Type);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", false, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(70), columnInfo.Width);
	});

	public void TestReportTypeDescription() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(nameof(CusExitReport.TypeDescription));
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", false, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
	});

	public void TestColumnCER_OfficeOfExit() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(nameof(CusExitReport.CER_OfficeOfExit));
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	});

	public void TestColumnCER_Location() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(nameof(CusExitReport.CER_Location));
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnInfo.Width);
	});

	public void TestColumn_CER_CXC_Consignment() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(AutoCusExitReport.Schema.CER_CXC_Consignment);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
	});

	public void TestColumn_CER_Status() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(AutoCusExitReport.Schema.CER_Status);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	});

	public void TestStatusDescription() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(nameof(CusExitReport.StatusDescription));
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
	});

	public void TestColumn_CER_MessageStatus() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(AutoCusExitReport.Schema.CER_MessageStatus);
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(80), columnInfo.Width);
	});

	public void TestMessageStatusDescription() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(nameof(CusExitReport.MessageStatusDescription));
		AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(150), columnInfo.Width);
	});

	public void TestColumn_CER_Calc_Discrepancies() => CombineAssertions(() =>
	{
		var columnInfo = reportStatusGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_Discrepancies);
		AssertType<ZCheckBoxColumnStyleInfo>("columnInfo", columnInfo);
		AssertEquals("IsVisible", true, columnInfo.IsVisible);
		AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(83), columnInfo.Width);
	});

	public void TestHeaderExitReportStatusGroupBox() => CombineAssertions(() =>
	{
		var groupBox = userControl.HeaderExitReportStatusGroupBox;
		AssertEquals("Caption", "Exit Report Status", groupBox.CaptionResourceString.Caption);
		AssertEquals("Dock", DockStyle.Fill, groupBox.Dock);
		AssertEquals("HeaderExitReportStatusGroupBox contains the Reports Grid", true, groupBox.Controls.Contains(reportStatusGrid));
	});

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new HeaderExitReportStatusUcc6GridUserControl();
		reportStatusGrid = userControl.ReportsGrid;
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	HeaderExitReportStatusUcc6GridUserControl userControl;
	ZGrid reportStatusGrid;
}
