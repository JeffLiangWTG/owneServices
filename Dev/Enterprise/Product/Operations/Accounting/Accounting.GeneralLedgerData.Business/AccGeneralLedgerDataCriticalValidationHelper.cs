using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccGeneralLedgerDataCriticalValidationHelper : IAccGeneralLedgerDataCriticalValidationHelper
	{
		public bool IsGLJournalEntriesNumberHasBeenAssigned(AccTransactionHeader header)
		{
			if (header != null
				&& header.AH_Ledger == LedgerTypes.General
				&& header.IsInDatabase
				&& !header.IsCancelled
				&& AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				var query = new ZQuery();
				query.AddToFilter(AccGeneralLedgerDataSchema.GLD_JournalEntriesNumber, SQLComparisonOperator.NotEqual, "");
				query.AddToFilter(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, SQLComparisonOperator.Equal, header.PK);
				return Factory.Exists(typeof(AccGeneralLedgerData), query);
			}
			return false;
		}

		BusinessObjectFactory fFactory;

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}
	}
}
