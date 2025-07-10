namespace Enterprise.ZArchitecture.GUI
{
	partial class ZStmALogFilterControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfoEventLocal = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfoEventUtc = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zCodeFindBoxColumnStyleInfo5.ColumnName = "SL_SE_NKEvent";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo4.ColumnName = "SL_EventTime";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			zDateEditColumnStyleInfoEventLocal.ColumnName = "EventLocalBranchTime";
			zDateEditColumnStyleInfoEventLocal.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfoEventLocal.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfoEventLocal.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogFilterControl|e9002cd8-d392-4d95-ba3c-ee21a99bf709", "Event Time (Local)");
			zDateEditColumnStyleInfoEventLocal.IsVisible = false;

			zDateEditColumnStyleInfoEventUtc.ColumnName = "SL_EventTimeUtc";
			zDateEditColumnStyleInfoEventUtc.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfoEventUtc.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfoEventUtc.IsVisible = false;

			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogFilterControl|2862f898-d42d-4a35-9934-11ff7aff69b9", "Posted (Local)", "Posted Time (Local)", "");
			zDateEditColumnStyleInfo5.ColumnName = "PostedLocalBranchTime";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo6.ColumnName = "SL_PostedTimeUtc";
			zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogFilterControl|f6c30f00-9b93-4960-a419-bf5c31975638", "Event Name");
			zTextBoxColumnStyleInfo7.ColumnName = "SL_EventDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo3.ColumnName = "SL_IsCancelled";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo4.ColumnName = "SL_IsEstimate";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.ColumnName = "SL_ReferenceForBinding";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("f469bf00-b18f-4810-a448-537f54c2c9fd", "Event Details");
			zTextBoxColumnStyleInfo9.ColumnName = "DisplayEventReference";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogFilterControl|c64444f4-39b2-4e3a-97c8-f7cdfbf0a2ad", "User");
			zTextBoxColumnStyleInfo10.ColumnName = "SL_UserNameAndInitials";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmALogFilterControl|71b337b6-8f34-4529-87d0-55093597eae5", "Source");
			zTextBoxColumnStyleInfo11.ColumnName = "SL_TableFriendlyNameForBinding";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("5adb957d-08d8-4cab-96d4-fe0cf89155f5", "Context", "Context Description", "Context description of related item.");
			zTextBoxColumnStyleInfo12.ColumnName = "SL_ContextDescription";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "SL_GB_NKBranch";
			zCodeFindBoxColumnStyleInfo6.IsVisible = false;
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo7.ColumnName = "SL_GE_NKDepartment";
			zCodeFindBoxColumnStyleInfo7.IsVisible = false;
			zCodeFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo8.ColumnName = "CompanyCode";
			zCodeFindBoxColumnStyleInfo8.IsVisible = false;
			zCodeFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfoEventLocal);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfoEventUtc);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo8);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmALog);
			// 
			// ZStmALogFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "ZStmALogFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
