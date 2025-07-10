using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	partial class JPAFRBillsDataObjectWriterTest
	{
		public void TestJPAFRBillsMappings_UNDG()
		{
			CombineAssertions("NormalUNDG", () =>
			{
				var undg1 = DGSubstanceTestHelper.Create("1234", "a", "IMO", (d) => d.DG_Class = "1");
				var undg2 = DGSubstanceTestHelper.Create("5678", "b", "IMO", (d) => d.DG_Class = "2");

				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var notifyParty1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

				var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");

				var bill1 = SetupNotificationForwardingParties(SetupInBondRelatedData(SetupJPAFRBills(header.Bills.AddNew(), "HB3243")), "NFP1", "NFP3", "NFP2");
				SetupJPAFRBillsVOCCFields(bill1, true, "COC1", "GTN1");

				bill1.JPB_DG = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1100", "", "IMO").First().PK;
				bill1.UNDGs.AddNew().DI_DG = undg1.PK;
				bill1.UNDGs.AddNew().DI_DG = undg2.PK;

				Factory.SaveForTesting();

				var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));

				var headerData = writer.GetDataObject(header);
				AssertEquals("headerData.SubShipmentCollection.Count", 1, headerData.SubShipmentCollection.Count);

				var billData = headerData.SubShipmentCollection[0];
				AssertEquals("UNDGRecord", 3, billData.PackingLineCollection[0].UNDGCollection.Count);

				var undgResult = billData.PackingLineCollection[0].UNDGCollection.Select(c => $"{c.UNDGCode}|{c.IMOClass}");

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"1100|3", "1234a|1", "5678b|2"
				}, undgResult);
			});

			CombineAssertions("EmptyUNDG", () =>
			{
				var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var notifyParty1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

				var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
				var bill1 = SetupNotificationForwardingParties(SetupInBondRelatedData(SetupJPAFRBills(header.Bills.AddNew(), "HB3243")), "NFP1", "NFP3", "NFP2");
				SetupJPAFRBillsVOCCFields(bill1, true, "COC1", "GTN1");

				Factory.SaveForTesting();

				var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
				var headerData = writer.GetDataObject(header);
				AssertEquals("headerData.SubShipmentCollection.Count", 1, headerData.SubShipmentCollection.Count);

				var billData = headerData.SubShipmentCollection[0];
				AssertNull("No_UNDGRecord", billData.PackingLineCollection[0].UNDGCollection);
			});
		}

		public void TestJPAFRBillsMappings_NVOCC()
		{
			CreateJapanPackageTypes();
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var notifyParty1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			var bill1 = SetupNotificationForwardingParties(SetupInBondRelatedData(SetupJPAFRBills(header.Bills.AddNew(), "HB3243")), "NFP1", "NFP3", "NFP2");
			SetupJPAFRBillsVOCCFields(bill1, true, "COC1", "GTN1");
			bill1.JPB_SpecialCargoCode = "PLQ";
			bill1.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			bill1.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			bill1.NotifyParty1.E2_OA_Address = notifyParty1.MainAddress.PK;
			bill1.NotifyParty2.E2_AddressOverride = ZBool.True;
			bill1.NotifyParty2.E2_CompanyName = "BOB THE BUILDER";
			bill1.NotifyParty2.E2_Address1 = "ADDRESS 1";
			bill1.NotifyParty2.E2_Address2 = "ADDRESS 2";
			bill1.NotifyParty2.E2_City = "CITY BOB";
			bill1.NotifyParty2.E2_State = "STATE BOB";
			bill1.NotifyParty2.E2_Postcode = "39234";
			bill1.NotifyParty2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
			bill1.JPB_DG = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1100", "", "IMO").First().PK;

			var container1 = bill1.Containers.AddNew();
			container1.JPC_ContainerNum = "CONT234";

			var container2 = bill1.Containers.AddNew();
			container2.JPC_ContainerNum = "CONT968";

			var bill2 = header.Bills.AddNew();
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var sendingBill2 = new MessageSendingObject(bill2, ActionCode.AmendingAdd, new MessageSendingAction(header, ActionCode.AmendingAdd)) { JPM_Send = ZBool.True };
			sendingBill2.UpdateAction(ActionCode.AmendingDelete);
			sendingBill2.JPM_DeleteReasonCode = "5";
			sendingBill2.JPM_DeleteReasonText = "this is test";

			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertEquals("headerData.SubShipmentCollection.Count", 2, headerData.SubShipmentCollection.Count);
			var billData = headerData.SubShipmentCollection[0];
			AssertAFRBillContents(billData, "HB3243", false);
			AssertAFRInBondDetailsContents(billData, 1500.50m, CodeDescriptionPairForTesting.New(USD.RX_Code, USD.RX_Desc), TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers, 3, new ZDateTime(2013, 10, 4), new ZDateTime(2013, 10, 7), TransportModeList.Codes.Others, CodeDescriptionPairForTesting.New("SD342", ""));
			AssertAFROtherRelevantLawContents(billData, "O1", "O3", "O5", "O4", "O2");
			AssertAFRNotficationForwardingPartyContents(billData, "NFP1", "NFP3", "NFP2");
			var organizationAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 4, organizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("Consignor", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsignorDocumentaryAddress)), nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertOrganizationBO_INTHEMSYD("NotifyParty1", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty));
			AssertAddress("NotifyParty2", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty2)), nameof(DocAddressType.NotifyParty2), null, "BOB THE BUILDER", ZBool.True,
				"ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas,
				"", "", "", "", "");
			AssertNotNull("billData.ContainerCollection", billData.ContainerCollection);
			AssertEquals("billData.ContainerCollection.Count", 2, billData.ContainerCollection.Count);
			AssertNotNull("CONT234", billData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT234"));
			AssertNotNull("CONT968", billData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT968"));
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.SpecialCargoCode)", "PLQ", billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.SpecialCargoCode));

			var undgResult = billData.PackingLineCollection[0].UNDGCollection[0];
			AssertEquals("UNDGCode", "1100", undgResult.UNDGCode);
			AssertEquals("UNDGIMOCode", "3", undgResult.IMOClass);

			var billData2 = headerData.SubShipmentCollection[1];
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.DeleteReasonCode)", "5", billData2.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonCode));
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.DeleteReasonText)", "this is test", billData2.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonText));
		}

		public void TestJPAFRBillsMappings_VOCC()
		{
			CreateJapanPackageTypes();
			var consignor = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var consignee = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var notifyParty1 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			var header = SetupJPAFRHeader(Factory.New<JPAFRHeader>(), "MB324242");
			header.JPH_IsShippingLineEntry = true;
			var bill1 = SetupNotificationForwardingParties(SetupInBondRelatedData(SetupJPAFRBills(header.Bills.AddNew(), "HB3243")), "NFP1", "NFP3", "NFP2");
			SetupJPAFRBillsVOCCFields(bill1, true, "COC1", "GTN1");
			bill1.JPB_SpecialCargoCode = "PLQ";
			bill1.Consignor.E2_OA_Address = consignor.MainAddress.PK;
			bill1.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			bill1.NotifyParty1.E2_OA_Address = notifyParty1.MainAddress.PK;
			bill1.NotifyParty2.E2_AddressOverride = ZBool.True;
			bill1.NotifyParty2.E2_CompanyName = "BOB THE BUILDER";
			bill1.NotifyParty2.E2_Address1 = "ADDRESS 1";
			bill1.NotifyParty2.E2_Address2 = "ADDRESS 2";
			bill1.NotifyParty2.E2_City = "CITY BOB";
			bill1.NotifyParty2.E2_State = "STATE BOB";
			bill1.NotifyParty2.E2_Postcode = "39234";
			bill1.NotifyParty2.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
			bill1.JPB_DG = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "1100", "", "IMO").First().PK;

			var container1 = bill1.Containers.AddNew();
			container1.JPC_ContainerNum = "CONT234";

			var container2 = bill1.Containers.AddNew();
			container2.JPC_ContainerNum = "CONT968";

			var bill2 = header.Bills.AddNew();
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var sendingBill2 = new MessageSendingObject(bill2, ActionCode.AmendingAdd, new MessageSendingAction(header, ActionCode.AmendingAdd)) { JPM_Send = ZBool.True };
			sendingBill2.UpdateAction(ActionCode.AmendingDelete);
			sendingBill2.JPM_DeleteReasonCode = "5";
			sendingBill2.JPM_DeleteReasonText = "this is test";

			Factory.SaveForTesting();
			var writer = new JPAFRHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = writer.GetDataObject(header);
			AssertEquals("headerData.SubShipmentCollection.Count", 2, headerData.SubShipmentCollection.Count);
			var billData = headerData.SubShipmentCollection[0];
			AssertAFRBillContents(billData, "HB3243", true);
			AssertAFRInBondDetailsContents(billData, 1500.50m, CodeDescriptionPairForTesting.New(USD.RX_Code, USD.RX_Desc), TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers, 3, new ZDateTime(2013, 10, 4), new ZDateTime(2013, 10, 7), TransportModeList.Codes.Others, CodeDescriptionPairForTesting.New("SD342", ""));
			AssertAFROtherRelevantLawContents(billData, "O1", "O3", "O5", "O4", "O2");
			AssertAFRNotficationForwardingPartyContents(billData, "NFP1", "NFP3", "NFP2");
			var organizationAddressCollection = billData.OrganizationAddressCollection;
			AssertEquals("billData.OrganizationAddressCollection.Count", 4, organizationAddressCollection.Count);
			AssertOrganizationBO_WUFSHIJNB("Consignor", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsignorDocumentaryAddress)), nameof(DocAddressType.ConsignorDocumentaryAddress));
			AssertOrganizationBO_CRAHOLSYD("Consignee", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ConsigneeAddress)), nameof(DocAddressType.ConsigneeAddress));
			AssertOrganizationBO_INTHEMSYD("NotifyParty1", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty)), nameof(DocAddressType.NotifyParty));
			AssertAddress("NotifyParty2", organizationAddressCollection.FirstOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.NotifyParty2)), nameof(DocAddressType.NotifyParty2), null, "BOB THE BUILDER", ZBool.True,
				"ADDRESS 1", "ADDRESS 2", "CITY BOB", "STATE BOB", "39234", Core.Constants.CountryCodes.Bahamas,
				"", "", "", "", "");
			AssertNotNull("billData.ContainerCollection", billData.ContainerCollection);
			AssertEquals("billData.ContainerCollection.Count", 2, billData.ContainerCollection.Count);
			AssertNotNull("CONT234", billData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT234"));
			AssertNotNull("CONT968", billData.ContainerCollection.FirstOrDefault(x => x.ContainerNumber.GetValueOrDefault() == "CONT968"));
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.SpecialCargoCode)", "PLQ", billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.SpecialCargoCode));

			var undgResult = billData.PackingLineCollection[0].UNDGCollection[0];
			AssertEquals("UNDGCode", "1100", undgResult.UNDGCode);
			AssertEquals("UNDGIMOCode", "3", undgResult.IMOClass);

			var billData2 = headerData.SubShipmentCollection[1];
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.DeleteReasonCode)", "5", billData2.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonCode));
			AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.DeleteReasonText)", "this is test", billData2.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.DeleteReasonText));
		}

		void AssertAFRBillContents(Shipment billData, ZString? wayBillNumber, bool isShippinglineEntry)
		{
			AssertAFRBillContents(billData, wayBillNumber, CodeDescriptionPairForTesting.New(SeaForeignPort2.RL_Code, SeaForeignPort2.RL_PortName), CodeDescriptionPairForTesting.New(SeaLocalPort3.RL_Code, SeaLocalPort3.RL_PortName), "102030", 125,
				CodeDescriptionPairForTesting.New(PackageTypeList.Codes.Barrel, PackageTypeList.Descriptions.Barrel), CodeDescriptionPairForTesting.New(Core.Constants.CountryCodes.Australia, "Australia"), 550.50m, CodeDescriptionPairForTesting.New(WeightUnitCodeList.Codes.Kilogram, WeightUnitCodeList.Descriptions.Kilogram),
				3.5m, CodeDescriptionPairForTesting.New(VolumeUnitCodeList.Codes.CubicMeter, VolumeUnitCodeList.Descriptions.CubicMeter), 1500.60m,
				CodeDescriptionPairForTesting.New(USD.RX_Code, USD.RX_Desc), "GOODS FOR TESTING", "MARKS AND MORE MARKS", "YEAH!", CodeDescriptionPairForTesting.New(SeaLocalPort3.RL_Code, SeaLocalPort3.RL_PortName));
			if (isShippinglineEntry)
			{
				AssertAFRBillVOCCCOntents(billData, "M", "COC1", "GTN1");
			}
			else
			{
				AssertAFRBillVOCCCOntents(billData, null, null, null);
			}
		}

		void AssertAFRBillContents(Shipment billData, ZString? wayBillNumber, ICodeDescription portOfOrigin, ICodeDescription portOfDestination, ZString? tariff, ZLong? manifestQty, ICodeDescription manifestUQ, ICodeDescription goodsOrigin, ZDecimal? grossWeight, ICodeDescription grossWeightUQ, ZDecimal? volume, ICodeDescription volumeUQ, ZDecimal? freightValue, ICodeDescription freightValueCurrency, ZString? goodsDescription, ZString? marksAndNumbers, ZString? remarks, ICodeDescription delivery)
		{
			AssertNotNull("Precondition: billData", billData);

			CombineAssertions(delegate
			{
				AssertEquals("billData.WayBillNumber", wayBillNumber, billData.WayBillNumber);
				AssertNotNull("billData.WayBillType", billData.WayBillType);
				AssertEquals("billData.WayBillType.Code", WayBillTypeList.Codes.House, billData.WayBillType.Code);
				AssertEquals("billData.WayBillType.Description", WayBillTypeList.Descriptions.House, billData.WayBillType.Description);
				AssertNotNull("billData.PortOfOrigin", billData.PortOfOrigin);
				AssertEquals("billData.PortOfOrigin.Code", portOfOrigin.Code, billData.PortOfOrigin.Code);
				AssertEquals("billData.PortOfOrigin.Name", portOfOrigin.Description, billData.PortOfOrigin.Name);
				AssertNotNull("billData.PortOfDestination", billData.PortOfDestination);
				AssertEquals("billData.PortOfDestination.Code", portOfDestination.Code, billData.PortOfDestination.Code);
				AssertEquals("billData.PortOfDestination.Name", portOfDestination.Description, billData.PortOfDestination.Name);
				AssertNotNull("billData.PackingLineCollection", billData.PackingLineCollection);
				AssertEquals("billData.PackingLineCollection.Count", 1, billData.PackingLineCollection.Count);
				var packingLineData = billData.PackingLineCollection[0];
				AssertEquals("packingLineData.HarmonisedCode", tariff, packingLineData.HarmonisedCode);
				AssertEquals("packingLineData.PackQty", manifestQty, packingLineData.PackQty);
				AssertNotNull("packingLineData.PackType", packingLineData.PackType);
				AssertEquals("packingLineData.PackType.Code", manifestUQ.Code, packingLineData.PackType.Code);
				AssertEquals("packingLineData.PackType.Description", manifestUQ.Description, packingLineData.PackType.Description);
				AssertNotNull("packingLineData.CountryOfOrigin", packingLineData.CountryOfOrigin);
				AssertEquals("packingLineData.CountryOfOrigin.Code", goodsOrigin.Code, packingLineData.CountryOfOrigin.Code);
				AssertEquals("packingLineData.CountryOfOrigin.Name", goodsOrigin.Description, packingLineData.CountryOfOrigin.Name);
				AssertEquals("packingLineData.Weight", grossWeight, packingLineData.Weight);
				AssertNotNull("packingLineData.WeightUnit", packingLineData.WeightUnit);
				AssertEquals("packingLineData.WeightUnit.Code", grossWeightUQ.Code, packingLineData.WeightUnit.Code);
				AssertEquals("packingLineData.WeightUnit.Description", grossWeightUQ.Description, packingLineData.WeightUnit.Description);
				AssertEquals("packingLineData.Volume", volume, packingLineData.Volume);
				AssertNotNull("packingLineData.VolumeUnit", packingLineData.VolumeUnit);
				AssertEquals("packingLineData.VolumeUnit.Code", volumeUQ.Code, packingLineData.VolumeUnit.Code);
				AssertEquals("packingLineData.VolumeUnit.Description", volumeUQ.Description, packingLineData.VolumeUnit.Description);
				AssertNotNull("billData.CommercialInfo", billData.CommercialInfo);
				AssertNotNull("billData.CommercialInfo.CommercialChargeCollection", billData.CommercialInfo.CommercialChargeCollection);
				AssertEquals("billData.CommercialInfo.CommercialChargeCollection.Count", 1, billData.CommercialInfo.CommercialChargeCollection.Count);
				var commercialChargeData = billData.CommercialInfo.CommercialChargeCollection[0];
				AssertEquals("commercialChargeData.Amount", freightValue, commercialChargeData.Amount);
				AssertNotNull("commercialChargeData.Currency", commercialChargeData.Currency);
				AssertEquals("commercialChargeData.Currency.Code", freightValueCurrency.Code, commercialChargeData.Currency.Code);
				AssertEquals("commercialChargeData.Currency.Description", freightValueCurrency.Description, commercialChargeData.Currency.Description);
				AssertEquals("packingLineData.DetailedDescription", goodsDescription, packingLineData.DetailedDescription);
				AssertEquals("packingLineData.MarksAndNos", marksAndNumbers, packingLineData.MarksAndNos);
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.PlaceOfDeliveryCode)", delivery.Code, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.PlaceOfDeliveryCode));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.PlaceOfDeliveryName)", delivery.Description, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.PlaceOfDeliveryName));
			});
			AssertNotNull("billData.NoteCollection", billData.NoteCollection);
			AssertEquals("billData.NoteCollection.Count", 1, billData.NoteCollection.Count);
			AssertContents(billData.NoteCollection[0], ZBool.True, AddInfoConstants.Bill.Remarks, remarks.Value);
		}

		void AssertAFRBillVOCCCOntents(Shipment billData, ZString? masterBillIdentifier, ZString? containerOperatorCode, ZString? generalCustomsTransitApprovalNumber)
		{
			AssertNotNull("Precondition: billData", billData);

			CombineAssertions(() =>
			{
				AssertEquals("billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.MasterBillIdentifier)", masterBillIdentifier, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.MasterBillIdentifier));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.ContainerOperatorCode)", containerOperatorCode, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.ContainerOperatorCode));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.GeneralCustomsTransitApprovalNumber)", generalCustomsTransitApprovalNumber, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.GeneralCustomsTransitApprovalNumber));
			});
		}

		void AssertAFRNotficationForwardingPartyContents(Shipment headerData, ZString? partCode1, ZString? partCode2, ZString? partCode3)
		{
			AssertNotNull("Precondition: headerData", headerData);

			CombineAssertions(delegate
			{
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Bill.NotificationForwardingPartyCode1)", partCode1, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode1));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Bill.NotificationForwardingPartyCode2)", partCode2, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode2));
				AssertEquals("headerData.AddInfoCollection.GetZStringValue(Constants.Bill.NotificationForwardingPartyCode3)", partCode3, headerData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.NotificationForwardingPartyCode3));
			});
		}

		void AssertAFROtherRelevantLawContents(Shipment billData, ZString? law1, ZString? law2, ZString? law3, ZString? law4, ZString? law5)
		{
			AssertNotNull("Precondition: billData", billData);

			CombineAssertions(delegate
			{
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.OtherRelevantLawCode1)", law1, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode1));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.OtherRelevantLawCode2)", law2, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode2));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.OtherRelevantLawCode3)", law3, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode3));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.OtherRelevantLawCode4)", law4, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode4));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.OtherRelevantLawCode5)", law5, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.OtherRelevantLawCode5));
			});
		}

		void AssertAFRInBondDetailsContents(Shipment billData, ZDecimal? goodsValue, ICodeDescription goodsValueCurrency, ZString? temporaryLandingReason, ZInt? temporaryLandingDuration, ZDateTime? eSDT, ZDateTime? eFDT, ZString? transportMode, ICodeDescription arrivalBondedAreaCode)
		{
			AssertNotNull("Precondition: billData", billData);

			CombineAssertions(delegate
			{
				AssertEquals("billData.GoodsValue", goodsValue, billData.GoodsValue);
				AssertNotNull("billData.GoodsValueCurrency", billData.GoodsValueCurrency);
				AssertEquals("billData.GoodsValueCurrency.Code", goodsValueCurrency.Code, billData.GoodsValueCurrency.Code);
				AssertEquals("billData.GoodsValueCurrency.Description", goodsValueCurrency.Description, billData.GoodsValueCurrency.Description);
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.TranshipmentReasonCode)", temporaryLandingReason, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentReasonCode));
				AssertEquals("billData.AddInfoCollection.GetZIntValue(Constants.Bill.TranshipmentDuration)", temporaryLandingDuration, billData.AddInfoCollection.GetZIntValue(AddInfoConstants.Bill.TranshipmentDuration));
				AssertEquals("billData.AddInfoCollection.GetZDateTimeValue(Constants.Bill.TranshipmentEstimatedStartDate)", eSDT, billData.AddInfoCollection.GetZDateTimeValue(AddInfoConstants.Bill.TranshipmentEstimatedStartDate));
				AssertEquals("billData.AddInfoCollection.GetZDateTimeValue(Constants.Bill.TranshipmentEstimatedFinishDate)", eFDT, billData.AddInfoCollection.GetZDateTimeValue(AddInfoConstants.Bill.TranshipmentEstimatedFinishDate));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.TranshipmentTransportMode)", transportMode, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentTransportMode));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.TranshipmentArrivalPlaceCode)", arrivalBondedAreaCode.Code, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentArrivalPlaceCode));
				AssertEquals("billData.AddInfoCollection.GetZStringValue(Constants.Bill.TranshipmentArrivalPlaceName)", arrivalBondedAreaCode.Description, billData.AddInfoCollection.GetZStringValue(AddInfoConstants.Bill.TranshipmentArrivalPlaceName));
			});
		}

		JPAFRBills SetupJPAFRBills(JPAFRBills bill, ZString wayBillNumber)
		{
			return SetupJPAFRBills(bill, wayBillNumber, SeaForeignPort2.RL_Code, SeaLocalPort3.RL_Code, "102030", 125, PackageTypeList.Codes.Barrel, Core.Constants.CountryCodes.Australia, 550.50m, WeightUnitCodeList.Codes.Kilogram, 3.5m, VolumeUnitCodeList.Codes.CubicMeter, 1500.60m, USD.RX_Code, "GOODS FOR TESTING", "MARKS AND MORE MARKS", "YEAH!", SeaLocalPort3.RL_Code);
		}

		JPAFRBills SetupJPAFRBills(JPAFRBills bill, ZString billNumber, ZString portOfOrigin, ZString portOfDestination, ZString tariff, ZInt manifestQty, ZString manifestUQ, ZString goodsOrigin, ZDecimal grossWeight, ZString grossWeightUQ, ZDecimal volume, ZString volumeUQ, ZDecimal freightValue, ZString freightValueCurrency, ZString goodsDescription, ZString marksAndNumbers, ZString remarks, ZString delivery)
		{
			bill.InBondDetailInitiator = InBondDetailInitiatorTestHelper;
			bill.JPB_BillNumber = billNumber;
			bill.JPB_RL_NKOrigin = portOfOrigin;
			bill.JPB_RL_NKFinalDestination = portOfDestination;
			bill.JPB_Tariff = tariff;
			bill.JPB_ManifestQty = manifestQty;
			bill.JPB_ManifestUQ = manifestUQ;
			bill.JPB_RN_NKGoodsOrigin = goodsOrigin;
			bill.JPB_GrossWeight = grossWeight;
			bill.JPB_GrossWeightUQ = grossWeightUQ;
			bill.JPB_Volume = volume;
			bill.JPB_VolumeUQ = volumeUQ;
			bill.JPB_FreightValue = freightValue;
			bill.JPB_RX_NKFreightValueCurrency = freightValueCurrency;
			bill.JPB_GoodsDescription = goodsDescription;
			bill.JPB_MarksAndNumbers = marksAndNumbers;
			bill.JPB_Remarks = remarks;
			bill.JPB_RL_NKDelivery = delivery;
			return bill;
		}

		void SetupJPAFRBillsVOCCFields(JPAFRBills bill, bool isMasterBill, ZString containerOperatorCode, ZString generalCustomsTransitApprovalNumber)
		{
			bill.JPB_IsMaterBill = isMasterBill;
			bill.JPB_ContainerOperatorCode = containerOperatorCode;
			bill.JPB_Calc_GeneralCustomsTransitApprovalNumber = generalCustomsTransitApprovalNumber;
		}

		JPAFRBills SetupNotificationForwardingParties(JPAFRBills bill, ZString nfp1, ZString nfp2, ZString nfp3)
		{
			if (!nfp1.IsEmpty)
			{
				bill.NotificationForwardingParties.AddNewIfNotExist(nfp1);
			}

			if (!nfp2.IsEmpty)
			{
				bill.NotificationForwardingParties.AddNewIfNotExist(nfp2);
			}

			if (!nfp3.IsEmpty)
			{
				bill.NotificationForwardingParties.AddNewIfNotExist(nfp3);
			}

			return bill;
		}

		JPAFRBills SetupInBondRelatedData(JPAFRBills bill, ZDecimal goodsValue, ZString goodsValueCurrency, ZString temporaryLandingReason, ZInt temporaryLandingDuration, ZDateTime eSDT, ZDateTime eFDT, ZString transportMode, ZString arrivalBondedAreaCode, ZString law1, ZString law2, ZString law3, ZString law4, ZString law5)
		{
			bill.InBondDetailInitiator = InBondDetailInitiatorTestHelper;
			bill.JPB_Calc_GoodsValue = goodsValue;
			bill.JPB_Calc_RX_NKGoodsValueCurrency = goodsValueCurrency;
			bill.JPB_Calc_TemporaryLandingReason = temporaryLandingReason;
			bill.JPB_Calc_ESDT = eSDT;
			bill.JPB_Calc_EFDT = eFDT;
			bill.JPB_Calc_TemporaryLandingDuration = temporaryLandingDuration;
			bill.JPB_Calc_TransportMode = transportMode;
			bill.JPB_Calc_ArrivalBondedAreaCode = arrivalBondedAreaCode;
			if (!law1.IsEmpty)
			{
				bill.OtherRelevantLaws.AddNewIfNotExist(law1);
			}

			if (!law2.IsEmpty)
			{
				bill.OtherRelevantLaws.AddNewIfNotExist(law2);
			}

			if (!law3.IsEmpty)
			{
				bill.OtherRelevantLaws.AddNewIfNotExist(law3);
			}

			if (!law4.IsEmpty)
			{
				bill.OtherRelevantLaws.AddNewIfNotExist(law4);
			}

			if (!law5.IsEmpty)
			{
				bill.OtherRelevantLaws.AddNewIfNotExist(law5);
			}

			return bill;
		}

		JPAFRBills SetupInBondRelatedData(JPAFRBills bill)
		{
			return SetupInBondRelatedData(bill, 1500.50m, USD.RX_Code, TemporaryLandingReasonCodeList.Codes.RepackingGoodsInOtherContainers, 3, new ZDateTime(2013, 10, 4), new ZDateTime(2013, 10, 7),
				TransportModeList.Codes.Others, "SD342", "O1", "O3", "O5", "O4", "O2");
		}

		#region Implementation

		RefCurrency USD
		{
			get { return usd ?? (usd = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates)); }
		}
		RefCurrency usd;

		#endregion
	}
}
