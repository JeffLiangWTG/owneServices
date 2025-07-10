using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentManagementGroupForm
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

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RelatedItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.incidentManagementGroupDetailsUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentManagementGroupDetailsUserControl();
			this.incidentControlCenterTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.incidentManagementControlCenterUserControl = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentManagementControlCenterUserControl();
			this.WorkflowTabPage = new Enterprise.Client.EDI.IncidentManager.GUI.EDIWorkflowTabPage(this.components);
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.incidentManagementGroupDetailsUserControl.SuspendLayout();
			this.incidentControlCenterTabPage.SuspendLayout();
			this.incidentManagementControlCenterUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabPage.Controls.Add(this.incidentManagementGroupDetailsUserControl);
			this.MainTabControl.Controls.Add(this.incidentControlCenterTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.RelatedItemsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 630, true);
			this.MainTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.MainTabControl_Selecting);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.incidentControlCenterTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RelatedItemsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 603, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(411);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			// 
			// RelatedItemsTabPage
			// 
			this.RelatedItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.RelatedItemsTabPage.Name = "RelatedItemsTabPage";
			this.RelatedItemsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
			this.RelatedItemsTabPage.TabIndex = 3;
			this.RelatedItemsTabPage.CaptionResourceString = ZClientEDI.Res.GetData("C39444AA-639F-4513-8E59-9DF0A8491079", "Related Items");
			this.RelatedItemsTabPage.UseVisualStyleBackColor = true;
			// 
			// incidentManagementGroupDetailsUserControl
			// 
			this.incidentManagementGroupDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.incidentManagementGroupDetailsUserControl, ".");
			this.incidentManagementGroupDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentManagementGroupDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.incidentManagementGroupDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 596, true);
			this.incidentManagementGroupDetailsUserControl.Name = "incidentManagementGroupDetailsUserControl";
			this.incidentManagementGroupDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 603, true);
			this.incidentManagementGroupDetailsUserControl.TabIndex = 0;
			// 
			// incidentControlCenterTabPage
			// 
			this.incidentControlCenterTabPage.CaptionResourceString = ZClientEDI.Res.GetData("83DA35EE-CCFF-4ADE-9C0E-9D602374D966", "Incident Control Center");
			this.incidentControlCenterTabPage.Controls.Add(this.incidentManagementControlCenterUserControl);
			this.incidentControlCenterTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.incidentControlCenterTabPage.Name = "incidentControlCenterTabPage";
			this.incidentControlCenterTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.incidentControlCenterTabPage.ShouldBeReadOnlyInViewMode = false;
			this.incidentControlCenterTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1452, 603, true);
			this.incidentControlCenterTabPage.TabIndex = 4;
			this.incidentControlCenterTabPage.UseVisualStyleBackColor = true;
			// 
			// incidentManagementControlCenterUserControl
			// 
			this.incidentManagementControlCenterUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.incidentManagementControlCenterUserControl, ".");
			this.incidentManagementControlCenterUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentManagementControlCenterUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.incidentManagementControlCenterUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1188, 494, true);
			this.incidentManagementControlCenterUserControl.Name = "incidentManagementControlCenterUserControl";
			this.incidentManagementControlCenterUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1446, 597, true);
			this.incidentManagementControlCenterUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
			this.WorkflowTabPage.TabIndex = 5;
			// 
			// IncidentManagementGroupForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1460, 686, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "IncidentManagementGroupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Incident Management Group";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.incidentManagementGroupDetailsUserControl.ResumeLayout(true);
			this.incidentManagementGroupDetailsUserControl.PerformLayout();
			this.incidentControlCenterTabPage.ResumeLayout(false);
			this.incidentControlCenterTabPage.PerformLayout();
			this.incidentManagementControlCenterUserControl.ResumeLayout(true);
			this.incidentManagementControlCenterUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private IncidentManager.GUI.IncidentManagementGroupDetailsUserControl incidentManagementGroupDetailsUserControl;
		private IncidentManager.GUI.IncidentManagementControlCenterUserControl incidentManagementControlCenterUserControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage incidentControlCenterTabPage;
		private EDIWorkflowTabPage WorkflowTabPage;
		private ZTabPage RelatedItemsTabPage;
	}
}
