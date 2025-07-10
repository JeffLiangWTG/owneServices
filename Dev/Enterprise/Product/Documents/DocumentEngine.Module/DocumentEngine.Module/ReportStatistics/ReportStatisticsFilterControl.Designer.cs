using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class ReportStatisticsFilterControl : ZFilterStripControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Report Name", "Report Name");
			zTextBoxColumnStyleInfo1.ColumnName = "RRI_ReportName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Report Description", "Report Description");
			zTextBoxColumnStyleInfo2.ColumnName = "RRI_ReportDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Status", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "RRI_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Running Server", "Running Server");
			zTextBoxColumnStyleInfo4.ColumnName = "RRI_RunningServer";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|SQL Server", "SQL Server");
			zTextBoxColumnStyleInfo6.ColumnName = "RRI_SQLServer";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Start Date", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "StartTimeLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|End Date", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "EndTimeLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Duration Seconds", "Duration");
			zDateEditColumnStyleInfo3.ColumnName = "Duration";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|SQLRunDuration", "SQL Duration");
			zDateEditColumnStyleInfo4.ColumnName = "SQLRunDuration";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|SQLCPUDuration", "SQL CPU Duration");
			zDateEditColumnStyleInfo5.ColumnName = "SQLCPUDuration";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo5.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|ClientRunDuration", "Client Duration");
			zDateEditColumnStyleInfo6.ColumnName = "ClientRunDuration";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo6.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|ClientCPUDuration", "Client CPU Duration");
			zDateEditColumnStyleInfo7.ColumnName = "ClientCPUDuration";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo7.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Queue Duration Seconds", "Queue Duration");
			zDateEditColumnStyleInfo8.ColumnName = "QueueDuration";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo8.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.TimeIncludingSeconds;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Report Size Bytes", "Report Size (Bytes)");
			zCalcEditColumnStyleInfo1.ColumnName = "RRI_ReportSizeBytes";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Rows Returned", "Rows Returned");
			zCalcEditColumnStyleInfo2.ColumnName = "RRI_RowsReturned";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportStatisticsFilterControl|Print User", "Print User");
			zTextBoxColumnStyleInfo5.ColumnName = "RRI_GS_NKPrintUser";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("cb1f3991-8a46-4263-81c5-08d907ef4ea6", "Is Preview");
			zTextBoxColumnStyleInfo7.ColumnName = "IsPreview";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("DDFE6F49-85F8-4F70-9D5E-F95AEC8C9DD8", "System");
			zTextBoxColumnStyleInfo8.ColumnName = "RRI_IsSystemDefined";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 428, true);
			this.grid.TabIndex = 12;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Scheduler.Business.StmReportRun);
			// 
			// StmScheduleTaskFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ReportStatisticsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 456, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
