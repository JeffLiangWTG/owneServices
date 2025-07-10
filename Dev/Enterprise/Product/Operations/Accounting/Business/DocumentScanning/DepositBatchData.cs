using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(DepositBatchData),
	Enterprise.Core.Constants.DocManagerCodes.DepositBatch)]

namespace Enterprise.Accounting.Business
{
	public class DepositBatchData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CashBook.DepositBatch.DepositBatch);

		public override string ReferenceType => Core.Constants.ReferenceTypes.Accounting;

		protected override Type CollectionType => typeof(TransactionHeaderCollection);

		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("19905FE7-E874-4849-AA5E-08E5ACF05872", "Deposit Batch"); } }

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.CashbookTransaction; } }

		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			var collection = new TransactionHeaderCollection(factory, filter);
			return collection;
		}

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new TransactionHeaderEDocsViaUniversalXmlSupport(this);
	}
}
