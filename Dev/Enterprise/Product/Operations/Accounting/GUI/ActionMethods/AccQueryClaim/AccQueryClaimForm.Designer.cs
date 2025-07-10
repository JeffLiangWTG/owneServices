using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimForm
	{


		#region Windows Form Designer generated code

		protected Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		protected ZTemplateTabControl TabControl;
		protected ZStmNoteTabPage StmNoteTabPage;
		protected ZPanel SaveButtonsPanel;
		protected ZPanel MainPanel;
		protected ZLogsTabPage EventTabPage;
		protected ZTabPage DetailsTabPage;
		protected AccQueryClaimUserControl accQueryClaimUserControl;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.ButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.TabControl = new ZTemplateTabControl();
			this.DetailsTabPage = new ZTabPage();
			this.accQueryClaimUserControl = new AccQueryClaimUserControl();
			this.StmNoteTabPage = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.SaveButtonsPanel = new ZPanel();
			this.MainPanel = new ZPanel();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.SaveButtonsPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 641, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(410);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MasterFiles.Business.AccQueryClaim);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 7, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ButtonsUserControl.TabIndex = 2;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.StmNoteTabPage);
			this.TabControl.Controls.Add(this.EventTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 604, true);
			this.TabControl.TabIndex = 29;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimForm|f874c33e-d6ab-4249-a517-69bcde20bb75", "Details");
			this.DetailsTabPage.Controls.Add(this.accQueryClaimUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 605, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// accQueryClaimUserControl
			// 
			this.accQueryClaimUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accQueryClaimUserControl, ".");
			this.accQueryClaimUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accQueryClaimUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.accQueryClaimUserControl.Name = "accQueryClaimUserControl";
			this.accQueryClaimUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 605, true);
			this.accQueryClaimUserControl.TabIndex = 0;
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.AutoScroll = true;
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 626, true);
			this.StmNoteTabPage.TabIndex = 1;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 577, true);
			this.EventTabPage.TabIndex = 2;
			// 
			// SaveButtonsPanel
			// 
			this.SaveButtonsPanel.Controls.Add(this.ButtonsUserControl);
			this.SaveButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SaveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 604, true);
			this.SaveButtonsPanel.Name = "SaveButtonsPanel";
			this.SaveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 37, true);
			this.SaveButtonsPanel.TabIndex = 30;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.TabControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 604, true);
			this.MainPanel.TabIndex = 31;
			// 
			// AccQueryClaimForm
			// 

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimForm|1e50d529-ef57-4989-ad10-3bdd083539ae", "Claims And Queries");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1240, 665, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.SaveButtonsPanel);
			this.DataSourceType = typeof(MasterFiles.Business.AccQueryClaim);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1260, 725, true);
			this.Name = "AccQueryClaimForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.SaveButtonsPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}