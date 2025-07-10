#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class APInvoiceLine
	{
		public APInvoiceLineValidation APInvoiceLineValidation_ForTestOnly => APInvoiceLineValidation;
	}
}

#endif
