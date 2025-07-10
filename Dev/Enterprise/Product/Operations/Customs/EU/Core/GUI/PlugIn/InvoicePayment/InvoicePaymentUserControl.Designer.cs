using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class InvoicePaymentUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DynamicInvoicePaymentPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader);
			// 
			// DynamicInvoicePaymentPanel
			// 
			this.DynamicInvoicePaymentPanel.AllowDrop = true;
			this.DynamicInvoicePaymentPanel.AutoScroll = true;
			this.DynamicInvoicePaymentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicInvoicePaymentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicInvoicePaymentPanel.Name = "DynamicInvoicePaymentPanel";
			this.DynamicInvoicePaymentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicInvoicePaymentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.DynamicInvoicePaymentPanel.TabIndex = 1;
			this.DynamicInvoicePaymentPanel.CaptionRenderingEnabled = true;

			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicInvoicePaymentPanel);
		}

		#endregion

		DynamicLayoutPanel DynamicInvoicePaymentPanel;
	}
}
