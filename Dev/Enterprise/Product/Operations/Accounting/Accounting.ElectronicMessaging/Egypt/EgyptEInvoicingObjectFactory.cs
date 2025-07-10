using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt
{
	public class EgyptEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Egypt;

		protected override ICredentialsLoader GetCredentialsLoader()
			=> new RegistryCredentialLoader(AccountingElectronicMessagingRegistry.Instance.EgyptEInvoicingCredentials);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> EgyptEInvoiceAPICommandList.Codes.GenerateInvoiceSubmission;

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
			=> new EInvoicingBatchCreator(company,
					maximumBatchSize: EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company, AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor)
			);

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => new EgyptPayloadWriter();

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => PopulateOptionalXUTFieldsSetting.AllTrue();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new EgyptGlobalXUEFunctionalityProvider(this);
	}
}
