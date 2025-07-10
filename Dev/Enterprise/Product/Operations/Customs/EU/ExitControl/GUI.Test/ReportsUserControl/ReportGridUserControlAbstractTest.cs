using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitReport.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

public abstract class ReportGridUserControlAbstractTest : TestCaseWithFactory
{
	public void TestIReportsGridUserControlMembers()
	{
		IReportsGridUserControl iControl = userControl;
		AssertSame("ReportsGrid", userControl.ReportsGrid, iControl.ReportsGrid);
	}

	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);
	}

	public abstract void TestAvailableColumns();

	public abstract void TestColumn_CER_Type();

	public void TestColumn_CER_TransportMode()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_TransportMode);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97), columnInfo.Width);
		});
	}

	public void TestColumn_CER_TransportType()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_TransportType);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85), columnInfo.Width);
		});
	}

	public void TestColumn_CER_TransportID()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_TransportID);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
		});
	}

	public void TestColumn_CER_RN_NKTransportNationality()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_RN_NKTransportNationality);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108), columnInfo.Width);
		});
	}

	public abstract void TestColumn_CER_Location();

	public void TestColumn_CER_OfficeOfExit()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_OfficeOfExit);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90), columnInfo.Width);
		});
	}

	public void TestColumn_CER_Status()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_Status);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53), columnInfo.Width);
		});
	}

	public void TestStatusDescription()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.StatusDescription));
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
		});
	}

	public void TestColumn_CER_MessageStatus()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_MessageStatus);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103), columnInfo.Width);
		});
	}

	public void TestMessageStatusDescription()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.MessageStatusDescription));
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
		});
	}

	public void TestColumn_CER_CXC_Consignment()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CER_CXC_Consignment);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168), columnInfo.Width);
		});
	}

	public void TestColumn_CER_Calc_Discrepancies()
	{
		CombineAssertions(() =>
		{
			var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_Discrepancies);
			AssertType<ZCheckBoxColumnStyleInfo>("columnInfo", columnInfo);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83), columnInfo.Width);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		userControl = new ReportsGridUserControl();
		reportsGrid = userControl.ReportsGrid;
	}

	protected override void TearDown()
	{
		base.TearDown();
		userControl.Dispose();
	}

	ReportsGridUserControl userControl;
	protected ZGrid reportsGrid;
}
