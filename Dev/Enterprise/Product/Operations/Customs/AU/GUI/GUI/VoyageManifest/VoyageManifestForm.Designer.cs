namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class VoyageManifestForm
	{
		private new void InitializeComponent()
		{
			this.voyageDetailsControl1 = new Enterprise.Customs.AU.Declaration.GUI.VoyageDetailsControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 643, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.voyageDetailsControl1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 616, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(498);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(499);
			// 
			// voyageDetailsControl1
			// 
			this.voyageDetailsControl1.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.voyageDetailsControl1.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusSeaManTranHead";
			this.voyageDetailsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.voyageDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.voyageDetailsControl1.Name = "voyageDetailsControl1";
			this.voyageDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 616, true);
			this.voyageDetailsControl1.TabIndex = 0;
			// 
			// VoyageManifestForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 680, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusSeaManTranHead";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 680, true);
			this.Name = "VoyageManifestForm";
			this.Text = "VoyageManifestForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		private VoyageDetailsControl voyageDetailsControl1;
	}
}
