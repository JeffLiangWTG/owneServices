namespace Enterprise.Customs.KR.GUI
{
	partial class ImportFTAEntryLineDetailsUserControl
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
            this.FTAEntryLineDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.MessageSendingEntryLineObject);
            // 
            // FTAEntryLineDetailsPanel
            // 
            this.FTAEntryLineDetailsPanel.AllowDrop = true;
            this.FTAEntryLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FTAEntryLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FTAEntryLineDetailsPanel.Name = "FTAEntryLineDetailsPanel";
            this.FTAEntryLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 266, true);
            this.FTAEntryLineDetailsPanel.TabIndex = 0;
            // 
            // ImportFTAEntryLineDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FTAEntryLineDetailsPanel);
            this.Name = "ImportFTAEntryLineDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(558, 278, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel FTAEntryLineDetailsPanel;
	}
}
