using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Spain
{
	public class SpainEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Spain;

		protected override IEInvoicingCredentialSettings Credentials => new SpainEInvoicingCredentialSettings();

		protected override ICredentialsLoader GetCredentialsLoader() => new ObjectFactoryCredentialLoader();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
		{
			return new EInvoicingBatchCreatorForSpain(company,
					maximumBatchSize: EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company, 98));
		}

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> SpainEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
		{
			return IncludeUniversalTransactionStrategy.BatchedTransactions;
		}

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new SpainGlobalXUEFunctionalityProvider(this);
	}

	public class SpainEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsBranchCredentialsRequired => true;
	}
}
