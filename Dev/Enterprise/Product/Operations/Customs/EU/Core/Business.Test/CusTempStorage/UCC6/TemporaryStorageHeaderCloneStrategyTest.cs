using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
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
				AssertEquals("SEA", clonedHeader.AMA_TransportMode);
				AssertEquals("STO", clonedHeader.AMA_ApplicationCode);
				AssertEquals("CNT", clonedHeader.AMA_ContainerMode);
				AssertEquals("AGT", clonedHeader.AMA_AgentType);
				AssertEquals("NAT", clonedHeader.AMA_Nature);
				AssertEquals("MANITP", clonedHeader.AMA_ManifestType);
				AssertEquals("LV001", clonedHeader.AMA_CustomsOffice);
				AssertEquals("CarrierCode", clonedHeader.AMA_CarrierCode);
				AssertEquals("LV6LV", clonedHeader.AMA_RL_NKPortOfFirstArrival);
				AssertEquals("FR27B", clonedHeader.AMA_RL_NKPortOfFinalDeparture);

				AssertEquals("Carrier", carrier.MainAddress.PK, clonedHeader.AMA_OA_Carrier);
				AssertEquals("Discharge Terminal", dischargeTerminal.MainAddress.PK, clonedHeader.AMA_OA_DischargeTerminalAddress);
				AssertEquals("Shipping Agent", shippingAgent.MainAddress.PK, clonedHeader.AMA_OA_ShippingAgent);
				AssertEquals("Declarant", declarant.MainAddress.PK, clonedHeader.AMA_OA_Declarant);
				AssertEquals("Presenter", presenter.MainAddress.PK, clonedHeader.AMA_OA_Presenter);
				AssertEquals("Representative", representative.MainAddress.PK, clonedHeader.AMA_OA_Representative);
			});

			CombineAssertions("Header.PreviousDocuments", () =>
			{
				AssertEquals(1, clonedHeader.PreviousDocuments.Count);
				AssertPreviousDocument(clonedHeader.PreviousDocuments[0], "P000", 0);
			});

			CombineAssertions("Bills", () =>
			{
				AssertEquals(3, clonedHeader.Bills.Count);
				var masterBill = clonedHeader.Bills[0];

				AssertEquals("BOL", masterBill.ABL_BolType);
				AssertEquals("FR27B", masterBill.ABL_RL_NKOrigin);
				AssertEquals("LV6LV", masterBill.ABL_RL_NKFinalDestination);

				AssertEquals(consignee.MainAddress.PK, masterBill.ABL_OA_Consignee);
				AssertEquals("4008208820", masterBill.ABL_ConsigneePhone);
				AssertEquals("225700", masterBill.ABL_ConsigneePostcode);
				AssertEquals("ConsigneeName", masterBill.ABL_ConsigneeName);
				AssertEquals("EoriConsignee", masterBill.ABL_ConsigneeRegNo);
				AssertEquals("1", masterBill.ABL_ConsigneeRegNoType);
				AssertEquals(Core.Constants.CountryCodes.Latvia, masterBill.ABL_RN_NKConsigneeCountry);
				AssertEquals("ConsigneeState", masterBill.ABL_ConsigneeState);
				AssertEquals("ConsigneeCity", masterBill.ABL_ConsigneeCity);
				AssertEquals("ConsigneeAddress1", masterBill.ABL_ConsigneeStreet1);
				AssertEquals("ConsigneeAddress2", masterBill.ABL_ConsigneeStreet2);

				AssertEquals(shipper.MainAddress.PK, masterBill.ABL_OA_Shipper);
				AssertEquals("12345678", masterBill.ABL_ShipperPhone);
				AssertEquals("ShipperName", masterBill.ABL_ShipperName);
				AssertEquals("233333", masterBill.ABL_ShipperPostcode);
				AssertEquals("EoriShipper", masterBill.ABL_ShipperRegNo);
				AssertEquals("2", masterBill.ABL_ShipperRegNoType);
				AssertEquals(Core.Constants.CountryCodes.Germany, masterBill.ABL_RN_NKShipperCountry);
				AssertEquals("ShipperState", masterBill.ABL_ShipperState);
				AssertEquals("ShipperCity", masterBill.ABL_ShipperCity);
				AssertEquals("ShipperAddress1", masterBill.ABL_ShipperStreet1);
				AssertEquals("ShipperAddress2", masterBill.ABL_ShipperStreet2);

				AssertEquals(notifyParty.MainAddress.PK, masterBill.ABL_OA_NotifyParty);
				AssertEquals("23382338", masterBill.ABL_NotifyPartyPhone);
				AssertEquals("210017", masterBill.ABL_NotifyPartyPostcode);
				AssertEquals("NotifyPartyName", masterBill.ABL_NotifyPartyName);
				AssertEquals("EoriNotifyParty", masterBill.ABL_NotifyPartyRegNo);
				AssertEquals("2", masterBill.ABL_NotifyPartyRegNoType);
				AssertEquals(Core.Constants.CountryCodes.France, masterBill.ABL_RN_NKNotifyPartyCountry);
				AssertEquals("NotifyPartyState", masterBill.ABL_NotifyPartyState);
				AssertEquals("NotifyPartyCity", masterBill.ABL_NotifyPartyCity);
				AssertEquals("NotifyPartyAddress1", masterBill.ABL_NotifyPartyStreet1);
				AssertEquals("NotifyPartyAddress2", masterBill.ABL_NotifyPartyStreet2);

				AssertEquals("LV6LV", masterBill.ABL_RL_NKPortOfDischarge);
				AssertEquals("FR27B", masterBill.ABL_RL_NKPortOfLoading);
				AssertEquals("SKK", masterBill.ABL_ShipmentType);
			});

			CombineAssertions("Bills.Packs", () =>
			{
				var packsOfHouseBill1 = clonedHeader.Bills[1].Packs;
				AssertEquals(1, packsOfHouseBill1.Count);
				var pack = packsOfHouseBill1[0];
				AssertEquals(23, pack.APA_PackQty);
				AssertEquals("KG", pack.APA_PackUQ);
				AssertEquals("CokeCola Pack", pack.APA_GoodsDescription);
				AssertEquals("0001", pack.APA_CommodityCode);
			});

			CombineAssertions("Bills.PackedItems", () =>
			{
				var packedItemOfHouseBill1 = clonedHeader.Bills[1].PackedItems;
				AssertEquals(1, packedItemOfHouseBill1.Count);
				var packedItem = packedItemOfHouseBill1[0];
				AssertEquals("987654321", packedItem.API_Tariff);
				AssertEquals("CokeCola Items", packedItem.API_GoodsDescription);
				AssertEquals("011233-2", packedItem.API_ChemicalSubstanceCode);
			});

			CombineAssertions("Bills.PreviousDocuments", () =>
			{
				var billPreviousDocuments = clonedHeader.Bills[2].PreviousDocuments;
				AssertEquals(1, billPreviousDocuments.Count);
				AssertPreviousDocument(billPreviousDocuments[0], "P003", 3);
			});

			CombineAssertions("Bills.SupportingDocuments", () =>
			{
				var billSupportingDocuments = clonedHeader.Bills[2].SupportingDocuments;
				AssertEquals(1, billSupportingDocuments.Count);
				AssertSupportingDocument(billSupportingDocuments[0], "S003", "SUP_REF3");
			});

			CombineAssertions("Bills.AdditionalInfos", () =>
			{
				var billAdditionalInfos = clonedHeader.Bills[2].AdditionalInfos;
				AssertEquals(1, billAdditionalInfos.Count);
				AssertAdditionalInfo(billAdditionalInfos[0], "A003", "ADD_REF3", "SUB3", "Additional Desc3");
			});

			CombineAssertions("Bills.SupplyChainActors", () =>
			{
				var billSupplyChainActors = clonedHeader.Bills[2].SupplyChainActors;
				AssertEquals(1, billSupplyChainActors.Count);
				AssertSupplyChainActor(billSupplyChainActors[0], "CS", actor3.MainAddress.PK, "LVEoriActor3");
			});

			CombineAssertions("Bills.PackedItems.PreviousDocuments", () =>
			{
				var packedItem = clonedHeader.Bills[1].PackedItems[0];
				var previousDocuments = packedItem.PreviousDocuments;
				AssertEquals(2, previousDocuments.Count);

				AssertPreviousDocument(previousDocuments[0], "P001", 1);
				AssertPreviousDocument(previousDocuments[1], "P002", 2);
			});

			CombineAssertions("Bills.PackedItems.SupportingDocuments", () =>
			{
				var packedItem = clonedHeader.Bills[1].PackedItems[0];
				var supportingDocuments = packedItem.SupportingDocuments;
				AssertEquals(2, supportingDocuments.Count);

				AssertSupportingDocument(supportingDocuments[0], "S001", "SUP_REF1");
				AssertSupportingDocument(supportingDocuments[1], "S002", "SUP_REF2");
			});

			CombineAssertions("Bills.PackedItems.AdditionalInfos", () =>
			{
				var packedItem = clonedHeader.Bills[1].PackedItems[0];
				var additionalInfos = packedItem.AdditionalInfos;
				AssertEquals(2, additionalInfos.Count);

				AssertAdditionalInfo(additionalInfos[0], "A001", "ADD_REF1", "SUB1", "Additional Desc1");
				AssertAdditionalInfo(additionalInfos[1], "A002", "ADD_REF2", "SUB2", "Additional Desc2");
			});

			CombineAssertions("Bills.PackedItems.SupplyChainActors", () =>
			{
				var packedItem = clonedHeader.Bills[1].PackedItems[0];
				var supplyChainActors = packedItem.SupplyChainActors;
				AssertEquals(2, supplyChainActors.Count);

				AssertSupplyChainActor(supplyChainActors[0], "FW", actor1.MainAddress.PK, "LVEoriActor1");
				AssertSupplyChainActor(supplyChainActors[1], "MF", actor2.MainAddress.PK, "LVEoriActor2");
			});

			void AssertPreviousDocument(TemporaryStoragePreviousDocument doc, ZString code, ZInt lineNo)
			{
				AssertEquals(code, doc.CSI_Code);
				AssertEquals(lineNo, doc.CSI_LineNo);
			}

			void AssertSupportingDocument(TemporaryStorageSupportingDocument doc, ZString code, ZString reference)
			{
				AssertEquals(code, doc.CSI_Code);
				AssertEquals(reference, doc.CSI_ReferenceNumber);
			}

			void AssertAdditionalInfo(TemporaryStorageAdditionalInfo additionalInfo, ZString code, ZString reference, ZString subType, ZString description)
			{
				AssertEquals(code, additionalInfo.CSI_Code);
				AssertEquals(reference, additionalInfo.CSI_ReferenceNumber);
				AssertEquals(subType, additionalInfo.CSI_SubType);
				AssertEquals(description, additionalInfo.CSI_Description);
			}

			void AssertSupplyChainActor(CusSupplyChainActorReference actor, ZString code, ZGuid owner, ZString reference)
			{
				AssertEquals(code, actor.CFR_Code);
				AssertEquals(owner, actor.CFR_OA_Owner);
				AssertEquals(reference, actor.CFR_Reference);
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

			actor1 = Factory.New<OrgHeader>();
			actor1.OH_Code = "actor1";
			actor1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriActor1", Core.Constants.CountryCodes.Latvia);
			actor2 = Factory.New<OrgHeader>();
			actor2.OH_Code = "actor2";
			actor2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriActor2", Core.Constants.CountryCodes.Latvia);
			actor3 = Factory.New<OrgHeader>();
			actor3.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EoriActor3", Core.Constants.CountryCodes.Latvia);
			actor3.OH_Code = "actor3";
		}

		TemporaryStorageHeader GetTempHeaderForTest()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			tempHeader.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
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
			headerPreDoc.CSI_LineNo = 0;

			var masterBill = tempHeader.MasterBill;
			masterBill.ABL_ShipmentType = "SKK";
			masterBill.ABL_RL_NKOrigin = "FR27B";
			masterBill.ABL_RL_NKFinalDestination = "LV6LV";
			masterBill.ABL_RL_NKPortOfDischarge = "LV6LV";
			masterBill.ABL_RL_NKPortOfLoading = "FR27B";
			masterBill.ABL_OA_Consignee = consignee.MainAddress.PK;
			masterBill.ABL_OA_Shipper = shipper.MainAddress.PK;
			masterBill.ABL_OA_NotifyParty = notifyParty.MainAddress.PK;

			var houseBill1 = tempHeader.Bills.AddNew();
			var houseBill2 = tempHeader.Bills.AddNew();

			var pack = houseBill1.Packs.AddNew();
			pack.APA_PackQty = 23;
			pack.APA_PackUQ = "KG";
			pack.APA_GoodsDescription = "CokeCola Pack";
			pack.APA_CommodityCode = "0001";

			var packedItem = houseBill1.PackedItems.AddNew();
			packedItem.API_Tariff = "987654321";
			packedItem.API_GoodsDescription = "CokeCola Items";
			packedItem.API_ChemicalSubstanceCode = "011233-2";

			var itemSupDoc1 = packedItem.SupportingDocuments.AddNew();
			itemSupDoc1.CSI_Code = "S001";
			itemSupDoc1.CSI_ReferenceNumber = "SUP_REF1";
			var itemSupDoc2 = packedItem.SupportingDocuments.AddNew();
			itemSupDoc2.CSI_Code = "S002";
			itemSupDoc2.CSI_ReferenceNumber = "SUP_REF2";

			var itemPreDoc1 = packedItem.PreviousDocuments.AddNew();
			itemPreDoc1.CSI_Code = "P001";
			itemPreDoc1.CSI_ReferenceNumber = "PRE_REF1";
			itemPreDoc1.CSI_LineNo = 1;
			var itemPreDoc2 = packedItem.PreviousDocuments.AddNew();
			itemPreDoc2.CSI_Code = "P002";
			itemPreDoc2.CSI_ReferenceNumber = "PRE_REF2";
			itemPreDoc2.CSI_LineNo = 2;

			var itemAddInfo1 = packedItem.AdditionalInfos.AddNew();
			itemAddInfo1.CSI_Code = "A001";
			itemAddInfo1.CSI_ReferenceNumber = "ADD_REF1";
			itemAddInfo1.CSI_SubType = "SUB1";
			itemAddInfo1.CSI_Description = "Additional Desc1";
			var itemAddInfo2 = packedItem.AdditionalInfos.AddNew();
			itemAddInfo2.CSI_Code = "A002";
			itemAddInfo2.CSI_ReferenceNumber = "ADD_REF2";
			itemAddInfo2.CSI_SubType = "SUB2";
			itemAddInfo2.CSI_Description = "Additional Desc2";

			var itemActor1 = packedItem.SupplyChainActors.AddNew();
			itemActor1.CFR_Code = "FW";
			itemActor1.OwnerOrgPK = actor1.PK;
			var itemActor2 = packedItem.SupplyChainActors.AddNew();
			itemActor2.CFR_Code = "MF";
			itemActor2.OwnerOrgPK = actor2.PK;

			var billSupDoc = houseBill2.SupportingDocuments.AddNew();
			billSupDoc.CSI_Code = "S003";
			billSupDoc.CSI_ReferenceNumber = "SUP_REF3";

			var billPreDoc = houseBill2.PreviousDocuments.AddNew();
			billPreDoc.CSI_Code = "P003";
			billPreDoc.CSI_ReferenceNumber = "PRE_REF3";
			billPreDoc.CSI_LineNo = 3;

			var billAddInfo = houseBill2.AdditionalInfos.AddNew();
			billAddInfo.CSI_Code = "A003";
			billAddInfo.CSI_ReferenceNumber = "ADD_REF3";
			billAddInfo.CSI_SubType = "SUB3";
			billAddInfo.CSI_Description = "Additional Desc3";

			var billActor = houseBill2.SupplyChainActors.AddNew();
			billActor.CFR_Code = "CS";
			billActor.OwnerOrgPK = actor3.PK;
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
		OrgHeader actor1;
		OrgHeader actor2;
		OrgHeader actor3;
	}
}
