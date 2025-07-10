using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class APCreditNoteLineLookups : TransactionLineLookups
	{
		public APCreditNoteLineLookups(DependentTransactionLine parent) : base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders => new APCreditNoteCollection(Factory);
	}
}
