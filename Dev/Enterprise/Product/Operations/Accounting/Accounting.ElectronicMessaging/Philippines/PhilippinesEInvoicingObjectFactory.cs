using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Philippines.Credential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines
{
	public sealed class PhilippinesEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Philippines;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice;

		protected override ICredentialsLoader GetCredentialsLoader() => new PhilippinesCredentialLoader();
	}
}
