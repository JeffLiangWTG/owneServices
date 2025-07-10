using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Export.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class TransactionBatchToGEIConverterForItaly : TransactionBatchToGEIConverter
	{
		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			return new GlobalElectronicInvoiceBuilderForItaly(batchNumber, UniversalBatch);
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
	}
}
