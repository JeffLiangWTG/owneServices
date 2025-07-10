using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan
{
	public class JordanEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(
			AccEInvoicingBatch batch, GlbBranch branch, UniversalTransactionBatch universalTransactionBatch,
			ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();

			var transaction = batch.TransactionPivots[0].ParentTransactionHeader;
			result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
			{
				Key = "UUID", Value = transaction.PK.ToString(),
			});
			result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
			{
				Key = "DebtorCategory", Value = transaction.Header.OH_Category,
			});

			if (transaction.AH_TransactionBelongsToGroup.IsValid)
			{
				var originalTransaction = transaction.Factory.Load<TransactionHeader>(transaction.AH_TransactionBelongsToGroup);
				if (originalTransaction != null)
				{
					result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigUUID", Value = originalTransaction.PK.ToString(),
					});
					result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigTotalOSAmt", Value = originalTransaction.AH_OSTotalAmount.ToStringTrimZeros(),
					});
					result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
					{
						Key = "OrigTotalLocalAmt", Value = originalTransaction.AH_LocalTotalAmount.ToStringTrimZeros(),
					});
				}
			}

			return result;
		}
	}
}
