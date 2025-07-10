using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface IBulkPostingCache
	{
		void FinalizePosting(IEnumerable<ZGuid> postedTransactions);
	}
}
