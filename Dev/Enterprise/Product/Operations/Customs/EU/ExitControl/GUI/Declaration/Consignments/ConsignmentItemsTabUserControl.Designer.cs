namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentItemsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConsignmentItemsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentItemsSplitContainer)).BeginInit();
			this.ConsignmentItemsSplitContainer.Panel2.SuspendLayout();
			this.ConsignmentItemsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentItemCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentItem>);
			// 
			// ConsignmentItemsSplitContainer
			// 
			this.ConsignmentItemsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentItemsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentItemsSplitContainer.Name = "ConsignmentItemsSplitContainer";
			this.ConsignmentItemsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 600, true);
			this.ConsignmentItemsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			// 
			// ConsignmentItemsSplitContainer.Panel2
			// 
			this.ConsignmentItemsSplitContainer.Panel2.Controls.Add(this.ConsignmentItemPackingAndContainerDynamicLayoutPanel);
			this.ConsignmentItemsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			this.ConsignmentItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.ConsignmentItemsSplitContainer.TabIndex = 0;
			// 
			// ConsignmentItemPackingAndContainerDynamicLayoutPanel
			// 
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.AllowDrop = true;
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.AutoScroll = true;
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.Name = "ConsignmentItemPackingAndContainerDynamicLayoutPanel";
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 600, true);
			this.ConsignmentItemPackingAndContainerDynamicLayoutPanel.TabIndex = 0;
			// 
			// ConsignmentItemsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsignmentItemsSplitContainer);
			this.Name = "ConsignmentItemsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignmentItemsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ConsignmentItemsSplitContainer)).EndInit();
			this.ConsignmentItemsSplitContainer.ResumeLayout(false);
			this.ConsignmentItemsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer ConsignmentItemsSplitContainer;
		internal ZArchitecture.GUI.DynamicLayoutPanel ConsignmentItemPackingAndContainerDynamicLayoutPanel;
	}
}

