namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EdiAccountVerificationWarning
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContactGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.FormTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SecondDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProceedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelProceedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContactNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactNameValue = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailValue = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactGrid)).BeginInit();
			this.ContactGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 356, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact);
			// 
			// ContactGrid
			// 
			this.ContactGrid.AllowNavigation = false;
			this.ContactGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContactGrid, "AccountVerificationStatusCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).ContactRelationshipStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).ServerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Organisations.EDIAccountVerificationStatus.EdiAccountVerificationStatus)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).AccountVerificationStatusCollection)).SyncRoot)).Email)));
			this.ContactGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|0b02a31b-a817-40ec-bb99-ede8f13a75be", "Pending Verification Status");
			zTextBoxColumnStyleInfo1.ColumnName = "ContactRelationshipStatus";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|010e0326-a32e-4ad9-9d86-74498307ad9f", "Product");
			zTextBoxColumnStyleInfo2.ColumnName = "Product";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|e5ad2749-0737-40e7-b1f9-6437c8b07b1e", "License Type");
			zTextBoxColumnStyleInfo3.ColumnName = "LicenceType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|180461df-b3e1-4d19-ac94-c0f4ea20fdab", "Server Code");
			zTextBoxColumnStyleInfo4.ColumnName = "ServerCode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|c3ebb9b9-b3d3-42c0-9388-bc0ba01e34bc", "User ID Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsActive";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|2081b7d2-20c7-4ef8-bb99-2529782e9c25", "User ID");
			zTextBoxColumnStyleInfo5.ColumnName = "UserID";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|c4f91063-0f26-4af9-9df9-ec7d79c3a66b", "User Full Name");
			zTextBoxColumnStyleInfo6.ColumnName = "FullName";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationStatusWarning|ef1c404f-70c4-4849-87a6-a08bcd6eebef", "User Email");
			zTextBoxColumnStyleInfo7.ColumnName = "Email";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ContactGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ContactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ContactGrid.GridId = "d01b186e-0ebb-4bc1-8b30-69af1e312a95";
			this.ContactGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContactGrid.LayoutKey = "ContactGrid";
			this.ContactGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 116, true);
			this.ContactGrid.Name = "ContactGrid";
			this.ContactGrid.ShouldSetErrorsOnTabPage = false;
			this.ContactGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 202, true);
			this.ContactGrid.TabIndex = 8;
			// 
			// FormTitleLabel
			// 
			this.FormTitleLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|3e65176f-6a6c-49db-a8c2-1ddde05a7748", "Account Verification Required");
			this.FormTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FormTitleLabel.IsFontBold = true;
			this.FormTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.FormTitleLabel.Name = "FormTitleLabel";
			this.FormTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 19, true);
			this.FormTitleLabel.TabIndex = 1;
			this.FormTitleLabel.Text = "Account Verification Required";
			// 
			// FirstDescriptionLabel
			// 
			this.FirstDescriptionLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|477711d9-22a0-410e-b84d-0f106c5decf0", "This Person has a pending account verification status set for the following related system user account. Proceeding will clear hold and enable the person to set a new password.");
			this.FirstDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FirstDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 28, true);
			this.FirstDescriptionLabel.Name = "FirstDescriptionLabel";
			this.FirstDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 17, true);
			this.FirstDescriptionLabel.TabIndex = 2;
			this.FirstDescriptionLabel.Text = "This Person has a pending account verification status set for the following relat" +
	"ed system user account. Proceeding will clear hold and enable the person to set " +
	"a new password.";
			// 
			// SecondDescriptionLabel
			// 
			this.SecondDescriptionLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|bd498798-27ee-425d-9b3e-107bdc473945", "Careful review is required, where this person is linked to Multiple Organization Contacts.");
			this.SecondDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SecondDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 57, true);
			this.SecondDescriptionLabel.Name = "SecondDescriptionLabel";
			this.SecondDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 18, true);
			this.SecondDescriptionLabel.TabIndex = 3;
			this.SecondDescriptionLabel.Text = "Careful review is required, where this person is linked to Multiple Organization " +
	"Contacts.";
			// 
			// ProceedButton
			// 
			this.ProceedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProceedButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|cd15409d-929b-41f8-9f52-be025bbf0364", "Cancel");
			this.ProceedButton.IsCaptionOverridden = true;
			this.ProceedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(694, 324, true);
			this.ProceedButton.Name = "ProceedButton";
			this.ProceedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ProceedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.ProceedButton.TabIndex = 9;
			this.ProceedButton.Text = "Proceed";
			this.ProceedButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ProceedButton.ToolTipCaption = null;
			this.ProceedButton.Click += new System.EventHandler(this.ProceedButton_Click);
			// 
			// CancelProceedButton
			// 
			this.CancelProceedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelProceedButton.IsCaptionOverridden = true;
			this.CancelProceedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(844, 324, true);
			this.CancelProceedButton.Name = "CancelProceedButton";
			this.CancelProceedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelProceedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 23, true);
			this.CancelProceedButton.TabIndex = 10;
			this.CancelProceedButton.Text = "Cancel";
			this.CancelProceedButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelProceedButton.ToolTipCaption = null;
			this.CancelProceedButton.Click += new System.EventHandler(this.CancelProceedButton_Click);
			// 
			// ContactNameLabel
			// 
			this.ContactNameLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|5f3138e2-af56-47b9-b341-0dbb2eacc3d2", "Contact Name");
			this.ContactNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 86, true);
			this.ContactNameLabel.Name = "ContactNameLabel";
			this.ContactNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.ContactNameLabel.TabIndex = 4;
			this.ContactNameLabel.Text = "Contact Name";
			// 
			// ContactNameValue
			// 
			this.BindingSource.SetBindingMember(this.ContactNameValue, "OC_ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).OC_ContactName)));
			this.ContactNameValue.CaptionResourceString = null;
			this.ContactNameValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 86, true);
			this.ContactNameValue.Name = "ContactNameValue";
			this.ContactNameValue.ReadOnly = true;
			this.ContactNameValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.ContactNameValue.TabIndex = 5;
			// 
			// ContactEmailLabel
			// 
			this.ContactEmailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ContactEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 86, true);
			this.ContactEmailLabel.Name = "ContactEmailLabel";
			this.ContactEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.ContactEmailLabel.TabIndex = 6;
			this.ContactEmailLabel.Text = "Contact Email";
			// 
			// ContactEmailValue
			// 
			this.BindingSource.SetBindingMember(this.ContactEmailValue, "OC_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact)(null)).OC_Email)));
			this.ContactEmailValue.CaptionResourceString = null;
			this.ContactEmailValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 86, true);
			this.ContactEmailValue.Name = "ContactEmailValue";
			this.ContactEmailValue.ReadOnly = true;
			this.ContactEmailValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.ContactEmailValue.TabIndex = 7;
			// 
			// EdiAccountVerificationWarning
			// 
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("EdiAccountVerificationWarning|9232db9f-a3bf-473d-91bd-a4958ba5dc0c", "Account Verification Required");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 380, true);
			this.Controls.Add(this.ContactEmailLabel);
			this.Controls.Add(this.ContactEmailValue);
			this.Controls.Add(this.ContactNameLabel);
			this.Controls.Add(this.CancelProceedButton);
			this.Controls.Add(this.SecondDescriptionLabel);
			this.Controls.Add(this.FormTitleLabel);
			this.Controls.Add(this.ContactGrid);
			this.Controls.Add(this.FirstDescriptionLabel);
			this.Controls.Add(this.ProceedButton);
			this.Controls.Add(this.ContactNameValue);
			this.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgContact);
			this.Name = "EdiAccountVerificationWarning";
			this.Text = "Account Verification Required";
			this.Controls.SetChildIndex(this.ContactNameValue, 0);
			this.Controls.SetChildIndex(this.ProceedButton, 0);
			this.Controls.SetChildIndex(this.FirstDescriptionLabel, 0);
			this.Controls.SetChildIndex(this.ContactGrid, 0);
			this.Controls.SetChildIndex(this.FormTitleLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SecondDescriptionLabel, 0);
			this.Controls.SetChildIndex(this.CancelProceedButton, 0);
			this.Controls.SetChildIndex(this.ContactNameLabel, 0);
			this.Controls.SetChildIndex(this.ContactEmailValue, 0);
			this.Controls.SetChildIndex(this.ContactEmailLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContactGrid)).EndInit();
			this.ContactGrid.ResumeLayout(false);
			this.ContactGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZDisplayGrid ContactGrid;
		protected Enterprise.ZArchitecture.ZLabel FormTitleLabel;
		protected Enterprise.ZArchitecture.ZLabel FirstDescriptionLabel;
		protected Enterprise.ZArchitecture.ZLabel SecondDescriptionLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelProceedButton;
		protected Enterprise.ZArchitecture.ZLabel ContactNameLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton ProceedButton;
		protected Enterprise.ZArchitecture.ZLabel ContactEmailLabel;
		protected Enterprise.ZArchitecture.ZTextBox ContactEmailValue;
		protected Enterprise.ZArchitecture.ZTextBox ContactNameValue;
	}
}
