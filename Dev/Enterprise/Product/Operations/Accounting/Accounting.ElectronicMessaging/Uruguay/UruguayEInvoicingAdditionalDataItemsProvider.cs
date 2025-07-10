
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public class UruguayEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, TransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var transaction = batch.TransactionPivots[0].ParentTransactionHeader;
			var bankAccount = transaction.ReceiptBankAccount;

			if (bankAccount == null)
			{
				return items;
			}

			AddItemIfNoEmpty(items, "CompanyBankName", bankAccount.AB_BankName);
			AddItemIfNoEmpty(items, "CompanyBankAccountNumber", bankAccount.AB_AccountNum);
			AddItemIfNoEmpty(items, "CompanyBankSWIFTCode", bankAccount.AB_SWIFT);

			return items;
		}

		void AddItemIfNoEmpty(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
				items.Add(item);
			}
		}
	}
}
