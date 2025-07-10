using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.ETail.Testing
{
	[TestedType(typeof(Report_eTailShipmentItemDetails))]
	internal class Report_eTailShipmentItemDetailsTest : DbCreateScriptTest
	{
		public void TestReport_eTailShipmentItem()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var companyPK = TestDbHelper.DefaultCompanyPK;
				var countryCode = TestDbHelper.DefaultCompanyCountryCode;

				PrepTestReport_eTailShipmentItem();

				var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_eTailShipmentItemDetails('{countryCode}', '{companyPK}', '', '', 'ALL', '1')");
				AssertShipmentItemResults(result);

				AssertEquals($"GoodsOrigin should be [{goodsOrigin}]", goodsOrigin, result.Rows[0]["GoodsOrigin"]);
				AssertEquals($"OriginHSCode should be [{originHSCode}]", originHSCode, result.Rows[0]["OriginHSCode"]);
				AssertEquals($"ItemLineGoodsDescription should be [{itemLineGoodsDescription}]", itemLineGoodsDescription, result.Rows[0]["ItemLineGoodsDescription"]);
				AssertEquals($"ProductCode should be [{productCode}]", productCode, result.Rows[0]["ProductCode"]);
				AssertEquals($"ItemLineCustomsValue should be [{itemLineCustomsValue}]", itemLineCustomsValue, result.Rows[0]["ItemLineCustomsValue"]);
				AssertEquals($"ItemLineQuantity should be [{itemLineQuantity}]", itemLineQuantity, result.Rows[0]["ItemLineQuantity"]);
				AssertEquals($"ItemLineGrossWeight should be [{itemLineGrossWeight}]", itemLineGrossWeight, result.Rows[0]["ItemLineGrossWeight"]);
				AssertEquals($"ItemLineNetWeight should be [{itemLineNetWeight}]", itemLineNetWeight, result.Rows[0]["ItemLineNetWeight"]);
				AssertEquals($"ItemLineWeightUnit should be [{itemLineWeightUnit}]", itemLineWeightUnit, result.Rows[0]["ItemLineWeightUnit"]);
				AssertEquals($"ItemLineURL should be [{itemLineUrl}]", itemLineUrl, result.Rows[0]["ItemLineURL"]);
			}
		}

		public void TestReport_eTailShipmentItemWithoutItemLine()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var companyPK = TestDbHelper.DefaultCompanyPK;
				var countryCode = TestDbHelper.DefaultCompanyCountryCode;

				PrepTestReport_eTailShipmentItem();

				var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_eTailShipmentItemDetails('{countryCode}', '{companyPK}', '', '', 'ALL', '0')");
				AssertShipmentItemResults(result);

				AssertEquals($"GoodsOrigin should be null", DBNull.Value, result.Rows[0]["GoodsOrigin"]);
				AssertEquals($"OriginHSCode should be null", DBNull.Value, result.Rows[0]["OriginHSCode"]);
				AssertEquals($"ItemLineGoodsDescription should be null", DBNull.Value, result.Rows[0]["ItemLineGoodsDescription"]);
				AssertEquals($"ProductCode should be null", DBNull.Value, result.Rows[0]["ProductCode"]);
				AssertEquals($"ItemLineCustomsValue should be null", DBNull.Value, result.Rows[0]["ItemLineCustomsValue"]);
				AssertEquals($"ItemLineQuantity should be null", DBNull.Value, result.Rows[0]["ItemLineQuantity"]);
				AssertEquals($"ItemLineGrossWeight should be null", DBNull.Value, result.Rows[0]["ItemLineGrossWeight"]);
				AssertEquals($"ItemLineNetWeight should be null", DBNull.Value, result.Rows[0]["ItemLineNetWeight"]);
				AssertEquals($"ItemLineWeightUnit should be null", DBNull.Value, result.Rows[0]["ItemLineWeightUnit"]);
				AssertEquals($"ItemLineURL should be null", DBNull.Value, result.Rows[0]["ItemLineURL"]);
			}
		}

		public void TestReport_eTailShipmentItem_CustomsStatusDescription_AU()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var countryCode = "AU";

			PrepTestReport_eTailShipmentItem_WithCustomsStatus_AU();

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_eTailShipmentItemDetails('{countryCode}', '{companyPK}', '', '', 'ALL', '1')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals($"importCustomsClearanceStatusDescription", importCustomsClearanceStatusDescriptionAU, result.Rows[0]["ImportCustomsClearanceStatusDescription"]);
			AssertEquals($"exportCustomsClearanceStatusDescription", exportCustomsClearanceStatusDescriptionAU, result.Rows[0]["ExportCustomsClearanceStatusDescription"]);
		}

		public void TestReport_eTailShipmentItem_CustomsStatusDescription_NZ()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var countryCode = "NZ";

			PrepTestReport_eTailShipmentItem_WithCustomsStatus_NZ();

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_eTailShipmentItemDetails('{countryCode}', '{companyPK}', '', '', 'ALL', '1')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals($"importCustomsClearanceStatusDescription", importCustomsClearanceStatusDescriptionNZ, result.Rows[0]["ImportCustomsClearanceStatusDescription"]);
			AssertEquals($"exportCustomsClearanceStatusDescription", exportCustomsClearanceStatusDescriptionNZ, result.Rows[0]["ExportCustomsClearanceStatusDescription"]);
		}

		static readonly int clusterKey = 1000;
		static readonly string orgCode = "TSTORG";
		static readonly string orgName = "Test Organisation";
		static readonly string addCode = "TSTORGADD";
		static readonly string address1 = "Test Organisation Address1";
		static readonly string shipmentNumber = "TSTShipment";
		static readonly string transportMode = "SEA";
		static readonly string packingMode = "LSE";
		static readonly string origin = "NZAKL";
		static readonly string destination = "AUSYD";
		static readonly string bookingReference = "TSTBookRef";
		static readonly string consignmentId = "TSTConsignmentId";
		static readonly string itemId = "TSTItemId";
		static readonly string consigneeName = "Test Consignee";
		static readonly string shipperName = "Test Shipper";

		static readonly string consignmentAddress1 = "Consignment Address 1";
		static readonly string consignmentAddress2 = "Consignment Address 2";
		static readonly string consigneeCity = "Consignee City";
		static readonly string consigneeState = "Consignee State";
		static readonly string consigneePostcode = "ConsgnePcd";
		static readonly string consigneeCountryCode = "AU";
		static readonly string consigneePhone = "0450 123 456";
		static readonly string consigneeEmail = "consignee@business.com";
		static readonly string consigneeContact = "Consignee Contact";
		static readonly string consignmentShippersReference = "Consignee Shipper Reference";
		static readonly string shipperAddress1 = "Shipper Address 1";
		static readonly string shipperAddress2 = "Shipper Address 2";
		static readonly string shipperCity = "Shipper City";
		static readonly string shipperState = "Shipper State";
		static readonly string shipperPostcode = "2000";
		static readonly string shipperCountryCode = "US";
		static readonly string shipperPhone = "0450 456 789";
		static readonly string shipperEmail = "shipper@business.com";
		static readonly string shipperContact = "Shipper Contact";
		static readonly string consignmentGoodsDescription = "Goods Description";
		static readonly string consignmentGoodsCurrency = "USD";
		static readonly decimal consignmentGoodsValue = 18.1m;
		static readonly string INCOTerms = "ABC";
		static readonly string lastMileCarrierCode = "LMCCODE";
		static readonly string lastMileCarrierName = "LMC FULL NAME";
		static readonly string destinationDepotCode = "DestinationDepotCode";
		static readonly string destinationDepotAddCode = "DestinationDepotAddCode";
		static readonly string destinationDepotName = "Destination Depot Company name";
		static readonly string volumeUnit = "L";
		static readonly string weightUnit = "KG";

		static readonly decimal itemManWeight = 1.23m;
		static readonly decimal itemActWeight = 2.34m;
		static readonly decimal itemManVolume = 3.45m;
		static readonly decimal itemActVolume = 4.56m;
		static readonly string itemPackType = "PKG";
		static readonly string itemShippersReference = "ref";
		static readonly string loadListReference = "TSTLoadListRef";
		static readonly string itemGoodsDescription = "Goods Description (Item)";
		static readonly string outerPackageReference = "TSTOuterPackageRef";
		static readonly string importCustomsClearanceStatusAU = "CCL";
		static readonly string importCustomsClearanceStatusDescriptionAU = "CONDCLEAR - Cargo can be released into home consumption subject to condition(s) These conditions are provided in Supplementary Information";
		static readonly string exportCustomsClearanceStatusAU = "REV";
		static readonly string exportCustomsClearanceStatusDescriptionAU = "REVOKED - The goods have not been exported within the allowed period after the notified date of exportation. A new EDN/CRN should be lodged to export the goods.";
		static readonly string importCustomsClearanceStatusNZ = "ACK";
		static readonly string importCustomsClearanceStatusDescriptionNZ = "Acknowledgement";
		static readonly string exportCustomsClearanceStatusNZ = "DTD";
		static readonly string exportCustomsClearanceStatusDescriptionNZ = "Domestic Transhipment Declined";
		const string DispatchedToLastMileCarrier = "DLC";

		static readonly string goodsOrigin = "NZ";
		static readonly string originHSCode = "12345678";
		static readonly string itemLineGoodsDescription = "line description";
		static readonly string productCode = "PRODUCTCODE";
		static readonly decimal itemLineCustomsValue = 10m;
		static readonly short itemLineQuantity = 3;
		static readonly decimal itemLineGrossWeight = 5.0m;
		static readonly decimal itemLineNetWeight = 2.0m;
		static readonly string itemLineWeightUnit = "KG";
		static readonly string itemLineUrl = "http://www.amazon.com/item/123456";

		void AssertShipmentItemResults(DataTable result)
		{
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals($"ShipmentID should be [{shipmentNumber}]", shipmentNumber, result.Rows[0]["ShipmentID"]);
			AssertEquals($"Origin should be [{origin}]", origin, result.Rows[0]["Origin"]);
			AssertEquals($"Destination should be [{destination}]", destination, result.Rows[0]["Destination"]);

			AssertEquals($"Consignee should be [{consigneeName}]", consigneeName, result.Rows[0]["ConsignmentConsignee"]);
			AssertEquals($"Shipper should be [{shipperName}]", shipperName, result.Rows[0]["ConsignmentShipper"]);
			AssertEquals($"Address1 should be [{consignmentAddress1}]", consignmentAddress1, result.Rows[0]["ConsignmentAddress1"]);
			AssertEquals($"Address2 should be [{consignmentAddress2}]", consignmentAddress2, result.Rows[0]["ConsignmentAddress2"]);
			AssertEquals($"ConsigneeCity should be [{consigneeCity}]", consigneeCity, result.Rows[0]["ConsigneeCity"]);
			AssertEquals($"ConsigneeState should be [{consigneeState}]", consigneeState, result.Rows[0]["ConsigneeState"]);
			AssertEquals($"ConsigneePostcode should be [{consigneePostcode}]", consigneePostcode, result.Rows[0]["ConsigneePostcode"]);
			AssertEquals($"ConsigneeCountryCode should be [{consigneeCountryCode}]", consigneeCountryCode, result.Rows[0]["ConsigneeCountryCode"]);
			AssertEquals($"ConsigneePhone should be [{consigneePhone}]", consigneePhone, result.Rows[0]["ConsigneePhone"]);
			AssertEquals($"ConsigneeEmail should be [{consigneeEmail}]", consigneeEmail, result.Rows[0]["ConsigneeEmail"]);
			AssertEquals($"ConsigneeContact should be [{consigneeContact}]", consigneeContact, result.Rows[0]["ConsigneeContact"]);
			AssertEquals($"Consignment Shippers Reference should be [{consignmentShippersReference}]", consignmentShippersReference, result.Rows[0]["ConsignmentShippersReference"]);
			AssertEquals($"Shipper Address 1 should be [{shipperAddress1}]", shipperAddress1, result.Rows[0]["ShipperAddress1"]);
			AssertEquals($"Shipper Address 2 should be [{shipperAddress2}]", shipperAddress2, result.Rows[0]["ShipperAddress2"]);
			AssertEquals($"Shipper City should be [{shipperCity}]", shipperCity, result.Rows[0]["ShipperCity"]);
			AssertEquals($"Shipper State should be [{shipperState}]", shipperState, result.Rows[0]["ShipperState"]);
			AssertEquals($"Shipper Postcode should be [{shipperPostcode}]", shipperPostcode, result.Rows[0]["ShipperPostcode"]);
			AssertEquals($"Shipper Country Code should be [{shipperCountryCode}]", shipperCountryCode, result.Rows[0]["ShipperCountryCode"]);
			AssertEquals($"Shipper Phone should be [{shipperPhone}]", shipperPhone, result.Rows[0]["ShipperPhone"]);
			AssertEquals($"ShipperEmail should be [{shipperEmail}]", shipperEmail, result.Rows[0]["ShipperEmail"]);
			AssertEquals($"ShipperContact should be [{shipperContact}]", shipperContact, result.Rows[0]["ShipperContact"]);
			AssertEquals($"ConsignmentGoodsDescription should be [{consignmentGoodsDescription}]", consignmentGoodsDescription, result.Rows[0]["ConsignmentGoodsDescription"]);
			AssertEquals($"ConsignmentGoodsValue should be [{consignmentGoodsValue}]", consignmentGoodsValue, result.Rows[0]["ConsignmentGoodsValue"]);
			AssertEquals($"GoodsValueCurrency should be [{consignmentGoodsCurrency}]", consignmentGoodsCurrency, result.Rows[0]["GoodsValueCurrency"]);
			AssertEquals($"INCOTerms should be [{INCOTerms}]", INCOTerms, result.Rows[0]["INCOTerms"]);
			AssertEquals($"LastMileCarrier should be [{lastMileCarrierName}]", lastMileCarrierName, result.Rows[0]["LastMileCarrier"]);
			AssertEquals($"DestinationDepot should be [{destinationDepotName}]", destinationDepotName, result.Rows[0]["DestinationDepot"]);
			AssertEquals($"VolumeUnit should be [{volumeUnit}]", volumeUnit, result.Rows[0]["VolumeUnit"]);
			AssertEquals($"WeightUnit should be [{weightUnit}]", weightUnit, result.Rows[0]["WeightUnit"]);

			AssertEquals($"Consignee should be [{consigneeName}]", consigneeName, result.Rows[0]["ConsignmentConsignee"]);
			AssertEquals($"Shipper should be [{shipperName}]", shipperName, result.Rows[0]["ConsignmentShipper"]);
			AssertEquals($"ItemManifestedWeight should be [{itemManWeight}]", itemManWeight, result.Rows[0]["ItemManifestedWeight"]);
			AssertEquals($"ItemActualWeight should be [{itemActWeight}]", itemActWeight, result.Rows[0]["ItemActualWeight"]);
			AssertEquals($"ItemManifestedVolume should be [{itemManVolume}]", itemManVolume, result.Rows[0]["ItemManifestedVolume"]);
			AssertEquals($"ItemActualVolume should be [{itemActVolume}]", itemActVolume, result.Rows[0]["ItemActualVolume"]);
			AssertEquals($"PackType should be [{itemPackType}]", itemPackType, result.Rows[0]["PackType"]);
			AssertEquals($"ItemID should be [{itemId}]", itemId, result.Rows[0]["ItemID"]);
			AssertEquals($"ItemShippersReference should be [{itemShippersReference}]", itemShippersReference, result.Rows[0]["ItemShippersReference"]);
			AssertEquals($"LoadListReference should be [{loadListReference}]", loadListReference, result.Rows[0]["LoadListReference"]);
			AssertEquals($"ItemGoodsDescription should be [{itemGoodsDescription}]", itemGoodsDescription, result.Rows[0]["ItemGoodsDescription"]);
			AssertEquals($"OuterPackageReference should be [{outerPackageReference}]", outerPackageReference, result.Rows[0]["OuterPackageReference"]);
			AssertEquals($"ItemStatus should be [DLC]", "DLC", result.Rows[0]["ItemStatus"]);
		}

		Guid CreateHVLVBookingHeader(Guid billToPartyAddressPK, string userCode)
		{
			var hvlvBookingHeader = new ActiveRowWrapper(HVLVBookingHeaderSchema.Instance)
			{
				[HVLVBookingHeaderSchema.HVH_BookingReference] = bookingReference,
				[HVLVBookingHeaderSchema.HVH_ClusterKey] = clusterKey,
				[HVLVBookingHeaderSchema.HVH_OA_BillToParty] = billToPartyAddressPK,
				[HVLVBookingHeaderSchema.HVH_SystemCreateTimeUtc] = DateTime.UtcNow,
				[HVLVBookingHeaderSchema.HVH_SystemCreateUser] = userCode,
				[HVLVBookingHeaderSchema.HVH_SystemLastEditTimeUtc] = DateTime.UtcNow,
				[HVLVBookingHeaderSchema.HVH_SystemLastEditUser] = userCode,
			};

			hvlvBookingHeader.Save();
			return hvlvBookingHeader.PK;
		}

		ActiveRowWrapper CreateHVLVConsignment()
		{
			var userCode = TestDbHelper.UserStaffCode;
			var organisationPK = TestDataCreator.CreateOrganisation(orgCode, orgName);
			var addressPK = TestDataCreator.CreateAddress(organisationPK, addCode, address1);

			var hvlvBookingHeaderPK = CreateHVLVBookingHeader(addressPK, userCode);

			var lastMileCarrierPK = TestDataCreator.CreateOrganisation(lastMileCarrierCode, lastMileCarrierName);

			var destinationDepotOrganisationPK = TestDataCreator.CreateOrganisation(destinationDepotCode, destinationDepotName);
			var destinationDepotAddressPK = TestDataCreator.CreateAddress(destinationDepotOrganisationPK, destinationDepotAddCode, address1);

			var hvlvConsignment = new ActiveRowWrapper(HVLVConsignmentSchema.Instance)
			{
				[HVLVConsignmentSchema.HVC_ClusterKey] = clusterKey,
				[HVLVConsignmentSchema.HVC_ConsignmentId] = consignmentId,
				[HVLVConsignmentSchema.HVC_HVH_BookingHeader] = hvlvBookingHeaderPK,
				[HVLVConsignmentSchema.HVC_Status] = "BKD",
				[HVLVConsignmentSchema.HVC_SystemCreateTimeUtc] = DateTime.UtcNow,
				[HVLVConsignmentSchema.HVC_SystemCreateUser] = userCode,
				[HVLVConsignmentSchema.HVC_SystemLastEditTimeUtc] = DateTime.UtcNow,
				[HVLVConsignmentSchema.HVC_SystemLastEditUser] = userCode,
				[HVLVConsignmentSchema.HVC_ConsigneeName] = consigneeName,
				[HVLVConsignmentSchema.HVC_ShipperName] = shipperName,

				[HVLVConsignmentSchema.HVC_ConsigneeAddress1] = consignmentAddress1,
				[HVLVConsignmentSchema.HVC_ConsigneeAddress2] = consignmentAddress2,
				[HVLVConsignmentSchema.HVC_ConsigneeCity] = consigneeCity,
				[HVLVConsignmentSchema.HVC_ConsigneeState] = consigneeState,
				[HVLVConsignmentSchema.HVC_ConsigneePostcode] = consigneePostcode,
				[HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode] = consigneeCountryCode,
				[HVLVConsignmentSchema.HVC_ConsigneePhone] = consigneePhone,
				[HVLVConsignmentSchema.HVC_ConsigneeEmail] = consigneeEmail,
				[HVLVConsignmentSchema.HVC_ConsigneeContact] = consigneeContact,
				[HVLVConsignmentSchema.HVC_ShipperReference] = consignmentShippersReference,
				[HVLVConsignmentSchema.HVC_ShipperAddress1] = shipperAddress1,
				[HVLVConsignmentSchema.HVC_ShipperAddress2] = shipperAddress2,
				[HVLVConsignmentSchema.HVC_ShipperCity] = shipperCity,
				[HVLVConsignmentSchema.HVC_ShipperState] = shipperState,
				[HVLVConsignmentSchema.HVC_ShipperPostcode] = shipperPostcode,
				[HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode] = shipperCountryCode,
				[HVLVConsignmentSchema.HVC_ShipperPhone] = shipperPhone,
				[HVLVConsignmentSchema.HVC_ShipperEmail] = shipperEmail,
				[HVLVConsignmentSchema.HVC_ShipperContact] = shipperContact,
				[HVLVConsignmentSchema.HVC_GoodsDescription] = consignmentGoodsDescription,
				[HVLVConsignmentSchema.HVC_RX_NKGoodsValueCurrency] = consignmentGoodsCurrency,
				[HVLVConsignmentSchema.HVC_GoodsValue] = consignmentGoodsValue,
				[HVLVConsignmentSchema.HVC_INCO] = INCOTerms,
				[HVLVConsignmentSchema.HVC_OH_LastMileCarrier] = lastMileCarrierPK,
				[HVLVConsignmentSchema.HVC_OA_DestinationDepot] = destinationDepotAddressPK,
				[HVLVConsignmentSchema.HVC_VolumeUQ] = volumeUnit,
				[HVLVConsignmentSchema.HVC_WeightUQ] = weightUnit
			};

			return hvlvConsignment;
		}

		Guid CreateHVLVItem(Guid hvlvConsignmentPK, Guid shipmentPK, string status = null)
		{
			var hvlvItem = new ActiveRowWrapper(HVLVItemSchema.Instance)
			{
				[HVLVItemSchema.HVI_ClusterKey] = clusterKey,
				[HVLVItemSchema.HVI_HVC_Consignment] = hvlvConsignmentPK,
				[HVLVItemSchema.HVI_ItemId] = itemId,
				[HVLVItemSchema.HVI_JS_LoadedOnShipment] = shipmentPK,
				[HVLVItemSchema.HVI_Status] = status ?? DispatchedToLastMileCarrier,
				[HVLVItemSchema.HVI_ManifestedWeight] = itemManWeight,
				[HVLVItemSchema.HVI_ActualWeight] = itemActWeight,
				[HVLVItemSchema.HVI_ManifestedVolume] = itemManVolume,
				[HVLVItemSchema.HVI_ActualVolume] = itemActVolume,
				[HVLVItemSchema.HVI_F3_NKPackType] = itemPackType,
				[HVLVItemSchema.HVI_ShipperReference] = itemShippersReference,
				[HVLVItemSchema.HVI_HVL_LoadList] = CreateHVLVOriginLoadList(),
				[HVLVItemSchema.HVI_GoodsDescription] = itemGoodsDescription,
				[HVLVItemSchema.HVI_HVO_OuterPackage] = CreateHVLVOuterPackage(),
			};

			hvlvItem.Save();
			return hvlvItem.PK;
		}

		Guid CreateHVLItemLine(Guid hvlvItemPK)
		{
			var hvlvItemLine = new ActiveRowWrapper(HVLVItemLineSchema.Instance)
			{
				[HVLVItemLineSchema.HVS_ClusterKey] = clusterKey,
				[HVLVItemLineSchema.HVS_HVI_HVLVItem] = hvlvItemPK,
				[HVLVItemLineSchema.HVS_RN_NKOriginCountryCode] = goodsOrigin,
				[HVLVItemLineSchema.HVS_OriginTariff] = originHSCode,
				[HVLVItemLineSchema.HVS_GoodsDescription] = itemLineGoodsDescription,
				[HVLVItemLineSchema.HVS_ProductCode] = productCode,
				[HVLVItemLineSchema.HVS_CustomsValue] = itemLineCustomsValue,
				[HVLVItemLineSchema.HVS_Quantity] = itemLineQuantity,
				[HVLVItemLineSchema.HVS_GrossWeight] = itemLineGrossWeight,
				[HVLVItemLineSchema.HVS_NetWeight] = itemLineNetWeight,
				[HVLVItemLineSchema.HVS_WeightUnit] = itemLineWeightUnit,
				[HVLVItemLineSchema.HVS_ItemURL] = itemLineUrl,
			};

			hvlvItemLine.Save();
			return hvlvItemLine.PK;
		}

		Guid CreateHVLVOriginLoadList()
		{
			var hvlvLoadList = new ActiveRowWrapper(HVLVOriginLoadListSchema.Instance)
			{
				[HVLVOriginLoadListSchema.HVL_UniqueReference] = loadListReference,
			};

			hvlvLoadList.Save();
			return hvlvLoadList.PK;
		}

		Guid CreateHVLVOuterPackage()
		{
			var hvlvOuterPackage = new ActiveRowWrapper(HVLVOuterPackageSchema.Instance)
			{
				[HVLVOuterPackageSchema.HVO_PackageReference] = outerPackageReference,
			};

			hvlvOuterPackage.Save();
			return hvlvOuterPackage.PK;
		}

		void CreateRefDataGrouping_AU()
		{
			var dataGroupingAU = new ActiveRowWrapper(RefDataGroupingSchema.Instance)
			{
				[RefDataGroupingSchema.ZZZ_DataGrouping] = "AU",
				[RefDataGroupingSchema.ZZZ_Description] = "Australia",
			};

			dataGroupingAU.Save();
		}

		void CreateRefDataGrouping_NZ()
		{
			var dataGroupingNZ = new ActiveRowWrapper(RefDataGroupingSchema.Instance)
			{
				[RefDataGroupingSchema.ZZZ_DataGrouping] = "NZ",
				[RefDataGroupingSchema.ZZZ_Description] = "New Zeland",
			};

			dataGroupingNZ.Save();
		}

		void CreateRefCusCodeType_AU()
		{
			var importRefCusCodeType = new ActiveRowWrapper(RefCusCodeTypeSchema.Instance)
			{
				[RefCusCodeTypeSchema.ZZK_CodeType] = "CSTA",
				[RefCusCodeTypeSchema.ZZK_Description] = "Customs Status",
				[RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping] = "AU",
			};

			var exportRefCusCodeType = new ActiveRowWrapper(RefCusCodeTypeSchema.Instance)
			{
				[RefCusCodeTypeSchema.ZZK_CodeType] = "CSTEX",
				[RefCusCodeTypeSchema.ZZK_Description] = "Customs Status",
				[RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping] = "AU",
			};

			importRefCusCodeType.Save();
			exportRefCusCodeType.Save();
		}

		void CreateRefCusCodeType_NZ()
		{
			var importRefCusCodeType = new ActiveRowWrapper(RefCusCodeTypeSchema.Instance)
			{
				[RefCusCodeTypeSchema.ZZK_CodeType] = "CSTA",
				[RefCusCodeTypeSchema.ZZK_Description] = "Customs Status",
				[RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping] = "NZ",
			};

			var exportRefCusCodeType = new ActiveRowWrapper(RefCusCodeTypeSchema.Instance)
			{
				[RefCusCodeTypeSchema.ZZK_CodeType] = "CSTEX",
				[RefCusCodeTypeSchema.ZZK_Description] = "Customs Status",
				[RefCusCodeTypeSchema.ZZK_ZZZ_NKDataGrouping] = "NZ",
			};

			importRefCusCodeType.Save();
			exportRefCusCodeType.Save();
		}

		void CreateRefCodeList_AU()
		{
			var importRefCodeListAU = new ActiveRowWrapper(RefCusCodeListSchema.Instance)
			{
				[RefCusCodeListSchema.ZZD_Code] = importCustomsClearanceStatusAU,
				[RefCusCodeListSchema.ZZD_Description] = importCustomsClearanceStatusDescriptionAU,
				[RefCusCodeListSchema.ZZD_ZZK_NKCodeType] = "CSTA",
				[RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping] = "AU"
			};

			var exportRefCodeListAU = new ActiveRowWrapper(RefCusCodeListSchema.Instance)
			{
				[RefCusCodeListSchema.ZZD_Code] = exportCustomsClearanceStatusAU,
				[RefCusCodeListSchema.ZZD_Description] = exportCustomsClearanceStatusDescriptionAU,
				[RefCusCodeListSchema.ZZD_ZZK_NKCodeType] = "CSTEX",
				[RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping] = "AU"
			};

			importRefCodeListAU.Save();
			exportRefCodeListAU.Save();
		}

		void CreateRefCodeList_NZ()
		{
			var importRefCodeListNZ = new ActiveRowWrapper(RefCusCodeListSchema.Instance)
			{
				[RefCusCodeListSchema.ZZD_Code] = importCustomsClearanceStatusNZ,
				[RefCusCodeListSchema.ZZD_Description] = importCustomsClearanceStatusDescriptionNZ,
				[RefCusCodeListSchema.ZZD_ZZK_NKCodeType] = "CSTA",
				[RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping] = "NZ"
			};

			var exportRefCodeListNZ = new ActiveRowWrapper(RefCusCodeListSchema.Instance)
			{
				[RefCusCodeListSchema.ZZD_Code] = exportCustomsClearanceStatusNZ,
				[RefCusCodeListSchema.ZZD_Description] = exportCustomsClearanceStatusDescriptionNZ,
				[RefCusCodeListSchema.ZZD_ZZK_NKCodeType] = "CSTA",
				[RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping] = "NZ"
			};

			importRefCodeListNZ.Save();
			exportRefCodeListNZ.Save();
		}

		void PrepTestReport_eTailShipmentItem()
		{
			var testDbHelper = new TestDbHelper(Db.Connection);
			var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

			var hvlvConsignment = CreateHVLVConsignment();
			hvlvConsignment.Save();

			var hvlvItem = CreateHVLVItem(hvlvConsignment.PK, shipmentPK);
			CreateHVLItemLine(hvlvItem);
		}

		void PrepTestReport_eTailShipmentItem_WithCustomsStatus_AU()
		{
			var testDbHelper = new TestDbHelper(Db.Connection);
			var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

			var hvlvConsignment = CreateHVLVConsignment();
			hvlvConsignment[HVLVConsignmentSchema.HVC_ImportCustomsClearanceStatus] = importCustomsClearanceStatusAU;
			hvlvConsignment[HVLVConsignmentSchema.HVC_ExportCustomsClearanceStatus] = exportCustomsClearanceStatusAU;
			hvlvConsignment.Save();

			CreateHVLVItem(hvlvConsignment.PK, shipmentPK);
			CreateRefDataGrouping_AU();
			CreateRefCusCodeType_AU();
			CreateRefCodeList_AU();
		}

		void PrepTestReport_eTailShipmentItem_WithCustomsStatus_NZ()
		{
			var testDbHelper = new TestDbHelper(Db.Connection);
			var shipmentPK = testDbHelper.InsertShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);

			var hvlvConsignment = CreateHVLVConsignment();
			hvlvConsignment[HVLVConsignmentSchema.HVC_ImportCustomsClearanceStatus] = importCustomsClearanceStatusNZ;
			hvlvConsignment[HVLVConsignmentSchema.HVC_ExportCustomsClearanceStatus] = exportCustomsClearanceStatusNZ;
			hvlvConsignment.Save();

			CreateHVLVItem(hvlvConsignment.PK, shipmentPK);
			CreateRefDataGrouping_NZ();
			CreateRefCusCodeType_NZ();
			CreateRefCodeList_NZ();
		}
	}
}

