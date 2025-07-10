using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia
{
	public class SaudiArabiaEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public  GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch batch, GlbBranch branch, UniversalTransactionBatch universalTransactionBatch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = "UUID", Value = GetTransactionPK(batch) });  // Key identifier for data item Only
			result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = "InvoiceTaxDate", Value = GetTransactionInvoiceTaxDate(batch).ToString("yyyy-MM-dd") });

			var registrationNum = GetRegistrationNumberForForeignBuyers(batch);

			if (!registrationNum.IsEmpty)
			{
				result.Add(new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = "OtherBuyerID", Value = registrationNum });
			}
			return result;
		}

		ZDateTime GetTransactionInvoiceTaxDate(AccEInvoicingBatch batch)
		{
			var result = ZDateTime.Empty;

			var transaction = batch.TransactionPivots[0]?.ParentTransactionHeader;
			if (transaction != null)
			{
				var invoice = transaction as InvoicingBase;
				result = invoice?.InvoiceTaxDate ?? ZDateTime.Empty;
			}

			return result;
		}

		ZString GetTransactionPK(AccEInvoicingBatch batch)
		{
			var transaction = batch.TransactionPivots[0]?.ParentTransactionHeader;
			return transaction?.PK.ToString() ?? ZString.Empty;
		}

		ZString GetRegistrationNumberForForeignBuyers(AccEInvoicingBatch batch)
		{
			var result = ZString.Empty;
			var transaction = batch.TransactionPivots[0]?.ParentTransactionHeader;
			var debtor = transaction?.Header;

			if (debtor != null && debtor.CountryCode != CountryCodes.SaudiArabia)
			{
				var businessRegistrationNumber = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(debtor.CountryCode);
				if (!businessRegistrationNumber.IsNullOrEmpty())
				{
					result = debtor.GetCodeForTaxRegistrationInOrgCountry(transaction.InvoiceAddressOverride);
				}
				else
				{
					result = debtor.OH_Code;
				}
				return Regex.Replace(result.RemoveDiacritics(), "[^a-zA-Z0-9]", "");
			}
			return result;
		}
	}
}
