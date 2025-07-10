namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class EDIProjectStatusControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		
		private void InitializeComponent()
		{
			this.FollowUpDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PlannedInstallDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InstallDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PlannedGoLiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GoLiveCompleteDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AgreedGoLiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatusRowPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FollowUpDateEdit.SuspendLayout();
			this.PlannedInstallDateEdit.SuspendLayout();
			this.InstallDateEdit.SuspendLayout();
			this.PlannedGoLiveDateEdit.SuspendLayout();
			this.GoLiveCompleteDateEdit.SuspendLayout();
			this.AgreedGoLiveDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// StatusRowPanel
			// 
			this.StatusRowPanel.Controls.Add(this.AgreedGoLiveDateEdit);
			this.StatusRowPanel.Controls.Add(this.GoLiveCompleteDateEdit);
			this.StatusRowPanel.Controls.Add(this.PlannedGoLiveDateEdit);
			this.StatusRowPanel.Controls.Add(this.InstallDateEdit);
			this.StatusRowPanel.Controls.Add(this.PlannedInstallDateEdit);
			this.StatusRowPanel.Controls.Add(this.FollowUpDateEdit);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.StatusRowPanel, true);
			this.StatusRowPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 299, true);
			this.StatusRowPanel.Controls.SetChildIndex(this.FollowUpDateEdit, 0);
			this.StatusRowPanel.Controls.SetChildIndex(this.PlannedInstallDateEdit, 0);
			this.StatusRowPanel.Controls.SetChildIndex(this.InstallDateEdit, 0);
			this.StatusRowPanel.Controls.SetChildIndex(this.PlannedGoLiveDateEdit, 0);
			this.StatusRowPanel.Controls.SetChildIndex(this.GoLiveCompleteDateEdit, 0);
			this.StatusRowPanel.Controls.SetChildIndex(this.AgreedGoLiveDateEdit, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.EDIProject);
			// 
			// FollowUpDateEdit
			// 
			this.FollowUpDateEdit.AllowDrop = true;
			this.FollowUpDateEdit.AutoCompleteMonthThreshold = 1;
			this.FollowUpDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FollowUpDateEdit, "CallbackBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).CallbackBy)));
			this.FollowUpDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("9d6e0f3e-a9f9-40e7-87a4-86449e3a0bc6", "Follow Up Date");
			this.FollowUpDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 201, true);
			this.FollowUpDateEdit.Name = "FollowUpDateEdit";
			this.StatusRowPanel.SetRow(this.FollowUpDateEdit, 8);
			this.FollowUpDateEdit.TabIndex = 9;
			// 
			// PlannedInstallDateEdit
			// 
			this.PlannedInstallDateEdit.AllowDrop = true;
			this.PlannedInstallDateEdit.AutoCompleteMonthThreshold = 1;
			this.PlannedInstallDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PlannedInstallDateEdit, "PlannedInstall");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).PlannedInstall)));
			this.PlannedInstallDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("02220919-7db5-4a1b-afaa-dab4581a251b", "Planned Install");
			this.PlannedInstallDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
			this.PlannedInstallDateEdit.Name = "PlannedInstallDateEdit";
			this.StatusRowPanel.SetRow(this.PlannedInstallDateEdit, 9);
			this.PlannedInstallDateEdit.TabIndex = 10;
			// 
			// InstallDateEdit
			// 
			this.InstallDateEdit.AllowDrop = true;
			this.InstallDateEdit.AutoCompleteMonthThreshold = 1;
			this.InstallDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InstallDateEdit, "InstallDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).InstallDate)));
			this.InstallDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("8fcacbb2-203a-440c-bf5a-a8c9954dbcab", "Install Complete");
			this.InstallDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 225, true);
			this.InstallDateEdit.Name = "InstallDateEdit";
			this.StatusRowPanel.SetRow(this.InstallDateEdit, 9);
			this.InstallDateEdit.TabIndex = 11;
			// 
			// PlannedGoLiveDateEdit
			// 
			this.PlannedGoLiveDateEdit.AllowDrop = true;
			this.PlannedGoLiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.PlannedGoLiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PlannedGoLiveDateEdit, "EstimatedSiteLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).EstimatedSiteLiveDate)));
			this.PlannedGoLiveDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("212d6ce9-4ab7-4aa0-9c25-cc3ff71c952d", "Planned Go-Live");
			this.PlannedGoLiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 250, true);
			this.PlannedGoLiveDateEdit.Name = "PlannedGoLiveDateEdit";
			this.StatusRowPanel.SetRow(this.PlannedGoLiveDateEdit, 10);
			this.PlannedGoLiveDateEdit.TabIndex = 12;
			// 
			// GoLiveCompleteDateEdit
			// 
			this.GoLiveCompleteDateEdit.AllowDrop = true;
			this.GoLiveCompleteDateEdit.AutoCompleteMonthThreshold = 1;
			this.GoLiveCompleteDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.GoLiveCompleteDateEdit, "SiteLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).SiteLiveDate)));
			this.GoLiveCompleteDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("b907b9fa-3a04-4153-ba77-2e660f09fe12", "Go-Live Complete");
			this.GoLiveCompleteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 250, true);
			this.GoLiveCompleteDateEdit.Name = "GoLiveCompleteDateEdit";
			this.StatusRowPanel.SetRow(this.GoLiveCompleteDateEdit, 10);
			this.GoLiveCompleteDateEdit.TabIndex = 13;
			// 
			// AgreedGoLiveDateEdit
			// 
			this.AgreedGoLiveDateEdit.AllowDrop = true;
			this.AgreedGoLiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.AgreedGoLiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AgreedGoLiveDateEdit, "AgreedLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).AgreedLiveDate)));
			this.AgreedGoLiveDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("95832856-e297-4573-a07f-83f288b40c7c", "Agreed Go-Live");
			this.AgreedGoLiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.AgreedGoLiveDateEdit.Name = "AgreedGoLiveDateEdit";
			this.StatusRowPanel.SetRow(this.AgreedGoLiveDateEdit, 11);
			this.AgreedGoLiveDateEdit.TabIndex = 14;
			// 
			// EDIProjectStatusControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EDIProjectStatusControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 333, true);
			this.StatusRowPanel.ResumeLayout(false);
			this.StatusRowPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FollowUpDateEdit.ResumeLayout(true);
			this.FollowUpDateEdit.PerformLayout();
			this.PlannedInstallDateEdit.ResumeLayout(true);
			this.PlannedInstallDateEdit.PerformLayout();
			this.InstallDateEdit.ResumeLayout(true);
			this.InstallDateEdit.PerformLayout();
			this.PlannedGoLiveDateEdit.ResumeLayout(true);
			this.PlannedGoLiveDateEdit.PerformLayout();
			this.GoLiveCompleteDateEdit.ResumeLayout(true);
			this.GoLiveCompleteDateEdit.PerformLayout();
			this.AgreedGoLiveDateEdit.ResumeLayout(true);
			this.AgreedGoLiveDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit FollowUpDateEdit;
		private ZArchitecture.GUI.ZDateEdit InstallDateEdit;
		private ZArchitecture.GUI.ZDateEdit PlannedInstallDateEdit;
		private ZArchitecture.GUI.ZDateEdit PlannedGoLiveDateEdit;
		private ZArchitecture.GUI.ZDateEdit GoLiveCompleteDateEdit;
		private ZArchitecture.GUI.ZDateEdit AgreedGoLiveDateEdit;
	}
}
