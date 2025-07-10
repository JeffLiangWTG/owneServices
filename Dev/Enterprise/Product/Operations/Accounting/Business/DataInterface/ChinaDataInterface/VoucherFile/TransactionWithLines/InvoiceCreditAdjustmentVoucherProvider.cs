using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	/// <summary>
	/// Summary description for InvoiceVoucherProvider.
	/// </summary>
	public class InvoiceCreditAdjustmentVoucherProvider : TransactionWithLinesVoucherProvider
	{
		public InvoiceCreditAdjustmentVoucherProvider(AccTransactionHeader invoice, IControlAccountProvider controlAccount)
			: base(invoice, controlAccount)
		{
		}

		public InvoiceCreditAdjustmentVoucherProvider(AccTransactionHeader invoice)
			: base(invoice)
		{
		}
	}
}
