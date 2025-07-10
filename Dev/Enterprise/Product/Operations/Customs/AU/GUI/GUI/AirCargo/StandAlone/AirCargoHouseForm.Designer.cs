namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoHouseForm
	{
		new void InitializeComponent()
		{
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.airCargoMessageUserControl = new Enterprise.Customs.AU.AirCargo.GUI.AirCargoMessageUserControl();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.workflowTabPage.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.airCargoMessageUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.messagesTabPage);
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 744, true);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.messagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 687, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 687, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 717, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 744, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 28, true);
			// 
			// WorkflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "WorkflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 650, true);
			this.workflowTabPage.TabIndex = 3;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.Controls.Add(this.airCargoMessageUserControl);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 650, true);
			this.messagesTabPage.TabIndex = 4;
			this.messagesTabPage.Text = "Messages";
			// 
			// AirCargoMessageUserControl
			// 
			this.airCargoMessageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.airCargoMessageUserControl, ".");
			this.airCargoMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCargoMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCargoMessageUserControl.Name = "AirCargoMessageUserControl";
			this.airCargoMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 650, true);
			this.airCargoMessageUserControl.TabIndex = 0;
			// 
			// AirCargoHouseForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 720, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 720, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 589, true);
			this.Name = "AirCargoHouseForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.workflowTabPage.ResumeLayout(false);
			this.workflowTabPage.PerformLayout();
			this.messagesTabPage.ResumeLayout(false);
			this.messagesTabPage.PerformLayout();
			this.airCargoMessageUserControl.ResumeLayout(true);
			this.airCargoMessageUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
		AirCargoMessageUserControl airCargoMessageUserControl;
		ZArchitecture.GUI.ZTabPage messagesTabPage;
	}
}
