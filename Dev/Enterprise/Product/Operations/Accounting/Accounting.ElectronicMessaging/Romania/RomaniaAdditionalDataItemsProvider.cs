using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, TransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var addtitionItems = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var registrationNumber = GetRegistrationNumberForForeignBuyers(batch);
			if (!registrationNumber.IsEmpty)
			{
				addtitionItems.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = "OtherBuyerID", Value = registrationNumber });
			}

			var eInvoicingNumber = GetEInvoicingNumber(batch);
			if (!eInvoicingNumber.IsEmpty)
			{
				addtitionItems.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = "EINV_Number", Value = eInvoicingNumber });
			}

			return addtitionItems;
		}

		ZString GetRegistrationNumberForForeignBuyers(AccEInvoicingBatch batch)
		{
			var transaction = batch.TransactionPivots[0]?.ParentTransactionHeader;
			var debtor = transaction?.Header;

			if (debtor == null || debtor.CountryCode == CountryCodes.Romania)
			{
				return ZString.Empty;
			}

			ZString registrationNumber;
			(_, registrationNumber) = debtor.GetCountryCodeAndTaxRegistrationWithoutPrefix(transaction.InvoiceAddressOverride);

			if (registrationNumber.IsEmpty)
			{
				registrationNumber = debtor.OH_Code;
			}

			return registrationNumber;
		}

		ZString GetEInvoicingNumber(AccEInvoicingBatch batch)
		{
			var pivot = batch.TransactionPivots[0];
			if (pivot == null || RomaniaMessageTypeHelper.GetMessageType(pivot.AIP_Status) == RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest)
			{
				return ZString.Empty;
			}

			return pivot.ParentTransactionHeader?.EInvoicingAuthorisationNumber ?? ZString.Empty;
		}
	}
}
