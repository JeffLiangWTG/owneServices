using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.EU.ExitControl.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ExitControlBase.Business.AutoCusExitReport.Schema;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	sealed class ReportsGridUserControlTest : ReportGridUserControlAbstractTest
	{
		public void TestReportsGridFieldsLayoutUserControl()
		{
			AssertNotNull(reportFieldsLayoutUserControl);
		}

		public override void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] {
				CER_CXC_Consignment, CusExitReport.Schema.CER_Calc_Discrepancies, CER_TransportMode, CER_TransportType, CER_TransportID, CER_RN_NKTransportNationality,
				CER_Location, CER_OfficeOfExit, CER_Status, nameof(CusExitReport.StatusDescription),
				CER_MessageStatus, nameof(CusExitReport.MessageStatusDescription), nameof(CusExitReport.ArrivalDate)
			},
			reportsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public override void TestColumn_CER_Type()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CER_Type);
				AssertNull(columnInfo);
			});
		}

		public override void TestColumn_CER_Location()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CER_Location);
				AssertType<ZCodeFindBoxColumnStyleInfo>(columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
			});
		}

		public void TestColumn_ArrivalDate()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.ArrivalDate));
				AssertType<ZDateEditColumnStyleInfo>(columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145), columnInfo.Width);
			});
		}

		public void TestReportsGridDock()
		{
			AssertEquals("Should be Fill", DockStyle.Fill, reportsGrid.Dock);
		}

		public void TestBottomGroupBox()
		{
			CombineAssertions(() =>
			{
				var bottomGroupBox = esUserControl.BottomGroupBox;
				AssertEquals("Dock should be Bottom", DockStyle.Bottom, bottomGroupBox.Dock);
				AssertEquals("Height", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), bottomGroupBox.Size.Height);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			esUserControl = new ReportsGridUserControl();
			reportsGrid = esUserControl.ReportsGrid;
			reportFieldsLayoutUserControl = esUserControl.ReportsGridFieldsLayoutUserControl;
		}

		protected override void TearDown()
		{
			base.TearDown();
			esUserControl.Dispose();
		}

		ReportsGridUserControl esUserControl;
		ReportsGridFieldsLayoutUserControl reportFieldsLayoutUserControl;
	}
}
