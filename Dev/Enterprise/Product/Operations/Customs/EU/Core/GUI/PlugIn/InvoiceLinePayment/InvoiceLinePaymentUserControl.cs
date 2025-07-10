using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLinePaymentUserControl : ZUserControl
	{
		public InvoiceLinePaymentUserControl()
		{
			InitializeComponent();

			DynamicInvoicLinePaymentPanel.UpdateLayout(GetPanelLayout());
		}

		IPanelLayoutProvider GetPanelLayout() => new InvoiceLinePaymentLayouts();
	}
}
