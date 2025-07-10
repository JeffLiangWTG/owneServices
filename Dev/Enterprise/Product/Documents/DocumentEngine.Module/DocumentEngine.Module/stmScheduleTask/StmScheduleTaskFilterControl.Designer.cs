using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class StmScheduleTaskFilterControl : ZFilterStripControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|22ad0b54-5be2-422b-b936-dea83aa7d1d6", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "S5_ScheduleDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|95832b47-5854-4915-b254-531718ab2493", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "CalcStartDateLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|2f01e923-5b53-4f1f-b392-b79eee057807", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "CalcEndDateLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|1301cc44-e0b0-478f-b423-20e625f49b82", "Next Run Date (local)");
			zDateEditColumnStyleInfo3.ColumnName = "CalcNextRunTimeLocal";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.MenuItems";
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|a7d3a3f9-f411-4364-a1bc-52bb6413fb79", "Report");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "S5_ParentID";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.StmMenuItem;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|99ff99e7-ff6a-4113-b699-20fedb1e0feb", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "S5_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|3abd181a-b4a1-410d-aefd-578c6fd89955", "One Off");
			zCheckBoxColumnStyleInfo2.ColumnName = "S5_IsPrivate";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|ba150781-6986-4852-a407-3660e525f08e", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "S5_GB";
			zDateEditColumnStyleInfo5.Caption = null;
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|8aed2cb4-1eaf-4ee1-aa3a-4ecceb807b6a", "First Run");
			zDateEditColumnStyleInfo5.ColumnName = "S5_DateScheduleFirstRun";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|00f4220a-8d25-4a7c-9192-e659c38d72e9", "Day Number");
			zCalcEditColumnStyleInfo1.ColumnName = "S5_DayNumber";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|0e3b5b84-84c7-4349-bcc7-5d60fe021fff", "End After Count");
			zCalcEditColumnStyleInfo2.ColumnName = "S5_EndAfterCount";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|a8ca2716-c061-4fcf-88b9-0660e097ce82", "Month Number");
			zCalcEditColumnStyleInfo3.ColumnName = "S5_MonthNumber";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|dc0cb6cb-01b3-473d-8cee-0553205a46ea", "Task Period");
			zTextBoxColumnStyleInfo2.ColumnName = "S5_TaskPeriod";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|9a50ba7f-a89c-479d-a803-3a26fa32c5fa", "Task Period Count");
			zCalcEditColumnStyleInfo4.ColumnName = "S5_TaskPeriodCount";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|08009be2-a4be-4ae0-8d20-5c0b558f30be", "Week Day Occurrence Number");
			zCalcEditColumnStyleInfo5.ColumnName = "S5_WeekDayOccurrenceNumber";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Caption = null;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|9575e4c6-b66f-48f0-8358-ee9d6f2a49c2", "Week Days Only");
			zCheckBoxColumnStyleInfo3.ColumnName = "S5_WeekDaysOnly";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|C10CAF61-35BF-4E2F-8DCB-083DC2703F67", "Print User");
			zTextBoxColumnStyleInfo3.ColumnName = "PrintUserName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|D72E1351-E2B1-4C74-9242-6BC1B6D769D2", "Create Edit User");
			zTextBoxColumnStyleInfo4.ColumnName = "S5_SystemCreateUser";
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|58A32256-75D5-4016-9A45-CF0CE9DA1AAD", "Last Edit User");
			zTextBoxColumnStyleInfo5.ColumnName = "S5_SystemLastEditUser";
			zDateEditColumnStyleInfo6.Caption = "";
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|E9044075-4504-4D67-A3E9-690B14D1D9B1", "Create Time UTC");
			zDateEditColumnStyleInfo6.ColumnName = "S5_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo7.Caption = "";
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.DocumentEngine.Module.Res.GetData("StmScheduleTaskFilterControl|AF9DE585-11B6-485C-9B22-F50DE8E669AC", "Last Time UTC");
			zDateEditColumnStyleInfo7.ColumnName = "S5_SystemLastEditTimeUtc";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
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
