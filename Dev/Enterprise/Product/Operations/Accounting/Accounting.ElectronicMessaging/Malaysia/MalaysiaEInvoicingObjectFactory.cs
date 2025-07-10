using System.Linq;
using CargoWise.EntityFramework;
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
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class MalaysiaEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Malaysia;

		protected override void UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request)
		{
			base.UpdateGEIBatchRequest(eInvoicingBatch, request);
			var pivot = (AccEInvoicingTransactionPivot)eInvoicingBatch?.TransactionPivots.FirstOrDefault();

			if (pivot != null)
			{
				request.MessageType = MalaysiaEInvoiceAPICommandList.GetMessageType(pivot.AIP_ActionType);

				if (pivot.AIP_ActionType == EInvoicingPivotActionType.StatusCheck)
				{
					request.GovernmentAllocatedNumber = GetSubmitBatchGovernmentAllocatedNumber(pivot);
				}
			}
		}

		ZString GetSubmitBatchGovernmentAllocatedNumber(AccEInvoicingTransactionPivot pivot)
		{
			var submitPivot = pivot.Factory.LoadTop1<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, pivot.AIP_ParentID)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit));
			return submitPivot.Batch.AIB_GovernmentAllocatedNumber;
		}

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ICredentialsLoader GetCredentialsLoader() => new MalaysiaCredentialLoader();

		protected override IEInvoicingCredentialSettings Credentials => new MalaysiaCredentialSettings();

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new MalaysiaEInvoicingAdditionalDataItemsProvider();

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company) => new EInvoicingDataValidatorForMalaysia(company, this);

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new MalaysiaGlobalXUEFunctionalityProvider(this);
	}
}
