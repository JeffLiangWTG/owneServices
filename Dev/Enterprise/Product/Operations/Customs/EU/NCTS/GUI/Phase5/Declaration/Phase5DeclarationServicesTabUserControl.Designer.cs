namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DeclarationServicesTabUserControl
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
			this.ServicesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ServiceDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ServicesSplitContainer)).BeginInit();
			this.ServicesSplitContainer.Panel2.SuspendLayout();
			this.ServicesSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IHaveServices);
			// 
			// ServicesSplitContainer
			// 
			this.ServicesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServicesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServicesSplitContainer.Name = "ServicesSplitContainer";
			this.ServicesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ServicesSplitContainer.Panel1
			// 
			this.ServicesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 600, true);
			this.ServicesSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// ServicesSplitContainer.Panel2
			// 
			this.ServicesSplitContainer.Panel2.Controls.Add(this.ServiceDynamicLayoutPanel);
			this.ServicesSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			this.ServicesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(110);
			this.ServicesSplitContainer.TabIndex = 0;
			// 
			// ServiceDynamicLayoutPanel
			// 
			this.ServiceDynamicLayoutPanel.AllowDrop = true;
			this.ServiceDynamicLayoutPanel.AutoScroll = true;
			this.ServiceDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ServiceDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ServiceDynamicLayoutPanel.Name = "ServiceDynamicLayoutPanel";
			this.ServiceDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 486, true);
			this.ServiceDynamicLayoutPanel.TabIndex = 0;
			// 
			// Phase5DeclarationServicesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ServicesSplitContainer);
			this.Name = "Phase5DeclarationServicesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ServicesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ServicesSplitContainer)).EndInit();
			this.ServicesSplitContainer.ResumeLayout(false);
			this.ServicesSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer ServicesSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel ServiceDynamicLayoutPanel;
	}
}
