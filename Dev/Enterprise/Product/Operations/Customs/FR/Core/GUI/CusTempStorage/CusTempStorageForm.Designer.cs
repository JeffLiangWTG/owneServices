namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	partial class CusTempStorageForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.MainTabControl.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.WorkflowTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.WorkflowTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 603, true);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.MainTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("B9752C08-F586-46BD-99A6-BD614D45EAAA", "Temporary Storage");
            this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 580, true);
            this.MainTabPage.TabIndex = 0;
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 580, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 580, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 603, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader);
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
            this.WorkflowTabPage.TabIndex = 3;
            this.WorkflowTabPage.UseVisualStyleBackColor = true;
            // 
            // CusTempStorageForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1102, 659, true);
            this.DataSourceAssemblyName = "Enterprise.Customs.FR.Business.CusTempStorage";
            this.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader);
            this.DataSourceTypeName = "Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1115, 695, true);
            this.Name = "CusTempStorageForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "CusTempStorageForm";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.WorkflowTabPage.ResumeLayout(false);
            this.WorkflowTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		private CINTemporyStorageUserControlForPlugin userControlForPLugin;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}

