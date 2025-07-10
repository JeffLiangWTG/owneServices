namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class ExitSummaryMainPanelUserControl
	{
		void InitializeComponent()
		{
			this.DynamicExitSummaryPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceLine);
			// 
			// DynamicExitSummaryPanel
			// 
			this.DynamicExitSummaryPanel.AllowDrop = true;
			this.DynamicExitSummaryPanel.AutoScroll = true;
			this.DynamicExitSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicExitSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicExitSummaryPanel.Name = "DynamicExitSummaryPanel";
			this.DynamicExitSummaryPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicExitSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 171, true);
			this.DynamicExitSummaryPanel.TabIndex = 1;
			this.DynamicExitSummaryPanel.CaptionRenderingEnabled = true;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(DynamicExitSummaryPanel);
		}

		ZArchitecture.GUI.DynamicLayoutPanel DynamicExitSummaryPanel;
	}
}
