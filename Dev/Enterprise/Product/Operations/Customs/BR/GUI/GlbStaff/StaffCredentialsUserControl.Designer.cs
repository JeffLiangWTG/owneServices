namespace Enterprise.Customs.BR.GUI
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
			this.CompaniesGroupBR = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NodeListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BRAccUserGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BRCertificatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificatePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateLoaderUserControl = new Enterprise.Customs.BR.GUI.BRDigitalCertificateControl_p12();
			this.CertForLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BRCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompaniesGroupBR.SuspendLayout();
			this.NodeListGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BRAccUserGrid)).BeginInit();
			this.BRAccUserGrid.SuspendLayout();
			this.BRCertificatePanel.SuspendLayout();
			this.PasswordStatusDropEdit.SuspendLayout();
			this.CertificateLoaderUserControl.SuspendLayout();
			this.BRCredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.BRGlbStaffWrapper);
			// 
			// CompaniesGroupBR
			// 
			this.CompaniesGroupBR.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BRStaffCredentialsUserControl|321bab9f-6b61-4d00-adf1-c39cbc5ffaf1", "Subscriptions Management");
			this.CompaniesGroupBR.Controls.Add(this.NodeListGroupBox);
			this.CompaniesGroupBR.Controls.Add(this.BRCertificatePanel);
			this.CompaniesGroupBR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompaniesGroupBR.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompaniesGroupBR.Name = "CompaniesGroupBR";
			this.CompaniesGroupBR.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.CompaniesGroupBR.TabIndex = 1;
			this.CompaniesGroupBR.TabStop = false;
			// 
			// NodeListGroupBox
			// 
			this.NodeListGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("E54885B4-F295-4860-810B-2A3E9C0F8C1C", "Subscriptions");
			this.NodeListGroupBox.Controls.Add(this.BRAccUserGrid);
			this.NodeListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 119, true);
			this.NodeListGroupBox.Name = "NodeListGroupBox";
			this.NodeListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 310, true);
			this.NodeListGroupBox.TabIndex = 15;
			this.NodeListGroupBox.TabStop = false;
			// 
			// BRAccUserGrid
			// 
			this.BRAccUserGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BRAccUserGrid, "EventSubscriptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.BRGlbStaffWrapper)(null)).EventSubscriptions)));
			this.BRAccUserGrid.CaptionVisible = false;
			this.BRAccUserGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BRAccUserGrid.GridId = "544153E0-2D45-4805-8C13-832F532D601D";
			this.BRAccUserGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BRAccUserGrid.LayoutKey = "BRAccUserGrid";
			this.BRAccUserGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BRAccUserGrid.Name = "BRAccUserGrid";
			this.BRAccUserGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(799, 291, true);
			this.BRAccUserGrid.TabIndex = 2;
			// 
			// BRCertificatePanel
			// 
			this.BRCertificatePanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.BRCertificatePanel.Controls.Add(this.CertificatePasswordTextBox);
			this.BRCertificatePanel.Controls.Add(this.PasswordStatusDropEdit);
			this.BRCertificatePanel.Controls.Add(this.CertificateLoaderUserControl);
			this.BRCertificatePanel.Controls.Add(this.CertForLabel);
			this.BRCertificatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
			this.BRCertificatePanel.Name = "BRCertificatePanel";
			this.BRCertificatePanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(58, 7, 58, 7, true);
			this.BRCertificatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 96, true);
			this.BRCertificatePanel.TabIndex = 14;
			// 
			// CertificatePasswordTextBox
			// 
			this.CertificatePasswordTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CertificatePasswordTextBox, "CCTPassword.CurrentDecryptedCertificatePassphrase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.BRGlbStaffWrapper)(null)).CCTPassword.CurrentDecryptedCertificatePassphrase)));
			this.CertificatePasswordTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ed8ca058-8184-4898-a4e1-5003a8309dd9", "Certificate Password");
			this.CertificatePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificatePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 39, true);
			this.CertificatePasswordTextBox.Name = "CertificatePasswordTextBox";
			this.CertificatePasswordTextBox.PasswordChar = '*';
			this.CertificatePasswordTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CertificatePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.CertificatePasswordTextBox.TabIndex = 1;
			// 
			// PasswordStatusDropEdit
			// 
			this.PasswordStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PasswordStatusDropEdit, "CCTPassword.GP_PasswordStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.BRGlbStaffWrapper)(null)).CCTPassword.GP_PasswordStatus)));
			this.PasswordStatusDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a858801a-00fa-442a-9659-ac3a33d12cc1", "Password Status");
			this.PasswordStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 63, true);
			this.PasswordStatusDropEdit.Name = "PasswordStatusDropEdit";
			this.PasswordStatusDropEdit.PreBoundMaxLength = 3;
			this.PasswordStatusDropEdit.ShouldResizeByMaxLength = true;
			this.PasswordStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.PasswordStatusDropEdit.TabIndex = 2;
			// 
			// CertificateLoaderUserControl
			// 
			this.CertificateLoaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateLoaderUserControl, "CCTPassword.GP_Certificate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.BRGlbStaffWrapper)(null)).CCTPassword.GP_Certificate)));
			this.CertificateLoaderUserControl.DaysBeforeExpiryWarningMessage = null;
			this.CertificateLoaderUserControl.FileDataAsString = "";
			this.CertificateLoaderUserControl.FileDialogTitle = "";
			this.CertificateLoaderUserControl.FileFilter = "All files (*.*)|*.*";
			this.CertificateLoaderUserControl.InitialDirectory = "";
			this.CertificateLoaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 9, true);
			this.CertificateLoaderUserControl.Name = "CertificateLoaderUserControl";
			this.CertificateLoaderUserControl.ReadOnly = false;
			this.CertificateLoaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 26, true);
			this.CertificateLoaderUserControl.State = Enterprise.Registry.GUI.DataLoadState.NoData;
			this.CertificateLoaderUserControl.TabIndex = 0;
			// 
			// CertForLabel
			// 
			this.CertForLabel.AutoSize = true;
			this.CertForLabel.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a0c5981f-d414-478a-9ca7-8e4e10def0aa", "", "Certificate");
			this.CertForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CertForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 15, true);
			this.CertForLabel.Name = "CertForLabel";
			this.CertForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 13, true);
			this.CertForLabel.TabIndex = 0;
			// 
			// BRCredentialsPanel
			// 
			this.BRCredentialsPanel.Controls.Add(this.CompaniesGroupBR);
			this.BRCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BRCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.BRCredentialsPanel.Name = "BRCredentialsPanel";
			this.BRCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.BRCredentialsPanel.TabIndex = 5;
			// 
			// StaffCredentialsUserControl
			// 
			this.Controls.Add(this.BRCredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.BRCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompaniesGroupBR.ResumeLayout(false);
			this.CompaniesGroupBR.PerformLayout();
			this.NodeListGroupBox.ResumeLayout(false);
			this.NodeListGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BRAccUserGrid)).EndInit();
			this.BRAccUserGrid.ResumeLayout(false);
			this.BRAccUserGrid.PerformLayout();
			this.BRCertificatePanel.ResumeLayout(false);
			this.BRCertificatePanel.PerformLayout();
			this.PasswordStatusDropEdit.ResumeLayout(true);
			this.PasswordStatusDropEdit.PerformLayout();
			this.CertificateLoaderUserControl.ResumeLayout(true);
			this.CertificateLoaderUserControl.PerformLayout();
			this.BRCredentialsPanel.ResumeLayout(false);
			this.BRCredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel BRCertificatePanel;
		Enterprise.ZArchitecture.GUI.ZPanel BRCredentialsPanel;
		BRDigitalCertificateControl_p12 CertificateLoaderUserControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox CompaniesGroupBR;
		Enterprise.ZArchitecture.ZLabel CertForLabel;
		public ZArchitecture.GUI.ZDropEdit PasswordStatusDropEdit;
		public ZArchitecture.ZTextBox CertificatePasswordTextBox;
		private ZArchitecture.GUI.ZGroupBox NodeListGroupBox;
		private ZArchitecture.ZGrid BRAccUserGrid;
	}
}
