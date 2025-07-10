#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBatchHeaderPrintTask
	{
		public ZGuid DocBuilderInvoiceMenuPK_ForTestOnly => DocBuilderInvoiceMenuPK;

		public PrintTask Task_ForTestOnly => Task;

		public DocumentCommand GetInvoicePrintCommand_ForTestOnly(InvoiceBatchHeader invoiceBatch, ZString nameOfMenu)
		{
			return GetInvoicePrintCommand(invoiceBatch, nameOfMenu);
		}

		public static TransactionHeaderCollection Transactions_ForTestOnly(ZQuery filter)
		{
			return Transactions(filter);
		}
	}
}

#endif
