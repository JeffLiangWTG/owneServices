namespace Enterprise.Customs.AU.AirCargo.GUI
{
	partial class AirCTOHawbImportForm
	{
		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 567, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 540, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 24, true);
			// 
			// AirCTOHawbImportForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 623, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CTOCusHAWB";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 650, true);
			this.Name = "AirCTOHawbImportForm";
			this.Text = "AirCTOHawbImport";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DetailsControl = new Enterprise.Customs.AU.AirCargo.GUI.AirCTOHawbImportUserControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.DetailsControl);
			// 
			// DetailsControl
			// 
			this.DetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsControl.ShouldSerializeTabPageMethods = true;
			this.DetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsControl.Name = "DetailsControl";
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 540, true);
			this.DetailsControl.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);

		}

		private AirCTOHawbImportUserControl DetailsControl;
	}
}
