using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ReportStatisticsModuleButtonGrid : ZModuleButtonGrid
	{
		public ReportStatisticsModuleButtonGrid()
		{
			ModuleID = ModuleIDs.ReportStatistics;
			ShowAttachButton = false;
			ShowDetachButton = false;
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetupColumns();
			}
		}

		protected override bool AllowOpenInEditFormEvenIfListIsReadOnly => true;

		#region Columns Setup

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected virtual void SetupColumns()
		{
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Report Name", "Report Name");
			zTextBoxColumnStyleInfo1.ColumnName = "RRI_ReportName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Report Description", "Report Description");
			zTextBoxColumnStyleInfo2.ColumnName = "RRI_ReportDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Status", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "RRI_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Running Server", "Running Server");
			zTextBoxColumnStyleInfo4.ColumnName = "RRI_RunningServer";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|SQL Server", "SQL Server");
			zTextBoxColumnStyleInfo6.ColumnName = "RRI_SQLServer";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Start Date", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "StartTimeLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|End Date", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "EndTimeLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Duration Seconds", "Duration");
			zDateEditColumnStyleInfo3.ColumnName = "Duration";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|SQLRunDuration", "SQL Duration");
			zDateEditColumnStyleInfo4.ColumnName = "SQLRunDuration";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|SQLCPUDuration", "SQL CPU Duration");
			zDateEditColumnStyleInfo5.ColumnName = "SQLCPUDuration";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo5.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|ClientRunDuration", "Client Duration");
			zDateEditColumnStyleInfo6.ColumnName = "ClientRunDuration";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo6.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|ClientCPUDuration", "Client CPU Duration");
			zDateEditColumnStyleInfo7.ColumnName = "ClientCPUDuration";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo7.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Report Size Bytes", "Report Size (Bytes)");
			zCalcEditColumnStyleInfo1.ColumnName = "RRI_ReportSizeBytes";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Rows Returned", "Rows Returned");
			zCalcEditColumnStyleInfo2.ColumnName = "RRI_RowsReturned";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportStatisticsModuleButtonGrid|Print User", "Print User");
			zTextBoxColumnStyleInfo5.ColumnName = "RRI_GS_NKPrintUser";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
		}

		#endregion
	}
}
