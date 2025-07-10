using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsForm
	{


		#region Windows Form Designer generated code

		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected ZPanel BottomButtonPanel;
		protected ZPanel TopPanel;
		protected ZArchitecture.ZTextBox LastUpdatedDateTextBox;
		protected MatchEPaymentRecipientsControl matchRecipientsControl;
		private ZArchitecture.ZLabel labelHeaderText;
		protected Core.Forms.ZPostOrCancelButton SyncButton;
		protected Core.Forms.ZPostOrCancelButton PostButton;

		new void InitializeComponent()
		{
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.BottomButtonPanel = new ZPanel();
			this.PostButton = new Core.Forms.ZPostOrCancelButton();
			this.TopPanel = new ZPanel();
			this.labelHeaderText = new ZArchitecture.ZLabel();
			this.LastUpdatedDateTextBox = new ZArchitecture.ZTextBox();
			this.SyncButton = new Core.Forms.ZPostOrCancelButton();
			this.matchRecipientsControl = new MatchEPaymentRecipientsControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomButtonPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.matchRecipientsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 601, true);
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
			this.BindingSource.DataSourceType = typeof(MatchEPaymentRecipients);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsForm|2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 12, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.PostButton);
			this.BottomButtonPanel.Controls.Add(this.CloseButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 557, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 44, true);
			this.BottomButtonPanel.TabIndex = 2;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("223ecc9a-b021-4530-90c4-2b0965cbbb58", "Confirm");
			this.PostButton.IsCaptionOverridden = false;
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(801, 12, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.PostButton.TabIndex = 0;
			this.PostButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PostButton.ToolTipCaption = null;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MatchEPaymentRecipients)(null)).RecipientListLastUpdatedDateForDisplay)));
			this.LastUpdatedDateTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsForm|2998d588-96d0-422e-98b4-a5b54e6fbe7f", "Recipient List Last Updated");
			this.LastUpdatedDateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LastUpdatedDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 55, true);
			this.LastUpdatedDateTextBox.Name = "LastUpdatedDateTextBox";
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
			this.matchRecipientsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.matchRecipientsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 97, true);
			this.matchRecipientsControl.Name = "matchRecipientsControl";
			this.matchRecipientsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 460, true);
			this.matchRecipientsControl.TabIndex = 1;
			// 
			// MatchEPaymentRecipientsForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("MatchEPaymentRecipientsForm|5877969d-7e6f-44bc-b7fa-d2fc23649f1b", "Match Recipients");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 621, true);
			this.Controls.Add(this.matchRecipientsControl);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomButtonPanel);
			this.DataSourceType = typeof(MatchEPaymentRecipients);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 660, true);
			this.Name = "MatchEPaymentRecipientsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.matchRecipientsControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.matchRecipientsControl.ResumeLayout(true);
			this.matchRecipientsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}