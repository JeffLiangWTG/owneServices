using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	public class IndiaEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.India;

		#region Batching

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
			=> new NoGroupingEInvoicingBatchCreator(company);

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company)
			=> new EInvoicingDataValidatorForIndia(company);

		#endregion

		#region GEI Processing

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> IndiaEInvoiceAPICommandList.Codes.GenerateIRN;

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider()
			=> new IndiaAdditionalDataItemsProvider();

		protected override ICredentialsLoader GetCredentialsLoader()
			=> new IndiaCredentialsLoader();

		#endregion

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new IndiaGlobalXUEFunctionalityProvider(this);
	}
}
