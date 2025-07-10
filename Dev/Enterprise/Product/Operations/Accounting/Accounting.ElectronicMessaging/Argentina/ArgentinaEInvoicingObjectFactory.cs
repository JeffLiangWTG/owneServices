using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public class ArgentinaEInvoicingObjectFactory : CountryEInvoicingObjectFactory, IBatchCreatorStrategy
	{
		protected override ZString CountryCode => CountryCodes.Argentina;

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => new ArgentinaEInvoiceXmlWriter();

		protected override IEInvoicingCredentialSettings Credentials => new ArgentinaEInvoicingCredentialSettings();

		protected override ICredentialsLoader GetCredentialsLoader() => new ObjectFactoryCredentialLoader();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForArgentina(company);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> ArgentinaEInvoiceAPICommandList.GetMessageType(transactionBatch.TransactionCollection[0]);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => PopulateOptionalXUTFieldsSetting.AllTrue();

		bool IBatchCreatorStrategy.SupportsWaitForOriginalTransactionForAmending => true;
	}
}
