using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	[CountrySpecificTest(Core.Constants.CountryCodes.Japan)]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestPropertyMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertMaxLength(bill.ABL_CountryOfOriginInfo, 2);
				AssertMaxLength(bill.ABL_GoodsLocationInfo, 5);
				AssertMaxLength(bill.ABL_GoodsDescriptionInfo, 350);
				AssertMaxLength(bill.ABL_BillNumberInfo, 35);
				AssertMaxLength(bill.ABL_LocationInformationInfo, 35);
				AssertMaxLength(bill.ABL_TariffInfo, 10);
				AssertMaxLength(bill.TemporaryLandingReasonInfo, 3);
				AssertMaxLength(bill.TemporaryLandingBondedTransportCodeInfo, 2);
				AssertMaxLength(bill.ABL_MarksAndNumbersInfo, 140);
				AssertMaxLength(bill.ABL_RemarksInfo, 140);
			});

			void AssertMaxLength(ZPropertyInfo info, int expectMaxLength)
			{
				AssertEquals(string.Format("Max length of {0} should be {1}.", info.HumanReadableName, expectMaxLength), expectMaxLength, info.MaxLength);
			}
		}

		public void TestDefaultPacksFromShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_ConversionFactor = 1m;
			refPacks.RP_CustomsPack = "BG";
			refPacks.RP_CommercialPack = "BAG";
			refPacks.RP_CustomsCountry = Core.Constants.CountryCodes.Japan;

			shipment.JS_F3_NKPackType = "BAG";
			shipment.JS_OuterPacks = 1;

			bill.ABL_JS_Shipment = shipment.PK;
			AssertEquals(1, bill.ABL_ManifestQty);
			AssertEquals("BG", bill.ABL_ManifestUQ);

			shipment.JS_F3_NKPackType = "123";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_JS_Shipment = shipment.PK;
			AssertEquals(1, bill2.ABL_ManifestQty);
			AssertEquals(ZString.Empty, bill2.ABL_ManifestUQ);
		}

		public void TestSpecialCargoCodeDescription()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);

			var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "Special Cargo Code");
			var coot1Code1 = universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "CAO", "Cargo Only", startDate, endDate);
			Factory.Save();

			bill.ABL_SpecialCargoCode = "CAO";
			AssertEquals("Cargo Only", bill.SpecialCargoCodeDescription);

			bill.ABL_SpecialCargoCode = string.Empty;
			AssertEquals(string.Empty, bill.SpecialCargoCodeDescription);
		}

		public void TestSpecialCargoCode()
		{
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "AOG", "EPG");
			CombineAssertions(() =>
			{
				bill.ABL_SpecialCargoCode = "AOG";
				AssertEquals("AOG Code", "AOG", bill.SpecialCargoCode.ZZD_Code);
				AssertEquals("AOG Description", "AOG Description", bill.SpecialCargoCode.ZZD_Description);

				bill.ABL_SpecialCargoCode = "EPG";
				AssertEquals("EPG Code", "EPG", bill.SpecialCargoCode.ZZD_Code);
				AssertEquals("EPG Description", "EPG Description", bill.SpecialCargoCode.ZZD_Description);
			});
		}

		public void TestCalculateCustomsWeightInGrossWeightUnit()
		{
			bill.ABL_GrossWeight = 100;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;

			bill.CustomsWeight = 200.999m;
			CombineAssertions("Pounds", () =>
			{
				AssertEquals("GrossWeight", 200.999m, bill.ABL_GrossWeight);
				AssertEquals("GrossWeightUQ", Core.Constants.Weight.Pounds, bill.ABL_GrossWeightUQ);
				AssertEquals("CustomsWeightUQ", CustomsWeightUnitList.Codes.Pound, bill.CustomsWeightUQ);
			});

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Ounces;
			bill.CustomsWeight = 1;
			CombineAssertions("Ounces", () =>
			{
				AssertEquals("GrossWeight", 35.274m, bill.ABL_GrossWeight);
				AssertEquals("GrossWeightUQ", Core.Constants.Weight.Ounces, bill.ABL_GrossWeightUQ);
				AssertEquals("CustomsWeightUQ", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsWeightUQ);
			});
		}

		public void TestTemporaryLandingStatusCaption()
		{
			var temporaryLandingStatusStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingStatusInfo);
			var temporaryLandingStatusDescriptionStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingStatusDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("TemporaryLandingStatus Caption", "Temporary Landing Status", temporaryLandingStatusStringData.Caption);
				AssertEquals("TemporaryLandingStatus ShortCaption", "TL Status", temporaryLandingStatusStringData.ShortCaption);
				AssertEquals("TemporaryLandingStatusDescription Caption", "Temporary Landing Status Description", temporaryLandingStatusDescriptionStringData.Caption);
				AssertEquals("TemporaryLandingStatusDescription ShortCaption", "TL Status Desc.", temporaryLandingStatusDescriptionStringData.ShortCaption);
			});
		}

		public void TestABL_RL_NKPortOfDischargeCaption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_RL_NKPortOfDischargeInfo);
			AssertEquals("ABL_CustomsDischargePort Caption", "Discharge Port", resourceStringData.Caption);
		}

		public void TestGrossWeightUQ()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("Pounds", CustomsWeightUnitList.Codes.Pound, bill.CustomsWeightUQ);

				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Tonnes;
				AssertEquals("Tonnes", CustomsWeightUnitList.Codes.Tonnes, bill.CustomsWeightUQ);

				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Kilograms", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsWeightUQ);

				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Milligrams;
				AssertEquals("Kilograms", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsWeightUQ);
			});
		}

		public void TestABL_Calc_WeightInKG()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_GrossWeight = 1000m;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("ABL_GrossWeightUQ is Kilograms", 1000m, bill.CustomsWeight);

				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Tonnes;
				AssertEquals("ABL_GrossWeightUQ is Kilograms", 1000m, bill.CustomsWeight);

				bill.ABL_GrossWeight = 1m;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("ABL_GrossWeightUQ is Pounds", 1m, bill.CustomsWeight);

				bill.ABL_GrossWeight = 1000m;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.ShortTons;
				AssertEquals("ABL_GrossWeightUQ is ShortTons", 907184.996m, bill.CustomsWeight);

				bill.ABL_GrossWeight = 10m;
				bill.ABL_GrossWeightUQ = Core.Constants.Weight.Milligrams;
				AssertEquals("ABL_GrossWeightUQ is Milligrams", 0m, bill.CustomsWeight);
			});
		}

		public void TestGetNewValidationForMasterChild()
		{
			bill.ABL_BolType = "BOL";
			AssertType<AsycudaBillValidationForMasterChild>(bill.Validation);
		}

		public void TestAsycudaBillValidationForRegularBill()
		{
			AssertType<AsycudaBillValidationForRegularBill>(bill.Validation);
		}

		public void TestAsycudaBillLookupsType()
		{
			var lookups = bill.Lookups;
			AssertEquals(lookups.GetType(), typeof(AsycudaBillLookups));
		}

		public void TestRegNoTypes()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			AssertArrayEqualsByElements([], bill.ShipperRegNoTypes());
			AssertArrayEqualsByElements([], bill.ConsigneeRegNoTypes());
			AssertArrayEqualsByElements([], bill.NotifyPartyRegNoTypes());

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			AssertArrayEqualsByElements([..CustomsRegNumTypeValidation.JPDefaultList], bill.ConsigneeRegNoTypes());
			AssertArrayEqualsByElements([.. CustomsRegNumTypeValidation.ForeignDefaultList], bill.ShipperRegNoTypes());
			AssertArrayEqualsByElements([], bill.NotifyPartyRegNoTypes());

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			AssertArrayEqualsByElements([..CustomsRegNumTypeValidation.NvcShipperList], bill.ShipperRegNoTypes());
			AssertArrayEqualsByElements([.. CustomsRegNumTypeValidation.JPDefaultList], bill.ConsigneeRegNoTypes());
			AssertArrayEqualsByElements([..CustomsRegNumTypeValidation.JPDefaultList], bill.NotifyPartyRegNoTypes());
		}

		public void TestDefaultConsigneeRegNoAndTypeAfterConsigneeIsSelected()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.LPC, "1234567890123", Core.Constants.CountryCodes.Japan);
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "12345678", Core.Constants.CountryCodes.Japan);
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "12345678", Core.Constants.CountryCodes.Japan);
			org2.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org3 = Factory.NewWithValidTestData<OrgAddress>();
			org3.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org4 = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			bill.ABL_OA_Consignee = org1.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill.ABL_ConsigneeRegNoType);
			AssertEquals("1234567890123", bill.ABL_ConsigneeRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ConsigneeRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be readonly when the chosen orgnization has a corresponding code.", bill.ABL_ConsigneeRegNoInfo.ReadOnly);

			bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals("123456789012", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = org2.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.CIE, bill.ABL_ConsigneeRegNoType);
			AssertEquals("12345678", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = org3.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.JAS, bill.ABL_ConsigneeRegNoType);
			AssertEquals("123456789012", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = org4.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ConsigneeRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be not readonly when the chosen orgnization does not have a corresponding code.", !bill.ABL_ConsigneeRegNoInfo.ReadOnly);
		}

		public void TestNVC01DefaultRegNoAndTypeAfterOrgIsSelected()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			var org1 = Factory.NewWithValidTestData<OrgAddress>();
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.LPC, "1234567890123", Core.Constants.CountryCodes.Japan);
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "12345678", Core.Constants.CountryCodes.Japan);
			org1.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org2 = Factory.NewWithValidTestData<OrgAddress>();
			org2.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "12345678", Core.Constants.CountryCodes.Japan);
			org2.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org3 = Factory.NewWithValidTestData<OrgAddress>();
			org3.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.JAS, "123456789012", Core.Constants.CountryCodes.Japan);

			var org4 = Factory.NewWithValidTestData<OrgAddress>();

			var org5 = Factory.NewWithValidTestData<OrgAddress>();
			org5.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.FSB, "F12345678901", Core.Constants.CountryCodes.Japan);

			Factory.Save();

			bill.ABL_OA_Consignee = org1.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill.ABL_ConsigneeRegNoType);
			AssertEquals("1234567890123", bill.ABL_ConsigneeRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ConsigneeRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be readonly when the chosen orgnization has a corresponding code.", bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals("123456789012", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Shipper = org1.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill.ABL_ShipperRegNoType);
			AssertEquals("1234567890123", bill.ABL_ShipperRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ShipperRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be readonly when the chosen orgnization has a corresponding code.", bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_ShipperRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals("123456789012", bill.ABL_ShipperRegNo);

			bill.ABL_OA_NotifyParty = org1.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.LPC, bill.ABL_NotifyPartyRegNoType);
			AssertEquals("1234567890123", bill.ABL_NotifyPartyRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_NotifyPartyRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be readonly when the chosen orgnization has a corresponding code.", bill.ABL_NotifyPartyRegNoInfo.ReadOnly);
			bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals("123456789012", bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_Shipper = org2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
			bill.ABL_OA_Consignee = org2.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.CIE, bill.ABL_ConsigneeRegNoType);
			AssertEquals("12345678", bill.ABL_ConsigneeRegNo);
			bill.ABL_OA_NotifyParty = org2.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.CIE, bill.ABL_NotifyPartyRegNoType);
			AssertEquals("12345678", bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_Shipper = org3.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
			bill.ABL_OA_Consignee = org3.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.JAS, bill.ABL_ConsigneeRegNoType);
			AssertEquals("123456789012", bill.ABL_ConsigneeRegNo);
			bill.ABL_OA_NotifyParty = org3.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.JAS, bill.ABL_NotifyPartyRegNoType);
			AssertEquals("123456789012", bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_Shipper = org4.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ShipperRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be not readonly when the chosen orgnization does not have a corresponding code.", !bill.ABL_ShipperRegNoInfo.ReadOnly);
			bill.ABL_OA_Consignee = org4.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_ConsigneeRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be not readonly when the chosen orgnization does not have a corresponding code.", !bill.ABL_ConsigneeRegNoInfo.ReadOnly);
			bill.ABL_OA_NotifyParty = org4.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
			Assert("Customs Reg No Type should not be readonly.", !bill.ABL_NotifyPartyRegNoTypeInfo.ReadOnly);
			Assert("Customs Reg No should be not readonly when the chosen orgnization does not have a corresponding code.", !bill.ABL_NotifyPartyRegNoInfo.ReadOnly);

			bill.ABL_OA_Shipper = org5.PK;
			AssertEquals(OrgCusCode.JapanCodeTypes.FSB, bill.ABL_ShipperRegNoType);
			AssertEquals("F12345678901", bill.ABL_ShipperRegNo);
		}

		public void TestUpdateHeaderMessageStatus()
		{
			var bill2 = header.Bills.AddNew();
			bill.ABL_MessageStatus = JPMessageStatusList.Codes.Rejected;
			bill2.ABL_MessageStatus = JPMessageStatusList.Codes.Acknowledged;

			Assert(header.NeedToUpdateMessageStatus);
			Factory.Save();

			AssertEquals(JPMessageStatusList.Codes.MultipleStatus, header.AMA_MessageStatus);
			Assert(!header.NeedToUpdateMessageStatus);

			bill.ABL_MessageStatus = JPMessageStatusList.Codes.Acknowledged;
			Factory.Save();

			AssertEquals(JPMessageStatusList.Codes.Acknowledged, header.AMA_MessageStatus);
		}

		public void TestTemporaryLandingReason()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingReasonInfo);
				AssertEquals("TemporaryLandingReason Caption", "Reason", resourceStringData.Caption);

				bill.TemporaryLandingReason = "POS";
				AssertEquals("TemporaryLandingInfo.CSI_Code", "POS", bill.TemporaryLandingInfo.CSI_Code);

				bill.TemporaryLandingInfo.CSI_Code = "REV";
				AssertEquals("bill.TemporaryLandingReason", "REV", bill.TemporaryLandingReason);
			});
		}

		public void TestTemporaryLandingStartDate()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingStartDateInfo);
				AssertEquals("TemporaryLandingStartDate Caption", "Start Date", resourceStringData.Caption);

				bill.TemporaryLandingStartDate = new ZDate(2024, 7, 27);
				AssertEquals("TemporaryLandingInfo.CSI_DateOfIssue", new ZDate(2024, 7, 27), bill.TemporaryLandingInfo.CSI_DateOfIssue.Date);

				bill.TemporaryLandingInfo.CSI_DateOfIssue = new ZDateTime(2024, 7, 28, 12, 30, 25);
				AssertEquals("bill.TemporaryLandingStartDate", new ZDate(2024, 7, 28), bill.TemporaryLandingStartDate);
			});
		}

		public void TestTemporaryLandingEndDate()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingEndDateInfo);
				AssertEquals("TemporaryLandingEndDate Caption", "End Date", resourceStringData.Caption);

				bill.TemporaryLandingEndDate = new ZDate(2024, 7, 27);
				AssertEquals("TemporaryLandingInfo.CSI_DateOfExpiry", new ZDate(2024, 7, 27), bill.TemporaryLandingInfo.CSI_DateOfExpiry.Date);

				bill.TemporaryLandingInfo.CSI_DateOfExpiry = new ZDateTime(2024, 7, 28, 12, 30, 25);
				AssertEquals("bill.TemporaryLandingEndDate", new ZDate(2024, 7, 28), bill.TemporaryLandingEndDate);
			});
		}

		public void TestTemporaryLandingPeriodDays()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingPeriodDaysInfo);
				AssertEquals("TemporaryLandingPeriodDays Caption", "Period (Days)", resourceStringData.Caption);

				bill.TemporaryLandingPeriodDays = 5;
				AssertEquals("TemporaryLandingInfo.CSI_ItemNumber", 5, bill.TemporaryLandingInfo.CSI_ItemNumber);

				bill.TemporaryLandingInfo.CSI_ItemNumber = 9;
				AssertEquals("bill.TemporaryLandingPeriodDays", 9, bill.TemporaryLandingPeriodDays);
			});
		}

		public void TestTemporaryLandingBondedTransportCode()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TemporaryLandingBondedTransportCodeInfo);
				AssertEquals("TemporaryLandingBondedTransportCode Caption", "Bonded Transport Code", resourceStringData.Caption);

				bill.TemporaryLandingBondedTransportCode = "6";
				AssertEquals("TemporaryLandingInfo.CSI_ReferenceNumber", "6", bill.TemporaryLandingInfo.CSI_ReferenceNumber);

				bill.TemporaryLandingInfo.CSI_ReferenceNumber = "16";
				AssertEquals("bill.TemporaryLandingBondedTransportCode", "16", bill.TemporaryLandingBondedTransportCode);
			});
		}

		public void TestABL_BillNumber()
		{
			var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_BillNumberInfo, string.Empty);
			AssertEquals("Caption", "Bill Number", resData.Caption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_BillNumberInfo, AsycudaManifestHeader.CaptionKeyAir + AsycudaManifestHeader.CaptionKeyImp);
			AssertEquals("Caption", "HAWB", resData.Caption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_BillNumberInfo, AsycudaManifestHeader.CaptionKeySea + AsycudaManifestHeader.CaptionKeyImp);
			AssertEquals("Caption", "House B/L", resData.Caption);
		}

		public void TestABL_GoodsLocation()
		{
			var resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_GoodsLocationInfo, string.Empty);
			AssertEquals("Caption", "Bonded Location", resData.Caption);

			resData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(bill.ABL_GoodsLocationInfo, AsycudaManifestHeader.CaptionKeyAir + AsycudaManifestHeader.CaptionKeyImp);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Into Bonded Warehouse", resData.Caption);
				AssertEquals("MediumCaption", "Into Warehouse", resData.MediumCaption);
				AssertEquals("ShortCaption", "Into Whs.", resData.ShortCaption);
			});
		}

		public void TestABL_Tariff()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Import, "123456789", "987654321");
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_Tariff = "123456789";
			var tariffInfo = bill.ABL_TariffInfo;
			var tariffResourceStringData = DataBoundResourceStrings.GetDataForProperty(tariffInfo);
			var tariffDescriptionResourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.TariffDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_Tariff Caption", "Statistical/HS Code", tariffResourceStringData.Caption);
				AssertEquals("ABL_Tariff ShortCaption", "Stat./HS", tariffResourceStringData.ShortCaption);
				AssertEquals("TariffDescription Caption", "Statistical/HS Description", tariffDescriptionResourceStringData.Caption);
				AssertEquals("TariffDescription Short Caption", "Stat./HS Desc.", tariffDescriptionResourceStringData.ShortCaption);
				AssertHasCustomAttribute<ListAttribute>(typeof(AsycudaBill), tariffInfo.Name, true, attribute => attribute.ListDataSourceMember == "Lookups.TariffCollection");
				AssertEquals("ABL_Tariff", "1234.56.789", bill.ABL_Tariff);
				AssertEquals("TariffDescription", "123456789 Description", bill.TariffDescription);

				bill.ABL_Tariff = "9876.54.32 1";
				AssertEquals("ABL_Tariff", "9876.54.321", bill.ABL_Tariff);
				AssertEquals("TariffDescription", "987654321 Description", bill.TariffDescription);
			});
		}

		public void TestShouldShowRepresentativeHSCode()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Export, "010121000", "631090000", "0101", "631090");
			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			bill.ABL_Tariff = "010121000";
			Assert("010121000", bill.ShouldShowRepresentativeHSCode);

			bill.ABL_Tariff = "631090000";
			Assert("631090000", bill.ShouldShowRepresentativeHSCode);

			bill.ABL_Tariff = "0101";
			Assert("0101", !bill.ShouldShowRepresentativeHSCode);

			bill.ABL_Tariff = "631090";
			Assert("631090", !bill.ShouldShowRepresentativeHSCode);

			bill.ABL_TariffInfo.ClearValue();
			Assert("Empty", !bill.ShouldShowRepresentativeHSCode);
		}

		public void TestABL_RepresentativeHSCode()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Export, "01", "01012", "631090000", "0101", "631090");
			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			bill.ABL_Tariff = "01";
			AssertEquals("Should be 01", "01", bill.ABL_RepresentativeHSCode);

			bill.ABL_Tariff = "01012";
			AssertEquals("Should be 0101", "0101", bill.ABL_RepresentativeHSCode);

			bill.ABL_Tariff = "631090000";
			AssertEquals("Should be 631090", "6310.90", bill.ABL_RepresentativeHSCode);

			bill.ABL_Tariff = "0101";
			AssertNullOrEmpty("Should be empty", bill.ABL_RepresentativeHSCode);

			bill.ABL_Tariff = "631090";
			AssertNullOrEmpty("Should be empty", bill.ABL_RepresentativeHSCode);

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.RepresentativeHSCodeInfo);
			CombineAssertions("Captions", () =>
			{
				AssertEquals("ABL_RepresentativeHSCode Caption", "Representative HS Code", resourceStringData.Caption);
				AssertEquals("ABL_RepresentativeHSCode ShortCaption", "Rep. HS ", resourceStringData.ShortCaption);
				AssertEquals("ABL_RepresentativeHSCode FullDescription", "Representative HS Code (HSN) to be sent in NVC messages", resourceStringData.FullDescription);
			});
		}

		public void TestABL_NetWeight()
		{
			var netWeightResourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_NetWeightInfo);
			var netWeightUQResourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_NetWeightUQInfo);
			CombineAssertions(() =>
			{
				AssertEquals("ABL_NetWeight Caption", "Net Weight", netWeightResourceStringData.Caption);
				AssertEquals("ABL_NetWeightUQ Caption", "Net Weight UQ", netWeightUQResourceStringData.Caption);
				AssertEquals("ABL_NetWeightUQ Short Caption", "UQ", netWeightUQResourceStringData.ShortCaption);
			});
		}

		public void TestCalculateCustomsNetWeightInNetWeightUnit()
		{
			bill.ABL_NetWeight = 100;
			bill.ABL_NetWeightUQ = Core.Constants.Weight.Pounds;

			bill.CustomsNetWeight = 200.999m;
			CombineAssertions("Pounds", () =>
			{
				AssertEquals("NetWeight", 200.999m, bill.ABL_NetWeight);
				AssertEquals("NetWeightUQ", Core.Constants.Weight.Pounds, bill.ABL_NetWeightUQ);
				AssertEquals("CustomsNetWeightUQ", CustomsWeightUnitList.Codes.Pound, bill.CustomsNetWeightUQ);
			});

			bill.ABL_NetWeightUQ = Core.Constants.Weight.Ounces;
			bill.CustomsNetWeight = 1;
			CombineAssertions("Ounces", () =>
			{
				AssertEquals("NetWeight", 35.274m, bill.ABL_NetWeight);
				AssertEquals("NetWeightUQ", Core.Constants.Weight.Ounces, bill.ABL_NetWeightUQ);
				AssertEquals("CustomsNetWeightUQ", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsNetWeightUQ);
			});
		}

		public void TestNetWeightUQ()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_NetWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("Pounds", CustomsWeightUnitList.Codes.Pound, bill.CustomsNetWeightUQ);

				bill.ABL_NetWeightUQ = Core.Constants.Weight.Tonnes;
				AssertEquals("Tonnes", CustomsWeightUnitList.Codes.Tonnes, bill.CustomsNetWeightUQ);

				bill.ABL_NetWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Kilograms", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsNetWeightUQ);

				bill.ABL_NetWeightUQ = Core.Constants.Weight.Milligrams;
				AssertEquals("Kilograms", CustomsWeightUnitList.Codes.Kilograms, bill.CustomsNetWeightUQ);
			});
		}

		public void TestABL_Calc_NetWeightInKG()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_NetWeight = 1000m;
				bill.ABL_NetWeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("ABL_NetWeightUQ is Kilograms", 1000m, bill.CustomsNetWeight);

				bill.ABL_NetWeightUQ = Core.Constants.Weight.Tonnes;
				AssertEquals("ABL_NetWeightUQ is Tonnes", 1000m, bill.CustomsNetWeight);

				bill.ABL_NetWeight = 1m;
				bill.ABL_NetWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("ABL_NetWeightUQ is Pounds", 1m, bill.CustomsNetWeight);

				bill.ABL_NetWeight = 1000m;
				bill.ABL_NetWeightUQ = Core.Constants.Weight.ShortTons;
				AssertEquals("ABL_NetWeightUQ is ShortTons", 907184.996m, bill.CustomsNetWeight);

				bill.ABL_NetWeight = 10m;
				bill.ABL_NetWeightUQ = Core.Constants.Weight.Milligrams;
				AssertEquals("ABL_NetWeightUQ is Milligrams", 0m, bill.CustomsNetWeight);
			});
		}

		public void TestCalculateCustomsVolumeInVolumeUnit()
		{
			bill.ABL_Volume = 100;
			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicFeet;

			bill.CustomsVolume = 200.999m;
			CombineAssertions("Pounds", () =>
			{
				AssertEquals("Volume", 200.999m, bill.ABL_Volume);
				AssertEquals("VolumeUQ", Core.Constants.Volume.CubicFeet, bill.ABL_VolumeUQ);
				AssertEquals("CustomsVolumeUQ", CustomsVolumeUnitList.Codes.CubicFeet, bill.CustomsVolumeUQ);
			});

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.CustomsVolume = 1;
			CombineAssertions("Ounces", () =>
			{
				AssertEquals("Volume", 1000m, bill.ABL_Volume);
				AssertEquals("VolumeUQ", Core.Constants.Volume.Litre, bill.ABL_VolumeUQ);
				AssertEquals("CustomsVolumeUQ", CustomsVolumeUnitList.Codes.CubicMeters, bill.CustomsVolumeUQ);
			});
		}

		public void TestVolumeUQ()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_VolumeUQ = VolumeList.Codes.BoardFoot;
				AssertEquals("BoardFeet", CustomsVolumeUnitList.Codes.BoardFeet, bill.CustomsVolumeUQ);

				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
				AssertEquals("CubicMeters", CustomsVolumeUnitList.Codes.CubicMeters, bill.CustomsVolumeUQ);

				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicFeet;
				AssertEquals("CubicFeet", CustomsVolumeUnitList.Codes.CubicFeet, bill.CustomsVolumeUQ);

				bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
				AssertEquals("CubicMeters", CustomsVolumeUnitList.Codes.CubicMeters, bill.CustomsVolumeUQ);
			});
		}

		public void TestABL_Calc_VolumeInM3()
		{
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			CombineAssertions(() =>
			{
				bill.ABL_Volume = 1000m;
				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
				AssertEquals("ABL_VolumeUQ is CubicMetres", 1000m, bill.CustomsVolume);

				bill.ABL_VolumeUQ = VolumeList.Codes.BoardFoot;
				AssertEquals("ABL_VolumeUQ is BoardFoot", 1000m, bill.CustomsVolume);

				bill.ABL_Volume = 1m;
				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicFeet;
				AssertEquals("ABL_VolumeUQ is CubicFeet", 1m, bill.CustomsVolume);

				bill.ABL_Volume = 1000m;
				bill.ABL_VolumeUQ = Core.Constants.Volume.MegaLitre;
				AssertEquals("ABL_VolumeUQ is Litre", 1000000m, bill.CustomsVolume);

				bill.ABL_Volume = 10m;
				bill.ABL_VolumeUQ = Core.Constants.Volume.CubicInches;
				AssertEquals("ABL_VolumeUQ is CubicInches", 0m, bill.CustomsVolume);
			});
		}

		public void TestABL_FreightValueDecimal()
		{
			var info = bill.ABL_FreightValueInfo;
			bill.ABL_RX_NKFreightValueCurrency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			bill.ABL_RX_NKFreightValueCurrency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}

		public void TestABL_TransportValueDecimal()
		{
			var info = bill.ABL_TransportValueInfo;
			bill.ABL_RX_NKTransportValueCurrency = "TWD";
			AssertHasDecimalPlacesAttribute(info, 2);

			bill.ABL_RX_NKTransportValueCurrency = "JPY";
			AssertHasDecimalPlacesAttribute(info, 0);
		}

		public void TestUpdateHeaderRegistrationStatus()
		{
			var bill2 = header.Bills.AddNew();
			bill.ABL_BillStatus = JPCustomsStatusList.Codes.Mismatch;
			bill2.ABL_BillStatus = JPCustomsStatusList.Codes.REG;

			Assert(header.NeedToUpdateRegistrationStatus);
			Factory.Save();
			AssertEquals(JPCustomsStatusList.Codes.Mismatch, header.RegistrationStatus);
			Assert(!header.NeedToUpdateRegistrationStatus);

			bill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
			Factory.Save();
			AssertEquals(JPCustomsStatusList.Codes.AMD, header.RegistrationStatus);

			bill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
			Factory.Save();
			AssertEquals(JPCustomsStatusList.Codes.REG, header.RegistrationStatus);

			bill2.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;
			Factory.Save();
			AssertEquals(JPCustomsStatusList.Codes.MultipleStatus, header.RegistrationStatus);
		}

		public void TestDefaultTemporaryLandingPeriodDays()
		{
			bill.TemporaryLandingPeriodDaysInfo.ClearValue();
			bill.TemporaryLandingStartDate = new ZDate(2024, 7, 21);
			bill.TemporaryLandingEndDate = new ZDate(2024, 7, 21);
			AssertEquals("Default value should be 1", 1, bill.TemporaryLandingPeriodDays);

			bill.TemporaryLandingStartDate = new ZDate(2024, 7, 18);
			AssertEquals("No changes", 1, bill.TemporaryLandingPeriodDays);

			bill.TemporaryLandingPeriodDaysInfo.ClearValue();
			bill.TemporaryLandingStartDate = new ZDate(2024, 4, 14);
			AssertEquals("Default value should be 99", 99, bill.TemporaryLandingPeriodDays);

			bill.TemporaryLandingPeriodDaysInfo.ClearValue();
			bill.TemporaryLandingEndDate = new ZDate(2024, 7, 22);
			AssertEquals("Do not default value because the days more than 99", 0, bill.TemporaryLandingPeriodDays);
		}

		public void TestFinalDestinationIATACode()
		{
			Factory.CreateUNLOCOData();

			bill.ABL_RL_NKFinalDestination = "JP001";
			AssertEquals(string.Empty, bill.FinalDestinationIATACode);

			bill.ABL_RL_NKFinalDestination = "JP002";
			AssertEquals("TKU", bill.FinalDestinationIATACode);

			bill.ABL_RL_NKFinalDestination = string.Empty;
			bill.FinalDestinationIATACode = "TKU";
			AssertEquals("JP002", bill.ABL_RL_NKFinalDestination);

			bill.FinalDestinationIATACode = "TK";
			AssertEquals("TK", bill.FinalDestinationIATACode);
			AssertEquals("JP002", bill.ABL_RL_NKFinalDestination);

			bill.FinalDestinationIATACode = string.Empty;
			AssertEquals("JP002", bill.ABL_RL_NKFinalDestination);
		}

		public void TestIsImport()
		{
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			Assert(bill.IsImport);

			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			Assert(!bill.IsImport);
		}

		public void TestIsExport()
		{
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			Assert(!bill.IsExport);

			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			Assert(bill.IsExport);
		}

		public void TestOtherLawsandRegulations()
		{
			var otherRow = bill.OtherLawsandRegulations.AddNew();
			AssertEquals("OLC", otherRow.CFR_Type);
		}

		public void TestGetTariffAttributesByKey()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "JP";
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Japan, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "AN", tariff);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws, "BN", tariff);
			helper.CreateNewOrGetExistingTariffAttribute(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, "CN", tariff);
			Factory.Save();

			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			bill.ABL_Tariff = "123456789";
			AssertContainsExactElementsInAnyOrder([], bill.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanOtherLaws));
			AssertContainsExactElementsInAnyOrder([], bill.GetTariffAttributesByKey(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		public void TestABL_GoodsLocation_Caption()
		{
			var goodsLocationResourceStringData = DataBoundResourceStrings.GetDataForProperty(bill.ABL_GoodsLocationInfo);
			AssertEquals("ABL_GoodsLocation Caption", "Bonded Location", goodsLocationResourceStringData.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			(header, bill) = GetBill(Factory);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;

		static (AsycudaManifestHeader Header, AsycudaBill Bill) GetBill(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return (header, bill);
		}
	}
}
