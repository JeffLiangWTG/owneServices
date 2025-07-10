using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TemporaryStoragePreviousDocument);
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AllowDrop = true;
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.DetailsPanel.TabIndex = 1;
			this.DetailsPanel.CaptionRenderingEnabled = true;
			// 
			// UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DetailsPanel);
			this.Name = "UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel DetailsPanel;
	}
}
