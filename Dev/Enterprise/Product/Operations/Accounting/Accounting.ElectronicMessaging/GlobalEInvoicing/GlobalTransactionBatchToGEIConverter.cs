using CargoWise.ComponentModel;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Export.Business;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public class GlobalTransactionBatchToGEIConverter : TransactionBatchToGEIConverter
	{
		public GlobalTransactionBatchToGEIConverter(ICountryEInvoicingObjectFactory countryFactory)
		{
			CountryFactory = countryFactory;
		}
		protected ICountryEInvoicingObjectFactory CountryFactory { get; }

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber) => new GlobalEInvoicingBuilder(CountryFactory, batchNumber, UniversalBatch, LoggerCreator);

		INotifications LoggerCreator() => new Logger();

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
		{
			return new TransactionBatchExporter(dataAccess, CountryFactory.GEIMessagePopulateOptionalXUTFieldsSetting());
		}
	}
}
