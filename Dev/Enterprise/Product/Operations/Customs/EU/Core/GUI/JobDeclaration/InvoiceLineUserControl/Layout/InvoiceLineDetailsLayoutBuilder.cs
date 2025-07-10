using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public class InvoiceLineDetailsLayoutBuilder<T> : CommonInvoiceLineDetailsLayoutBuilder<T>
		 where T : JobComInvoiceLine
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			var euBag = InvoiceLineDetailsControlBag.Instance;

			SetVisibility(euBag.CusNumberCodeFindBox, invoice => invoice.Declaration?.IsUCC6AndIsExport ?? false);
		}
	}
}
