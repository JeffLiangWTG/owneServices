using CargoWise.Data;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class TransactionBatchToGEIConverter : AccEInvoiceBatchToGEIConverter
	{
		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			var dataAccess = new BatchExportDataAccess(((IDbConnectionInternals)Db.Connection).ADOConnection, ((IDbConnectionInternals)Db.Connection).ADOTransaction);
			var exporter = GetBatchExporter(dataAccess);
			UniversalBatch = exporter.CreateTransactionBatch(batch, SchemaVersionManager.Current.Namespace);
		}

		protected UniversalTransactionBatch UniversalBatch;

		protected virtual TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess) => new TransactionBatchExporter(dataAccess);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UniversalBatch?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
