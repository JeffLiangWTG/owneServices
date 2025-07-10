namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsPackagePanelUserControl
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
			this.PackagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PackagesDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).BeginInit();
			this.PackagesSplitContainer.Panel2.SuspendLayout();
			this.PackagesSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
			// 
			// PackagesSplitContainer
			// 
			this.PackagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesSplitContainer.Name = "PackagesSplitContainer";
			this.PackagesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PackagesSplitContainer.Panel2
			//
			this.PackagesSplitContainer.Panel2.Controls.Add(this.PackagesDynamicLayoutPanel);
			this.PackagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1322, 747, true);
			this.PackagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			this.PackagesSplitContainer.TabIndex = 0;
			this.PackagesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// PackagesDynamicLayoutPanel
			// 
			this.PackagesDynamicLayoutPanel.AllowDrop = true;
			this.PackagesDynamicLayoutPanel.AutoScroll = true;
			this.PackagesDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesDynamicLayoutPanel.Name = "PackagesDynamicLayoutPanel";
			this.PackagesDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1322, 483, true);
			this.PackagesDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentPackagesPanelUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackagesSplitContainer);
			this.Name = "HouseConsignmentPackagesPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).EndInit();
			this.PackagesSplitContainer.ResumeLayout(false);
			this.PackagesSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer PackagesSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel PackagesDynamicLayoutPanel;
	}
}
