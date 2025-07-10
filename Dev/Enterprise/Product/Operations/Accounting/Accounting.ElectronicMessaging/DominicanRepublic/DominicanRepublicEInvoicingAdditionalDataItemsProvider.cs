
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic
{
	public class DominicanRepublicEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, TransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var transactionHeader = batch.TransactionPivots[0].ParentTransactionHeader;

			var expiryDate = transactionHeader.ComplianceSequence?.XD_ExpiryDate;

			if (expiryDate?.IsValid ?? false)
			{
				AddItem(items, DominicanRepublicConstants.AditionalDataItemsKey.FechaVencimientoSecuencia, expiryDate?.ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture));
			}

			return items;
		}

		void AddItem(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, ZString key, ZString value)
		{
			var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
			items.Add(item);
		}
	}
}
