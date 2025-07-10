using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

sealed class TemporaryStorageHeaderCloneStrategyTest : TestCaseWithFactory
{
	public void TestCloneTemporaryStorageHeader()
	{
		var tempHeader = GetTempHeaderForTest();
		var clonedHeader = (TemporaryStorageHeader)new TemporaryStorageHeaderCloneStrategy(tempHeader).Clone();
		Factory.Save();

		CombineAssertions("ClusterKey is not duplicated:", () =>
		{
			AssertEquals(1, tempHeader.AMA_ClusterKey);
			AssertEquals(2, clonedHeader.AMA_ClusterKey);
		});

		CombineAssertions("Temporary Header properties:", () =>
		{
			AssertEquals("AMA_MessageType is cloned", "G5P", clonedHeader.AMA_MessageType);
			AssertEquals("AMA_TransportMode is cloned", "SEA", clonedHeader.AMA_TransportMode);
			AssertEquals("AMA_CustomsOffice is cloned", "LV001", clonedHeader.AMA_CustomsOffice);
			AssertEquals("AMA_ApplicationCode is not cloned, set by default", "STO", clonedHeader.AMA_ApplicationCode);
			AssertEquals("AMA_AgentType is not cloned, set by default", "AGT", clonedHeader.AMA_AgentType);
			AssertEquals("AMA_ContainerMode is not cloned", ZString.Empty, clonedHeader.AMA_ContainerMode);
			AssertEquals("AMA_Nature is not cloned", ZString.Empty, clonedHeader.AMA_Nature);
			AssertEquals("AMA_ManifestType is not cloned", ZString.Empty, clonedHeader.AMA_ManifestType);
			AssertEquals("AMA_CarrierCode is not cloned", ZString.Empty, clonedHeader.AMA_CarrierCode);
			AssertEquals("AMA_RL_NKPortOfFirstArrival is not cloned", ZString.Empty, clonedHeader.AMA_RL_NKPortOfFirstArrival);
			AssertEquals("AMA_RL_NKPortOfFinalDeparture is not cloned", ZString.Empty, clonedHeader.AMA_RL_NKPortOfFinalDeparture);

			AssertEquals("Declarant is cloned", declarant.MainAddress.PK, clonedHeader.AMA_OA_Declarant);
			AssertEquals("Representative is cloned", representative.MainAddress.PK, clonedHeader.AMA_OA_Representative);
			AssertEquals("Carrier is not cloned", ZGuid.Empty, clonedHeader.AMA_OA_Carrier);
			AssertEquals("Discharge Terminal is not cloned", ZGuid.Empty, clonedHeader.AMA_OA_DischargeTerminalAddress);
			AssertEquals("Shipping Agent is not cloned", ZGuid.Empty, clonedHeader.AMA_OA_ShippingAgent);
			AssertEquals("Presenter is not cloned", ZGuid.Empty, clonedHeader.AMA_OA_Presenter);
		});

		CombineAssertions("Header.Guarantee", () =>
		{
			var clonedGuarantee = clonedHeader.Guarantee;
			AssertEquals("PW_BondNumber is cloned", "Test1", clonedGuarantee.PW_BondNumber);
			AssertEquals("PW_Override is cloned", true, clonedGuarantee.PW_Override);
			AssertEquals("PW_BondAmount is cloned", 5.0m, clonedGuarantee.PW_BondAmount);
		});

		CombineAssertions("Header.PreviousDocuments", () =>
		{
			AssertEquals(1, clonedHeader.PreviousDocuments.Count);
			AssertPreviousDocument(clonedHeader.PreviousDocuments[0], "P000", 0, "PRE_REF0", "PRE_REF20");
		});

		CombineAssertions("Bills", () =>
		{
			AssertEquals(3, clonedHeader.Bills.Count);
			var masterBill = clonedHeader.Bills[0];

			AssertEquals("ABL_BolType is cloned", "BOL", masterBill.ABL_BolType);
			AssertEquals("ABL_BillNumber is cloned", "AABBCCDDEEFFGG", masterBill.ABL_BillNumber);
			AssertEquals("ABL_RL_NKOrigin is not cloned", ZString.Empty, masterBill.ABL_RL_NKOrigin);
			AssertEquals("ABL_RL_NKFinalDestination is not cloned", ZString.Empty, masterBill.ABL_RL_NKFinalDestination);
			AssertEquals("ABL_RL_NKPortOfDischarge is not cloned", ZString.Empty, masterBill.ABL_RL_NKPortOfDischarge);
			AssertEquals("ABL_RL_NKPortOfLoading is not cloned", ZString.Empty, masterBill.ABL_RL_NKPortOfLoading);
			AssertEquals("ABL_ShipmentType is not cloned", ZString.Empty, masterBill.ABL_ShipmentType);

			AssertEquals("ABL_OA_Consignee is cloned", consignee.MainAddress.PK, masterBill.ABL_OA_Consignee);
			AssertEquals("ABL_ConsigneePhone is cloned", "4008208820", masterBill.ABL_ConsigneePhone);
			AssertEquals("ABL_ConsigneePostcode is cloned", "225700", masterBill.ABL_ConsigneePostcode);
			AssertEquals("ABL_ConsigneeName is cloned", "ConsigneeName", masterBill.ABL_ConsigneeName);
			AssertEquals("ABL_ConsigneeRegNo is cloned", "LVEORICONSIGNEE", masterBill.ABL_ConsigneeRegNo);
			AssertEquals("ABL_ConsigneeRegNoType is cloned", "1", masterBill.ABL_ConsigneeRegNoType);
			AssertEquals("ABL_RN_NKConsigneeCountry is cloned", Core.Constants.CountryCodes.Latvia, masterBill.ABL_RN_NKConsigneeCountry);
			AssertEquals("ABL_ConsigneeState is cloned", "ConsigneeState", masterBill.ABL_ConsigneeState);
			AssertEquals("ABL_ConsigneeCity is cloned", "ConsigneeCity", masterBill.ABL_ConsigneeCity);
			AssertEquals("ABL_ConsigneeStreet1 is cloned", "ConsigneeAddress1", masterBill.ABL_ConsigneeStreet1);
			AssertEquals("ABL_ConsigneeStreet2 is cloned", "ConsigneeAddress2", masterBill.ABL_ConsigneeStreet2);

			AssertEquals("ABL_OA_Shipper is cloned", shipper.MainAddress.PK, masterBill.ABL_OA_Shipper);
			AssertEquals("ABL_ShipperPhone is cloned", "12345678", masterBill.ABL_ShipperPhone);
			AssertEquals("ABL_ShipperName is cloned", "ShipperName", masterBill.ABL_ShipperName);
			AssertEquals("ABL_ShipperPostcode is cloned", "233333", masterBill.ABL_ShipperPostcode);
			AssertEquals("ABL_ShipperRegNo is cloned", "LVEORISHIPPER", masterBill.ABL_ShipperRegNo);
			AssertEquals("ABL_ShipperRegNoType is cloned", "2", masterBill.ABL_ShipperRegNoType);
			AssertEquals("ABL_RN_NKShipperCountry is cloned", Core.Constants.CountryCodes.Germany, masterBill.ABL_RN_NKShipperCountry);
			AssertEquals("ABL_ShipperState is cloned", "ShipperState", masterBill.ABL_ShipperState);
			AssertEquals("ABL_ShipperCity is cloned", "ShipperCity", masterBill.ABL_ShipperCity);
			AssertEquals("ABL_ShipperStreet1 is cloned", "ShipperAddress1", masterBill.ABL_ShipperStreet1);
			AssertEquals("ABL_ShipperStreet2 is cloned", "ShipperAddress2", masterBill.ABL_ShipperStreet2);

			AssertEquals("ABL_OA_NotifyParty is not cloned", ZGuid.Empty, masterBill.ABL_OA_NotifyParty);
			AssertEquals("ABL_NotifyPartyPhone is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyPhone);
			AssertEquals("ABL_NotifyPartyPostcode is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyPostcode);
			AssertEquals("ABL_NotifyPartyName is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyName);
			AssertEquals("ABL_NotifyPartyRegNo is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyRegNo);
			AssertEquals("ABL_NotifyPartyRegNoType is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyRegNoType);
			AssertEquals("ABL_RN_NKNotifyPartyCountry is not cloned", ZString.Empty, masterBill.ABL_RN_NKNotifyPartyCountry);
			AssertEquals("ABL_NotifyPartyState is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyState);
			AssertEquals("ABL_NotifyPartyCity is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyCity);
			AssertEquals("ABL_NotifyPartyStreet1 is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyStreet1);
			AssertEquals("ABL_NotifyPartyStreet2 is not cloned", ZString.Empty, masterBill.ABL_NotifyPartyStreet2);

			AssertEquals("ABL_BolType is cloned for second bill", "AAA", clonedHeader.Bills[1].ABL_BolType);
			AssertEquals("ABL_BolType is cloned for third bill", "BBB", clonedHeader.Bills[2].ABL_BolType);
		});

		CombineAssertions("Bills.PackedItems", () =>
		{
			var packedItemOfHouseBill1 = clonedHeader.Bills[1].PackedItems;
			AssertEquals(1, packedItemOfHouseBill1.Count);
			var packedItem = packedItemOfHouseBill1[0];
			AssertEquals("API_Tariff is cloned", "987654321", packedItem.API_Tariff);
			AssertEquals("API_GoodsDescription is cloned", "CokeCola Items", packedItem.API_GoodsDescription);
			AssertEquals("API_ChemicalSubstanceCode is cloned", "011233-2", packedItem.API_ChemicalSubstanceCode);
			AssertEquals("API_GrossWeight is cloned", 20.23m, packedItem.API_GrossWeight);
			AssertEquals("API_GrossWeightUQ is cloned", "KG", packedItem.API_GrossWeightUQ);
			AssertEquals("API_PackStatus is cloned", "MIS", packedItem.API_PackStatus);
			AssertEquals("API_RN_NKGoodsOrigin is cloned", Core.Constants.CountryCodes.Spain, packedItem.API_RN_NKGoodsOrigin);
			AssertEquals("API_GoodsValue is cloned", 100m, packedItem.API_GoodsValue);
			AssertEquals("API_RX_NKGoodsValueCurrency is cloned", Core.Constants.CurrencyCodes.Spain, packedItem.API_RX_NKGoodsValueCurrency);
			AssertEquals("API_CustomsQty is cloned", 1.1m, packedItem.API_CustomsQty);
			AssertEquals("API_CustomsUQ is cloned", Core.Constants.Weight.Grams, packedItem.API_CustomsUQ);
			AssertEquals("API_CustomsQty2 is cloned", 2.2m, packedItem.API_CustomsQty2);
			AssertEquals("API_CustomsUQ2 is cloned", Core.Constants.Weight.Kilograms, packedItem.API_CustomsUQ2);
			AssertEquals("API_CustomsQty3 is cloned", 3.3m, packedItem.API_CustomsQty3);
			AssertEquals("API_CustomsUQ3 is cloned", Core.Constants.Weight.Hectograms, packedItem.API_CustomsUQ3);
			AssertEquals("API_Supplements is cloned", "ABCD", packedItem.API_Supplements);
		});

		CombineAssertions("Bills.PreviousDocuments", () =>
		{
			var billPreviousDocuments = clonedHeader.Bills[2].PreviousDocuments;
			AssertEquals(1, billPreviousDocuments.Count);
			AssertPreviousDocument(billPreviousDocuments[0], "P003", 3, "PRE_REF3", "PRE_REF23");
		});

		CombineAssertions("Bills.PackedItems.PreviousDocuments", () =>
		{
			var packedItem = clonedHeader.Bills[1].PackedItems[0];
			var previousDocuments = packedItem.PreviousDocuments;
			AssertEquals(2, previousDocuments.Count);

			AssertPreviousDocument(previousDocuments[0], "P001", 1, "PRE_REF1", "PRE_REF21");
			AssertPreviousDocument(previousDocuments[1], "P002", 2, "PRE_REF2", "PRE_REF22");
		});

		void AssertPreviousDocument(EU.Business.CusTempStorage.TemporaryStoragePreviousDocument doc, ZString code, ZInt lineNo, ZString refNumber, ZString refNumber2)
		{
			AssertEquals("CSI_Code is cloned", code, doc.CSI_Code);
			AssertEquals("CSI_LineNo is cloned", lineNo, doc.CSI_LineNo);
			AssertEquals("CSI_ReferenceNumber is cloned", refNumber, doc.CSI_ReferenceNumber);
			AssertEquals("CSI_ReferenceNumber2 is cloned", refNumber2, doc.CSI_ReferenceNumber2);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		carrier = Factory.New<OrgHeader>();
		carrier.OH_Code = "Carrier";

		dischargeTerminal = Factory.New<OrgHeader>();
		dischargeTerminal.OH_Code = "Terminal";

		shippingAgent = Factory.New<OrgHeader>();
		shippingAgent.OH_Code = "Agent";

		declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "Declarant";

		presenter = Factory.New<OrgHeader>();
		presenter.OH_Code = "Presenter";

		representative = Factory.New<OrgHeader>();
		representative.OH_Code = "Represent";

		consignee = Factory.New<OrgHeader>();
		consignee.OH_Code = "Consignee";
		consignee.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriConsignee", Core.Constants.CountryCodes.Latvia);
		consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
		consignee.MainAddress.State = "ConsigneeState";
		consignee.MainAddress.City = "ConsigneeCity";
		consignee.MainAddress.Address1 = "ConsigneeAddress1";
		consignee.MainAddress.Address2 = "ConsigneeAddress2";
		consignee.MainAddress.OA_Phone = "4008208820";
		consignee.MainAddress.OA_PostCode = "225700";
		consignee.MainAddress.OA_CompanyNameOverride = "ConsigneeName";

		notifyParty = Factory.New<OrgHeader>();
		notifyParty.OH_Code = "NotifyParty";
		notifyParty.OH_Category = OrgConstants.Category.Business;
		notifyParty.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriNotifyParty", Core.Constants.CountryCodes.Latvia);
		notifyParty.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		notifyParty.MainAddress.State = "NotifyPartyState";
		notifyParty.MainAddress.City = "NotifyPartyCity";
		notifyParty.MainAddress.Address1 = "NotifyPartyAddress1";
		notifyParty.MainAddress.Address2 = "NotifyPartyAddress2";
		notifyParty.MainAddress.OA_Phone = "23382338";
		notifyParty.MainAddress.OA_PostCode = "210017";
		notifyParty.MainAddress.OA_CompanyNameOverride = "NotifyPartyName";

		shipper = Factory.New<OrgHeader>();
		shipper.OH_Code = "Shipper";
		shipper.OH_Category = OrgConstants.Category.Government;
		shipper.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriShipper", Core.Constants.CountryCodes.Latvia);
		shipper.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		shipper.MainAddress.State = "ShipperState";
		shipper.MainAddress.City = "ShipperCity";
		shipper.MainAddress.Address1 = "ShipperAddress1";
		shipper.MainAddress.Address2 = "ShipperAddress2";
		shipper.MainAddress.OA_Phone = "12345678";
		shipper.MainAddress.OA_PostCode = "233333";
		shipper.MainAddress.OA_CompanyNameOverride = "ShipperName";
	}

	TemporaryStorageHeader GetTempHeaderForTest()
	{
		var tempHeader = Factory.New<TemporaryStorageHeader>();
		tempHeader.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
		tempHeader.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
		tempHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TWForwarderManifest;
		tempHeader.AMA_AgentType = Core.Constants.AgentType.CoLoad;
		tempHeader.AMA_ContainerMode = "CNT";
		tempHeader.AMA_Nature = "NAT";
		tempHeader.AMA_ManifestType = "MANITP";
		tempHeader.AMA_CustomsOffice = "LV001";
		tempHeader.AMA_CarrierCode = "CarrierCode";
		tempHeader.AMA_RL_NKPortOfFirstArrival = "LV6LV";
		tempHeader.AMA_RL_NKPortOfFinalDeparture = "FR27B";
		tempHeader.AMA_OA_Carrier = carrier.MainAddress.PK;
		tempHeader.AMA_OA_DischargeTerminalAddress = dischargeTerminal.MainAddress.PK;
		tempHeader.AMA_OA_ShippingAgent = shippingAgent.MainAddress.PK;
		tempHeader.AMA_OA_Declarant = declarant.MainAddress.PK;
		tempHeader.AMA_OA_Presenter = presenter.MainAddress.PK;
		tempHeader.AMA_OA_Representative = representative.MainAddress.PK;
		var headerPreDoc = tempHeader.PreviousDocuments.AddNew();
		headerPreDoc.CSI_Code = "P000";
		headerPreDoc.CSI_ReferenceNumber = "PRE_REF0";
		headerPreDoc.CSI_ReferenceNumber2 = "PRE_REF20";
		headerPreDoc.CSI_LineNo = 0;

		var headerGuarantee = tempHeader.Guarantee;
		headerGuarantee.PW_BondNumber = "Test1";
		headerGuarantee.PW_Override = true;
		headerGuarantee.PW_BondAmount = 5.0m;

		var masterBill = tempHeader.MasterBill;
		masterBill.ABL_BillNumber = "AABBCCDDEEFFGG";
		masterBill.ABL_ShipmentType = "SKK";
		masterBill.ABL_RL_NKOrigin = "FR27B";
		masterBill.ABL_RL_NKFinalDestination = "LV6LV";
		masterBill.ABL_RL_NKPortOfDischarge = "LV6LV";
		masterBill.ABL_RL_NKPortOfLoading = "FR27B";
		masterBill.ABL_OA_Consignee = consignee.MainAddress.PK;
		masterBill.ABL_OA_Shipper = shipper.MainAddress.PK;
		masterBill.ABL_OA_NotifyParty = notifyParty.MainAddress.PK;

		var houseBill1 = tempHeader.Bills.AddNew();
		houseBill1.ABL_BolType = "AAA";
		var houseBill2 = tempHeader.Bills.AddNew();
		houseBill2.ABL_BolType = "BBB";

		var packedItem = houseBill1.PackedItems.AddNew();
		packedItem.API_Tariff = "987654321";
		packedItem.API_GoodsDescription = "CokeCola Items";
		packedItem.API_ChemicalSubstanceCode = "011233-2";
		packedItem.API_GrossWeight = 20.23m;
		packedItem.API_GrossWeightUQ = "KG";
		packedItem.API_PackStatus = "MIS";
		packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Spain;
		packedItem.API_GoodsValue = 100m;
		packedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Spain;
		packedItem.API_CustomsQty = 1.1m;
		packedItem.API_CustomsUQ = Core.Constants.Weight.Grams;
		packedItem.API_CustomsQty2 = 2.2m;
		packedItem.API_CustomsUQ2 = Core.Constants.Weight.Kilograms;
		packedItem.API_CustomsQty3 = 3.3m;
		packedItem.API_CustomsUQ3 = Core.Constants.Weight.Hectograms;

		var newSupplementaryCode = packedItem.AdditionalSupplementaryCodes.AddNew("ABCD");
		newSupplementaryCode.CY_Type = EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode;
		newSupplementaryCode.CY_ParentTableCode = "API";

		var itemPreDoc1 = packedItem.PreviousDocuments.AddNew();
		itemPreDoc1.CSI_Code = "P001";
		itemPreDoc1.CSI_ReferenceNumber = "PRE_REF1";
		itemPreDoc1.CSI_ReferenceNumber2 = "PRE_REF21";
		itemPreDoc1.CSI_LineNo = 1;
		var itemPreDoc2 = packedItem.PreviousDocuments.AddNew();
		itemPreDoc2.CSI_Code = "P002";
		itemPreDoc2.CSI_ReferenceNumber = "PRE_REF2";
		itemPreDoc2.CSI_ReferenceNumber2 = "PRE_REF22";
		itemPreDoc2.CSI_LineNo = 2;

		var billPreDoc = houseBill2.PreviousDocuments.AddNew();
		billPreDoc.CSI_Code = "P003";
		billPreDoc.CSI_ReferenceNumber = "PRE_REF3";
		billPreDoc.CSI_ReferenceNumber2 = "PRE_REF23";
		billPreDoc.CSI_LineNo = 3;
		Factory.Save();

		return tempHeader;
	}

	OrgHeader carrier;
	OrgHeader dischargeTerminal;
	OrgHeader shippingAgent;
	OrgHeader declarant;
	OrgHeader presenter;
	OrgHeader representative;
	OrgHeader consignee;
	OrgHeader notifyParty;
	OrgHeader shipper;
}
