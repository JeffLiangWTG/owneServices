namespace Enterprise.Customs.EU.ExitControl.GUI;

partial class ExitControlForm
{
	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	new void InitializeComponent()
	{
		this.ExitControlUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ExitControlUserControl();
		this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
		this.MainTabControl.SuspendLayout();
		this.MainTabPage.SuspendLayout();
		this.NotesTabPage.SuspendLayout();
		this.MainPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.ExitControlUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// MainTabControl
		// 
		this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 631, true);
		this.MainTabControl.Controls.Add(this.WorkflowTabPage);
		// 
		// MainTabPage
		// 
		this.MainTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("eeac2262-26d4-41cc-a687-5899bbafd04e", "Exit Control");
		this.MainTabPage.Controls.Add(this.ExitControlUserControl);
		this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
		this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 601, true);
		// 
		// NotesTabPage
		// 
		this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
		this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 601, true);
		// 
		// LogsTabPage
		// 
		this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 27, true);
		this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 601, true);
		// 
		// MainPanel
		// 
		this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 631, true);
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
		// 
		// ExitControlUserControl
		// 
		this.ExitControlUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ExitControlUserControl, ".");
		this.ExitControlUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ExitControlUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ExitControlUserControl.Name = "ExitControlUserControl";
		this.ExitControlUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1345, 601, true);
		this.ExitControlUserControl.TabIndex = 0;
		// 
		// WorkflowTabPage
		// 
		this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.WorkflowTabPage.Name = "WorkflowTabPage";
		this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
		this.WorkflowTabPage.TabIndex = 8;
		this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
		// 
		// ExitControlForm
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1351, 687, true);
		this.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
		this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
		this.Name = "ExitControlForm";
		this.ShouldSerializeTabPageMethods = false;
		this.Text = "ExitControlForm";
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
		this.ExitControlUserControl.ResumeLayout(true);
		this.ExitControlUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	public ExitControlUserControl ExitControlUserControl;
	internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
}
