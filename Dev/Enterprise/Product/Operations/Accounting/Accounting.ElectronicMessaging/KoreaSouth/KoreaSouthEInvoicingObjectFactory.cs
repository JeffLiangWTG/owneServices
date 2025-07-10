using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.KoreaSouth;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
		{
			var pivot = eInvoicingBatch.TransactionPivots.FirstOrDefault() as AccEInvoicingTransactionPivot;
			var messageType = "";
			if (pivot != null)
			{
				switch (pivot.AIP_ActionType)
				{
					case EInvoicingPivotActionType.Submit:
						messageType = KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
						break;
					case EInvoicingPivotActionType.StatusCheck:
						messageType = KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest;
						break;
					default:
						break;
				}
			}
			return messageType;
		}

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter()
			=> new KoreaSouthEInvoiceXmlWriter(
				new TaxInvoiceBuilder(new TaxInvoiceValidation(), new AdditionalInfoConverter()));

		protected override IEInvoicingCredentialSettings Credentials => new KoreaSouthEInvoicingCredentialSettings();

		protected override ICredentialsLoader GetCredentialsLoader() => new ObjectFactoryCredentialLoader();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new KoreaSouthGlobalXUEFunctionalityProvider(this);
	}
}
