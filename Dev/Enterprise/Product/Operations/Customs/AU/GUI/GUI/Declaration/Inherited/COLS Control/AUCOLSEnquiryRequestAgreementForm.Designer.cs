namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSEnquiryRequestAgreementForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.DeclarationAgreementControl = new Enterprise.Customs.AU.GUI.DeclarationAgreementUserControl();
			this.EnquiryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContactDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContactEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactPhoneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DefaultContactDetailsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireDocumentationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EnquiryTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationAgreementControl.SuspendLayout();
			this.EnquiryTypeDropEdit.SuspendLayout();
			this.ContactDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 672, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation);
			// 
			// DeclarationAgreementControl
			// 
			this.DeclarationAgreementControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationAgreementControl, "DeclarationAcceptance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.COLSDeclarationAcceptance)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).DeclarationAcceptance)));
			this.DeclarationAgreementControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 159, true);
			this.DeclarationAgreementControl.Name = "DeclarationAgreementControl";
			this.DeclarationAgreementControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 473, true);
			this.DeclarationAgreementControl.TabIndex = 5;
			// 
			// EnquiryTypeDropEdit
			// 
			this.EnquiryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EnquiryTypeDropEdit, "EnquiryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).EnquiryType)));
			this.EnquiryTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EnquiryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 11, true);
			this.EnquiryTypeDropEdit.Name = "EnquiryTypeDropEdit";
			this.EnquiryTypeDropEdit.ShouldResizeByMaxLength = false;
			this.EnquiryTypeDropEdit.ShowDescriptionBox = false;
			this.EnquiryTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.EnquiryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 15, true);
			this.EnquiryTypeDropEdit.TabIndex = 2;
			this.EnquiryTypeDropEdit.UseFullWidthForCodeBox = true;
			// 
			// ContactDetailsGroupBox
			// 
			this.ContactDetailsGroupBox.Controls.Add(this.ContactEmailLabel);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactPhoneLabel);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactNameLabel);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactEmailTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactPhoneTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.ContactNameTextBox);
			this.ContactDetailsGroupBox.Controls.Add(this.DefaultContactDetailsCheckBox);
			this.ContactDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 34, true);
			this.ContactDetailsGroupBox.Name = "ContactDetailsGroupBox";
			this.ContactDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 96, true);
			this.ContactDetailsGroupBox.TabIndex = 3;
			this.ContactDetailsGroupBox.TabStop = false;
			this.ContactDetailsGroupBox.Text = "Contact Details";
			// 
			// ContactEmailLabel
			// 
			this.ContactEmailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 75, true);
			this.ContactEmailLabel.Name = "ContactEmailLabel";
			this.ContactEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.ContactEmailLabel.TabIndex = 6;
			this.ContactEmailLabel.Text = "Email";
			this.ContactEmailLabel.UseMnemonic = false;
			// 
			// ContactPhoneLabel
			// 
			this.ContactPhoneLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactPhoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 55, true);
			this.ContactPhoneLabel.Name = "ContactPhoneLabel";
			this.ContactPhoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.ContactPhoneLabel.TabIndex = 5;
			this.ContactPhoneLabel.Text = "Phone";
			this.ContactPhoneLabel.UseMnemonic = false;
			// 
			// ContactNameLabel
			// 
			this.ContactNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 35, true);
			this.ContactNameLabel.Name = "ContactNameLabel";
			this.ContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);
			this.ContactNameLabel.TabIndex = 4;
			this.ContactNameLabel.Text = "Name";
			this.ContactNameLabel.UseMnemonic = false;
			// 
			// ContactEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactEmailTextBox, "ContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).ContactEmail)));
			this.ContactEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 76, true);
			this.ContactEmailTextBox.Name = "ContactEmailTextBox";
			this.ContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 15, true);
			this.ContactEmailTextBox.TabIndex = 3;
			// 
			// ContactPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactPhoneTextBox, "ContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).ContactPhone)));
			this.ContactPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 56, true);
			this.ContactPhoneTextBox.Name = "ContactPhoneTextBox";
			this.ContactPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 15, true);
			this.ContactPhoneTextBox.TabIndex = 2;
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).ContactName)));
			this.ContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 36, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 15, true);
			this.ContactNameTextBox.TabIndex = 1;
			// 
			// DefaultContactDetailsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DefaultContactDetailsCheckBox, "DefaultContactDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).DefaultContactDetails)));
			this.DefaultContactDetailsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 10, true);
			this.DefaultContactDetailsCheckBox.Name = "DefaultContactDetailsCheckBox";
			this.DefaultContactDetailsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 24, true);
			this.DefaultContactDetailsCheckBox.TabIndex = 0;
			this.DefaultContactDetailsCheckBox.Text = "Default from Responsible Party";
			this.DefaultContactDetailsCheckBox.UseVisualStyleBackColor = true;
			// 
			// RequireDocumentationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RequireDocumentationCheckBox, "DocumentRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation)(null)).DocumentRequired)));
			this.RequireDocumentationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 136, true);
			this.RequireDocumentationCheckBox.Name = "RequireDocumentationCheckBox";
			this.RequireDocumentationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.RequireDocumentationCheckBox.TabIndex = 4;
			this.RequireDocumentationCheckBox.Text = "Documentation Required";
			this.RequireDocumentationCheckBox.UseVisualStyleBackColor = true;
			// 
			// SendButton
			// 
			this.SendButton.IsCaptionOverridden = true;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 638, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.SendButton.TabIndex = 6;
			this.SendButton.Text = "Send";
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 638, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// EnquiryTypeLabel
			// 
			this.EnquiryTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EnquiryTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 11, true);
			this.EnquiryTypeLabel.Name = "EnquiryTypeLabel";
			this.EnquiryTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 15, true);
			this.EnquiryTypeLabel.TabIndex = 1;
			this.EnquiryTypeLabel.Text = "Enquiry Type";
			this.EnquiryTypeLabel.UseMnemonic = false;
			// 
			// AUCOLSEnquiryRequestAgreementForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 696, true);
			this.Controls.Add(this.EnquiryTypeLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.RequireDocumentationCheckBox);
			this.Controls.Add(this.ContactDetailsGroupBox);
			this.Controls.Add(this.EnquiryTypeDropEdit);
			this.Controls.Add(this.DeclarationAgreementControl);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.COLSEnquiryAdditionalInformation);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 731, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 725, true);
			this.Name = "AUCOLSEnquiryRequestAgreementForm";
			this.Text = "Make an Enquiry";
			this.Controls.SetChildIndex(this.DeclarationAgreementControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EnquiryTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.ContactDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.RequireDocumentationCheckBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.EnquiryTypeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationAgreementControl.ResumeLayout(true);
			this.DeclarationAgreementControl.PerformLayout();
			this.EnquiryTypeDropEdit.ResumeLayout(true);
			this.EnquiryTypeDropEdit.PerformLayout();
			this.ContactDetailsGroupBox.ResumeLayout(false);
			this.ContactDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DeclarationAgreementUserControl DeclarationAgreementControl;
		internal ZArchitecture.GUI.ZDropEdit EnquiryTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox ContactDetailsGroupBox;
		internal ZArchitecture.ZTextBox ContactNameTextBox;
		internal ZArchitecture.GUI.ZCheckBox DefaultContactDetailsCheckBox;
		internal ZArchitecture.ZTextBox ContactPhoneTextBox;
		internal ZArchitecture.ZTextBox ContactEmailTextBox;
		internal ZArchitecture.GUI.ZCheckBox RequireDocumentationCheckBox;
		internal ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZLabel ContactEmailLabel;
		private ZArchitecture.ZLabel ContactPhoneLabel;
		private ZArchitecture.ZLabel ContactNameLabel;
		private ZArchitecture.ZLabel EnquiryTypeLabel;
	}
}
