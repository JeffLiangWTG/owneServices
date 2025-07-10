using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLinePaymentUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DynamicInvoicLinePaymentPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// DynamicInvoicePaymentPanel
			// 
			this.DynamicInvoicLinePaymentPanel.AllowDrop = true;
			this.DynamicInvoicLinePaymentPanel.AutoScroll = true;
			this.DynamicInvoicLinePaymentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicInvoicLinePaymentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicInvoicLinePaymentPanel.Name = "DynamicInvoicLinePaymentPanel";
			this.DynamicInvoicLinePaymentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicInvoicLinePaymentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.DynamicInvoicLinePaymentPanel.TabIndex = 1;
			this.DynamicInvoicLinePaymentPanel.CaptionRenderingEnabled = true;

			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicInvoicLinePaymentPanel);
		}

		#endregion

		DynamicLayoutPanel DynamicInvoicLinePaymentPanel;
	}
}
