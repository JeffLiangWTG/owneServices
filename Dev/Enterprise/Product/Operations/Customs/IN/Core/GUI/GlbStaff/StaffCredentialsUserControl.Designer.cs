using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IN.GUI
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
			this.ICEGATEProfileGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ICEGATELoginDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NeedCopyOfEmailsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoGenerateEmailIdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendCopyToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ICEGATEPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ICEGATELoginTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ICEGATEEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.INCredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DscTokenProfileGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DscTokenDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CertificateAuthorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChipsetDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChooseCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ICEGATEProfileGroupBox.SuspendLayout();
			this.ICEGATELoginDetailsPanel.SuspendLayout();
			this.INCredentialsPanel.SuspendLayout();
			this.DscTokenProfileGroupBox.SuspendLayout();
			this.DscTokenDetailsPanel.SuspendLayout();
			this.CertificateAuthorityDropEdit.SuspendLayout();
			this.ChipsetDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.GlbStaffWrapper);
			// 
			// ICEGATEProfileGroupBox
			// 
			this.ICEGATEProfileGroupBox.CaptionResourceString = Res.GetData("280497A7-9997-4D42-92C3-0A94E5E641BF", "ICEGATE Profile");
			this.ICEGATEProfileGroupBox.Controls.Add(this.ICEGATELoginDetailsPanel);
			this.ICEGATEProfileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ICEGATEProfileGroupBox.Name = "ICEGATEProfileGroupBox";
			this.ICEGATEProfileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 126, true);
			this.ICEGATEProfileGroupBox.TabIndex = 1;
			this.ICEGATEProfileGroupBox.TabStop = false;
			// 
			// ICEGATELoginDetailsPanel
			// 
			this.ICEGATELoginDetailsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.ICEGATELoginDetailsPanel.Controls.Add(this.NeedCopyOfEmailsCheckBox);
			this.ICEGATELoginDetailsPanel.Controls.Add(this.AutoGenerateEmailIdCheckBox);
			this.ICEGATELoginDetailsPanel.Controls.Add(this.SendCopyToTextBox);
			this.ICEGATELoginDetailsPanel.Controls.Add(this.ICEGATEPasswordTextBox);
			this.ICEGATELoginDetailsPanel.Controls.Add(this.ICEGATELoginTextBox);
			this.ICEGATELoginDetailsPanel.Controls.Add(this.ICEGATEEmailTextBox);
			this.ICEGATELoginDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
			this.ICEGATELoginDetailsPanel.Name = "ICEGATELoginDetailsPanel";
			this.ICEGATELoginDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.ICEGATELoginDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 94, true);
			this.ICEGATELoginDetailsPanel.TabIndex = 14;
			// 
			// NeedCopyOfEmailsCheckBox
			// 
			this.NeedCopyOfEmailsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NeedCopyOfEmailsCheckBox, "LoginPassword.NeedCopyOfEmails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.NeedCopyOfEmails)));
			this.NeedCopyOfEmailsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NeedCopyOfEmailsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 49, true);
			this.NeedCopyOfEmailsCheckBox.Name = "NeedCopyOfEmailsCheckBox";
			this.NeedCopyOfEmailsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.NeedCopyOfEmailsCheckBox.TabIndex = 8;
			this.NeedCopyOfEmailsCheckBox.UseVisualStyleBackColor = true;
			// 
			// SendCopyToTextBox
			// 
			this.SendCopyToTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SendCopyToTextBox, "LoginPassword.CopyToMailBox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.CopyToMailBox)));
			this.SendCopyToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SendCopyToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 46, true);
			this.SendCopyToTextBox.Name = "SendCopyToTextBox";
			this.SendCopyToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.SendCopyToTextBox.TabIndex = 9;
			// 
			// ICEGATEPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.ICEGATEPasswordTextBox, "LoginPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.CurrentDecryptedPassword)));
			this.ICEGATEPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ICEGATEPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(499, 10, true);
			this.ICEGATEPasswordTextBox.Name = "ICEGATEPasswordTextBox";
			this.ICEGATEPasswordTextBox.PasswordChar = '*';
			this.ICEGATEPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ICEGATEPasswordTextBox.TabIndex = 1;
			// 
			// ICEGATELoginTextBox
			// 
			this.BindingSource.SetBindingMember(this.ICEGATELoginTextBox, "LoginPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.GP_UserID)));
			this.ICEGATELoginTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ICEGATELoginTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 10, true);
			this.ICEGATELoginTextBox.Name = "ICEGATELoginTextBox";
			this.ICEGATELoginTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ICEGATELoginTextBox.TabIndex = 0;
			// 
			// AutoGenerateEmailIdCheckBox
			// 
			this.AutoGenerateEmailIdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoGenerateEmailIdCheckBox, "LoginPassword.AutoGenerateEmailId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.AutoGenerateEmailId)));
			this.AutoGenerateEmailIdCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AutoGenerateEmailIdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 49, true);
			this.AutoGenerateEmailIdCheckBox.Name = "AutoGenerateEmailIdCheckBox";
			this.AutoGenerateEmailIdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AutoGenerateEmailIdCheckBox.TabIndex = 6;
			this.AutoGenerateEmailIdCheckBox.UseVisualStyleBackColor = true;
			// 
			// ICEGATEEmailTextBox
			// 
			this.ICEGATEEmailTextBox.CaptionResourceString = null;
			this.ICEGATEEmailTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ICEGATEEmailTextBox, "LoginPassword.GP_MailBoxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).LoginPassword.GP_MailBoxID)));
			this.ICEGATEEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ICEGATEEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 46, true);
			this.ICEGATEEmailTextBox.Name = "ICEGATEEmailTextBox";
			this.ICEGATEEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.ICEGATEEmailTextBox.TabIndex = 7;
			// 
			// INCredentialsPanel
			// 
			this.INCredentialsPanel.Controls.Add(this.DscTokenProfileGroupBox);
			this.INCredentialsPanel.Controls.Add(this.ICEGATEProfileGroupBox);
			this.INCredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.INCredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.INCredentialsPanel.Name = "INCredentialsPanel";
			this.INCredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 435, true);
			this.INCredentialsPanel.TabIndex = 5;
			// 
			// DscTokenProfileGroupBox
			// 
			this.DscTokenProfileGroupBox.CaptionResourceString = Res.GetData("263b5e18-72ac-485e-8408-63e9864d2ba6", "Digital Signature Certificate Token Profile");
			this.DscTokenProfileGroupBox.Controls.Add(this.DscTokenDetailsPanel);
			this.DscTokenProfileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.DscTokenProfileGroupBox.Name = "DscTokenProfileGroupBox";
			this.DscTokenProfileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 312, true);
			this.DscTokenProfileGroupBox.TabIndex = 16;
			this.DscTokenProfileGroupBox.TabStop = false;
			// 
			// DscTokenDetailsPanel
			// 
			this.DscTokenDetailsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.DscTokenDetailsPanel.Controls.Add(this.CertificateAuthorityDropEdit);
			this.DscTokenDetailsPanel.Controls.Add(this.ChipsetDropEdit);
			this.DscTokenDetailsPanel.Controls.Add(this.ChooseCertificateButton);
			this.DscTokenDetailsPanel.Controls.Add(this.SerialNumberTextBox);
			this.DscTokenDetailsPanel.Controls.Add(this.ClearCertificateButton);
			this.DscTokenDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 17, true);
			this.DscTokenDetailsPanel.Name = "DscTokenDetailsPanel";
			this.DscTokenDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.DscTokenDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 184, true);
			this.DscTokenDetailsPanel.TabIndex = 15;
			// 
			// CertificateAuthorityDropEdit
			// 
			this.CertificateAuthorityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateAuthorityDropEdit, "CertificatePassword.GP_CertificateAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).CertificatePassword.GP_CertificateAuthority)));
			this.CertificateAuthorityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateAuthorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 22, true);
			this.CertificateAuthorityDropEdit.Name = "CertificateAuthorityDropEdit";
			this.CertificateAuthorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 20, true);
			this.CertificateAuthorityDropEdit.TabIndex = 7;
			// 
			// ChipsetDropEdit
			// 
			this.ChipsetDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChipsetDropEdit, "CertificatePassword.GP_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).CertificatePassword.GP_Name)));
			this.ChipsetDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ChipsetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 48, true);
			this.ChipsetDropEdit.Name = "ChipsetDropEdit";
			this.ChipsetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 20, true);
			this.ChipsetDropEdit.TabIndex = 8;
			// 
			// ChooseCertificateButton
			// 
			this.ChooseCertificateButton.CaptionResourceString = Res.GetData("f5883a46-253f-4fcd-a7d3-fa3d64c0f20d", "Choose Certificate");
			this.ChooseCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 74, true);
			this.ChooseCertificateButton.Name = "ChooseCertificateButton";
			this.ChooseCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.ChooseCertificateButton.TabIndex = 10;
			this.ChooseCertificateButton.ToolTipCaption = null;
			this.ChooseCertificateButton.UseVisualStyleBackColor = true;
			// 
			// SerialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SerialNumberTextBox, "CertificatePassword.GP_CertificateSerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IN.Business.GlbStaffWrapper)(null)).CertificatePassword.GP_CertificateSerialNumber)));
			this.SerialNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 103, true);
			this.SerialNumberTextBox.Name = "SerialNumberTextBox";
			this.SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 20, true);
			this.SerialNumberTextBox.TabIndex = 11;
			// 
			// ClearCertificateButton
			// 
			this.ClearCertificateButton.CaptionResourceString = Res.GetData("f318f6dd-a48a-4e9f-8541-b6d13ed1d129", "Clear Certificate");
			this.ClearCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 129, true);
			this.ClearCertificateButton.Name = "ClearCertificateButton";
			this.ClearCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.ClearCertificateButton.TabIndex = 12;
			this.ClearCertificateButton.ToolTipCaption = null;
			this.ClearCertificateButton.UseVisualStyleBackColor = true;
			// 
			// StaffCredentialsUserControl
			// 
			this.Controls.Add(this.INCredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.INCredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ICEGATEProfileGroupBox.ResumeLayout(false);
			this.ICEGATEProfileGroupBox.PerformLayout();
			this.ICEGATELoginDetailsPanel.ResumeLayout(false);
			this.ICEGATELoginDetailsPanel.PerformLayout();
			this.INCredentialsPanel.ResumeLayout(false);
			this.INCredentialsPanel.PerformLayout();
			this.DscTokenProfileGroupBox.ResumeLayout(false);
			this.DscTokenProfileGroupBox.PerformLayout();
			this.DscTokenDetailsPanel.ResumeLayout(false);
			this.DscTokenDetailsPanel.PerformLayout();
			this.CertificateAuthorityDropEdit.ResumeLayout(true);
			this.CertificateAuthorityDropEdit.PerformLayout();
			this.ChipsetDropEdit.ResumeLayout(true);
			this.ChipsetDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel ICEGATELoginDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZPanel INCredentialsPanel;
		internal ZArchitecture.ZTextBox ICEGATELoginTextBox;
		internal ZArchitecture.ZTextBox ICEGATEPasswordTextBox;
		internal ZArchitecture.ZTextBox ICEGATEEmailTextBox;
		private ZArchitecture.GUI.ZPanel DscTokenDetailsPanel;
		private ZArchitecture.GUI.ZButton ChooseCertificateButton;
		private ZArchitecture.GUI.ZButton ClearCertificateButton;
		internal ZArchitecture.GUI.ZGroupBox ICEGATEProfileGroupBox;
		internal ZArchitecture.GUI.ZGroupBox DscTokenProfileGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ChipsetDropEdit;
		internal ZArchitecture.ZTextBox SerialNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit CertificateAuthorityDropEdit;
		internal ZArchitecture.ZTextBox SendCopyToTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox NeedCopyOfEmailsCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox AutoGenerateEmailIdCheckBox;
	}
}
