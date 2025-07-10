using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	class ReportsGridUserControlTest : TestCaseWithFactory
	{
		public void TestReportsGridFieldsLayoutUserControl()
		{
			AssertNotNull(reportFieldsLayoutUserControl);
		}

		public void TestIReportsGridUserControlMembers()
		{
			IReportsGridUserControl iControl = userControl;
			AssertSame("ReportsGrid", userControl.ReportsGrid, iControl.ReportsGrid);
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertContainsExactElementsInAnyOrder("Columns", new[] {
			CusExitReport.Schema.CER_Type,
			CusExitReport.Schema.CER_AdditionalDeclarationType,
			CusExitReport.Schema.CER_Calc_Discrepancies,
			CusExitReport.Schema.CER_CXC_Consignment,
			CusExitReport.Schema.CER_OfficeOfExport,
			CusExitReport.Schema.CER_OfficeOfExit,
			CusExitReport.Schema.CER_Calc_FormattedDateTime,
			CusExitReport.Schema.CER_Calc_TypeOfLocation,
			CusExitReport.Schema.CER_Calc_UNLOCO,
			CusExitReport.Schema.CER_Location,
			CusExitReport.Schema.CER_TransportMode,
			CusExitReport.Schema.CER_TransportType,
			CusExitReport.Schema.CER_TransportID,
			CusExitReport.Schema.CER_RN_NKTransportNationality,
			CusExitReport.Schema.DeclarantOrgPK,
			CusExitReport.Schema.DeclarantAddressPK,
			CusExitReport.Schema.RepresentativeOrgPK,
			CusExitReport.Schema.RepresentativeAddressPK,
			CusExitReport.Schema.CER_DeclarantType,
			CusExitReport.Schema.CER_Status,
			CusExitReport.Schema.StatusDescription,
			CusExitReport.Schema.CER_MessageStatus,
			CusExitReport.Schema.MessageStatusDescription,
			CusExitReport.Schema.CER_EnquiryInformationCode,
			CusExitReport.Schema.TypeDescription,
			},
				reportsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
		}

		public void TestGridColumnOrder()
		{
			var exitReport = Factory.New<CusExitReport>();
			using (var form = new ZForm())
			using (var control = new ReportsGridUserControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(exitReport, null);
				form.Show();
				var reportsGrid = control.FindSingle<ZGrid>("ReportsGrid");

				AssertSequencesEqual(ExpectedOrderedColumns, reportsGrid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestColumn_CER_OfficeOfExport()
		{
			var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_OfficeOfExport);
			AssertType<ZCodeFindBoxColumnStyleInfo>("columnInfo", columnInfo);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85), columnInfo.Width);
		}

		public void TestColumn_CER_EnquiryInformationCode()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_EnquiryInformationCode);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168), columnInfo.Width);
			});
		}

		public void TestColumn_CER_AdditionalDeclarationType()
		{
			var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_AdditionalDeclarationType);
			AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
			AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
			AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83), columnInfo.Width);
		}

		public void TestColumn_CER_Type()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Type);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83), columnInfo.Width);
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

		public void TestColumn_CER_CXC_Consignment()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_CXC_Consignment);
				AssertType<ZGuidDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168), columnInfo.Width);
			});
		}

		public void TestColumn_CER_OfficeOfExit()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_OfficeOfExit);
				AssertType<ZCodeFindBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85), columnInfo.Width);
			});
		}

		public void TestColumn_CER_Calc_FormattedDateTime()
		{
			CombineAssertions(() =>
			{
				var columnInfo = (ZMultiControlColumnStyleInfo)reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_FormattedDateTime);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121), columnInfo.Width);
				AssertEquals("FieldTypeColumnName", nameof(CusExitReport.CER_Calc_FormattedDateTime_FieldType), columnInfo.FieldTypeColumnName);
			});
		}

		public void TestColumn_CER_Calc_TypeOfLocation()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_TypeOfLocation);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98), columnInfo.Width);
			});
		}

		public void TestColumn_CER_Calc_UNLOCO()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Calc_UNLOCO);
				AssertType<ZCodeFindBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65), columnInfo.Width);
			});
		}

		public void TestColumn_CER_Location()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Location);
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), columnInfo.Width);
			});
		}

		public void TestColumn_CER_TransportMode()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportMode);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97), columnInfo.Width);
			});
		}

		public void TestColumn_CER_TransportType()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportType);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85), columnInfo.Width);
			});
		}

		public void TestColumn_CER_TransportID()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_TransportID);
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), columnInfo.Width);
			});
		}

		public void TestColumn_CER_RN_NKTransportNationality()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_RN_NKTransportNationality);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(108), columnInfo.Width);
			});
		}

		public void TestColumn_CER_UniqueConsignmentReference()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_CXC_Consignment);
				AssertType<ZGuidDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(168), columnInfo.Width);
			});
		}

		public void TestColumn_DeclarantOrgPK()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.DeclarantOrgPK);
				AssertType<ZOrganisationFindBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109), columnInfo.Width);
			});
		}

		public void TestColumn_DeclarantAddressPK()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.DeclarantAddressPK);
				AssertType<ZGuidDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114), columnInfo.Width);
			});
		}

		public void TestColumn_RepresentativeOrgPK()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.RepresentativeOrgPK);
				AssertType<ZOrganisationFindBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114), columnInfo.Width);
			});
		}

		public void TestColumn_RepresentativeAddressPK()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.RepresentativeAddressPK);
				AssertType<ZGuidDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143), columnInfo.Width);
			});
		}

		public void TestColumn_CER_DeclarantType()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_DeclarantType);
				AssertType<ZDropEditColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129), columnInfo.Width);
			});
		}

		public void TestColumn_CER_Status()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_Status);
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53), columnInfo.Width);
			});
		}

		public void TestStatusDescription()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.StatusDescription));
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
			});
		}

		public void TestColumn_CER_MessageStatus()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_MessageStatus);
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103), columnInfo.Width);
			});
		}

		public void TestMessageStatusDescription()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.MessageStatusDescription));
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200), columnInfo.Width);
			});
		}

		public void TestTypeDescription()
		{
			CombineAssertions(() =>
			{
				var columnInfo = reportsGrid.GetColumnStyle(nameof(CusExitReport.TypeDescription));
				AssertType<ZTextBoxColumnStyleInfo>("columnInfo", columnInfo);
				AssertEquals("CharacterCasing", CharacterCasing.Upper, columnInfo.CharacterCasing);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220), columnInfo.Width);
			});
		}

		string[] ExpectedOrderedColumns => new[]
		{
			CusExitReport.Schema.CER_Type,
			CusExitReport.Schema.CER_CXC_Consignment,
			CusExitReport.Schema.CER_OfficeOfExit,
			CusExitReport.Schema.CER_Calc_FormattedDateTime,
			CusExitReport.Schema.CER_Calc_Discrepancies,
			CusExitReport.Schema.CER_EnquiryInformationCode,
			CusExitReport.Schema.CER_Calc_TypeOfLocation,
			CusExitReport.Schema.CER_Calc_UNLOCO,
			CusExitReport.Schema.CER_Status,
			CusExitReport.Schema.StatusDescription,
			CusExitReport.Schema.CER_MessageStatus,
			CusExitReport.Schema.MessageStatusDescription,
		};

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ReportsGridUserControl();
			reportsGrid = userControl.ReportsGrid;
			reportFieldsLayoutUserControl = userControl.ReportsGridFieldsLayoutUserControl;
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		ReportsGridUserControl userControl;
		ZGrid reportsGrid;
		ReportsGridFieldsLayoutUserControl reportFieldsLayoutUserControl;
	}
}
