using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectWriter : Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		public CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override CommercialInvoiceHeader PopulateDataObject(BaseJobComInvoiceHeader invoiceBO)
		{
			var result = base.PopulateDataObject(invoiceBO);
			var addInfoCollection = result.AddInfoCollection ?? new List<AddInfo>();

			UpdateAddInfoCollection(addInfoCollection, Constants.AddInfoKeys.InvoiceHeader.NetWeight, invoiceBO.JZ_NetWeight);
			UpdateAddInfoCollection(addInfoCollection, Constants.AddInfoKeys.InvoiceHeader.NetWeightUQ, invoiceBO.JZ_NetWeightUQ);
			UpdateAddInfoCollection(addInfoCollection, Constants.AddInfoKeys.InvoiceHeader.ValuationDateOverride, invoiceBO.JZ_ValuationDateOverride);
			UpdateAddInfoCollection(addInfoCollection, Constants.AddInfoKeys.InvoiceHeader.CountryOfOrigin, invoiceBO.JZ_RN_NKDefaultOrigin);
			UpdateAddInfoCollection(addInfoCollection, Constants.AddInfoKeys.InvoiceHeader.ProvinceOfOrigin, invoiceBO.JZ_RW_NKOriginState);

			result.AddInfoCollection = addInfoCollection;
			return result;
		}

		protected override void PopulateCommercialInvoiceOrganizationAddressData(BaseJobComInvoiceHeader invoiceBO, CommercialInvoiceHeader invoiceData)
		{
			base.PopulateCommercialInvoiceOrganizationAddressData(invoiceBO, invoiceData);

			var invoice = (JobComInvoiceHeader)invoiceBO;
			if (invoice.IsImport)
			{
				invoiceData.AddOrgAddress(writeManager, invoice.Consignee, Constants.AddressType.Consignee);
				invoiceData.AddOrgAddress(writeManager, invoice.SupplierPickupDeliveryAddress.Organisation, Constants.AddressType.Shipper);
			}
		}

		protected override List<AddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;

			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.ValueForDutyCode, invoiceLine.CA_ValueForDutyCode);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.AuthorityNumber, invoiceLine.CA_AuthorityNumber);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.TRSNumber, invoiceLine.CA_TRSNumber);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.IsCompliantCompletion, invoiceLine.CA_CompliantCompletion);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.IsImportDateCompliant, invoiceLine.CA_CompliantImportDate);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.CustomsValueInUSD, invoiceLine.JI_CustomsValueInUSD);
			UpdateAddInfoCollection(result, Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin, invoiceLine.JI_StateOrRegionOfOrigin);

			if (!invoiceLine.JI_CustomsSecondQuantity.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.Qty2, invoiceLine.JI_CustomsSecondQuantity);
			}

			if (!invoiceLine.JI_CustomsSecondUnitQty.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.Qty2UM, invoiceLine.JI_CustomsSecondUnitQty);
			}

			if (!invoiceLine.JI_CustomsThirdQuantity.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.Qty3, invoiceLine.JI_CustomsThirdQuantity);
			}

			if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty)
			{
				helper.Update(result, Constants.AddInfoKeys.InvoiceLine.Qty3UM, invoiceLine.JI_CustomsThirdUnitQty);
			}

			return result;
		}

		void UpdateAddInfoCollection(List<AddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty)
			{
				helper.Update(addInfoList, key, value);
			}
		}

		void UpdateAddInfoCollection(List<AddInfo> addInfoList, ZString oldKey, ZString newKey, IZType value)
		{
			if (!value.IsEmpty)
			{
				foreach (var addInfoToRemove in addInfoList.Where(x => x.Key.GetValueOrDefault() == oldKey).ToArray())
				{
					addInfoList.Remove(addInfoToRemove);
				}

				helper.Update(addInfoList, newKey, value);
			}
		}

		protected override List<AddInfoGroup> GetInvoiceLineAddInfoGroupCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoGroupCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;

			var hcHeader = invoiceLine.HCPGAHeader;
			if (hcHeader != null)
			{
				var chcAddInfoGroup = result.Find(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAHCPGAHeader);
				if (chcAddInfoGroup != null && chcAddInfoGroup.AddInfoCollection != null)
				{
					AddOldAddInfoIfNeeded(chcAddInfoGroup.AddInfoCollection, Constants.AddInfoKeys.HCPGAHeader.Category,
						GetValues(hcHeader, HCPGAHeader.Schema.CA_CategoryAPI,
							HCPGAHeader.Schema.CA_CategoryBBC, HCPGAHeader.Schema.CA_CategoryCPR,
							HCPGAHeader.Schema.CA_CategoryCTO, HCPGAHeader.Schema.CA_CategoryDSE,
							HCPGAHeader.Schema.CA_CategoryHDR, HCPGAHeader.Schema.CA_CategoryMDE,
							HCPGAHeader.Schema.CA_CategoryNHP, HCPGAHeader.Schema.CA_CategoryOCS,
							HCPGAHeader.Schema.CA_CategoryPES, HCPGAHeader.Schema.CA_CategoryVET));
					AddOldAddInfoIfNeeded(chcAddInfoGroup.AddInfoCollection,
						Constants.AddInfoKeys.HCPGAHeader.IntendedUseCode,
						GetValues(hcHeader, HCPGAHeader.Schema.CA_IntendedUseCodeAPI,
							HCPGAHeader.Schema.CA_IntendedUseCodeBBC, HCPGAHeader.Schema.CA_IntendedUseCodeCPR,
							HCPGAHeader.Schema.CA_IntendedUseCodeCTO, HCPGAHeader.Schema.CA_IntendedUseCodeDSE,
							HCPGAHeader.Schema.CA_IntendedUseCodeHDR, HCPGAHeader.Schema.CA_IntendedUseCodeMDE,
							HCPGAHeader.Schema.CA_IntendedUseCodeNHP, HCPGAHeader.Schema.CA_IntendedUseCodeOCS,
							HCPGAHeader.Schema.CA_IntendedUseCodePES, HCPGAHeader.Schema.CA_IntendedUseCodeRED,
							HCPGAHeader.Schema.CA_IntendedUseCodeVET));
				}
			}

			if (invoiceLine.TCPGAHeader != null)
			{
				var ctcAddInfoGroup = result.Find(x => x.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CATCPGAHeader);
				if (ctcAddInfoGroup != null)
				{
					if (invoiceLine.TCPGAHeader.IsZZImporterDeclared)
					{
						UpdateAddInfoCollection(ctcAddInfoGroup.AddInfoCollection, Constants.AddInfoKeys.InvoiceLine.ImporterDeclarationCode, Constants.AddInfoKeys.InvoiceLine.ImporterDeclarationCode2, invoiceLine.TCPGAHeader.CA_ImporterDeclarationCode);
					}

					var productClassDesc = invoiceLine.TCPGAHeader.AddInfoLookups.ProductClassList.GetDescriptionFromCode(invoiceLine.TCPGAHeader.CA_ProductClass);
					if (productClassDesc != null)
					{
						UpdateAddInfoCollection(ctcAddInfoGroup.AddInfoCollection, Constants.AddInfoKeys.InvoiceLine.ProductClassDescription, (ZString)productClassDesc);
					}

					var vehicleConditionDesc = invoiceLine.TCPGAHeader.AddInfoLookups.VehicleConditionList.GetDescriptionFromCode(invoiceLine.TCPGAHeader.CA_VehicleCondition);
					if (vehicleConditionDesc != null)
					{
						UpdateAddInfoCollection(ctcAddInfoGroup.AddInfoCollection, Constants.AddInfoKeys.InvoiceLine.VehicleConditionDescription, (ZString)vehicleConditionDesc);
					}
				}
			}
			return result;
		}

		void AddOldAddInfoIfNeeded(List<AddInfo> addInfoCollection, string oldAddInfo, IEnumerable<ZString> codes)
		{
			var uniqueCodes = codes.Distinct().Take(2).ToArray();
			if (uniqueCodes.Length == 1)
			{
				helper.Update(addInfoCollection, oldAddInfo, uniqueCodes[0]);
			}
		}

		IEnumerable<ZString> GetValues(HCPGAHeader hcHeader, params string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
			{
				var info = hcHeader.ZPropertyInfoHash.GetPropertySafe(propertyName);
				if (info != null)
				{
					var value = (ZString)info.Value;
					if (!value.IsEmpty)
					{
						yield return value;
					}
				}
			}
		}

		protected override ZBool IsPopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO)
		{
			var declaration = invoiceLineBO.Declaration;
			return invoiceLineBO.SupplierPart != null && declaration != null && declaration.SupportsBondedWarehousing && (declaration.IsInwardBondedWarehousingEnabled || declaration.IsOutwardBondedWarehousingEnabled);
		}

		protected override void PopulateBondedWarehouseQuantityAndUnit(BaseJobComInvoiceLine invoiceLineBO, CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.BondedWarehouseQuantity = invoiceLineBO.JI_InvoiceQuantity;
			invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_InvoiceUQ, invoiceLineBO.Lookups.InvoiceUQList);
		}
	}
}
