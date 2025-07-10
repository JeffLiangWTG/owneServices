namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class NctsPreviousProceduresATAVPanel
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
			this.AuthorizationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MonitoringCustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SimplifiedGrantAuthorizationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizationNumberDropEdit.SuspendLayout();
			this.MonitoringCustomsOfficeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// AuthorizationNumberDropEdit
			// 
			this.AuthorizationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropEdit, "PreviousProcedureMaster.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.AuthorizationNumber)));
			this.AuthorizationNumberDropEdit.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("05136270-DB6F-4EA7-A389-977BC5A97BF2", "Authorization Number");
			this.AuthorizationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 29, true);
			this.AuthorizationNumberDropEdit.Name = "AuthorizationNumberDropEdit";
			this.AuthorizationNumberDropEdit.ShowDescriptionBox = false;
			this.AuthorizationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AuthorizationNumberDropEdit.TabIndex = 1;
			// 
			// MonitoringCustomsOfficeFindBox
			// 
			this.MonitoringCustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MonitoringCustomsOfficeFindBox, "PreviousProcedureMaster.CSI_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.CSI_CustomsOffice)));
			this.MonitoringCustomsOfficeFindBox.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("AC0D52AD-4518-4AAA-90F4-DC3B6F3F3684", "Monitoring Customs Office");
			this.MonitoringCustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 29, true);
			this.MonitoringCustomsOfficeFindBox.Name = "MonitoringCustomsOfficeFindBox";
			this.MonitoringCustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.MonitoringCustomsOfficeFindBox.ParentType = null;
			this.MonitoringCustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
			this.MonitoringCustomsOfficeFindBox.TabIndex = 2;
			// 
			// SimplifiedGrantAuthorizationCheckBox
			// 
			this.SimplifiedGrantAuthorizationCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimplifiedGrantAuthorizationCheckBox, "PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag)));
			this.SimplifiedGrantAuthorizationCheckBox.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("646DB203-0966-4D73-80D4-6871088D0F06", "Simplified Grant Authorization?");
			this.SimplifiedGrantAuthorizationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SimplifiedGrantAuthorizationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 3, true);
			this.SimplifiedGrantAuthorizationCheckBox.Name = "SimplifiedGrantAuthorizationCheckBox";
			this.SimplifiedGrantAuthorizationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.SimplifiedGrantAuthorizationCheckBox.TabIndex = 3;
			// 
			// NctsPreviousProceduresATAVPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SimplifiedGrantAuthorizationCheckBox);
			this.Controls.Add(this.AuthorizationNumberDropEdit);
			this.Controls.Add(this.MonitoringCustomsOfficeFindBox);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 43, true);
			this.Name = "NctsPreviousProceduresATAVPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizationNumberDropEdit.ResumeLayout(true);
			this.AuthorizationNumberDropEdit.PerformLayout();
			this.MonitoringCustomsOfficeFindBox.ResumeLayout(true);
			this.MonitoringCustomsOfficeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit AuthorizationNumberDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox MonitoringCustomsOfficeFindBox;
		internal ZArchitecture.GUI.ZCheckBox SimplifiedGrantAuthorizationCheckBox;
	}
}
