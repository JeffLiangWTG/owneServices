using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsControl
	{


		#region Component Designer generated code

		public AccountingOnFormFilterControl MatchEPaymentRecipientsFilterControl;
		private MatchEPaymentRecipientsFilterBusinessObject MatchEPaymentRecipientsFilterBuisnessObject;
		public ZTemplateTabControl TabControl;
		public ZTabPage SyncRecipientTabPage;
		public ZTabPage FilteredRecipientsTabPage;
		protected ZPanel AllRecipientsFilterPanel;
		protected ZPanel FilteredRecipientsGridPanel;
		protected ZPanel FilteredRecipientsNotificationPanel;
		internal ZLabel FilteredRecipientsInfoLabel;
		protected ZPanel FilteredRecipientsFilterPanel;
		public ZGrid FilteredRecipientsGrid;

		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			this.TabControl = new ZTemplateTabControl();
			this.FilteredRecipientsTabPage = new ZTabPage();
			this.FilteredRecipientsGridPanel = new ZPanel();
			this.FilteredRecipientsGrid = new ZGrid();
			this.FilteredRecipientsNotificationPanel = new ZPanel();
			this.FilteredRecipientsInfoLabel = new ZLabel();
			this.FilteredRecipientsFilterPanel = new ZPanel();
			this.SyncRecipientTabPage = new ZTabPage();
			this.AllRecipientsFilterPanel = new ZPanel();
			this.RefreshButton = new ZButton();
			this.ErrorDescriptionTextBox = new ZTextBox();
			this.LastResponseTextBox = new ZTextBox();
			this.StatusTextBox = new ZTextBox();
			this.RequestedByTextBox = new ZTextBox();
			this.LastRequestedTextBox = new ZTextBox();
			this.ProviderCodeTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.FilteredRecipientsTabPage.SuspendLayout();
			this.FilteredRecipientsGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FilteredRecipientsGrid)).BeginInit();
			this.FilteredRecipientsGrid.SuspendLayout();
			this.FilteredRecipientsNotificationPanel.SuspendLayout();
			this.SyncRecipientTabPage.SuspendLayout();
			this.AllRecipientsFilterPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MatchEPaymentRecipients);
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.FilteredRecipientsTabPage);
			this.TabControl.Controls.Add(this.SyncRecipientTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 272, true);
			this.TabControl.TabIndex = 0;
			// 
			// FilteredRecipientsTabPage
			// 
			this.FilteredRecipientsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.FilteredRecipientsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsControl|4FF87116-6CE8-42EF-87D1-8275DF917F43", "Match Recipients");
			this.FilteredRecipientsTabPage.Controls.Add(this.FilteredRecipientsGridPanel);
			this.FilteredRecipientsTabPage.Controls.Add(this.FilteredRecipientsFilterPanel);
			this.FilteredRecipientsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FilteredRecipientsTabPage.Name = "FilteredRecipientsTabPage";
			this.FilteredRecipientsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FilteredRecipientsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 245, true);
			this.FilteredRecipientsTabPage.TabIndex = 0;
			// 
			// FilteredRecipientsGridPanel
			// 
			this.FilteredRecipientsGridPanel.Controls.Add(this.FilteredRecipientsGrid);
			this.FilteredRecipientsGridPanel.Controls.Add(this.FilteredRecipientsNotificationPanel);
			this.FilteredRecipientsGridPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.FilteredRecipientsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 66, true);
			this.FilteredRecipientsGridPanel.Name = "FilteredRecipientsGridPanel";
			this.FilteredRecipientsGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 176, true);
			this.FilteredRecipientsGridPanel.TabIndex = 11;
			// 
			// FilteredRecipientsGrid
			// 
			this.FilteredRecipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FilteredRecipientsGrid, "FilteredRecipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BeneficiaryFullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BeneficiaryNickName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_RX_NKAccountCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_SystemLastEditTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BankBsb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BankBranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccEPaymentBeneficiary)(((System.Collections.IList)(((MatchEPaymentRecipients)(null)).FilteredRecipients)).SyncRoot)).ABF_BankAccount)));
			this.FilteredRecipientsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("50401aeb-2244-4725-ad10-c55ec189ab7e", "Recipient Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ABF_BeneficiaryFullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("12a33000-5a25-4318-bcdd-2810f7dad80e", "Nickname");
			zTextBoxColumnStyleInfo2.ColumnName = "ABF_BeneficiaryNickName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e3a0e6be-603c-48a8-a043-6ad9a14700a4", "Currency");
			zTextBoxColumnStyleInfo3.ColumnName = "ABF_RX_NKAccountCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("de6ecc9b-0509-4a5c-ab54-bf6e8da058dd", "Country/Region");
			zTextBoxColumnStyleInfo4.ColumnName = "ABF_RN_NKCountryCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2e42f1b2-93e3-4d64-a4d0-f1075fca06fb", "Last Updated");
			zDateEditColumnStyleInfo1.ColumnName = "ABF_SystemLastEditTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e9421ba7-8b73-435c-a71c-6124a6f82e1d", "Bank");
			zTextBoxColumnStyleInfo5.ColumnName = "ABF_BankName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("73f288e6-0779-4b18-b46c-74ab3a77b0eb", "BSB");
			zTextBoxColumnStyleInfo6.ColumnName = "ABF_BankBsb";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ea6083a4-6a54-41f7-95f8-ec4121f136d9", "Branch");
			zTextBoxColumnStyleInfo7.ColumnName = "ABF_BankBranchName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bb802ef1-a4b8-4ce7-802c-53a8f0bdde8e", "Account No.");
			zTextBoxColumnStyleInfo8.ColumnName = "ABF_BankAccount";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredRecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredRecipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilteredRecipientsGrid.GridId = "0537496D-4C77-4A16-8BD0-6862E9ADF068";
			this.FilteredRecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FilteredRecipientsGrid.LayoutKey = "EPaymentBeneficiaryGrid";
			this.FilteredRecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.FilteredRecipientsGrid.Name = "FilteredRecipientsGrid";
			this.FilteredRecipientsGrid.ReadOnly = true;
			this.FilteredRecipientsGrid.AfterBind += new EventHandler(this.FilteredRecipientsGrid_AfterBind);
			this.FilteredRecipientsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.FilteredRecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 144, true);
			this.FilteredRecipientsGrid.TabIndex = 1;
			// 
			// FilteredRecipientsNotificationPanel
			// 
			this.FilteredRecipientsNotificationPanel.BackColor = System.Drawing.SystemColors.Control;
			this.FilteredRecipientsNotificationPanel.Controls.Add(this.FilteredRecipientsInfoLabel);
			this.FilteredRecipientsNotificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.FilteredRecipientsNotificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilteredRecipientsNotificationPanel.Name = "FilteredRecipientsNotificationPanel";
			this.FilteredRecipientsNotificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 32, true);
			this.FilteredRecipientsNotificationPanel.TabIndex = 0;
			// 
			// FilteredRecipientsInfoLabel
			// 
			this.FilteredRecipientsInfoLabel.AutoSize = true;
			this.FilteredRecipientsInfoLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsControl|B38AAA0F-DADE-49CE-9405-E15B31E52F7F", "Please enter the filter criteria.");
			this.FilteredRecipientsInfoLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FilteredRecipientsInfoLabel.ForeColor = System.Drawing.SystemColors.InfoText;
			this.FilteredRecipientsInfoLabel.IsFontBold = true;
			this.FilteredRecipientsInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.FilteredRecipientsInfoLabel.Name = "FilteredRecipientsInfoLabel";
			this.FilteredRecipientsInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 13, true);
			this.FilteredRecipientsInfoLabel.TabIndex = 0;
			// 
			// FilteredRecipientsFilterPanel
			// 
			this.FilteredRecipientsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilteredRecipientsFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FilteredRecipientsFilterPanel.Name = "FilteredRecipientsFilterPanel";
			this.FilteredRecipientsFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 239, true);
			this.FilteredRecipientsFilterPanel.TabIndex = 0;
			// 
			// SyncRecipientTabPage
			// 
			this.SyncRecipientTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.SyncRecipientTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsControl|78B9336C-9F3C-4609-BCDB-B851CC1659ED", "Sync Recipients");
			this.SyncRecipientTabPage.Controls.Add(this.AllRecipientsFilterPanel);
			this.SyncRecipientTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SyncRecipientTabPage.Name = "SyncRecipientTabPage";
			this.SyncRecipientTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SyncRecipientTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 245, true);
			this.SyncRecipientTabPage.TabIndex = 1;
			// 
			// AllRecipientsFilterPanel
			// 
			this.AllRecipientsFilterPanel.Controls.Add(this.RefreshButton);
			this.AllRecipientsFilterPanel.Controls.Add(this.ErrorDescriptionTextBox);
			this.AllRecipientsFilterPanel.Controls.Add(this.LastResponseTextBox);
			this.AllRecipientsFilterPanel.Controls.Add(this.StatusTextBox);
			this.AllRecipientsFilterPanel.Controls.Add(this.RequestedByTextBox);
			this.AllRecipientsFilterPanel.Controls.Add(this.LastRequestedTextBox);
			this.AllRecipientsFilterPanel.Controls.Add(this.ProviderCodeTextBox);
			this.AllRecipientsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllRecipientsFilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AllRecipientsFilterPanel.Name = "AllRecipientsFilterPanel";
			this.AllRecipientsFilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 239, true);
			this.AllRecipientsFilterPanel.TabIndex = 0;
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bdb0ca36-d047-4bd5-b502-716028f65fa5", "Refresh");
			this.RefreshButton.IsCaptionOverridden = false;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 187, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 23, true);
			this.RefreshButton.TabIndex = 9;
			this.RefreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.Click += new EventHandler(this.RefreshButton_Click);
			// 
			// ErrorDescriptionTextBox
			// 
			this.ErrorDescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ErrorDescriptionTextBox, "ErrorDescriptionForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).ErrorDescriptionForDisplay)));
			this.ErrorDescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ee8159a2-c7e2-4b4b-a9f7-dbb020a44c0d", "Error");
			this.ErrorDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ErrorDescriptionTextBox, false);
			this.ErrorDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 148, true);
			this.ErrorDescriptionTextBox.Name = "ErrorDescriptionTextBox";
			this.ErrorDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 40, true);
			this.ErrorDescriptionTextBox.TabIndex = 8;
			this.ErrorDescriptionTextBox.Multiline = true;
			// 
			// LastResponseTextBox
			// 
			this.LastResponseTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LastResponseTextBox, "LastResponseDateForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).LastResponseDateForDisplay)));
			this.LastResponseTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fc7999c3-5c5a-4869-8ac2-556b7c1f7093", "Last Response");
			this.LastResponseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LastResponseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 60, true);
			this.LastResponseTextBox.Name = "LastResponseTextBox";
			this.LastResponseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.LastResponseTextBox.TabIndex = 7;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusTextBox, "StatusForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).StatusForDisplay)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6f4cd123-05b6-4b44-8254-e7b681c59fb7", "Status");
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 148, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.StatusTextBox.TabIndex = 6;
			// 
			// RequestedByTextBox
			// 
			this.RequestedByTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestedByTextBox, "RequestedByForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).RequestedByForDisplay)));
			this.RequestedByTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3b6473b6-9a0b-4564-9469-aa310d61e92e", "Requested By");
			this.RequestedByTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RequestedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 105, true);
			this.RequestedByTextBox.Name = "RequestedByTextBox";
			this.RequestedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.RequestedByTextBox.TabIndex = 5;
			// 
			// LastRequestedTextBox
			// 
			this.LastRequestedTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LastRequestedTextBox, "LastRequestedDateForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).LastRequestedDateForDisplay)));
			this.LastRequestedTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e5d1c494-c7f0-4030-9655-849f75466468", "Last Requested");
			this.LastRequestedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LastRequestedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 60, true);
			this.LastRequestedTextBox.Name = "LastRequestedTextBox";
			this.LastRequestedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.LastRequestedTextBox.TabIndex = 4;
			// 
			// ProviderCodeTextBox
			// 
			this.ProviderCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProviderCodeTextBox, "ProviderCodeForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).ProviderCodeForDisplay)));
			this.ProviderCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("591b93a9-39ab-4ec2-bb69-fc1d25bc3180", "Provider");
			this.ProviderCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 21, true);
			this.ProviderCodeTextBox.Name = "ProviderCodeTextBox";
			this.ProviderCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ProviderCodeTextBox.TabIndex = 3;
			// 
			// MatchEPaymentRecipientsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TabControl);
			this.Name = "MatchEPaymentRecipientsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 272, true);
			this.BackColorChanged += new EventHandler(this.MatchEPaymentRecipientsControl_BackColorChanged);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.FilteredRecipientsTabPage.ResumeLayout(false);
			this.FilteredRecipientsTabPage.PerformLayout();
			this.FilteredRecipientsGridPanel.ResumeLayout(false);
			this.FilteredRecipientsGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FilteredRecipientsGrid)).EndInit();
			this.FilteredRecipientsGrid.ResumeLayout(false);
			this.FilteredRecipientsGrid.PerformLayout();
			this.FilteredRecipientsNotificationPanel.ResumeLayout(false);
			this.FilteredRecipientsNotificationPanel.PerformLayout();
			this.SyncRecipientTabPage.ResumeLayout(false);
			this.SyncRecipientTabPage.PerformLayout();
			this.AllRecipientsFilterPanel.ResumeLayout(false);
			this.AllRecipientsFilterPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}