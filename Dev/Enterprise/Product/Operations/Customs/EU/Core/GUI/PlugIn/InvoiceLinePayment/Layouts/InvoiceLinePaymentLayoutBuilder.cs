using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class InvoiceLinePaymentLayoutBuilder<T> : ColumnLayoutBuilder<T, InvoiceLinePaymentControlBag> where T : JobComInvoiceLine
	{
		public override InvoiceLinePaymentControlBag CommonBag { get; } = InvoiceLinePaymentControlBag.Instance;
	}
}
