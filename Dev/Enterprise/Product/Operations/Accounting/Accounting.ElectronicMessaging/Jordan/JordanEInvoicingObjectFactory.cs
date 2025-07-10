using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	public class JordanEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Jordan;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
		{
			return JordanEInvoiceAPICommandList.Codes.SubmitTransaction;
		}

		protected override ICredentialsLoader GetCredentialsLoader() => new JordanCredentialLoader();

		protected override IEInvoicingCredentialSettings Credentials => new JordanCredentialSettings();

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new JordanEInvoicingAdditionalDataItemsProvider();

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany glbCompany) => new EInvoicingDataValidatorForJordan(glbCompany);
	}
}
