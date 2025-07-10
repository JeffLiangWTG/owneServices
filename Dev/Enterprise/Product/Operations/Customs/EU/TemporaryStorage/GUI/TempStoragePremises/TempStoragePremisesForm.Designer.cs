using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TempStoragePremisesForm
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
			this.NumberRangeSettingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.customsNumberViewStmNumsTabPageUserControl = new Enterprise.MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl();
			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.customsNumberViewStmNumsTabPageUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.NumberRangeSettingTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 334, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NumberRangeSettingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 307, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 307, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 307, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 334, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusTempStorageRegPremises);
			// 
			// NumberRangeSettingTabPage
			// 
			this.NumberRangeSettingTabPage.CaptionResourceString = Enterprise.Customs.EU.TemporaryStorage.GUI.Res.GetData("B45EE663-9C7D-4A81-9047-D26016CD1083", "Number Range Setting");
			this.NumberRangeSettingTabPage.Controls.Add(this.customsNumberViewStmNumsTabPageUserControl);
			this.NumberRangeSettingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NumberRangeSettingTabPage.Name = "NumberRangeSettingTabPage";
			this.NumberRangeSettingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 307, true);
			this.NumberRangeSettingTabPage.TabIndex = 0;
			// 
			// customsNumberViewStmNumsTabPageUserControl
			// 
			this.customsNumberViewStmNumsTabPageUserControl.AllowDrop = true;
			this.customsNumberViewStmNumsTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customsNumberViewStmNumsTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.customsNumberViewStmNumsTabPageUserControl.Name = "customsNumberViewStmNumsTabPageUserControl";
			this.customsNumberViewStmNumsTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 307, true);
			this.customsNumberViewStmNumsTabPageUserControl.TabStop = false;
			// 
			// TempStoragePremisesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 390, true);
			this.DataSourceType = typeof(CusTempStorageRegPremises);
			this.Name = "TempStoragePremisesForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "TempStoragePremisesForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			this.customsNumberViewStmNumsTabPageUserControl.ResumeLayout(true);
			this.customsNumberViewStmNumsTabPageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabPage NumberRangeSettingTabPage;
		internal Enterprise.MasterFiles.GUI.CustomsNumberViewStmNumsTabPageUserControl customsNumberViewStmNumsTabPageUserControl;
	}
}
