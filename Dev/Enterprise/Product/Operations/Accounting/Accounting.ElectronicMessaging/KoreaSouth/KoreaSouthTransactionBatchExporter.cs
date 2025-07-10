using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthTransactionBatchExporter : TransactionBatchExporter
	{
		public KoreaSouthTransactionBatchExporter(BatchExportDataAccess dataAccess, PopulateOptionalXUTFieldsSetting optionalXUTFieldsSetting = null)
			: base(dataAccess, optionalXUTFieldsSetting: optionalXUTFieldsSetting)
		{
		}

		protected override TransactionInfo ConvertToUniversalTransaction(TransactionExporter transactionExporter, AccountingTransactionDataObjectWriterStrategy writerStrategy, AccEInvoicingBatch eInvoicingBatch, InvoicingBase invoicingBase)
		{
			var transactionInfo = base.ConvertToUniversalTransaction(transactionExporter, writerStrategy, eInvoicingBatch, invoicingBase);
			transactionInfo.ComplianceSubType = invoicingBase.PK.ToString();
			return transactionInfo;
		}
	}
}
