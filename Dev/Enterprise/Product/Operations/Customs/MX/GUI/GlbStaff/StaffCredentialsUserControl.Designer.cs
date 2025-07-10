namespace Enterprise.Customs.MX.GUI
{
	partial class StaffCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CredentialGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CredentialsUserGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CredentialGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CredentialsUserGrid)).BeginInit();
			this.CredentialsUserGrid.SuspendLayout();
			this.CredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.MXGlbStaffWrapper);
			// 
			// CredentialGroupBox
			// 
			this.CredentialGroupBox.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("F67213A4-FEC5-414D-8255-7E4648A8342E", "Credentials");
			this.CredentialGroupBox.Controls.Add(this.CredentialsUserGrid);
			this.CredentialGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialGroupBox.Name = "CredentialGroupBox";
			this.CredentialGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CredentialGroupBox.TabIndex = 1;
			this.CredentialGroupBox.TabStop = false;
			// 
			// CredentialsUserGrid
			// 
			this.CredentialsUserGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CredentialsUserGrid, "StaffLicenses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.MX.Business.MXGlbStaffWrapper)(null)).StaffLicenses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.GlbExternalPassword_MXL)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.MXGlbStaffWrapper)(null)).StaffLicenses)).SyncRoot)).GP_CertificateAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.GlbExternalPassword_MXL)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.MXGlbStaffWrapper)(null)).StaffLicenses)).SyncRoot)).GP_UserID)));
			this.CredentialsUserGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GP_CertificateAuthority";
			zCodeFindBoxColumnStyleInfo1.ModuleID = ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.CredentialsUserGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CredentialsUserGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CredentialsUserGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsUserGrid.GridId = "DFB7EA4D-AC25-4905-B749-68080F79FF90";
			this.CredentialsUserGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CredentialsUserGrid.LayoutKey = "CredentialsUserGrid";
			this.CredentialsUserGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CredentialsUserGrid.Name = "CredentialsUserGrid";
			this.CredentialsUserGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 416, true);
			this.CredentialsUserGrid.TabIndex = 1;
			// 
			// CredentialsPanel
			// 
			this.CredentialsPanel.Controls.Add(this.CredentialGroupBox);
			this.CredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.CredentialsPanel.Name = "CredentialsPanel";
			this.CredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CredentialsPanel.TabIndex = 5;
			// 
			// StaffCredentialsUserControl
			// 
			this.Controls.Add(this.CredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.CredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CredentialGroupBox.ResumeLayout(false);
			this.CredentialGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CredentialsUserGrid)).EndInit();
			this.CredentialsUserGrid.ResumeLayout(false);
			this.CredentialsUserGrid.PerformLayout();
			this.CredentialsPanel.ResumeLayout(false);
			this.CredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel CredentialsPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox CredentialGroupBox;
		Enterprise.ZArchitecture.ZGrid CredentialsUserGrid;
	}
}
