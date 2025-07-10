using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentAdditionalDocumentsTabUserControl
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
			this.AdditionalDocumentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AdditionalDocumentDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsSplitContainer)).BeginInit();
			this.AdditionalDocumentsSplitContainer.Panel2.SuspendLayout();
			this.AdditionalDocumentsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>);
			// 
			// AdditionalDocumentsSplitContainer
			// 
			this.AdditionalDocumentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsSplitContainer.Name = "AdditionalDocumentsSplitContainer";
			this.AdditionalDocumentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// AdditionalDocumentsSplitContainer.Panel1
			//
			this.AdditionalDocumentsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// AdditionalDocumentsSplitContainer.Panel2
			//
			this.AdditionalDocumentsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.AdditionalDocumentsSplitContainer.Panel2.Controls.Add(this.AdditionalDocumentDynamicLayoutPanel);
			this.AdditionalDocumentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 182, true);
			this.AdditionalDocumentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10000);
			this.AdditionalDocumentsSplitContainer.TabIndex = 0;
			// 
			// AdditionalDocumentDynamicLayoutPanel
			// 
			this.AdditionalDocumentDynamicLayoutPanel.AllowDrop = true;
			this.AdditionalDocumentDynamicLayoutPanel.AutoScroll = true;
			this.AdditionalDocumentDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentDynamicLayoutPanel.Name = "AdditionalDocumentDynamicLayoutPanel";
			this.AdditionalDocumentDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 75, true);
			this.AdditionalDocumentDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentAdditionalDocumentsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentsSplitContainer);
			this.Name = "HouseConsignmentAdditionalDocumentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDocumentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsSplitContainer)).EndInit();
			this.AdditionalDocumentsSplitContainer.ResumeLayout(false);
			this.AdditionalDocumentsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer AdditionalDocumentsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel AdditionalDocumentDynamicLayoutPanel;
	}
}
