using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CusExitReport = Enterprise.Customs.DE.ExitControl.Business.CusExitReport;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing.ReportsUserControl
{
	sealed class ReportsGridUserControlTest : TestCaseWithFactory
	{
		public void TestIReportsGridUserControlMembers()
		{
			IReportsGridUserControl control = userControl;
			AssertSame("ReportsGrid", userControl.ReportsGrid, control.ReportsGrid);
		}

		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitReportCollection<CusExitReport>), userControl.BindingSource.DataSourceType);
		}

		public void TestGroupBoxCaption()
		{
			AssertEquals("Caption", "Exit Reports", userControl.ReportsGroupBox.CaptionResourceString.Caption);
		}

		public void TestGridColumnOrder()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			using (var zForm = new ZForm(exitHeader.CusExitReports))
			using (var reportsGridUserControl = new ReportsGridUserControl())
			{
				zForm.Controls.Add(reportsGridUserControl);
				zForm.Show();
				AssertSequencesEqual(
					orderedGridColumnDetails.Select(x => x.Item1),
					reportsGridUserControl.ReportsGrid.Columns.Where(x => x.IsVisible).Select(x => x.ColumnName)
				);
			}
		}

		public void TestGridColumnCaptions()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			using (var zForm = new ZForm(exitHeader.CusExitReports))
			using (var reportsGridUserControl = new ReportsGridUserControl())
			{
				zForm.Controls.Add(reportsGridUserControl);
				zForm.Show();
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnCaption) in orderedGridColumnDetails)
					{
						AssertEquals(columnName, columnCaption, reportsGridUserControl.ReportsGrid.GetColumnCaption(columnName));
					}
				});
			}
		}

		public void TestGridColumnWidths()
		{
			using (var reportsGridUserControl = new ReportsGridUserControl())
			{
				CombineAssertions(() =>
				{
					foreach (var (columnName, columnWidth, _) in orderedGridColumnDetails)
					{
						AssertEquals(columnName, columnWidth, reportsGridUserControl.ReportsGrid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		public void TestCER_TypeColumnBindToList()
		{
			var columnStyle = (ZGuidDropEditColumnStyleInfo)reportsGrid.GetColumnStyle(nameof(CusExitReport.CER_CXC_Consignment));
			AssertEquals("BindToList", nameof(CusExitReport.Header) + "+" + nameof(CusExitReport.Header.CusExitConsignments), columnStyle.BindToList);
		}

		public void TestDeclarantOrganisationPKColumnBindToList()
		{
			var columnStyle = (MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo)reportsGrid.GetColumnStyle($"{nameof(CusExitReport.Declarant)}+{nameof(CusExitReport.Declarant.OrganisationPK)}");
			AssertEquals("BindToList", nameof(CusExitReport.Declarant) + "+" + nameof(JobDocAddress.Lookups) + "+" + nameof(JobDocAddressLookups.OrgHeader_List), columnStyle.BindToList);
		}

		public void TestDeclarantAddressPKColumnBindToList()
		{
			var columnStyle = (ZGuidDropEditColumnStyleInfo)reportsGrid.GetColumnStyle($"{nameof(CusExitReport.Declarant)}+{nameof(CusExitReport.Declarant.E2_OA_Address)}");
			AssertEquals("BindToList", nameof(CusExitReport.Declarant) + "+" + nameof(JobDocAddress.Organisation) + "+" + nameof(OrgHeader.Addresses), columnStyle.BindToList);
		}

		public void TestRepresentativeOrgPKColumnBindToList()
		{
			var columnStyle = (MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo)reportsGrid.GetColumnStyle($"{nameof(CusExitReport.Representative)}+{nameof(CusExitReport.Declarant.OrganisationPK)}");
			AssertEquals("BindToList", nameof(CusExitReport.Representative) + "+" + nameof(JobDocAddress.Lookups) + "+" + nameof(JobDocAddressLookups.OrgHeader_List), columnStyle.BindToList);
		}

		public void TestRepresentativeAddressPKColumnBindToList()
		{
			var columnStyle = (ZGuidDropEditColumnStyleInfo)reportsGrid.GetColumnStyle($"{nameof(CusExitReport.Representative)}+{nameof(CusExitReport.Declarant.E2_OA_Address)}");
			AssertEquals("BindToList", nameof(CusExitReport.Representative) + "+" + nameof(JobDocAddress.Organisation) + "+" + nameof(OrgHeader.Addresses), columnStyle.BindToList);
		}

		public void TestGridColumnType()
		{
			AssertEquals("NKTransportNationalityColumn is CodeFindBox", typeof(ZCodeFindBoxColumnStyle), reportsGrid.GetColumnStyle(CusExitReport.Schema.CER_RN_NKTransportNationality).ColumnStyleType);
		}

		readonly (string, int, string)[] orderedGridColumnDetails = {
			(CusExitReport.Schema.CER_Type, 83, "Report Type"),
			(CusExitReport.Schema.CER_TransportMode, 115, "Mode of Transport"),
			(CusExitReport.Schema.CER_TransportType, 100, "Transport Type"),
			(CusExitReport.Schema.CER_TransportID, 120, "Transport ID"),
			(CusExitReport.Schema.CER_RN_NKTransportNationality, 125, "Transport Nationality"),
			(nameof(CusExitReport.Location), 120, "Location"),
			(CusExitReport.Schema.CER_DateTime, 93, "Exit Date"),
			(CusExitReport.Schema.CER_OfficeOfExit, 90, "Office of Exit"),
			(CusExitReport.Schema.CER_OfficeOfExport, 120, "Int. Office of Exit"),
			(CusExitReport.Schema.CER_Status, 53, "Status"),
			(nameof(CusExitReport.StatusDescription), 200, "Status Description"),
			(CusExitReport.Schema.CER_MessageStatus, 103, "Message Status"),
			(nameof(CusExitReport.MessageStatusDescription), 200, "Message Status Description"),
			(CusExitReport.Schema.CER_CXC_Consignment, 168, "Entry/Consignment"),
			(CusExitReport.Schema.CER_Calc_Discrepancies, 80, "Discrepancies"),
			(CusExitReport.Schema.CER_IsFinalized, 72, "Finalization"),
			($"{nameof(CusExitReport.Declarant)}+{nameof(CusExitReport.Declarant.OrganisationPK)}", 120, "Declarant"),
			($"{nameof(CusExitReport.Declarant)}+{nameof(CusExitReport.Declarant.E2_OA_Address)}", 120, "Declarant Address"),
			($"{nameof(CusExitReport.Representative)}+{nameof(CusExitReport.Representative.OrganisationPK)}", 120, "Representative"),
			($"{nameof(CusExitReport.Representative)}+{nameof(CusExitReport.Representative.E2_OA_Address)}", 120, "Representative Address"),
		};

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
		ZGrid reportsGrid;
	}
}
