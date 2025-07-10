using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoicePaymentUserControl : ZUserControl
	{
		public InvoicePaymentUserControl()
		{
			InitializeComponent();

			DynamicInvoicePaymentPanel.UpdateLayout(GetPanelLayout());
		}

		IPanelLayoutProvider GetPanelLayout() => new InvoicePaymentLayouts();
	}
}
