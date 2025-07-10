using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType: typeof(JobComInvoiceHeader))
		{
		}

		protected override void ImportCountrySpecificRelatedData(BaseJobComInvoiceHeader invoicBO, Dictionary<string, ValueSetter> delaySetters)
		{
			var invoiceRow = GetColumnIndexer(invoicBO);
			if (dataObject.AddInfoCollection != null && dataObject.AddInfoCollection.Count > 0)
			{
				var netWeight = dataObject.AddInfoCollection.GetZDecimalValue(Constants.AddInfoKeys.InvoiceHeader.NetWeight);
				if (netWeight.HasValue)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_NetWeight, netWeight, delaySetters);
				}
				var netWeightUQ = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceHeader.NetWeightUQ);
				if (netWeightUQ.HasValue)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_NetWeightUQ, netWeightUQ, delaySetters);
				}
				var valuationDateOverride = dataObject.AddInfoCollection.GetZDateTimeValue(Constants.AddInfoKeys.InvoiceHeader.ValuationDateOverride);
				if (valuationDateOverride.HasValue)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_ValuationDateOverride, valuationDateOverride, delaySetters);
				}
				var countryOfOrigin = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceHeader.CountryOfOrigin);
				if (countryOfOrigin.HasValue)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_RN_NKDefaultOrigin, countryOfOrigin, delaySetters);
				}
				var provinceOfOrigin = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceHeader.ProvinceOfOrigin);
				if (provinceOfOrigin.HasValue)
				{
					SetValue(invoiceRow, JobComInvoiceHeaderSchema.JZ_RW_NKOriginState, provinceOfOrigin, delaySetters);
				}
			}

			var customsReferenceCollection = dataObject.CustomsReferenceCollection?.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.CCN);
			if (customsReferenceCollection != null && customsReferenceCollection.Any())
			{
				var caInvoice = (JobComInvoiceHeader)invoicBO;
				caInvoice.CargoControlNumbersList.DeleteAll();
				foreach (var ccnNumber in customsReferenceCollection)
				{
					caInvoice.CargoControlNumbersList.AddNew(ccnNumber.Reference.GetValueOrDefault());
				}
			}
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);

			var stateOfOrigin = invoiceLineData.StateOfOrigin.GetCodeAsUpperCase();
			if (invoiceLineData.AddInfoCollection != null && stateOfOrigin.IsEmpty)
			{
				var provinceOfOriginAddInfo = invoiceLineData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin, logger);

				if (provinceOfOriginAddInfo.HasValue)
				{
					SetValue(invoiceLineRow, JobComInvoiceLineSchema.JI_StateOrRegionOfOrigin, provinceOfOriginAddInfo, delaySetters);
				}
			}
		}

		protected override void FillOrganizationsCore(BaseJobComInvoiceHeader invoice, CommercialInvoiceHeaderRelatedData commercialInvoiceHeaderRelatedData, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(invoice, commercialInvoiceHeaderRelatedData, delaySetters);

			var caInvoice = (JobComInvoiceHeader)invoice;
			var invoiceRow = GetColumnIndexer(caInvoice);
			Action<SchemaGuidColumn, Func<JobComInvoiceHeader, ZGuid?>> setValue = (column, getOrganisationPK) => SetValueWithDelay(invoiceRow, column, () => getOrganisationPK(caInvoice), delaySetters, JobComInvoiceHeaderSchema.PK);
			var organizationAddressCollection = dataObject.OrganizationAddressCollection;

			if (invoice.IsImport && organizationAddressCollection != null)
			{
				foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
				{
					var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
					switch (addressType)
					{
						case Constants.AddressType.Consignee:
							setValue(JobComInvoiceHeaderSchema.JZ_OH_Consignee, (o) => GetOrganizationPK(o, addressType));
							break;
						case Constants.AddressType.Shipper:
							FillOrganizationAddress(caInvoice, DocAddressTypes.Codes.SupplierPickupDeliveryAddress, GetOriginatorAddressPK(caInvoice, addressType));
							break;
						case Constants.AddressType.Originator:
							FillOrganizationAddress(caInvoice, DocAddressTypes.Codes.CommercialInvoiceOriginator, GetOriginatorAddressPK(caInvoice, addressType));
							break;
						case Constants.AddressType.FinalConsigneeAddress:
							FillOrganizationAddress(caInvoice, DocAddressTypes.Codes.FinalConsigneeAddress, GetOriginatorAddressPK(caInvoice, addressType));
							break;
						default:
							break;
					}
				}
			}
			else if (caInvoice.JZ_MessageType == JobMessageTypeList.Codes.LVSForConsolidation && organizationAddressCollection != null)
			{
				foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
				{
					var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
					if (addressType == AddressTypes.Importer)
					{
						setValue(JobComInvoiceHeaderSchema.JZ_OH_Buyer, (o) => GetOrganizationPK(o, addressType));
					}
					else if (addressType == AddressTypes.Supplier)
					{
						setValue(JobComInvoiceHeaderSchema.JZ_OH_Supplier, (o) => GetOrganizationPK(o, addressType));
					}
				}
			}
			invoice.DocAddresses.ReloadFromLocalCache();
		}

		ZGuid? GetOrganizationPK(JobComInvoiceHeader invoice, ZString addressType)
		{
			return helper.GetOrganisationPK(this, dataObject, invoice, addressType, OrganisationTypes.None);
		}

		ZGuid? GetOriginatorAddressPK(JobComInvoiceHeader invoice, ZString addressType)
		{
			return helper.GetAddressPK(this, dataObject, invoice, addressType, OrganisationTypes.Consignor);
		}

		void FillOrganizationAddress(JobComInvoiceHeader invoice, ZString addressType, ZGuid? addressPK)
		{
			if (addressPK != null && !addressType.IsEmpty)
			{
				var query = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
				query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, 0);
				query.AddToFilter(JobDocAddressSchema.E2_ParentID, invoice.PK);
				query.FetchOnlyFromLocalCache = true;

				var docAddress = factory.LoadTop1<JobDocAddress>(query);
				if (docAddress == null)
				{
					docAddress = factory.New<JobDocAddress>();
					docAddress.E2_ParentID = invoice.PK;
					docAddress.E2_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
					docAddress.E2_AddressType = addressType;
					docAddress.E2_AddressSequence = 0;
				}

				var docAddresRow = GetColumnIndexer(docAddress);
				SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, addressPK.Value);
				SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.False);
			}
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForInvoiceLine(BaseJobComInvoiceLine invoiceLineBO, CommercialInvoiceLine invoiceLineData)
		{
			var addInfoColumnsToDbColumnsMapping = new Dictionary<string, SchemaColumn>();

			var secondQuantityUnit = invoiceLineData.CustomsSecondQuantityUnit;
			var thirdQuantityUnit = invoiceLineData.CustomsThirdQuantityUnit;
			var isDataFilledByInvoiceLineData = invoiceLineData.CustomsSecondQuantity.HasValue
												  || (secondQuantityUnit != null && secondQuantityUnit.Code.HasValue)
												  || invoiceLineData.CustomsThirdQuantity.HasValue
												  || (thirdQuantityUnit != null && thirdQuantityUnit.Code.HasValue);
			if (!isDataFilledByInvoiceLineData)
			{
				addInfoColumnsToDbColumnsMapping.Add(Constants.AddInfoKeys.InvoiceLine.Qty2, JobComInvoiceLineSchema.JI_CustomsSecondQuantity);
				addInfoColumnsToDbColumnsMapping.Add(Constants.AddInfoKeys.InvoiceLine.Qty2UM, JobComInvoiceLineSchema.JI_CustomsSecondUnitQty);
				addInfoColumnsToDbColumnsMapping.Add(Constants.AddInfoKeys.InvoiceLine.Qty3, JobComInvoiceLineSchema.JI_CustomsThirdQuantity);
				addInfoColumnsToDbColumnsMapping.Add(Constants.AddInfoKeys.InvoiceLine.Qty3UM, JobComInvoiceLineSchema.JI_CustomsThirdUnitQty);
			}
			return new AddInfoDataObjectReader<JobComInvoiceLine>(logger, helper, JobComInvoiceLineSchema.JI_AddInfo, CAAddInfoSchema.Instance, null, addInfoColumnsToDbColumnsMapping);
		}

		protected override IEnumerable<ZString> GetColumnNamesForSuspendSetting(CommercialInvoiceLine invoiceLineData)
		{
			foreach (var columnName in base.GetColumnNamesForSuspendSetting(invoiceLineData))
			{
				if (columnName != JobComInvoiceLine.Schema.JI_Tariff)
				{
					yield return columnName;
				}
			}
		}

		protected override BaseJobComInvoiceLine GetNewInvoiceLine(BaseJobComInvoiceHeader invoice)
		{
			if (topLevelObject is Shipment shipment && shipment.IsHVLV())
			{
				var result = invoice.Factory.New<JobComInvoiceLine>();
				result.JI_JZ = invoice.PK;
				result.JI_ClusterKey = invoice.JZ_ClusterKey;
				return result;
			}

			return base.GetNewInvoiceLine(invoice);
		}
	}
}
