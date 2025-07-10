using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class EDIProjectContactControl
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
			this.LicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ClientInfoGroupBox.SuspendLayout();
			this.zAddressControl1.SuspendLayout();
			this.ContactFindBox.SuspendLayout();
			this.ContactPhoneDiallerUserControl.SuspendLayout();
			this.TechContactPhoneDiallerUserControl.SuspendLayout();
			this.ClientOrganisationFindBox.SuspendLayout();
			this.TechContactGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicenceGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ClientInfoGroupBox
			// 
			this.ClientInfoGroupBox.Controls.Add(this.LicenceGuidFindBox);
			this.ClientInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 173, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.EDIProject);
			// 
			// LicenceGuidFindBox
			// 
			this.LicenceGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceGuidFindBox, "LicenceHeaderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).LicenceHeaderPK)));
			this.LicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("5791177D-BE89-4376-BE8F-3C1E909A0FA9", "Client License");
			this.LicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 136, true);
			this.LicenceGuidFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.LicenceGuidFindBox.Name = "LicenceGuidFindBox";
			this.LicenceGuidFindBox.PreBoundMaxLength = 45;
			this.LicenceGuidFindBox.ShowDescriptionBox = false;
			this.LicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 20, true);
			this.LicenceGuidFindBox.TabIndex = 5;
			// 
			// EDIProjectContactControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Name = "EDIProjectContactControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 307, true);
			this.ClientInfoGroupBox.ResumeLayout(false);
			this.ClientInfoGroupBox.PerformLayout();
			this.zAddressControl1.ResumeLayout(true);
			this.zAddressControl1.PerformLayout();
			this.ContactFindBox.ResumeLayout(true);
			this.ContactFindBox.PerformLayout();
			this.ContactPhoneDiallerUserControl.ResumeLayout(true);
			this.ContactPhoneDiallerUserControl.PerformLayout();
			this.TechContactPhoneDiallerUserControl.ResumeLayout(true);
			this.TechContactPhoneDiallerUserControl.PerformLayout();
			this.ClientOrganisationFindBox.ResumeLayout(true);
			this.ClientOrganisationFindBox.PerformLayout();
			this.TechContactGuidFindBox.ResumeLayout(true);
			this.TechContactGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicenceGuidFindBox.ResumeLayout(true);
			this.LicenceGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZGuidFindBox LicenceGuidFindBox;

		#endregion
	}
}
