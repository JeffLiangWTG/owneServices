using System.Windows.Forms;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
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
			this.userControlForPLugin = new ISTTemporyStorageUserControlForPlugin();
			this.userControlForPLugin.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 602, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.userControlForPLugin);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1093, 575, true);
			this.MainTabPage.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("9BF73152-C59E-4296-9184-62E26CFE5AAB", "Temporary Storage");
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1095, 559, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1095, 579, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 601, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// userControlForPLugin
			// 
			this.userControlForPLugin.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.userControlForPLugin, ".");
			this.userControlForPLugin.Dock = DockStyle.Fill;
			this.userControlForPLugin.Name = "userControlForPLugin";
			this.userControlForPLugin.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 553, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// CusTempStorageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 658, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.ES.Business.CusTempStorage";
			this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader);
			this.DataSourceTypeName = "Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1115, 695, true);
			this.Name = "CusTempStorageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "CusTempStorageForm";
			this.userControlForPLugin.ResumeLayout(true);
			this.userControlForPLugin.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
		private ISTTemporyStorageUserControlForPlugin userControlForPLugin;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}

