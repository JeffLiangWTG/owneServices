using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam
{
	class VietnamEInvoicingPivotsToRequeueInfo : IEInvoicingRequeueProvider
	{
		public AccEInvoicingTransactionPivot[] GetAdditionalTransactionPivotsToRequeue(BusinessObjectFactory factory, IEnumerable<TransactionHeader> selectedTransactions)
		{
			if (selectedTransactions.First().AH_Ledger == LedgerTypes.AccountsPayable || !GlbStaff.CurrentUser.IsSupportUser)
			{
				return System.Array.Empty<AccEInvoicingTransactionPivot>();
			}
			var pivotsQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, selectedTransactions.Select(x => x.PK))
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.Equal, EInvoicingPivotState.Discarded);

			return factory.Load<AccEInvoicingTransactionPivot>(pivotsQuery);
		}
	}
}
