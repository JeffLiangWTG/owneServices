using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Panama;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging
{
	public class PanamaEInvoicingObjectFactory : CountryEInvoicingObjectFactory, IBatchCreatorStrategy
	{
		protected override ZString CountryCode => CountryCodes.Panama;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForPanama(company);

		bool IBatchCreatorStrategy.SupportsWaitForOriginalTransactionForAmending => true;

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new PanamaEInvoicingAdditionalDataItemsProvider();

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: true, populateAuthorizationDetails: true);
	}
}
