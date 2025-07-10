#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceForm
	{
		public ZPanel ReceiptPaymentPanel_ForTestOnly
		{
			get { return ReceiptPaymentPanel; }
			set { ReceiptPaymentPanel = value; }
		}
	}
}

#endif
