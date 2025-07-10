using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Panama
{
	public class PanamaEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch accBatch, GlbBranch branch, UniversalTransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var transaction = accBatch.TransactionPivots[0]?.ParentTransactionHeader;

			if (transaction != null)
			{
				var orgProxy = transaction.Branch.OrgProxy ?? transaction.Company.OrgProxy;
				var (latitude, longitude) = GetGPSCoordinates(orgProxy);

				AddItem(items, "dTipoRucEmisor", GetCategory(orgProxy));
				AddItem(items, "dCoordEmLongitude", longitude.ToString(CultureInfo.InvariantCulture));
				AddItem(items, "dCoordEmLatitude", latitude.ToString(CultureInfo.InvariantCulture));
				AddItem(items, "dTipoRucReceptor", GetCategory(transaction.Header));

				if (!transaction.AH_TransactionReference.IsEmpty && transaction.AH_TransactionReference.Length >= 3)
				{
					var (dPtoFacDF, dNroDF) = GetNroAndPtoFacDF(transaction.AH_TransactionReference, transaction.ComplianceBook);
					AddItem(items, "dPtoFacDF", dPtoFacDF);
					AddItem(items, "dNroDF", dNroDF);
				}

				if (!transaction.InvoiceTransactionReference.IsEmpty)
				{
					AddItem(items, "InvoiceTransactionReference", transaction.InvoiceTransactionReference);
				}
			}

			return items;
		}

		string GetCategory(OrgHeader orgHeader)
		{
			if (orgHeader == null || orgHeader.OH_Category.IsEmpty)
			{
				return OrgConstants.Category.Business;
			}

			return orgHeader.OH_Category;
		}

		(double latitude, double longitude) GetGPSCoordinates(OrgHeader orgHeader)
		{
			if (orgHeader != null)
			{
				var geoLocation = orgHeader.MainAddress.OA_GeoLocation;

				return (geoLocation.Latitude.GetValueOrDefault(), geoLocation.Longitude.GetValueOrDefault());
			}

			return (0d, 0d);
		}

		void AddItem(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, ZString key, ZString value)
		{
			var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
			items.Add(item);
		}

		(string dPtoFacDF, string dNroDF) GetNroAndPtoFacDF(ZString transactionReference, AccComplianceSequence accComplianceSequence)
		{
			if (accComplianceSequence != null)
			{
				return (accComplianceSequence.XD_Prefix, Regex.Replace(transactionReference, $"^{accComplianceSequence.XD_Prefix}", ""));
			}

			var nro = transactionReference.Substring(3).IsEmpty ? (ZString)"0" : transactionReference.Substring(3);

			return (transactionReference.Substring(0, 3), nro);
		}
	}
}
