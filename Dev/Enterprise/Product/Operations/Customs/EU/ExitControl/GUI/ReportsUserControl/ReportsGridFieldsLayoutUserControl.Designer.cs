namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportsGridFieldsLayoutUserControl
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
		private void InitializeComponent()
		{
			this.dynamicDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitReport);
			// 
			// dynamicDetailsPanel
			// 
			this.dynamicDetailsPanel.AllowDrop = true;
			this.dynamicDetailsPanel.AutoScroll = true;
			this.dynamicDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dynamicDetailsPanel.Name = "dynamicDetailsPanel";
			this.dynamicDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dynamicDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 148, true);
			this.dynamicDetailsPanel.TabIndex = 1;
			// 
			// ReportsGridFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.dynamicDetailsPanel);
			this.Name = "ReportsGridFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 148, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.DynamicLayoutPanel dynamicDetailsPanel;
	}
}
