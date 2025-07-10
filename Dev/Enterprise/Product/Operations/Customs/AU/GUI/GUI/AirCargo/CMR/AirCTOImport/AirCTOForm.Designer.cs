namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOForm
	{
		protected override void InitializeComponent()
		{
			this.airCTOUserControl1 = new Enterprise.Customs.AU.AirCargo.GUI.AirCTOUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 579, true);
			this.MainTabControl.TabIndex = 4;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.airCTOUserControl1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 552, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.Text = "Main";
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(493);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(494);
			// 
			// airCTOUserControl1
			// 
			this.airCTOUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCTOUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCTOUserControl1.Name = "airCTOUserControl1";
			this.airCTOUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 552, true);
			this.airCTOUserControl1.TabIndex = 0;
			// 
			// AirCTOForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 635, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 669, true);
			this.Name = "AirCTOForm";
			this.Text = "AirCTOForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		private AirCTOUserControl airCTOUserControl1;
	}
}
