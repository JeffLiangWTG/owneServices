using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Chile
{
	public class ChileEInvoicingObjectFactory : CountryEInvoicingObjectFactory, IBatchCreatorStrategy
	{
		protected override ZString CountryCode => CountryCodes.Chile;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> ChileEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForChile(company);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true, populateAuthorizationDetails: true);

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new ChileEInvoicingAdditionalDataItemsProvider();

		bool IBatchCreatorStrategy.SupportsWaitForOriginalTransactionForAmending => true;
	}
}
