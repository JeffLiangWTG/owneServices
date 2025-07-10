namespace Enterprise.Customs.KR.GUI
{
	partial class ImportFTAMessageDetailsUserControl
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FTAMessageSendingObject);
			// 
			// FTAEntryLineDetailsPanel
			// 
			this.FTAEntryLineDetailsPanel.AllowDrop = true;
			this.FTAEntryLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FTAEntryLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FTAEntryLineDetailsPanel.Name = "FTAEntryLineDetailsPanel";
			this.FTAEntryLineDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.FTAEntryLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 291, true);
			this.FTAEntryLineDetailsPanel.TabIndex = 0;
			// 
			// ImportFTAMessageDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FTAEntryLineDetailsPanel);
			this.Name = "ImportFTAMessageDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 291, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel FTAEntryLineDetailsPanel;
	}
}
