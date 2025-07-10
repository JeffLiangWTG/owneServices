#if DEBUG

using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public partial class AccountingUtils
	{
		public List<Base.Transaction.TransactionHeader> GetPaymentsFromMixedTransactionsCollection_ForTestOnly(ICollection transactions, BusinessObjectFactory factory)
		{
			return GetPaymentsFromMixedTransactionsCollection(transactions, factory);
		}
	}
}

#endif
