namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class CusTempStorageRegLineItemDetailsLayoutControl
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
			this.ItemsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader);
			// 
			// ItemsPanel
			// 
			this.ItemsPanel.AllowDrop = true;
			this.ItemsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsPanel.Name = "ItemsPanel";
			this.ItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.ItemsPanel.TabIndex = 0;
			this.ItemsPanel.CaptionRenderingEnabled = true;
			// 
			// CusTempStorageRegLineItemDetailsLayoutControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemsPanel);
			this.Name = "CusTempStorageRegLineItemDetailsLayoutControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel ItemsPanel;
	}
}
