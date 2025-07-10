using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.Module
{
	public partial class IncidentApprovalFilterControl : ZFilterStripControl
	{
		readonly System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "IA_ClientReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("IncidentApprovalFilterControl|145bd0b2-72c4-40c6-963d-27137999eb65", "Incident Summary");
			zTextBoxColumnStyleInfo2.ColumnName = "IA_IncidentSummary";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("IncidentApprovalFilterControl|2a5e976f-887e-4b21-8e2e-bb3676384eaf", "Incident No");
			zTextBoxColumnStyleInfo3.ColumnName = "IA_IncidentNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("IncidentApprovalFilterControl|a6533d1c-aa69-48f7-9ad7-908b3f5ecfe9", "License Code");
			zTextBoxColumnStyleInfo4.ColumnName = "IA_LicenceCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("71e16cd6-c804-48c5-a320-a5531b3f0235", "Reported By");
			zTextBoxColumnStyleInfo5.ColumnName = "ReportedByStaffCode";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("IncidentApprovalFilterControl|f11969d6-fc7e-489d-91fd-6cc44dad4a4a", "Approved By");
			zTextBoxColumnStyleInfo6.ColumnName = "IA_GS_NKApprovingStaff";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("27994199-b7a7-4496-a59e-c93387bb3a2a", "Third Party Notify");
			zTextBoxColumnStyleInfo7.ColumnName = "IA_GS_NKReportingStaff";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.ColumnName = "IA_Status";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.ColumnName = "IA_ClientSpecifiedStatus";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo10.ColumnName = "IA_Criticality";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("f7c71353-a8ce-492d-9852-5efd99ce843a", "Sec. / Req. / Srv. Code", "Menu Section / Requirement / Service Code", "Sec. / Req. / Srv. Code", "");
			zTextBoxColumnStyleInfo11.ColumnName = "IA_Module";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("d50d015f-5d87-4b3b-a446-1a7969a2b07e", "Sec. / Req. / Srv.", "Menu Section / Requirement / Service", "Sec. / Req. / Srv.", "");
			zTextBoxColumnStyleInfo12.ColumnName = "IA_ModuleDescription";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.CustomerService.GUI.Res.GetData("IncidentApprovalFilterControl|f38cf466-a988-4f10-a1cd-ed4af755aac0", "Created");
			zDateEditColumnStyleInfo1.ColumnName = "IA_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 392, true);
			this.grid.TabIndex = 19;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(611, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.IncidentApproval);
			// 
			// IncidentApprovalFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "IncidentApprovalFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
