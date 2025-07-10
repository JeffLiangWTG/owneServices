using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class SupplyChainActorTabUserControl
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
			this.SupplyChainActorSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SupplyChainActorDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorSplitContainer)).BeginInit();
			this.SupplyChainActorSplitContainer.Panel2.SuspendLayout();
			this.SupplyChainActorSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>);
			// 
			// SupplyChainActorSplitContainer
			// 
			this.SupplyChainActorSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorSplitContainer.Name = "SupplyChainActorSplitContainer";
			this.SupplyChainActorSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SupplyChainActorSplitContainer.Panel1
			//
			this.SupplyChainActorSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// SupplyChainActorSplitContainer.Panel2
			// 
			this.SupplyChainActorSplitContainer.Panel2.Controls.Add(this.SupplyChainActorDynamicLayoutPanel);
			this.SupplyChainActorSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.SupplyChainActorSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 295, true);
			this.SupplyChainActorSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10000);
			this.SupplyChainActorSplitContainer.TabIndex = 0;
			// 
			// SupplyChainActorDynamicLayoutPanel
			// 
			this.SupplyChainActorDynamicLayoutPanel.AllowDrop = true;
			this.SupplyChainActorDynamicLayoutPanel.AutoScroll = true;
			this.SupplyChainActorDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorDynamicLayoutPanel.Name = "SupplyChainActorDynamicLayoutPanel";
			this.SupplyChainActorDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 191, true);
			this.SupplyChainActorDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentSupportingDocumentsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplyChainActorSplitContainer);
			this.Name = "SupplyChainActorTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplyChainActorSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorSplitContainer)).EndInit();
			this.SupplyChainActorSplitContainer.ResumeLayout(false);
			this.SupplyChainActorSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	#endregion

	internal CargoWise.Windows.UI.KSplitContainer SupplyChainActorSplitContainer;
	internal ZArchitecture.GUI.DynamicLayoutPanel SupplyChainActorDynamicLayoutPanel;
}
}
