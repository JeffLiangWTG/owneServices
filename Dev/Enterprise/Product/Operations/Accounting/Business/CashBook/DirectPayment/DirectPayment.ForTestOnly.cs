#if DEBUG
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public partial class DirectPayment
	{
		public void SetDDRCollection_ForTestOnly(DirectDebitBatchLineCollection dDRCollection)
		{
			fDDRCollection = dDRCollection;
		}
	}
}

#endif
