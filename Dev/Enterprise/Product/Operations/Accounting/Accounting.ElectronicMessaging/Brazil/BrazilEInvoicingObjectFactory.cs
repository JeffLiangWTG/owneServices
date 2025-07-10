using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil
{
	public class BrazilEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Brazil;

		protected override IEInvoicingCredentialSettings Credentials { get; } = new BrazilEInvoicingCredentialSettings();

		protected override ICredentialsLoader GetCredentialsLoader() => new ObjectFactoryCredentialLoader();

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => new BrazilPayloadWriter();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new NoGroupingCancellationEInvoicingBatchCreator(company);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
		{
			var transaction = (transactionBatch?.TransactionCollection ?? Enumerable.Empty<TransactionInfo>()).SingleOrDefault()
				?? throw new ArgumentException("TransactionBatch does not have exactly one transaction");

			return BrazilEInvoiceAPICommandList.GetMessageType(transaction);
		}

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
		{
			var messageType = geiRequest?.MessageType ?? ZString.Empty;
			if (messageType == BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest
			 || messageType == BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest)
			{
				return IncludeUniversalTransactionStrategy.SingleTransaction;
			}
			return IncludeUniversalTransactionStrategy.NoTransaction;
		}

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company)
			=> new EInvoicingDataValidatorForBrazil(company);
	}

	public class BrazilEInvoicingCredentialSettings : GlobalEInvoicingCertificateCredentialSettings
	{
		protected override bool IsBranchCredentialsRequired => true;
	}
}
