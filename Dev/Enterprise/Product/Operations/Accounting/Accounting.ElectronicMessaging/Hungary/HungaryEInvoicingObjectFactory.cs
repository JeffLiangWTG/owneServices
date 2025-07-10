using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class HungaryEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Hungary;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request)
			=> IncludeUniversalTransactionStrategy.NoTransaction;

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
			=> new EInvoicingBatchCreatorForHungary(company);

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company)
			=> new NullEInvoicingDataValidator(company);

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new HungaryGlobalXUEFunctionalityProvider(this);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter()
			=> new HungaryPayloadWriter();

		protected override void UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request)
		{
			request.MessagingSystem = (NoResString)"Hungary NAV Online Invoicing System"; // eHub dependency. Not user visible.

			base.UpdateGEIBatchRequest(eInvoicingBatch, request);

			var credentials = GlbCompanyExternalPasswordHUI.LoadForCompany(eInvoicingBatch.Factory, eInvoicingBatch.Company);
			if (credentials != null)
			{
				if (!string.IsNullOrEmpty(credentials.SignatureKey))
				{
					request.SignKey = CredentialSender.EncryptPasswordAsString(credentials.SignatureKey);
				}
				if (!string.IsNullOrEmpty(credentials.ReplacementKey))
				{
					request.ReplacementKey = CredentialSender.EncryptPasswordAsString(credentials.ReplacementKey);
				}
			}
		}
	}
}
