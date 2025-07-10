namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemWarehouseTabUserControl
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WarehouseDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// WarehouseDynamicLayoutPanel
			// 
			this.WarehouseDynamicLayoutPanel.AllowDrop = true;
			this.WarehouseDynamicLayoutPanel.AutoScroll = true;
			this.WarehouseDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarehouseDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarehouseDynamicLayoutPanel.Name = "WarehouseDynamicLayoutPanel";
			this.WarehouseDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 483, true);
			this.WarehouseDynamicLayoutPanel.TabIndex = 0;
			// 
			// Phase5GoodsItemWarehouseTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WarehouseDynamicLayoutPanel);
			this.Name = "Phase5GoodsItemWarehouseTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(934, 624, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.GUI.DynamicLayoutPanel WarehouseDynamicLayoutPanel;
	}
}
