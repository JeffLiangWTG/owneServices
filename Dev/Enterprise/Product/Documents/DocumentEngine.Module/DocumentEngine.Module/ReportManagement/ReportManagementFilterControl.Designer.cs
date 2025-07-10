using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class ReportManagementFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|05e2a5f0-21c0-4595-8d4c-ef12fefcbbf9", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "S5_ScheduleDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|e9ee4dad-df93-45b5-8f25-08ffcb508e5a", "Start Date");
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|f1235fd2-eb41-44c3-8e72-d8d46a2944d2", "Last processed time");
			zDateEditColumnStyleInfo1.ColumnName = "LastProcessedReport.Duration";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZDateTimePickerFormat.TimeIncludingSeconds;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|6ba27482-191f-4993-bc2d-c075754168dd", "Time being processed");
			zDateEditColumnStyleInfo2.ColumnName = "RunningReport.TimeBeingProcessed";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZDateTimePickerFormat.TimeIncludingSeconds;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.MenuItems";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|732e7d15-055d-4b82-9982-bbe85c2c74ef", "Report");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "S5_ParentID";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.StmMenuItem;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|90782763-7de5-4a18-a229-e601edf115d2", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "S5_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|97b84e56-5823-42c5-ab55-50dc8b2a5d28", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "S5_GB";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|a94a805e-9497-41d3-98c2-a99b25745000", "Running Server");
			zTextBoxColumnStyleInfo2.ColumnName = "RunningReport.RRI_RunningServer";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("ReportManagementFilterControl|098cfebe-c9da-4387-8313-b9989871b0a6", "Print User");
			zTextBoxColumnStyleInfo3.ColumnName = "PrintUserName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 428, true);
			this.grid.TabIndex = 12;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Scheduler.Business.StmScheduleTask);
			// 
			// StmScheduleTaskFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "StmScheduleTaskFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 456, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
