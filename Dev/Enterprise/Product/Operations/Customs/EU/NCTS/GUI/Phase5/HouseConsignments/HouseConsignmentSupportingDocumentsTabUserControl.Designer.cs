namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentSupportingDocumentsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.HouseConsignmentSupportingDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentSupportingDocumentsSplitContainer)).BeginInit();
			this.HouseConsignmentSupportingDocumentsSplitContainer.Panel2.SuspendLayout();
			this.HouseConsignmentSupportingDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>);
			// 
			// HouseConsignmentSupportingDocumentsSplitContainer
			// 
			this.HouseConsignmentSupportingDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentSupportingDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentSupportingDocumentsSplitContainer.Name = "HouseConsignmentSupportingDocumentsSplitContainer";
			this.HouseConsignmentSupportingDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// HouseConsignmentSupportingDocumentsSplitContainer.Panel1
			//
			this.HouseConsignmentSupportingDocumentsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// HouseConsignmentSupportingDocumentsSplitContainer.Panel2
			// 
			this.HouseConsignmentSupportingDocumentsSplitContainer.Panel2.Controls.Add(this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel);
			this.HouseConsignmentSupportingDocumentsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.HouseConsignmentSupportingDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 295, true);
			this.HouseConsignmentSupportingDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10000);
			this.HouseConsignmentSupportingDocumentsSplitContainer.TabIndex = 0;
			// 
			// HouseConsignmentSupportingDocumentsDynamicLayoutPanel
			// 
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.AllowDrop = true;
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.AutoScroll = true;
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.Name = "HouseConsignmentSupportingDocumentsDynamicLayoutPanel";
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 191, true);
			this.HouseConsignmentSupportingDocumentsDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentSupportingDocumentsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentSupportingDocumentsSplitContainer);
			this.Name = "HouseConsignmentSupportingDocumentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseConsignmentSupportingDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentSupportingDocumentsSplitContainer)).EndInit();
			this.HouseConsignmentSupportingDocumentsSplitContainer.ResumeLayout(false);
			this.HouseConsignmentSupportingDocumentsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer HouseConsignmentSupportingDocumentsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel HouseConsignmentSupportingDocumentsDynamicLayoutPanel;
	}
}
