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

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia
{
	public class SaudiArabiaEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.SaudiArabia;

		protected override IEInvoicingCredentialSettings Credentials => new SaudiArabiaEInvoicingCredentialSettings();

		protected override ICredentialsLoader GetCredentialsLoader() => new ObjectFactoryCredentialLoader(null, true);

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new SaudiArabiaEInvoicingAdditionalDataItemsProvider();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForSaudiArabia(company);

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new SaudiArabiaGlobalXUEFunctionalityProvider(this);
	}

	public class SaudiArabiaEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsBranchCredentialsRequired => true;
		protected override bool IsBranchRegistrationRequired => true;
	}
}
