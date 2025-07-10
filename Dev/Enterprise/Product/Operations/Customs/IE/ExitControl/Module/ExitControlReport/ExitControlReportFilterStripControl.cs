using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.Module
{
	public partial class ExitControlReportFilterStripControl : EU.ExitControl.Module.ExitControlReportFilterStripControl
	{
		public ExitControlReportFilterStripControl(IBusinessObjectCollection gridCollection, ExitControlReportFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			AddExtraColumns();
			ReorderGridColumns();
		}

		#region Grid columns & ordering.

		void AddExtraColumns()
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.CER_Calc_Discrepancies),
					CaptionResourceString = Res.GetData("2B5CE2A6-E740-49BC-BCE6-3AA6EE380EA9", "Discrepancies"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(83),
				});

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.CER_Calc_FormattedDateTime),
					CaptionResourceString = Res.GetData("62EBAAB1-9A56-4382-9CB5-097819DD3E35", "Exit/Arrival Date"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(83),
				});

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.CER_Calc_TypeOfLocation),
					CaptionResourceString = Res.GetData("F8012736-6C73-4AD6-B6F2-59D517CC2C0A", "Type of Location"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				});

				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.CER_Calc_UNLOCO),
					CaptionResourceString = Res.GetData("FE715556-2250-438B-8BC2-F532828CA9DE", "UNLOCO"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(65),
				});

				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
				{
					BindToList = $"{nameof(CusExitReport.Declarant)}+{nameof(JobDocAddress.Lookups)}+{nameof(JobDocAddressLookups.OrgHeader_List)}",
					ColumnName = nameof(CusExitReport.DeclarantOrgPK),
					CaptionResourceString = Res.GetData("4CF504A7-F985-4CDE-838E-ADBA16F3B17A", "Declarant"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(109),
				});

				grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
				{
					BindToList = $"{nameof(CusExitReport.Representative)}+{nameof(JobDocAddress.Lookups)}+{nameof(JobDocAddressLookups.OrgHeader_List)}",
					ColumnName = nameof(CusExitReport.RepresentativeOrgPK),
					CaptionResourceString = Res.GetData("BBE2A0EB-7D96-4001-A0D5-A5C727F9587D", "Representative"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(109),
				});

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusExitReport.CER_DeclarantType),
					CaptionResourceString = Res.GetData("63C9124F-EDA7-4C74-8C8C-6E843BE91017", "Rep. Status", "Representation Status"),
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				});
			}
		}

		void ReorderGridColumns()
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.SetAllColumnsVisible(false);
				grid.SetColumnVisible(true, defaultOrderedColumns);
				grid.ReOrderColumns(defaultOrderedColumns);
			}
		}

		readonly string[] defaultOrderedColumns = new[] {
			$"{nameof(CusExitReport.Header)}+{nameof(CusExitHeader.CXH_JobReference)}",
			$"{nameof(CusExitReport.Header)}+{nameof(CusExitHeader.BranchCode)}",
			$"{nameof(CusExitReport.Header)}+{nameof(CusExitHeader.ExporterCode)}",
			$"{nameof(CusExitReport.Header)}+{nameof(CusExitHeader.CarrierCode)}",
			CusExitReport.Schema.CER_Type,
			CusExitReport.Schema.CER_CXC_Consignment,
			CusExitReport.Schema.CER_Status,
			CusExitReport.Schema.StatusDescription,
			CusExitReport.Schema.CER_MessageStatus,
			CusExitReport.Schema.MessageStatusDescription,
			CusExitReport.Schema.CER_OfficeOfExit,
			CusExitReport.Schema.CER_Calc_FormattedDateTime,
		};

		#endregion
	}
}
