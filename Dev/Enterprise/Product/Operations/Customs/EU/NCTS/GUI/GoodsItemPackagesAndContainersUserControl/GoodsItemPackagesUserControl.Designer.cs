using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemPackagesUserControl
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
			this.PackageDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PackagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).BeginInit();
			this.PackagesSplitContainer.Panel1.SuspendLayout();
			this.PackagesSplitContainer.Panel2.SuspendLayout();
			this.PackagesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).BeginInit();
			this.PackagesGrid.SuspendLayout();
			this.PackagesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDescCollection<NctsCommonCargoDesc>);
			// 
			// PackageDynamicLayoutPanel
			// 
			this.PackageDynamicLayoutPanel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageDynamicLayoutPanel, "Packages");
			this.PackageDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackageDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackageDynamicLayoutPanel.Name = "PackageDynamicLayoutPanel";
			this.PackageDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 384, true);
			this.PackageDynamicLayoutPanel.TabIndex = 0;
			// 
			// PackagesSplitContainer
			// 
			this.PackagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackagesSplitContainer.Name = "PackagesSplitContainer";
			this.PackagesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PackagesSplitContainer.Panel1
			// 
			this.PackagesSplitContainer.Panel1.Controls.Add(this.PackagesGrid);
			// 
			// PackagesSplitContainer.Panel2
			// 
			this.PackagesSplitContainer.Panel2.Controls.Add(this.PackageDynamicLayoutPanel);
			this.PackagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 581, true);
			this.PackagesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.PackagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(193);
			this.PackagesSplitContainer.TabIndex = 0;
			// 
			// PackagesGrid
			// 
			this.PackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackagesGrid, "Packages");
			this.PackagesGrid.CaptionVisible = false;
			this.PackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesGrid.GridId = "D3AB9D9B-103D-4F83-A7A5-F39AF16E1D16";
			this.PackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagesGrid.LayoutKey = "zGrid1";
			this.PackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesGrid.Name = "PackagesGrid";
			this.PackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 193, true);
			this.PackagesGrid.TabIndex = 0;
			// 
			// PackagesGroupBox
			// 
			this.PackagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("88BBD674-D1B9-4B65-BCB0-B111A6D2191D", "Packages");
			this.PackagesGroupBox.Controls.Add(this.PackagesSplitContainer);
			this.PackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PackagesGroupBox, false);
			this.PackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackagesGroupBox.Name = "PackagesGroupBox";
			this.PackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 600, true);
			this.PackagesGroupBox.TabIndex = 0;
			this.PackagesGroupBox.TabStop = false;
			// 
			// GoodsItemPackagesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackagesGroupBox);
			this.Name = "GoodsItemPackagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackagesSplitContainer.Panel1.ResumeLayout(false);
			this.PackagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackagesSplitContainer)).EndInit();
			this.PackagesSplitContainer.ResumeLayout(false);
			this.PackagesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesGrid)).EndInit();
			this.PackagesGrid.ResumeLayout(false);
			this.PackagesGrid.PerformLayout();
			this.PackagesGroupBox.ResumeLayout(false);
			this.PackagesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer PackagesSplitContainer;
		public ZArchitecture.ZGrid PackagesGrid;
		internal ZArchitecture.GUI.ZGroupBox PackagesGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel PackageDynamicLayoutPanel;
	}
}
