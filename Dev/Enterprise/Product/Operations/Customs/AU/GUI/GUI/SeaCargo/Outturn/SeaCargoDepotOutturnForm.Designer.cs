using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDepotOutturnForm
	{
		private new void InitializeComponent()
		{
			this.workflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.seaCargoDepotOutturnUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoDepotOutturnUserControl();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.workflowTabPage.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 28, true);
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Controls.Add(this.messagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 605, true);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.messagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.seaCargoDepotOutturnUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 578, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader);
			// 
			// SeaCargoDepotOutturnUserControl
			// 
			this.seaCargoDepotOutturnUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seaCargoDepotOutturnUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seaCargoDepotOutturnUserControl.Name = "SeaCargoDepotOutturnUserControl";
			this.seaCargoDepotOutturnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 578, true);
			this.seaCargoDepotOutturnUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.Controls.Add(this.messageUserControl);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 531, true);
			this.messagesTabPage.TabIndex = 3;
			this.messagesTabPage.Text = "Outturn Messages";
			// 
			// WorkflowTabPage
			// 
			this.workflowTabPage.CheckForNotifications = true;
			this.workflowTabPage.ExcludeFromBindingOnSave = true;
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "WorkflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 550, true);
			this.workflowTabPage.TabIndex = 3;
			// 
			// MessageUserControl
			// 
			this.BindingSource.SetBindingMember(this.messageUserControl, ".");
			this.messageUserControl.BindPrepend = "";
			this.messageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageUserControl.Name = "MessageUserControl";
			this.messageUserControl.ShowChangingBlueMessageHeading = false;
			this.messageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 531, true);
			this.messageUserControl.TabIndex = 0;
			// 
			// SeaCargoDepotOutturnForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 657, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 409, true);
			this.Name = "SeaCargoDepotOutturnForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.messagesTabPage.ResumeLayout(false);
			this.workflowTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		internal SeaCargoDepotOutturnUserControl seaCargoDepotOutturnUserControl;
		private ZTabPage messagesTabPage;
		private Messaging.GUI.EDIMessageUserControl messageUserControl;
		private System.ComponentModel.Container components = null;
		private MasterFiles.GUI.ZWorkflowTabPage workflowTabPage;
	}
}
