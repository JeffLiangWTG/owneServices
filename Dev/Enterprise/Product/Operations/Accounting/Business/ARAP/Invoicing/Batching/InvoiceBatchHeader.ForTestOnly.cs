#if DEBUG

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeader
	{
		public void GenerateReverseTransactionCore_ForTestOnly(bool mustTransform)
		{
			GenerateReverseTransactionCore(mustTransform);
		}

		public InvoiceTypeModuleList InvoiceTypeLookUp_ForTestOnly
		{
			get { return InvoiceTypeLookUp; }
			set { fInvoiceTypeLookUp = value; }
		}
	}
}

#endif
