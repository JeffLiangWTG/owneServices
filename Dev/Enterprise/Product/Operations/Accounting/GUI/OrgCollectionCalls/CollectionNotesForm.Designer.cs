using System;
using System.ComponentModel;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.OrgCollectionCalls
{
	public partial class CollectionNotesForm
	{


		#region Windows Form Designer generated code

		private ZTemplateTabControl DebtorSelectionTab;
		private ZTabPage CollectionCallsTabPage;
		private ZStmNoteTabPage StmNoteTabPage;
		private ZPostingButtonsUserControl ButtonsUserControl;
		private OrgCollectionNotesControl orgCollectionNotesControl1;
		private ZLogsTabPage EventTabPage;
		private ZPanel SaveButtonsPanel;
		private ZPanel MainPanel;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.SaveButtonsPanel = new ZPanel();
			this.ButtonsUserControl = new ZPostingButtonsUserControl();
			this.MainPanel = new ZPanel();
			this.DebtorSelectionTab = new ZTemplateTabControl();
			this.CollectionCallsTabPage = new ZTabPage();
			this.StmNoteTabPage = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SaveButtonsPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.DebtorSelectionTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 568, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 23, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(901);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OrgHeader);
			// 
			// SaveButtonsPanel
			// 
			this.SaveButtonsPanel.Controls.Add(this.ButtonsUserControl);
			this.SaveButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SaveButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 531, true);
			this.SaveButtonsPanel.Name = "SaveButtonsPanel";
			this.SaveButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 37, true);
			this.SaveButtonsPanel.TabIndex = 0;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 10, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ButtonsUserControl.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.DebtorSelectionTab);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 531, true);
			this.MainPanel.TabIndex = 31;
			// 
			// DebtorSelectionTab
			// 
			this.DebtorSelectionTab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DebtorSelectionTab.Controls.Add(this.CollectionCallsTabPage);
			this.DebtorSelectionTab.Controls.Add(this.StmNoteTabPage);
			this.DebtorSelectionTab.Controls.Add(this.EventTabPage);
			this.DebtorSelectionTab.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DebtorSelectionTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DebtorSelectionTab.Name = "DebtorSelectionTab";
			this.DebtorSelectionTab.SelectedIndex = 0;
			this.DebtorSelectionTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 531, true);
			this.DebtorSelectionTab.TabIndex = 0;
			// 
			// CollectionCallsTabPage
			// 
			this.CollectionCallsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CollectionNotesForm|57ae7b47-59f8-42d0-a9a8-79e6f8ca31a6", "Collection Calls");
			this.CollectionCallsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CollectionCallsTabPage.Name = "CollectionCallsTabPage";
			this.CollectionCallsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 504, true);
			this.CollectionCallsTabPage.TabIndex = 0;
			this.CollectionCallsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.CollectionCallsTabPage_InitializeTab));
			// 
			// StmNoteTabPage
			// 
			this.StmNoteTabPage.AutoScroll = true;
			this.StmNoteTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StmNoteTabPage.Name = "StmNoteTabPage";
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 504, true);
			this.StmNoteTabPage.TabIndex = 1;
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 504, true);
			this.EventTabPage.TabIndex = 2;
			// 
			// CollectionNotesForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 591, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.SaveButtonsPanel);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(OrgHeader);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.OrgHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(879, 580, true);
			this.Name = "CollectionNotesForm";
			this.ShouldSerializeTabPageMethods = true;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.SaveButtonsPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.DebtorSelectionTab.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private void CollectionCallsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.orgCollectionNotesControl1 = new OrgCollectionNotesControl();
			this.CollectionCallsTabPage.SuspendLayout();
			this.CollectionCallsTabPage.Controls.Add(this.orgCollectionNotesControl1);
			// 
			// orgCollectionNotesControl1
			// 
			this.BindingSource.SetBindingMember(this.orgCollectionNotesControl1, ".");
			this.orgCollectionNotesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orgCollectionNotesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.orgCollectionNotesControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 460);
			this.orgCollectionNotesControl1.Name = "orgCollectionNotesControl1";
			this.orgCollectionNotesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 504);
			this.orgCollectionNotesControl1.TabIndex = 0;
			this.CollectionCallsTabPage.ResumeLayout(true);
		}

		#endregion

	}
}