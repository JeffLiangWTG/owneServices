using CargoWise.ComponentModel;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Export.Business;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthTransactionBatchToGEIConverter : GlobalTransactionBatchToGEIConverter
	{
		public KoreaSouthTransactionBatchToGEIConverter(ICountryEInvoicingObjectFactory countryFactory) : base(countryFactory)
		{
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new KoreaSouthTransactionBatchExporter(dataAccess, CountryFactory.GEIMessagePopulateOptionalXUTFieldsSetting());

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
			=> new GlobalEInvoicingBuilder(CountryFactory, batchNumber, UniversalBatch, LoggerCreator);

		INotifications LoggerCreator() => new LoggerWithGroupKey();
	}
}
