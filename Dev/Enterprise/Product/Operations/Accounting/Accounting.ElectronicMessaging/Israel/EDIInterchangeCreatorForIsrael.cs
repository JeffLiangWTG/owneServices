using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	public class EDIInterchangeCreatorForIsrael : GlobalEDIInterchangeCreator
	{
		public EDIInterchangeCreatorForIsrael(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory) : base(company, countryFactory, string.Empty)
		{
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new TransactionBatchToGEIConverterForIsrael(CountryFactory);
	}

	public class TransactionBatchToGEIConverterForIsrael : GlobalTransactionBatchToGEIConverter
	{
		public TransactionBatchToGEIConverterForIsrael(ICountryEInvoicingObjectFactory countryFactory) : base(countryFactory)
		{
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new TransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
	}
}
