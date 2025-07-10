using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Colombia
{
	public class ColombiaEInvoicingAdditionalDataItemsProvider : IAdditionalDataItemsProvider
	{
		public GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(AccEInvoicingBatch accBatch, GlbBranch branch, UniversalTransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var items = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var transaction = accBatch.TransactionPivots[0].ParentTransactionHeader;
			var orgProxy = branch.OrgProxy ?? branch.Company.OrgProxy;
			var (prefijo, correlativo) = GetPrefijoAndCorrelativo(transaction.AH_TransactionReference, transaction.ComplianceBook);

			AddItem(items, "TipoEmisor", GetCategory(orgProxy));
			AddItem(items, "TipoAdquirente", GetCategory(transaction.Header));
			AddItem(items, (NoResString)"Prefijo", prefijo);
			AddItem(items, (NoResString)"Correlativo", correlativo);

			if (!transaction.AH_TransactionReference.IsEmpty && transaction.ComplianceSequence != null)
			{
				AddAdditionalDataItemsFromComplianceSequence(items, transaction.ComplianceSequence);
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

		void AddItem(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, ZString key, ZString value)
		{
			var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem { Key = key, Value = value };
			items.Add(item);
		}

		(string prefijo, string correlativo) GetPrefijoAndCorrelativo(ZString transactionReference, AccComplianceSequence accComplianceSequence = null)
		{
			if (accComplianceSequence != null)
			{
				return (accComplianceSequence.XD_Prefix, Regex.Replace(transactionReference, $"^{accComplianceSequence.XD_Prefix}", ""));
			}

			return ("0", "0");
		}

		void AddAdditionalDataItemsFromComplianceSequence(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, AccComplianceSequence complianceSequence)
		{
			void AddItemIfNotNullOrEmpty<T>(Func<AccComplianceSequence, T> getProperty, string itemKey)
			{
				var propertyValue = getProperty(complianceSequence).ToString();
				if (!propertyValue.IsNullOrEmpty())
				{
					AddItem(items, itemKey, propertyValue);
				}
			}

			AddItemIfNotNullOrEmpty(complianceSequence => complianceSequence.XD_ExpiryDate.IsValid ? complianceSequence.XD_ExpiryDate.ToString(ColombiaConstants.DateFormat) : "", ColombiaConstants.AdditionalDataItemsKey.FechaFin);
			AddItemIfNotNullOrEmpty(complianceSequence => complianceSequence.XD_StartDate.IsValid ? complianceSequence.XD_StartDate.ToString(ColombiaConstants.DateFormat) : "" , ColombiaConstants.AdditionalDataItemsKey.FechaInicio);
			AddItemIfNotNullOrEmpty(complianceSequence => complianceSequence.XD_PrintingAuthorizationNumber, ColombiaConstants.AdditionalDataItemsKey.NumeroResolucion);
			AddItemIfNotNullOrEmpty(complianceSequence => complianceSequence.XD_StartNumber.ToString(), ColombiaConstants.AdditionalDataItemsKey.ConsecutivoInicial);
			AddItemIfNotNullOrEmpty(complianceSequence => complianceSequence.XD_EndNumber.ToString(), ColombiaConstants.AdditionalDataItemsKey.ConsecutivoFinal);
		}
	}
}
