using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.D365
{
	public class D365ObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.UnitedKingdom;

		#region Batching

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new NoGroupingEInvoicingBatchCreator(company);

		protected override void UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request)
		{
			request.MessagingSystem = (NoResString)"Electronic Accounting Integration";
			base.UpdateGEIBatchRequest(eInvoicingBatch, request);
		}

		protected override ZString GetEInvoicingServicePoint(ZString suffix) => "XHUB_D365_EACCOUNTING";

		#endregion

		#region GEI Processing

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.BatchedTransactions;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> D365APICommandList.Codes.GenerateInvoiceRequest;

		#endregion

		protected override ICredentialsLoader GetCredentialsLoader() => new D365CredentialsLoader();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider => new D365GlobalXUEFunctionalityProvider(this);
	}
}
