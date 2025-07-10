#if DEBUG

using System.Collections.Generic;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class JobTransactionReverser
	{
		public IEnumerable<ARAP.Invoicing.InvoicingBase> TransactionsToReverse_ForTestOnly
		{
			get { return TransactionsToReverse; }
			set { TransactionsToReverse = value; }
		}
	}
}

#endif
