namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class ManifestForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.asycudaManifestUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaManifestUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.asycudaManifestUserControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 653, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("ManifestForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Main");
			this.MainTabPage.Controls.Add(this.asycudaManifestUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 626, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 626, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 626, true);
			// 
			// MainPanel
			// 
			this.MainPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 653, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// asycudaManifestUserControl
			// 
			this.asycudaManifestUserControl.AllowDrop = true;
			this.asycudaManifestUserControl.AutoScroll = true;
			this.asycudaManifestUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.asycudaManifestUserControl, ".");
			this.asycudaManifestUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaManifestUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asycudaManifestUserControl.Name = "asycudaManifestUserControl";
			this.asycudaManifestUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 626, true);
			this.asycudaManifestUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("2331361e-7b71-4ea8-8061-d686816e2a0e", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 3;
			// 
			// ManifestForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1294, 709, true);
			this.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 725, true);
			this.Name = "ManifestForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ManifestForm";
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
			this.asycudaManifestUserControl.ResumeLayout(true);
			this.asycudaManifestUserControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal AsycudaManifestUserControl asycudaManifestUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
