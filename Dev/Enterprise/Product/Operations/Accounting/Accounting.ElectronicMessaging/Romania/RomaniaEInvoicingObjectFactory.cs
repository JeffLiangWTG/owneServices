using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Romania;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> RomaniaMessageTypeHelper.GetMessageType(eInvoicingBatch.TransactionPivots.ToArray<AccEInvoicingTransactionPivot>().Single().AIP_Status);

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new RomaniaAdditionalDataItemsProvider();

		protected override IEInvoicingCredentialSettings Credentials => new RomaniaEInvoicingCredentialSettings();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new RomaniaGlobalXUEFunctionalityProvider(this);

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new RomaniaEInvoicingBatchCreator(company);

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company) => new RomaniaEInvoicingDataValidator(company);
	}
}
