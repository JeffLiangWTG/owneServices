using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Poland
{
	public sealed class PolandEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Poland;

		protected override IEInvoicingCredentialSettings Credentials => new PolandCredentialSettings();

		public sealed class PolandCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
		{
			protected override bool IsCompanyCredentialsRequired => true;

			protected override string[] HiddenGridColumns => new[] { "GP_MailBoxID" };
		}

		protected override ICredentialsLoader GetCredentialsLoader() => new PolandCredentialLoader();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
			=> new EInvoicingBatchCreator(company,
				maximumBatchSize: EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company, 3)
			);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> PolandEInvoiceAPICommandList.Codes.SendInvoiceBatch;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.BatchedTransactions;

		// Note that Poland populates ShipmentCollection just for the IAdditionalDataItemsProvider
		// ShipmentCollection is removed before the GEI message is sent
		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting()
			=> new PopulateOptionalXUTFieldsSetting(populateShipments: true);

		protected override void ModifyUniversalTransactionBeforeGEI(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> GlobalEInvoicingBuilder.RemoveShipmentCollection(transactionBatch, geiRequest);

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider()
			=> new PolandEInvoicingAdditionalDataItemsProvider();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new PolandGlobalXUEFunctionalityProvider(this);
	}
}
