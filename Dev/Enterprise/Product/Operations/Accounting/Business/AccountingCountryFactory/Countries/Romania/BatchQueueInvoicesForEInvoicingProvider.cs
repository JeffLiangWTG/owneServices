using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class BatchQueueInvoicesForEInvoicingProvider : IBatchQueueInvoicesForEInvoicingProvider
	{
		public IEnumerable<AccTransactionHeader> GetTransactionsToBeQueued(BusinessObjectFactory factory, ZGuid companyPK, DateTime startDate, int batchSize)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.MaximumRows = batchSize;
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, companyPK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote });
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);

			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID, true);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, companyPK);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return factory.Load<AccTransactionHeader>(query);
		}
	}
}
