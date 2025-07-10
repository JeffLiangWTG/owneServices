using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaBillDataObjectReader : ShipmentDataObjectReader<AsycudaBill>
	{
		public AsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
			: base(dataObject, logger, factory)
		{
			this.header = Argument.NotNull(header, "header");
			this.helper = Argument.NotNull(helper, "helper");
			this.isUpdateEnabled = isUpdateEnabled;
		}

		protected readonly AsycudaManifestHeader header;
		protected readonly AsycudaManifestDataObjectReaderHelper helper;
		readonly bool isUpdateEnabled;

		public override DataContextType DataContextType
		{
			get { return DataContextType.AsycudaBill; }
		}

		protected override AsycudaBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			AsycudaBill result = null;
			var matchingReference = dataObject.AddInfoCollection?.GetZStringValue(AddInfoConstants.Bill.MatchingReference, logger);
			AsycudaBill[] bills = null;
			if (matchingReference.HasValue && !matchingReference.Value.IsEmpty)
			{
				bills = GetBills();
				result = bills.FirstOrDefault(x => x.MatchingReference == matchingReference.Value);
			}
			if (result == null && dataObject.WayBillNumber.HasValue)
			{
				bills = bills ?? GetBills();
				result = bills.Where(x => x.ABL_BillNumber == dataObject.WayBillNumber.Value).OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		protected AsycudaBill[] GetBills()
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, header.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			query.FetchOnlyFromLocalCache = !header.IsInDatabase;
			return factory.Load<AsycudaBill>(query);
		}

		protected override IMatchingBusinessEntityFinder<AsycudaBill> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference Matching has been implemented for AsycudaManifestHeader.
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AsycudaBill targetBO)
		{
			var message = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);

			if (string.IsNullOrWhiteSpace(message))
			{
				var countries = GetBillCountriesData();

				if (countries.Length > 1)
				{
					message = Res.GetString("{AAC648FC-80B3-4E98-A88D-73C2608BB4C3}", "The entry header collection for Bill '{0}' contains multiple record with country code '{1}'.", dataObject.WayBillNumber.GetValueOrDefault(), helper.CountryCode);
				}
			}

			return message;
		}

		protected override AsycudaBill GetNewBusinessObject()
		{
			return header.Bills.AddNew();
		}

		protected override void PopulateBusinessObject(AsycudaBill bill)
		{
			if (!isUpdateEnabled || !bill.HasManifestBeenSubmittedToCustomsIncludingChildren)
			{
				var billRow = GetColumnIndexer(bill);
				using (SuspendSetters(bill))
				{
					helper.FilterAndSet(AsycudaBillSchema.ABL_BillNumber, dataObject, (column) => SetValue(billRow, column, dataObject.WayBillNumber));
					helper.FilterAndSet(AsycudaBillSchema.ABL_RL_NKOrigin, dataObject, (column) => SetValue(billRow, column, dataObject.PortOfOrigin));
					helper.FilterAndSet(AsycudaBillSchema.ABL_RL_NKFinalDestination, dataObject, (column) => SetValue(billRow, column, dataObject.PortOfDestination));
					helper.FilterAndSet(AsycudaBillSchema.ABL_GrossWeight, dataObject, (column) => SetValue(billRow, column, dataObject.TotalWeight));
					helper.FilterAndSet(AsycudaBillSchema.ABL_GrossWeightUQ, dataObject, (column) => SetValue(billRow, column, dataObject.TotalWeightUnit));
					helper.FilterAndSet(AsycudaBillSchema.ABL_GoodsDescription, dataObject, (column) => SetValue(billRow, column, dataObject.GoodsDescription));
					helper.FilterAndSet(AsycudaBillSchema.ABL_Volume, dataObject, (column) => SetValue(billRow, column, dataObject.TotalVolume));
					helper.FilterAndSet(AsycudaBillSchema.ABL_VolumeUQ, dataObject, (column) => SetValue(billRow, column, dataObject.TotalVolumeUnit));
					helper.FilterAndSet(AsycudaBillSchema.ABL_GoodsValue, dataObject, (column) => SetValue(billRow, column, dataObject.GoodsValue));
					helper.FilterAndSet(AsycudaBillSchema.ABL_RX_NKGoodsValueCurrency, dataObject, (column) => SetValue(billRow, column, dataObject.GoodsValueCurrency));
					helper.FilterAndSet(AsycudaBillSchema.ABL_ManifestQty, dataObject, (column) => SetValue(billRow, column, dataObject.OuterPacks ?? dataObject.TotalNoOfPacks));
					helper.FilterAndSet(AsycudaBillSchema.ABL_ManifestUQ, dataObject, (column) => SetValue(billRow, column, dataObject.OuterPacksPackageType ?? dataObject.TotalNoOfPacksPackageType));
					helper.FilterAndSet(AsycudaBillSchema.ABL_CarrierReference, dataObject, (column) => SetValue(billRow, column, dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.ABL_CarrierReference, logger)));
					helper.FilterAndSet(AsycudaBillSchema.ABL_BolType, dataObject, (column) => SetValue(billRow, column, dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.ABL_BolType, logger)));
					helper.FilterAndSet(AsycudaBillSchema.ABL_PrepaidCollect, dataObject, (column) => SetValue(billRow, column, dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.ABL_PrepaidCollect, logger)));
					helper.FilterAndSet(AsycudaBillSchema.ABL_MarksAndNumbers, dataObject, (column) => SetValue(billRow, column, dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.ABL_MarksAndNumbers, logger)));
					helper.FilterAndSet(AsycudaBillSchema.ABL_UCRNumber, dataObject, (column) => SetValue(billRow, column, dataObject.AddInfoCollection.GetZStringValue(AsycudaBill.Schema.ABL_UCRNumber, logger)));

					PopulateBillForSpecificRules(bill);
					FillAdditionalInfoColumn(bill);
					FillCustomsSupportingInfo(bill);
					FillCustomsJobNumber(bill);
					FillMatchingReference(bill);
					FillCountryData(bill);
					FillCharges(bill);
					FillOrganizations(bill);
					FillNotes(bill);
					FillPackingLines(bill);
					PopulateWorkflowCustomFields(bill, dataObject);
				}
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("{A66F84B6-B102-49D0-84CE-420212E04E16}", "Bill '{1}' on Global Manifest Job '{0}' cannot be updated as this bill has been submitted to Customs.", header.AMA_JobReference, bill.ABL_BillNumber));
			}
		}

		protected virtual void PopulateBillForSpecificRules(AsycudaBill bill)
		{
		}

		IDisposable SuspendSetters(AsycudaBill bill)
		{
			return bill.SetterSuspender.SuspendSetting(GetBillPropertiesToSuspendSetting().ToArray());
		}

		protected virtual IEnumerable<ZString> GetBillPropertiesToSuspendSetting()
		{
			return Enumerable.Empty<ZString>();
		}

		void FillAdditionalInfoColumn(AsycudaBill bill)
		{
			var additionalInfoColumnReader = new GenAddOnColumnCollectionDataObjectReader(logger);
			additionalInfoColumnReader.ReadIntoBusinessObject(dataObject.AddInfoCollection, helper.GetAdditionalInfoColumnList(bill), bill);
		}

		void FillCustomsJobNumber(AsycudaBill bill)
		{
			var customsJobNumber = dataObject.DataContext.GetMatchingDataSource(DataContextType.CustomsDeclaration)?.Key;
			if (customsJobNumber.HasValue)
			{
				var customsJobNumberDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.CustomsJobNumber, PropertyName = AsycudaBill.Schema.CustomsJobNumber, Value = customsJobNumber.Value };
				customsJobNumberDetail.ReadIntoBusinessObject(IsDefaultingEnabled, bill);
			}
		}

		void FillMatchingReference(AsycudaBill bill)
		{
			var matchingReference = dataObject.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.MatchingReference, logger);
			if (matchingReference.HasValue)
			{
				var matchingReferenceDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.MatchingReference, PropertyName = AsycudaBill.Schema.MatchingReference, Value = matchingReference.Value };
				matchingReferenceDetail.ReadIntoBusinessObject(IsDefaultingEnabled, bill);
			}
		}

		void FillCustomsSupportingInfo(AsycudaBill bill)
		{
			var supportedTypes = helper.GetBillSupportedCusSupportingInfoCSI_Types(bill);
			if (supportedTypes.Length > 0)
			{
				var reader = new CustomsSupportingInformationCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(factory, header.AMA_RN_NKCountry, header.AMA_RN_NKCountry));
				reader.ReadIntoDataRows(bill.PK, bill.TablePrefix, bill.IsInDatabase, dataObject, supportedTypes);
			}
		}

		EntryHeader[] GetBillCountriesData()
		{
			return dataObject.EntryHeaderCollection?.Where(x => x.Type.GetCodeAsUpperCase().Left(2) == helper.CountryCode).ToArray() ?? Array.Empty<EntryHeader>();
		}

		protected EntryHeader BillCountryData
		{
			get
			{
				if (!hasLoadedBillCountryData)
				{
					hasLoadedBillCountryData = true;
					var billCountriesData = GetBillCountriesData();
					fbillCountryData = billCountriesData.Length != 1 ? null : billCountriesData[0];
				}
				return fbillCountryData;
			}
		}
		EntryHeader fbillCountryData;
		bool hasLoadedBillCountryData;

		protected EntryInstruction BillCountryAdditionalData
		{
			get
			{
				if (!hasLoadedBillCountryAdditionalData)
				{
					hasLoadedBillCountryAdditionalData = true;
					if (dataObject.EntryInstructionCollection != null && dataObject.EntryInstructionCollection.Count > 0)
					{
						var entryInstructionLink = BillCountryData?.EntryInstructionLink;
						if (entryInstructionLink.HasValue)
						{
							fbillCountryAdditionalData = dataObject.EntryInstructionCollection.FirstOrDefault(x => x.Link == entryInstructionLink);
						}
					}
				}
				return fbillCountryAdditionalData;
			}
		}
		EntryInstruction fbillCountryAdditionalData;
		bool hasLoadedBillCountryAdditionalData;

		void FillCountryData(AsycudaBill bill)
		{
			var billCountryData = BillCountryData;
			if (billCountryData != null)
			{
				new AsycudaBillEntryHeaderDataObjectReader(billCountryData, logger, factory, bill, helper).ReadIntoBusinessObject();
			}

			var billCountryAdditionalData = BillCountryAdditionalData;
			if (billCountryAdditionalData != null)
			{
				new AsycudaBillEntryInstructionDataObjectReader(billCountryAdditionalData, logger, factory, bill, helper).ReadIntoBusinessObject();
			}
		}

		protected virtual void FillPackingLines(AsycudaBill bill)
		{
			if ((header.FeatureProvider?.SupportsAsycudaPacks ?? false) && dataObject.PackingLineCollection != null)
			{
				helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(factory, bill);
				foreach (var packingLineDataObject in dataObject.PackingLineCollection)
				{
					var pack = GetAsycudaPackDataObjectReader(packingLineDataObject, logger, factory, bill, helper, isUpdateEnabled).ReadIntoBusinessObject();
					helper.PacksReaderHelper.MarkProcessed(pack);
				}
				helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(bill, logger);
			}
		}

		protected virtual AsycudaPackDataObjectReader GetAsycudaPackDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaBill bill, AsycudaManifestDataObjectReaderHelper helper, bool isUpdateEnabled)
		{
			return new AsycudaPackDataObjectReader(packingLineDataObject, logger, factory, bill, helper, isUpdateEnabled);
		}

		void FillCharges(AsycudaBill bill)
		{
			var commercialChargeCollection = dataObject.CommercialInfo?.CommercialChargeCollection;
			if (commercialChargeCollection != null)
			{
				var billRow = GetColumnIndexer(bill);
				var isDefaultingEnabled = IsDefaultingEnabled;
				foreach (var commercialCharge in commercialChargeCollection)
				{
					var chargeType = commercialCharge.ChargeType.Code.GetValueOrDefault();
					switch (chargeType)
					{
						case CustomsChargeTypeList.Codes.ExWorks:
							helper.FilterAndSet(AsycudaBillSchema.ABL_RX_NKFreightValueCurrency, dataObject, (column) => SetValue(billRow, column, commercialCharge.Currency));
							helper.FilterAndSet(AsycudaBillSchema.ABL_FreightValue, dataObject, (column) => SetValue(billRow, column, commercialCharge.Amount));
							break;
						case CustomsChargeTypeList.Codes.OverseasFreight:
							helper.FilterAndSet(AsycudaBillSchema.ABL_RX_NKTransportValueCurrency, dataObject, (column) => SetValue(billRow, column, commercialCharge.Currency));
							helper.FilterAndSet(AsycudaBillSchema.ABL_TransportValue, dataObject, (column) => SetValue(billRow, column, commercialCharge.Amount));
							break;
						case CustomsChargeTypeList.Codes.OverseasInsurance:
							helper.FilterAndSet(AsycudaBillSchema.ABL_RX_NKInsuranceValueCurrency, dataObject, (column) => SetValue(billRow, column, commercialCharge.Currency));
							helper.FilterAndSet(AsycudaBillSchema.ABL_InsuranceValue, dataObject, (column) => SetValue(billRow, column, commercialCharge.Amount));
							break;
						case CustomsChargeTypeList.Codes.OtherCharges:
							var otherChargesDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.OtherChargesValueCurrency, PropertyName = AsycudaBill.Schema.OtherChargesValueCurrency, Value = commercialCharge.Currency.GetCodeAsUpperCase() };
							otherChargesDetail.ReadIntoBusinessObject(isDefaultingEnabled, bill);
							otherChargesDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.OtherChargesValue, PropertyName = AsycudaBill.Schema.OtherChargesValue, Value = commercialCharge.Amount.GetValueOrDefault() };
							otherChargesDetail.ReadIntoBusinessObject(isDefaultingEnabled, bill);
							break;
						case CustomsChargeTypeList.Codes.Discount:
							var discountDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.DiscountValueCurrency, PropertyName = AsycudaBill.Schema.DiscountValueCurrency, Value = commercialCharge.Currency.GetCodeAsUpperCase() };
							discountDetail.ReadIntoBusinessObject(isDefaultingEnabled, bill);
							discountDetail = new GenAddOnDetail() { GenAddOnColumnName = AsycudaBill.Schema.DiscountValue, PropertyName = AsycudaBill.Schema.DiscountValue, Value = commercialCharge.Amount.GetValueOrDefault() };
							discountDetail.ReadIntoBusinessObject(isDefaultingEnabled, bill);
							break;
						case Constants.CustomsChargeType.CustomsChargeCode:
							helper.FilterAndSet(AsycudaBillSchema.ABL_RX_NKCustomsValueCurrency, dataObject, (column) => SetValue(billRow, column, commercialCharge.Currency));
							helper.FilterAndSet(AsycudaBillSchema.ABL_CustomsValue, dataObject, (column) => SetValue(billRow, column, commercialCharge.Amount));
							break;
					}
				}
			}
		}

		protected void FillOrganizations(IColumnIndexer billRow)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				FillNotifyPartyAddress(billRow);
				FillShipperAddress(billRow);
				FillConsigneeAddress(billRow);
				FillForwarderAddress(billRow);
				FillOrganizationsForSpecificRules(billRow);
			}
		}

		void FillForwarderAddress(IColumnIndexer billRow)
		{
			var forwarderAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Forwarder));
			if (forwarderAddress != null)
			{
				var addressBO = new OrganisationDataObjectReader(forwarderAddress, logger, factory).GetMatched();
				if (addressBO != null)
				{
					helper.FilterAndSet(AsycudaBillSchema.ABL_OA_Forwarder, dataObject, (column) => SetValue(billRow, column, addressBO.PK));
				}
			}
		}

		void FillNotifyPartyAddress(IColumnIndexer billRow)
		{
			var notifyPartyAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.NotifyParty));
			if (notifyPartyAddress != null)
			{
				var addressOverride = notifyPartyAddress.AddressOverride;
				if (addressOverride.HasValue && addressOverride.Value)
				{
					FillNotifyPartyAddressOverride(billRow, notifyPartyAddress);
				}
				else
				{
					var addressBO = new OrganisationDataObjectReader(notifyPartyAddress, logger, factory).GetMatched();
					if (addressBO != null)
					{
						helper.FilterAndSet(AsycudaBillSchema.ABL_OA_NotifyParty, dataObject, (column) => SetValue(billRow, column, addressBO.PK));
					}
					else
					{
						FillNotifyPartyAddressOverride(billRow, notifyPartyAddress);
					}
				}
			}
		}

		void FillNotifyPartyAddressOverride(IColumnIndexer billRow, OrganizationAddress notifyPartyAddress)
		{
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyName, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.CompanyName));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyStreet1, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.Address1));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyStreet2, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.Address2));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyCity, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.City));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyState, dataObject, (column) => SetValue(billRow, column, (ZString?)notifyPartyAddress.State));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyPostcode, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.Postcode));
			helper.FilterAndSet(AsycudaBillSchema.ABL_NotifyPartyPhone, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.Phone));
			helper.FilterAndSet(AsycudaBillSchema.ABL_RN_NKNotifyPartyCountry, dataObject, (column) => SetValue(billRow, column, notifyPartyAddress.Country));
		}

		void FillShipperAddress(IColumnIndexer billRow)
		{
			var shipperAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsignorDocumentaryAddress));
			if (shipperAddress != null)
			{
				var shipperAddressFormatted = new OrganizationAddressFormatted(shipperAddress);
				var addressOverride = shipperAddressFormatted.AddressOverride;

				if (addressOverride.HasValue && addressOverride.Value)
				{
					FillShipperAddressOverride(billRow, shipperAddressFormatted);
				}
				else
				{
					var addressBO = new OrganisationDataObjectReader(shipperAddress, logger, factory).GetMatched();
					if (addressBO != null)
					{
						helper.FilterAndSet(AsycudaBillSchema.ABL_OA_Shipper, dataObject, (column) => SetValue(billRow, column, addressBO.PK));
					}
					else
					{
						FillShipperAddressOverride(billRow, shipperAddressFormatted);
					}
				}
			}
		}

		void FillShipperAddressOverride(IColumnIndexer billRow, OrganizationAddressFormatted shipperAddressFormatted)
		{
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperName, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.CompanyName));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperStreet1, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.Address1));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperStreet2, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.Address2));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperCity, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.City));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperState, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.State));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperPostcode, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.Postcode));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ShipperPhone, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.Phone));
			helper.FilterAndSet(AsycudaBillSchema.ABL_RN_NKShipperCountry, dataObject, (column) => SetValue(billRow, column, shipperAddressFormatted.Country));
		}

		void FillConsigneeAddress(IColumnIndexer billRow)
		{
			var consigneeAddress = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			if (consigneeAddress != null)
			{
				var consigneeAddressFormatted = new OrganizationAddressFormatted(consigneeAddress);
				var addressOverride = consigneeAddressFormatted.AddressOverride;

				if (addressOverride.HasValue && addressOverride.Value)
				{
					FillConsigneeAddressOverride(billRow, consigneeAddressFormatted);
				}
				else
				{
					var addressBO = new OrganisationDataObjectReader(consigneeAddress, logger, factory).GetMatched();
					if (addressBO != null)
					{
						helper.FilterAndSet(AsycudaBillSchema.ABL_OA_Consignee, dataObject, (column) => SetValue(billRow, column, addressBO.PK));
					}
					else
					{
						FillConsigneeAddressOverride(billRow, consigneeAddressFormatted);
					}
				}
			}
		}

		void FillConsigneeAddressOverride(IColumnIndexer billRow, OrganizationAddressFormatted consigneeAddressFormatted)
		{
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneeName, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.CompanyName));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneeStreet1, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.Address1));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneeStreet2, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.Address2));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneeCity, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.City));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneeState, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.State));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneePostcode, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.Postcode));
			helper.FilterAndSet(AsycudaBillSchema.ABL_ConsigneePhone, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.Phone));
			helper.FilterAndSet(AsycudaBillSchema.ABL_RN_NKConsigneeCountry, dataObject, (column) => SetValue(billRow, column, consigneeAddressFormatted.Country));
		}

		protected virtual void FillOrganizationsForSpecificRules(IColumnIndexer billRow)
		{
		}

		void FillNotes(AsycudaBill bill)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, bill).ReadIntoCollection();
			}
		}
	}
}
