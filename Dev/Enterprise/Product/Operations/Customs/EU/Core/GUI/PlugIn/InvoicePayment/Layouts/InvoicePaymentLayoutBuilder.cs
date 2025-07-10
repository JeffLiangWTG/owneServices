using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public class InvoicePaymentLayoutBuilder<T> : ColumnLayoutBuilder<T, InvoicePaymentControlBag> where T : JobComInvoiceHeader
	{
		public override InvoicePaymentControlBag CommonBag { get; } = InvoicePaymentControlBag.Instance;
	}
}
