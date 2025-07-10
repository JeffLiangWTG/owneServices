namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemSupportingDocumentsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportingDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SupportingDocumentDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsSplitContainer)).BeginInit();
			this.SupportingDocumentsSplitContainer.Panel2.SuspendLayout();
			this.SupportingDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>);
			// 
			// SupportingDocumentsSplitContainer
			// 
			this.SupportingDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsSplitContainer.Name = "SupportingDocumentsSplitContainer";
			this.SupportingDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SupportingDocumentsSplitContainer.Panel2
			// 
			this.SupportingDocumentsSplitContainer.Panel2.Controls.Add(this.SupportingDocumentDynamicLayoutPanel);
			this.SupportingDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			this.SupportingDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(260);
			this.SupportingDocumentsSplitContainer.TabIndex = 0;
			// 
			// SupportingDocumentDynamicLayoutPanel
			// 
			this.SupportingDocumentDynamicLayoutPanel.AllowDrop = true;
			this.SupportingDocumentDynamicLayoutPanel.AutoScroll = true;
			this.SupportingDocumentDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentDynamicLayoutPanel.Name = "SupportingDocumentDynamicLayoutPanel";
			this.SupportingDocumentDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 483, true);
			this.SupportingDocumentDynamicLayoutPanel.TabIndex = 0;
			// 
			// Phase5GoodsItemSupportingDocumentsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsSplitContainer);
			this.Name = "Phase5GoodsItemSupportingDocumentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsSplitContainer)).EndInit();
			this.SupportingDocumentsSplitContainer.ResumeLayout(false);
			this.SupportingDocumentsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer SupportingDocumentsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentDynamicLayoutPanel;
	}
}

