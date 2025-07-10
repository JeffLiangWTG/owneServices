namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentPreviousDocumentsTabUserControl
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
			this.PreviousDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PreviousDocumentDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsSplitContainer)).BeginInit();
			this.PreviousDocumentsSplitContainer.Panel2.SuspendLayout();
			this.PreviousDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocumentCollection<Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument>);
			// 
			// PreviousDocumentsSplitContainer
			// 
			this.PreviousDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsSplitContainer.Name = "PreviousDocumentsSplitContainer";
			this.PreviousDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.PreviousDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 365, true);
			this.PreviousDocumentsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// PreviousDocumentsSplitContainer.Panel2
			// 
			this.PreviousDocumentsSplitContainer.Panel2.Controls.Add(this.PreviousDocumentDynamicLayoutPanel);
			this.PreviousDocumentsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			this.PreviousDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(336);
			this.PreviousDocumentsSplitContainer.TabIndex = 0;
			// 
			// PreviousDocumentDynamicLayoutPanel
			// 
			this.PreviousDocumentDynamicLayoutPanel.AllowDrop = true;
			this.PreviousDocumentDynamicLayoutPanel.AutoScroll = true;
			this.PreviousDocumentDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentDynamicLayoutPanel.Name = "PreviousDocumentDynamicLayoutPanel";
			this.PreviousDocumentDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 75, true);
			this.PreviousDocumentDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentPreviousDocumentsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousDocumentsSplitContainer);
			this.Name = "HouseConsignmentPreviousDocumentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsSplitContainer)).EndInit();
			this.PreviousDocumentsSplitContainer.ResumeLayout(false);
			this.PreviousDocumentsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer PreviousDocumentsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel PreviousDocumentDynamicLayoutPanel;
	}
}
