using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZLogsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ChangeLogsTabPage = new Enterprise.ZArchitecture.GUI.ZStmALogTabPage();
			this.ActivityLogsTabPage = new Enterprise.ZArchitecture.GUI.ZActivityLoggingTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ChangeLogsTabPage);
			this.MainTabControl.Controls.Add(this.ActivityLogsTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 657, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// ChangeLogsTabPage
			// 
			this.ChangeLogsTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZLogsUserControl|55188c3d-22ea-485a-81b9-ae9de7638e27", "Change Logs");
			this.ChangeLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ChangeLogsTabPage.Name = "ChangeLogsTabPage";
			this.ChangeLogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ChangeLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 630, true);
			this.ChangeLogsTabPage.TabIndex = 0;
			// 
			// ActivityLogsTabPage
			// 
			this.ActivityLogsTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZLogsUserControl|70df29eb-9555-4bb7-bfff-ea9c9fd8fc80", "Activity Logs");
			this.ActivityLogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ActivityLogsTabPage.Name = "ActivityLogsTabPage";
			this.ActivityLogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ActivityLogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(901, 630, true);
			this.ActivityLogsTabPage.TabIndex = 1;
			// 
			// ZLogsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "ZLogsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 663, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal ZTabControl MainTabControl;
		internal Enterprise.ZArchitecture.GUI.ZStmALogTabPage ChangeLogsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZActivityLoggingTabPage ActivityLogsTabPage;
	}
}
