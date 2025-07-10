namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseForm
	{
		private new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.SeaCargoHouseUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoHouseUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WorkflowTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessageUserControl.SuspendLayout();
			this.SeaCargoHouseUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 504, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("E9F7453E-6B47-41E6-98A3-17B5EF1E44BE", "Details");
			this.MainTabPage.Controls.Add(this.SeaCargoHouseUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 477, true);
			this.MainTabPage.Text = "Details";
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 477, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 477, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 504, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BA158166-9C06-45FE-8413-167E6F390EBB", "Workflow & Tracking");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 650, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("616AC157-7186-4615-9DF5-BB702F4B7EF8", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessageUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.Text = "Messages";
			// 
			// MessageUserControl
			// 
			this.MessageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)))));
			this.MessageUserControl.BindPrepend = "";
			this.MessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageUserControl.Name = "MessageUserControl";
			this.MessageUserControl.ShowChangingBlueMessageHeading = false;
			this.MessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.MessageUserControl.TabIndex = 0;
			// 
			// SeaCargoHouseUserControl
			// 
			this.SeaCargoHouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SeaCargoHouseUserControl, ".");
			this.SeaCargoHouseUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SeaCargoHouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SeaCargoHouseUserControl.Name = "SeaCargoHouseUserControl";
			this.SeaCargoHouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 477, true);
			this.SeaCargoHouseUserControl.TabIndex = 0;
			// 
			// SeaCargoHouseForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 570, true);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 570, true);
			this.Name = "SeaCargoHouseForm";
			this.ShouldSerializeTabPageMethods = false;
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
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessageUserControl.ResumeLayout(true);
			this.MessageUserControl.PerformLayout();
			this.SeaCargoHouseUserControl.ResumeLayout(true);
			this.SeaCargoHouseUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		Enterprise.Messaging.GUI.EDIMessageUserControl MessageUserControl;
		SeaCargoHouseUserControl SeaCargoHouseUserControl;
	}
}
