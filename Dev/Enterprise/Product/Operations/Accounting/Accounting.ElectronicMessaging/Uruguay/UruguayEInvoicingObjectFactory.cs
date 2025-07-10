using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public class UruguayEInvoicingObjectFactory : CountryEInvoicingObjectFactory, IBatchCreatorStrategy
	{
		protected override ZString CountryCode => CountryCodes.Uruguay;

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => new CFEXmlWriter();

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForUruguay(company);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true);

		bool IBatchCreatorStrategy.SupportsWaitForOriginalTransactionForAmending => true;

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new UruguayEInvoicingAdditionalDataItemsProvider();
	}
}
