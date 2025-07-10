using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		internal JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment forwardingShipment = null)
			: base(declarationDataObject, logger, factory, forwardingShipment)
		{
		}

		protected override void ImportCountrySpecificRelatedData(JobDeclaration declaration)
		{
			var cusRefCollection = dataObject.CustomsReferenceCollection?.Where(x => x.Type.Code.GetValueOrDefault() == CusCodeDataTypeList.Codes.CCN);
			if (cusRefCollection != null && cusRefCollection.Any())
			{
				declaration.CargoControlNumbers.RemoveAndDeleteAll();

				foreach (var num in cusRefCollection)
				{
					var refDefaultValue = num.Reference.GetValueOrDefault();
					var newCCN = declaration.CargoControlNumbers.AddNew(refDefaultValue);
					if (declaration.AdditionalReferenceNumbers.OfType<CusEntryNumber>().Any(x => x.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN
					&& x.CE_EntryNum.EqualsIgnoringCase(refDefaultValue)))
					{
						newCCN.CA_IsFromNumbersTab = true;
					}
				}
			}
		}

		protected override void FillDates(JobDeclaration declaration, Shipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillDates(declaration, dataObject, delaySetters);

			var dateCollection = dataObject.DateCollection;
			if (dateCollection != null && dateCollection.Count > 0)
			{
				var declarationRow = GetColumnIndexer(declaration);
				FillDates(declarationRow, delaySetters, dateCollection, ZBool.False,
					new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_WarehouseReleaseDate, DateType.WarehouseRelease),
					new DateTypeSchemaColumnMap(JobDeclarationSchema.JE_EntryAuthorisationDate, DateType.EntryAuthorisation));
			}
		}

		protected override void FillCollections(JobDeclaration declaration, Shipment dataObject, bool isStandalone)
		{
			using (declaration.Invoices.SuspendRecalculatePageNumbers())
			{
				base.FillCollections(declaration, dataObject, isStandalone);
			}

			if(shipmentDataObject?.ShipmentType is { Code: not null }
&& shipmentDataObject.ShipmentType.Code.Value.EqualsIgnoringCase((Core.Constants.ShipmentTypes.HighVolumeLowValue)))
			{
				declaration.InvoiceLines.LoadWithoutRebuildSubCollection();
			}

			declaration.Invoices.RecalculateAllPageNumbers();
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader<Bill>(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override void FillRealFieldFromAddInfoCore(IColumnIndexer declarationRow, JobDeclaration declaration, List<AddInfo> addInfos, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(declarationRow, JobDeclarationSchema.JE_CustomsOffice, addInfos.GetZStringValue(Constants.AddInfoKeys.Declaration.PortOfClearance), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_LocationOfGoods, addInfos.GetZStringValue(Constants.AddInfoKeys.Declaration.SubLocationCode), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_DateOfFirstArrival, addInfos.GetZDateTimeValue(Constants.AddInfoKeys.Declaration.PARSETA), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_CarrierCode, addInfos.GetZStringValue(Constants.AddInfoKeys.Declaration.CarrierCode), delaySetters);
		}

		protected override void FillOrganizationsCore(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(organizationAddressCollection, declaration, delaySetters);

			var declarationRow = GetColumnIndexer(declaration);
			Action<SchemaGuidColumn, Func<JobDeclaration, ZGuid?>> setValue = (column, getOrganisationPK) => SetValueWithDelay(declarationRow, column, () => getOrganisationPK(declaration), delaySetters, JobDeclarationSchema.PK);

			setValue(JobDeclarationSchema.JE_OH_NotifyParty, GetMailToPK);
		}

		ZGuid? GetMailToPK(JobDeclaration declaration)
		{
			return Helper.GetOrganisationPK(this, dataObject, declaration, Constants.AddressType.MailTo, OrganisationTypes.None);
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(JobDeclaration declaration, List<ZString> harmonisedCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);
			if (!declaration.IsDataLoadingModule)
			{
				foreach (ZString tariffCode in harmonisedCodes)
				{
					for (var length = tariffCode.Length; length > 3; length--)
					{
						declaration.Factory.AddFetchHint(CACClassSchema.CT_Tariff, tariffCode.SubstringSafe(0, length));
					}
				}
			}
		}

		protected override void SetupJobApplicationData(JobDeclaration declaration)
		{
			base.SetupJobApplicationData(declaration);
			SetValue(declaration, CAAddInfoSchema.CA_ServiceOption, dataObject.AddInfoCollection.GetZStringValue(CAAddInfoSchema.CA_ServiceOption.Name.Substring(3)));
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override IEnumerable<ZString> GetSettingOrder(JobDeclaration declaration)
		{
			var result = base.GetSettingOrder(declaration).ToList();
			var importerKey = ColumnValueSetter.GetKey(declaration.PK, JobDeclarationSchema.JE_OH_Importer);
			var useImporterAccountSecurityNumberKey = ColumnValueSetter.GetKey(declaration.PK, CAAddInfoSchema.CA_UseImporterAccountSecurityNumber);

			if (result.Contains(importerKey))
			{
				result.Insert(result.IndexOf(importerKey), useImporterAccountSecurityNumberKey);
			}
			else
			{
				result.AddRange(new[] { useImporterAccountSecurityNumberKey, importerKey });
			}

			return result;
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForDeclaration(JobDeclaration declaration)
		{
			return new AddInfoDataObjectReader<JobDeclaration>(logger, Helper, JobDeclarationSchema.JE_AddInfo, CAAddInfoSchema.Instance);
		}
	}
}
