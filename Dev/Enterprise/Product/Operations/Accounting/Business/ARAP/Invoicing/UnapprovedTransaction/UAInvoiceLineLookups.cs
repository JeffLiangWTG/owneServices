using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class UAInvoiceLineLookups : TransactionLineLookups
	{
		public UAInvoiceLineLookups(DependentTransactionLine parent) : base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders => new UAInvoiceCollection(Factory);
	}
}
