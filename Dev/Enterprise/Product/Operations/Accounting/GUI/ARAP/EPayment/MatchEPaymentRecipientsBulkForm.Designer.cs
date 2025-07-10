using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsBulkForm
	{


		#region Windows Form Designer generated code

		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected ZPanel BottomButtonPanel;
		protected ZPanel TopPanel;
		protected ZArchitecture.ZTextBox LastUpdatedDateTextBox;
		protected MatchEPaymentRecipientsControl matchRecipientsControl;
		private ZArchitecture.ZLabel labelHeaderText;
		protected Core.Forms.ZPostOrCancelButton SyncButton;
		protected ZGuidFindBox CreditorGuidFindBox;
		private ZArchitecture.ZLabel LabelBulkMatch;
		protected ZGuidFindBox BankAccountGuidFindBox;
		protected ZDropEdit DropEditAgreedPaymentMethod;
		protected ZCheckBox CheckBoxOverrideDefault;
		protected Core.Forms.ZPostOrCancelButton MatchButton;
		protected ZDropEdit DefaultPaymentReasonDropEdit;
		protected ZDropEdit DropEditPaymentReferenceType;
		protected ZArchitecture.ZTextBox TextPaymentReference;
		protected Core.Forms.ZPostOrCancelButton UnmatchButton;

		new void InitializeComponent()
		{
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.BottomButtonPanel = new ZPanel();
			this.DefaultPaymentReasonDropEdit = new ZDropEdit();
			this.MatchButton = new Core.Forms.ZPostOrCancelButton();
			this.CheckBoxOverrideDefault = new ZCheckBox();
			this.DropEditAgreedPaymentMethod = new ZDropEdit();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.LabelBulkMatch = new ZArchitecture.ZLabel();
			this.CreditorGuidFindBox = new ZGuidFindBox();
			this.UnmatchButton = new Core.Forms.ZPostOrCancelButton();
			this.TopPanel = new ZPanel();
			this.labelHeaderText = new ZArchitecture.ZLabel();
			this.LastUpdatedDateTextBox = new ZArchitecture.ZTextBox();
			this.SyncButton = new Core.Forms.ZPostOrCancelButton();
			this.matchRecipientsControl = new MatchEPaymentRecipientsControl();
			this.DropEditPaymentReferenceType = new ZDropEdit();
			this.TextPaymentReference = new ZArchitecture.ZTextBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomButtonPanel.SuspendLayout();
			this.DefaultPaymentReasonDropEdit.SuspendLayout();
			this.DropEditAgreedPaymentMethod.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.CreditorGuidFindBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.matchRecipientsControl.SuspendLayout();
			this.DropEditPaymentReferenceType.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 610, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 20, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(436);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(437);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MatchEPaymentRecipientsBulk);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsBulkForm|2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 163, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.TextPaymentReference);
			this.BottomButtonPanel.Controls.Add(this.DefaultPaymentReasonDropEdit);
			this.BottomButtonPanel.Controls.Add(this.MatchButton);
			this.BottomButtonPanel.Controls.Add(this.CheckBoxOverrideDefault);
			this.BottomButtonPanel.Controls.Add(this.DropEditPaymentReferenceType);
			this.BottomButtonPanel.Controls.Add(this.DropEditAgreedPaymentMethod);
			this.BottomButtonPanel.Controls.Add(this.BankAccountGuidFindBox);
			this.BottomButtonPanel.Controls.Add(this.LabelBulkMatch);
			this.BottomButtonPanel.Controls.Add(this.CreditorGuidFindBox);
			this.BottomButtonPanel.Controls.Add(this.UnmatchButton);
			this.BottomButtonPanel.Controls.Add(this.CloseButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 415, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 195, true);
			this.BottomButtonPanel.TabIndex = 2;
			// 
			// MatchButton
			// 
			this.MatchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MatchButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4712cca4-d4d2-411e-a87b-58dbff3d6621", "Match");
			this.MatchButton.IsCaptionOverridden = false;
			this.MatchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(675, 163, true);
			this.MatchButton.Name = "MatchButton";
			this.MatchButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MatchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.MatchButton.TabIndex = 25;
			this.MatchButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.MatchButton.ToolTipCaption = null;
			this.MatchButton.Click += new EventHandler(this.MatchButton_Click);
			// 
			// CheckBoxOverrideDefault
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxOverrideDefault, "AllowOverrideDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((MatchEPaymentRecipientsBulk)(null)).AllowOverrideDefault)));
			this.CheckBoxOverrideDefault.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b13c4330-d690-4e10-9fb5-b1cab30127e7", "Override Default Bank and Payment Method");
			this.CheckBoxOverrideDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CheckBoxOverrideDefault.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 65, true);
			this.CheckBoxOverrideDefault.Name = "CheckBoxOverrideDefault";
			this.CheckBoxOverrideDefault.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 22, true);
			this.CheckBoxOverrideDefault.TabIndex = 24;
			// 
			// DropEditAgreedPaymentMethod
			// 
			this.DropEditAgreedPaymentMethod.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditAgreedPaymentMethod, "AgreedPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MatchEPaymentRecipientsBulk)(null)).AgreedPaymentMethod)));
			this.DropEditAgreedPaymentMethod.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("94d03eaa-7304-4a56-bec3-c69f449e656b", "Agreed Payment Method");
			this.DropEditAgreedPaymentMethod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 97, true);
			this.DropEditAgreedPaymentMethod.Name = "DropEditAgreedPaymentMethod";
			this.DropEditAgreedPaymentMethod.ShouldResizeByMaxLength = true;
			this.DropEditAgreedPaymentMethod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.DropEditAgreedPaymentMethod.TabIndex = 20;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "DefaultBankAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((MatchEPaymentRecipientsBulk)(null)).DefaultBankAccountPK)));
			this.BankAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b9ce11e1-7c7c-4eff-9036-02226c62abcb", "Default Bank Account");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 71, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BankAccountGuidFindBox.ParentType = null;
			this.BankAccountGuidFindBox.PopupCaption = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 7;
			// 
			// LabelBulkMatch
			// 
			this.LabelBulkMatch.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("838785e7-90ce-4939-b4d5-0655371ffac0", "Match the selected recipient with the following Payables Organization:");
			this.LabelBulkMatch.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.LabelBulkMatch.IsFontBold = true;
			this.LabelBulkMatch.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.LabelBulkMatch.Name = "LabelBulkMatch";
			this.LabelBulkMatch.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 39, true);
			this.LabelBulkMatch.TabIndex = 6;
			// 
			// CreditorGuidFindBox
			// 
			this.CreditorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorGuidFindBox, "CreditorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((MatchEPaymentRecipientsBulk)(null)).CreditorPK)));
			this.CreditorGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c1a97c2b-013c-4420-90b7-ebc8496260be", "Creditor");
			this.CreditorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 19, true);
			this.CreditorGuidFindBox.Name = "CreditorGuidFindBox";
			this.CreditorGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreditorGuidFindBox.ParentType = null;
			this.CreditorGuidFindBox.PopupCaption = null;
			this.CreditorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.CreditorGuidFindBox.TabIndex = 3;
			// 
			// UnmatchButton
			// 
			this.UnmatchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UnmatchButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("56157265-2f7a-4508-abf6-d6c8add4801b", "Unmatch");
			this.UnmatchButton.IsCaptionOverridden = false;
			this.UnmatchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(801, 163, true);
			this.UnmatchButton.Name = "UnmatchButton";
			this.UnmatchButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnmatchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.UnmatchButton.TabIndex = 0;
			this.UnmatchButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UnmatchButton.ToolTipCaption = null;
			this.UnmatchButton.Click += new EventHandler(this.UnmatchButton_Click);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.labelHeaderText);
			this.TopPanel.Controls.Add(this.LastUpdatedDateTextBox);
			this.TopPanel.Controls.Add(this.SyncButton);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 97, true);
			this.TopPanel.TabIndex = 0;
			// 
			// labelHeaderText
			// 
			this.labelHeaderText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21E5D2F8-C3F4-4EB2-B344-C681C50D09F0", "To Configure AP Organization for E-Payment, find the matching recipient from the External Provider\'s system, then select Confirm. If you are unable to locate the required recipient, click Sync Recipients.");
			this.labelHeaderText.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.labelHeaderText.IsFontBold = true;
			this.labelHeaderText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.labelHeaderText.Name = "labelHeaderText";
			this.labelHeaderText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 39, true);
			this.labelHeaderText.TabIndex = 5;
			// 
			// LastUpdatedDateTextBox
			// 
			this.LastUpdatedDateTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LastUpdatedDateTextBox, "RecipientListLastUpdatedDateForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((MatchEPaymentRecipientsBulk)(null)).RecipientListLastUpdatedDateForDisplay)));
			this.LastUpdatedDateTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsBulkForm|2998d588-96d0-422e-98b4-a5b54e6fbe7f", "Recipient List Last Updated");
			this.LastUpdatedDateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LastUpdatedDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 55, true);
			this.LastUpdatedDateTextBox.Name = "LastUpdatedDateTextBox";
			this.LastUpdatedDateTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LastUpdatedDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.LastUpdatedDateTextBox.TabIndex = 2;
			// 
			// SyncButton
			// 
			this.SyncButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SyncButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a089b019-1194-4acc-a077-5588e6bc0826", "Sync Recipients");
			this.SyncButton.IsCaptionOverridden = false;
			this.SyncButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 55, true);
			this.SyncButton.Name = "SyncButton";
			this.SyncButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SyncButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.SyncButton.TabIndex = 6;
			this.SyncButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SyncButton.ToolTipCaption = null;
			this.SyncButton.Click += new EventHandler(this.SyncButton_Click);
			// 
			// matchRecipientsControl
			// 
			this.matchRecipientsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.matchRecipientsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MatchEPaymentRecipients)(((MatchEPaymentRecipientsBulk)(null)))));
			this.matchRecipientsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.matchRecipientsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 97, true);
			this.matchRecipientsControl.Name = "matchRecipientsControl";
			this.matchRecipientsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 318, true);
			this.matchRecipientsControl.TabIndex = 1;
			// 
			// DefaultPaymentReasonDropEdit
			// 
			this.DefaultPaymentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultPaymentReasonDropEdit, "DefaultPaymentReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MatchEPaymentRecipientsBulk)(null)).DefaultPaymentReason)));
			this.DefaultPaymentReasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a4d8ece8-a76b-411b-98ed-1da7a02066e2", "Default Payment Reason");
			this.DefaultPaymentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 45, true);
			this.DefaultPaymentReasonDropEdit.Name = "DefaultPaymentReasonDropEdit";
			this.DefaultPaymentReasonDropEdit.ShouldResizeByMaxLength = true;
			this.DefaultPaymentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 20, true);
			this.DefaultPaymentReasonDropEdit.TabIndex = 21;
			// 
			// DropEditPaymentReferenceType
			// 
			this.DropEditPaymentReferenceType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropEditPaymentReferenceType, "PaymentReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MatchEPaymentRecipientsBulk)(null)).PaymentReferenceType)));
			this.DropEditPaymentReferenceType.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("071794bc-6f01-4958-8ed9-a032caa4a476", "Payment Reference Type");
			this.DropEditPaymentReferenceType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 123, true);
			this.DropEditPaymentReferenceType.Name = "DropEditPaymentReferenceType";
			this.DropEditPaymentReferenceType.ShowDescriptionBox = false;
			this.DropEditPaymentReferenceType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DropEditPaymentReferenceType.TabIndex = 22;
			// 
			// TextPaymentReference
			// 
			this.TextPaymentReference.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TextPaymentReference, "PaymentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((MatchEPaymentRecipientsBulk)(null)).PaymentReference)));
			this.TextPaymentReference.CaptionResourceString = null;
			this.TextPaymentReference.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextPaymentReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(673, 123, true);
			this.TextPaymentReference.Name = "TextPaymentReference";
			this.TextPaymentReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.TextPaymentReference.TabIndex = 23;
			this.TextPaymentReference.Visible = false;
			// 
			// MatchEPaymentRecipientsBulkForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsBulkForm|cebdb3f7-6e7d-4802-ac05-68849daac0ee", "Bulk Match Recipients");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 630, true);
			this.Controls.Add(this.matchRecipientsControl);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomButtonPanel);
			this.DataSourceType = typeof(MatchEPaymentRecipientsBulk);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 690, true);
			this.Name = "MatchEPaymentRecipientsBulkForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.matchRecipientsControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.DefaultPaymentReasonDropEdit.ResumeLayout(true);
			this.DefaultPaymentReasonDropEdit.PerformLayout();
			this.DropEditAgreedPaymentMethod.ResumeLayout(true);
			this.DropEditAgreedPaymentMethod.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.CreditorGuidFindBox.ResumeLayout(true);
			this.CreditorGuidFindBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.matchRecipientsControl.ResumeLayout(true);
			this.matchRecipientsControl.PerformLayout();
			this.DropEditPaymentReferenceType.ResumeLayout(true);
			this.DropEditPaymentReferenceType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}