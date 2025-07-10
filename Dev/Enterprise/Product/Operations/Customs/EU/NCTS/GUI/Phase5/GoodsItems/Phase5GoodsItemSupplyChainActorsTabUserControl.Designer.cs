
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemSupplyChainActorsTabUserControl
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
			this.SupplyChainActorsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SupplyChainActorDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorsSplitContainer)).BeginInit();
			this.SupplyChainActorsSplitContainer.Panel2.SuspendLayout();
			this.SupplyChainActorsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference>);
			// 
			// SupplyChainActorsSplitContainer
			// 
			this.SupplyChainActorsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorsSplitContainer.Name = "SupplyChainActorsSplitContainer";
			this.SupplyChainActorsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SupplyChainActorsSplitContainer.Panel1
			//
			this.SupplyChainActorsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// SupplyChainActorsSplitContainer.Panel2
			// 
			this.SupplyChainActorsSplitContainer.Panel2.Controls.Add(this.SupplyChainActorDynamicLayoutPanel);
			this.SupplyChainActorsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			this.SupplyChainActorsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			this.SupplyChainActorsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10000);
			this.SupplyChainActorsSplitContainer.TabIndex = 0;
			// 
			// SupplyChainActorDynamicLayoutPanel
			// 
			this.SupplyChainActorDynamicLayoutPanel.AllowDrop = true;
			this.SupplyChainActorDynamicLayoutPanel.AutoScroll = true;
			this.SupplyChainActorDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupplyChainActorDynamicLayoutPanel.Name = "SupplyChainActorDynamicLayoutPanel";
			this.SupplyChainActorDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 483, true);
			this.SupplyChainActorDynamicLayoutPanel.TabIndex = 0;
			// 
			// Phase5GoodsItemSupplyChainActorsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplyChainActorsSplitContainer);
			this.Name = "Phase5GoodsItemSupplyChainActorsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1046, 747, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplyChainActorsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SupplyChainActorsSplitContainer)).EndInit();
			this.SupplyChainActorsSplitContainer.ResumeLayout(false);
			this.SupplyChainActorsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer SupplyChainActorsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel SupplyChainActorDynamicLayoutPanel;

	}
}
