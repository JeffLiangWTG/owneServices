using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class APInvoiceLineLookups : TransactionLineLookups
	{
		public APInvoiceLineLookups(DependentTransactionLine parent) : base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders => new APInvoiceCollection(Factory);
	}
}
