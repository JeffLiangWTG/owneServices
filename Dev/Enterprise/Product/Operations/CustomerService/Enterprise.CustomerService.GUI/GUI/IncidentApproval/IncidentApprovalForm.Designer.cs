using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture;
using System.Windows.Forms;
using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI
{
	public partial class IncidentApprovalForm : ZTemplateForm
	{
		ZButton ApproveButton;
		ZButton SaveAwaitingApprovalButton;
		ZButton CloseButton;
		ZLabel QuoteAcceptedDateLabel;
		internal IncidentApprovalControl IncidentApprovalControl;
		ZTabPage RedirectToPortalTabPage;
		ZButton portalButton;

		readonly System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			this.ApproveButton = new ZButton();
			this.IncidentApprovalControl = new IncidentApprovalControl();
			this.QuoteAcceptedDateLabel = new ZLabel();
			this.CloseButton = new ZButton();
			this.SaveAwaitingApprovalButton = new ZButton();
			this.RedirectToPortalTabPage = new ZTabPage();
			this.portalButton = new ZButton();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncidentApprovalControl.SuspendLayout();
			this.RedirectToPortalTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.RedirectToPortalTabPage);
			this.MainTabControl.Size = ControlDpiScalingHelper.NewScaledSize(955, 631, true);
			this.MainTabControl.SelectedIndexChanged += new EventHandler(this.MainTabControl_SelectedIndexChanged);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.IncidentApprovalControl);
			this.MainTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = ControlDpiScalingHelper.NewScaledSize(950, 609, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = ControlDpiScalingHelper.NewScaledSize(950, 609, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = ControlDpiScalingHelper.NewScaledSize(950, 609, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = ControlDpiScalingHelper.NewScaledSize(955, 631, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.SaveAwaitingApprovalButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.CloseButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.QuoteAcceptedDateLabel);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.ApproveButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Dock = DockStyle.Fill;
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.MinimumSize = ControlDpiScalingHelper.NewScaledSize(939, 32, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = ControlDpiScalingHelper.NewScaledSize(955, 32, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(955, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(932);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IncidentApproval);
			// 
			// ApproveButton
			// 
			this.ApproveButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.ApproveButton.CaptionResourceString = Res.GetData("IncidentApprovalForm|15ccb6c9-e2e8-47cc-87a9-56dda4b160cc", "Send eRequest");
			this.ApproveButton.Location = ControlDpiScalingHelper.NewScaledPoint(569, 5, true);
			this.ApproveButton.Name = "ApproveButton";
			this.ApproveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApproveButton.Size = ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.ApproveButton.TabIndex = 8;
			this.ApproveButton.ToolTipCaption = null;
			this.ApproveButton.UseVisualStyleBackColor = false;
			this.ApproveButton.Click += new EventHandler(this.ApproveButton_Click);
			// 
			// IncidentApprovalControl
			// 
			this.IncidentApprovalControl.AllowDrop = true;
			this.IncidentApprovalControl.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.IncidentApprovalControl, ".");
			this.IncidentApprovalControl.Dock = DockStyle.Fill;
			this.IncidentApprovalControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncidentApprovalControl.Name = "IncidentApprovalControl";
			this.IncidentApprovalControl.Size = ControlDpiScalingHelper.NewScaledSize(950, 609, true);
			this.IncidentApprovalControl.TabIndex = 0;
			// 
			// QuoteAcceptedDateLabel
			// 
			this.QuoteAcceptedDateLabel.Anchor = ((AnchorStyles.Bottom | AnchorStyles.Left)
			| AnchorStyles.Right);

			/* Unmerged change from project 'Enterprise.CustomerService.GUI.Winzor'
			Before:
						this.QuoteAcceptedDateLabel.FontType = ((OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			After:
						this.QuoteAcceptedDateLabel.FontType = ((OFontTypes)((OFontTypes.Normal | OFontTypes.Bold)));
			*/
			this.QuoteAcceptedDateLabel.FontType = (OFontTypes.Normal | OFontTypes.Bold);
			this.QuoteAcceptedDateLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuoteAcceptedDateLabel, false);
			this.QuoteAcceptedDateLabel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.QuoteAcceptedDateLabel.Name = "QuoteAcceptedDateLabel";
			this.QuoteAcceptedDateLabel.Size = ControlDpiScalingHelper.NewScaledSize(301, 22, true);
			this.QuoteAcceptedDateLabel.TabIndex = 11;
			this.QuoteAcceptedDateLabel.Text = "<Quote Accepted Date>";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.CloseButton.CaptionResourceString = Res.GetData("dfa9a98c-7ed0-4393-84ab-90c716670fb3", "Close");
			this.CloseButton.Location = ControlDpiScalingHelper.NewScaledPoint(851, 5, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CloseButton.TabIndex = 10;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = false;
			// 
			// SaveAwaitingApprovalButton
			// 
			this.SaveAwaitingApprovalButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
			this.SaveAwaitingApprovalButton.CaptionResourceString = Res.GetData("a420a82e-9a3d-4f33-b27d-c0dc1f59446a", "Save Awaiting Approval");
			this.SaveAwaitingApprovalButton.Location = ControlDpiScalingHelper.NewScaledPoint(711, 5, true);
			this.SaveAwaitingApprovalButton.Name = "SaveAwaitingApprovalButton";
			this.SaveAwaitingApprovalButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAwaitingApprovalButton.Size = ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.SaveAwaitingApprovalButton.TabIndex = 9;
			this.SaveAwaitingApprovalButton.ToolTipCaption = null;
			this.SaveAwaitingApprovalButton.UseVisualStyleBackColor = false;
			this.SaveAwaitingApprovalButton.Click += new EventHandler(this.SaveAwaitingApprovalButton_Click);
			// 
			// RedirectToPortalTabPage
			// 
			this.RedirectToPortalTabPage.CaptionResourceString = Res.GetData("37780cf3-7902-4297-b6aa-2ca9aae8c495", "Portal");
			this.RedirectToPortalTabPage.Controls.Add(this.portalButton);
			this.RedirectToPortalTabPage.Location = ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RedirectToPortalTabPage.Name = "RedirectToPortalTabPage";
			this.RedirectToPortalTabPage.Size = ControlDpiScalingHelper.NewScaledSize(950, 609, true);
			this.RedirectToPortalTabPage.TabIndex = 3;
			// 
			// portalButton
			// 
			this.portalButton.CaptionResourceString = Res.GetData("fca6f7be-be68-4ec1-b0ae-04fb064f34a7", "View Online");
			this.portalButton.Location = ControlDpiScalingHelper.NewScaledPoint(15, 17, true);
			this.portalButton.Name = "portalButton";
			this.portalButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.portalButton.Size = ControlDpiScalingHelper.NewScaledSize(126, 23, true);
			this.portalButton.TabIndex = 11;
			this.portalButton.ToolTipCaption = null;
			this.portalButton.UseVisualStyleBackColor = false;
			this.portalButton.Click += new EventHandler(this.portalButton_Click);
			// 
			// IncidentApprovalForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(955, 687, true);
			this.DataSourceType = typeof(IncidentApproval);
			this.MinimumSize = ControlDpiScalingHelper.NewScaledSize(970, 725, true);
			this.Name = "IncidentApprovalForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.ResumeLayout(false);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncidentApprovalControl.ResumeLayout(true);
			this.IncidentApprovalControl.PerformLayout();
			this.RedirectToPortalTabPage.ResumeLayout(false);
			this.RedirectToPortalTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
