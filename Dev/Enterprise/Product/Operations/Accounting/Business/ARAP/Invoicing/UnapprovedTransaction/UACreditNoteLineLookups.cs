using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class UACreditNoteLineLookups : TransactionLineLookups
	{
		public UACreditNoteLineLookups(DependentTransactionLine parent) : base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders => new UACreditNoteCollection(Factory);
	}
}
