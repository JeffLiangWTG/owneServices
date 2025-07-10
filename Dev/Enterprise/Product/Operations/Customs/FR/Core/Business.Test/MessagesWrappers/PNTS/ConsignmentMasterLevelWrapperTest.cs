using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class ConsignmentMasterLevelWrapperTest : ConsignmentDataProviderTestCase<ConsignmentMasterLevelWrapper>
	{
		public void TestAdditionalInformation()
		{
			AssertType<Collection<IAdditionalInformation>>("AdditionalInformation type", Provider.AdditionalInformation);
			AssertContainsExactElementsInAnyOrder("There should be 2 distinct (filtered on code) elements in AdditionalInformation, matching entry additional infos of type INF. ", new string[] { "INF1", "INF2" }, Provider.AdditionalInformation.Select(x => x.Code));

			var wrapper = ConsignmentHouseLevelWrapper.New(Factory.New<TemporaryStorageBill>());
			AssertEquals("There should be 0 AdditionalInformation.", 0, wrapper.AdditionalInformation.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertType<Collection<IAdditionalSupplyChainActor>>("AdditionalSupplyChainActor type", Provider.AdditionalSupplyChainActor);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on role and reference number) elements in AdditionalSupplyChainActor, matching entry instruction.", new string[] { "ABC|SCREF1", "ABC|SCREF1", "ABC|SCREF2", "DEF|SCREF2" }, Provider.AdditionalSupplyChainActor.Select(x => x.Role + "|" + x.IdentificationNumber));

			var wrapper = ConsignmentHouseLevelWrapper.New(Factory.New<TemporaryStorageBill>());
			AssertEquals("There should be 0 AdditionalSupplyChainActor.", 0, wrapper.AdditionalSupplyChainActor.Count);
		}

		public void TestConsignee()
		{
			AssertEquals("City should equal OA_City", "Insomnia", Provider.Consignee.Address.City);
			AssertEquals("Country should equal OA_RN_NKCountryCode", "LS", Provider.Consignee.Address.Country);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignee.Address.Number);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignee.Address.PoBox);
			AssertEquals("PostCode should equal OA_PostCode", "000000", Provider.Consignee.Address.PostCode);
			AssertEquals("Street should equal OA_Address1", "Kings Street", Provider.Consignee.Address.Street);
			AssertEquals("StreetAdditionalLine should equal OA_Address2", "No.001", Provider.Consignee.Address.StreetAdditionalLine);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignee.Address.SubDivision);
			AssertEquals("Communication has 2 elements", 2, Provider.Consignee.Communication.Count);
			AssertEquals("Identifier should equal OC_Email", "a@a.com", Provider.Consignee.Communication.First().Identifier);
			AssertEquals("Type should equal EM", "EM", Provider.Consignee.Communication.First().Type);
			AssertEquals("IdentificationNumber should return Consignee EORI when no EORI has been set against the address.", "FR1", Provider.Consignee.IdentificationNumber);
			AssertEquals("Name should equal bill.Consignee?.Header?.OH_FullName", "Name", Provider.Consignee.Name);
			AssertEquals("TypeOfPerson should equal bill.ABL_ConsigneeRegNoType", (byte)2, Provider.Consignee.TypeOfPerson);
		}

		public void TestConsignmentItemMasterLevel()
		{
			AssertEquals("ConsignmentItemMasterLevel has 2 elements", 2, Provider.ConsignmentItemMasterLevel.Count);
			AssertType<ConsignmentItemWrapper>("First Consignment Item has a correct wrapper", Provider.ConsignmentItemMasterLevel.First());
			AssertType<ConsignmentItemWrapper>("Second Consignment Item has a correct wrapper", Provider.ConsignmentItemMasterLevel.Skip(1).First());
		}

		public void TestConsignor()
		{
			AssertEquals("City should equal OA_City", "Sleep", Provider.Consignor.Address.City);
			AssertEquals("Country should equal OA_RN_NKCountryCode", "FR", Provider.Consignor.Address.Country);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignor.Address.Number);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignor.Address.PoBox);
			AssertEquals("PostCode should equal OA_PostCode", "000001", Provider.Consignor.Address.PostCode);
			AssertEquals("Street should equal OA_Address1", "Queens Street", Provider.Consignor.Address.Street);
			AssertEquals("StreetAdditionalLine should equal OA_Address2", "No.002", Provider.Consignor.Address.StreetAdditionalLine);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.Consignor.Address.SubDivision);
			AssertEquals("Communication has 2 elements", 2, Provider.Consignor.Communication.Count);
			AssertEquals("Identifier should equal OC_Email", "a@a.com", Provider.Consignor.Communication.First().Identifier);
			AssertEquals("Type should equal EM", "EM", Provider.Consignor.Communication.First().Type);
			AssertEquals("IdentificationNumber should return Consignor EORI when no EORI has been set against the address.", "FR1", Provider.Consignor.IdentificationNumber);
			AssertEquals("Name should equal bill.Consignee?.Header?.OH_FullName", "Name", Provider.Consignor.Name);
			AssertEquals("TypeOfPerson should equal bill.ABL_ShipperRegNoType", (byte)3, Provider.Consignor.TypeOfPerson);
		}

		public void TestNotifyParty()
		{
			AssertEquals("City should equal OA_City", "BaldursGate", Provider.NotifyParty.Address.City);
			AssertEquals("Country should equal OA_RN_NKCountryCode", "SP", Provider.NotifyParty.Address.Country);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.NotifyParty.Address.Number);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.NotifyParty.Address.PoBox);
			AssertEquals("PostCode should equal OA_PostCode", "000002", Provider.NotifyParty.Address.PostCode);
			AssertEquals("Street should equal OA_Address1", "Blushing Mermaid", Provider.NotifyParty.Address.Street);
			AssertEquals("StreetAdditionalLine should equal OA_Address2", "No.003", Provider.NotifyParty.Address.StreetAdditionalLine);
			AssertEquals("waiting to be done in the future", string.Empty, Provider.NotifyParty.Address.SubDivision);
			AssertEquals("Communication has 2 elements", 2, Provider.NotifyParty.Communication.Count);
			AssertEquals("Identifier should equal OC_Email", "a@a.com", Provider.NotifyParty.Communication.First().Identifier);
			AssertEquals("Type should equal EM", "EM", Provider.NotifyParty.Communication.First().Type);
			AssertEquals("IdentificationNumber should return NotifyParty EORI when no EORI has been set against the address.", "FR1", Provider.NotifyParty.IdentificationNumber);
			AssertEquals("Name should equal bill.Consignee?.Header?.OH_FullName", "Name", Provider.NotifyParty.Name);
			AssertEquals("TypeOfPerson should equal bill.ABL_NotifyPartyRegNoType", (byte)4, Provider.NotifyParty.TypeOfPerson);
		}

		public void TestPreviousDocument()
		{
			AssertEquals("Wrapper PreviousDocument is null if no previous document.", null, Provider.PreviousDocument);

			var previousDocument = bill.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "N355";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_ReferenceNumber = "10";

			var provider = ConsignmentHouseLevelWrapper.New(bill);
			AssertType<PreviousDocumentWrapper>("Wrapper PreviousDocument is not null if previous document is under bill.", provider.PreviousDocument);
			AssertEquals("1", provider.PreviousDocument.GoodsItemIdentifier);
			AssertEquals("10", provider.PreviousDocument.ReferenceNumber);
			AssertEquals("N355", provider.PreviousDocument.Type);

			previousDocument = bill.Header.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "N821";
			previousDocument.CSI_LineNo = 2;
			previousDocument.CSI_ReferenceNumber = "20";
			provider = ConsignmentHouseLevelWrapper.New(bill);
			AssertType<PreviousDocumentWrapper>("Wrapper PreviousDocument is not null if previous document is under bill and header.", provider.PreviousDocument);
			AssertEquals("1", provider.PreviousDocument.GoodsItemIdentifier);
			AssertEquals("10", provider.PreviousDocument.ReferenceNumber);
			AssertEquals("N355", provider.PreviousDocument.Type);

			bill.PreviousDocuments.DeleteAll();
			provider = ConsignmentHouseLevelWrapper.New(bill);
			AssertType<PreviousDocumentWrapper>("Wrapper PreviousDocument is not null if previous document is under header.", provider.PreviousDocument);
			AssertEquals("2", provider.PreviousDocument.GoodsItemIdentifier);
			AssertEquals("20", provider.PreviousDocument.ReferenceNumber);
			AssertEquals("N821", provider.PreviousDocument.Type);
		}

		public void TestReceptacle()
		{
			AssertEquals("Waiting to check if it is necessary", 0, Provider.Receptacle.Count);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCRProperty should equal ABL_UCRNumber", "UCRNumber", Provider.ReferenceNumberUCR.ReferenceNumberUCRProperty);
		}

		public void TestSupportingDocument()
		{
			AssertType<Collection<ISupportingDocument>>("SupportingDocument type", Provider.SupportingDocument);
			AssertContainsExactElementsInAnyOrder("There should be 4 elements in SupportingDocument, matching all entry supporting documents.", new string[] { "N380|SUP1", "N380|SUP1", "N380|SUP2", "N270|SUP1" }, Provider.SupportingDocument.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 SupportingDocument.", 0, wrapper.SupportingDocument.Count);
		}

		public void TestTotalGrossMass()
		{
			AssertEquals("TotalGrossMass should equal ABL_GrossWeight", (decimal)1.00, Provider.TotalGrossMass);
		}

		public void TestTotalGrossMassValue()
		{
			AssertEquals("TotalGrossMassValue should equal TotalGrossMass's value", (decimal)1.00, Provider.TotalGrossMassValue);
		}

		public void TestTotalGrossMassValueSpecified()
		{
			AssertEquals("TotalGrossMassValueSpecified should be true", true, Provider.TotalGrossMassValueSpecified);
		}

		public void TestTransportDocument()
		{
			AssertEquals("ReferenceNumber should equal ABL_BillNumber", "1X", Provider.TransportDocument.ReferenceNumber);
			AssertEquals("TODO: WI00524630", string.Empty, Provider.TransportDocument.Type);
		}

		public void TestTransportEquipment()
		{
			AssertEquals("TransportEquipment has 3 elements", 3, Provider.TransportEquipment.Count);

			var transportEquipment1 = Provider.TransportEquipment.First();

			AssertEquals("Wrapper ContainerIdentificationNumber should equal ACN_ContainerNumber.", "ContainerNumber1", transportEquipment1.ContainerIdentificationNumber);
			AssertEquals("Wrapper ContainerPackedStatus should equal ACN_EmptyFullIndicator.", "FUL", transportEquipment1.ContainerPackedStatus);
			AssertEquals("Wrapper ContainerIdentificationNumber should count non empty seals.", "3", transportEquipment1.NumberOfSeals);
			AssertContainsExactElementsInAnyOrder("Wrapper ContainerIdentificationNumber count.", new string[] { "SEAL1", "SEAL2", "SEAL3" }, transportEquipment1.Seal.Select(x => x.Identifier).ToArray());

			var transportEquipment2 = Provider.TransportEquipment.Skip(1).First();
			AssertEquals("Wrapper ContainerIdentificationNumber should equal ACN_ContainerNumber.", "ContainerNumber3", transportEquipment2.ContainerIdentificationNumber);
			AssertEquals("Wrapper ContainerPackedStatus should equal ACN_EmptyFullIndicator.", "FUL", transportEquipment2.ContainerPackedStatus);
			AssertEquals("Wrapper ContainerIdentificationNumber should count non empty seals.", "2", transportEquipment2.NumberOfSeals);
			AssertContainsExactElementsInAnyOrder("Wrapper ContainerIdentificationNumber count.", new string[] { "SEAL1", "SEAL2" }, transportEquipment2.Seal.Select(x => x.Identifier).ToArray());

			var transportEquipment3 = Provider.TransportEquipment.Skip(2).First();
			AssertEquals("Wrapper ContainerIdentificationNumber should equal ACN_ContainerNumber.", "ContainerNumber4", transportEquipment3.ContainerIdentificationNumber);
			AssertEquals("Wrapper ContainerPackedStatus should equal ACN_EmptyFullIndicator.", "FUL", transportEquipment3.ContainerPackedStatus);
			AssertEquals("Wrapper ContainerIdentificationNumber should count non empty seals.", "1", transportEquipment3.NumberOfSeals);
			AssertContainsExactElementsInAnyOrder("Wrapper ContainerIdentificationNumber count.", new string[] { "SEAL1" }, transportEquipment3.Seal.Select(x => x.Identifier).ToArray());

			var bill = Factory.New<TemporaryStorageBill>();
			var pack = bill.Packs.AddNew();
			var wrapper = ConsignmentMasterLevelWrapper.New(bill);
			AssertEquals("TransportEquipment should have 0 element", 0, wrapper.TransportEquipment.Count);
		}

		protected override ConsignmentMasterLevelWrapper GetProvider()
		{
			bill = GetBill();
			return ConsignmentMasterLevelWrapper.New(bill);
		}

		TemporaryStorageBill GetBill()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = temporaryStorageHeader.Bills.AddNew();

			var addressConsignee = Factory.New<OrgAddress>();
			addressConsignee.OA_City = "Insomnia";
			addressConsignee.OA_RN_NKCountryCode = "LS";
			addressConsignee.OA_PostCode = "000000";
			addressConsignee.OA_Address1 = "Kings Street";
			addressConsignee.OA_Address2 = "No.001";

			var addressConsignor = Factory.New<OrgAddress>();
			addressConsignor.OA_City = "Sleep";
			addressConsignor.OA_RN_NKCountryCode = "FR";
			addressConsignor.OA_PostCode = "000001";
			addressConsignor.OA_Address1 = "Queens Street";
			addressConsignor.OA_Address2 = "No.002";

			var addressNotify = Factory.New<OrgAddress>();
			addressNotify.OA_City = "BaldursGate";
			addressNotify.OA_RN_NKCountryCode = "SP";
			addressNotify.OA_PostCode = "000002";
			addressNotify.OA_Address1 = "Blushing Mermaid";
			addressNotify.OA_Address2 = "No.003";

			var header = Factory.New<OrgHeader>();
			var contact = header.Contacts.AddNew();
			contact.OC_Email = "a@a.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			var contact2 = header.Contacts.AddNew();
			contact2.OC_Email = "c@c.com";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			var contact3 = header.Contacts.AddNew();
			contact3.OC_Email = "b@b.com";

			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = GlbCompany.CurrentCompany.Country.LocalBusinessRegNoCodeType;
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			header.LocalBusinessRegNoObject.OK_CustomsRegNo = "1";
			header.OH_FullName = "Name";
			addressConsignee.OA_OH = header.PK;
			addressConsignor.OA_OH = header.PK;
			addressNotify.OA_OH = header.PK;

			bill.ABL_OA_Consignee = addressConsignee.PK;
			bill.ABL_ConsigneeRegNoType = "2";
			bill.ABL_OA_Shipper = addressConsignor.PK;
			bill.ABL_ShipperRegNoType = "3";
			bill.ABL_OA_NotifyParty = addressNotify.PK;
			bill.ABL_NotifyPartyRegNoType = "4";

			var container1 = CreateContainer(temporaryStorageHeader, "ContainerNumber1", "FUL", "SEAL1", "SEAL2", "SEAL3");
			var container2 = CreateContainer(temporaryStorageHeader, "ContainerNumber2", "FUL", "SEAL1");
			var container3 = CreateContainer(temporaryStorageHeader, "ContainerNumber3", "FUL", "SEAL1", "SEAL2");
			var container4 = CreateContainer(temporaryStorageHeader, "ContainerNumber4", "FUL", "SEAL1");

			var pack1 = CreatePack(bill, container1, 3, "Marks & numbers 1", "CTN");
			var pack2 = CreatePack(bill, container1, 5, "Marks & numbers 2", "CTN");
			var pack3 = CreatePack(bill, container3, 7, "Marks & numbers 3", "CTN");
			var pack4 = CreatePack(bill, container4, 9, "Marks & numbers 4", "CTN");

			var packedItem1 = pack1.PackedItems.AddNewPackedItem();
			packedItem1.API_GoodsDescription = "GoodsDescription1";
			packedItem1.API_Tariff = "2345167890";
			packedItem1.API_GrossWeight = 55.33m;

			packedItem1.AsycudaPackPackedItemPivots.AddPivotFor(pack2);
			packedItem1.AsycudaPackPackedItemPivots.AddPivotFor(pack3);

			var packedItem2 = pack1.PackedItems.AddNewPackedItem();
			packedItem2.API_GoodsDescription = "GoodsDescription2";
			packedItem2.API_Tariff = "4516789012";
			packedItem2.API_GrossWeight = 45.22m;
			packedItem2.AsycudaPackPackedItemPivots.AddPivotFor(pack4);

			bill.ABL_UCRNumber = "UCRNumber";
			bill.ABL_GrossWeight = 1.00;
			bill.ABL_BillNumber = "1X";
			bill.ContainerPK = container1.PK;

			var informationAdditionalInfo1 = bill.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";
			informationAdditionalInfo1.CSI_Description = "INFDescription1";

			var informationAdditionalInfo2 = bill.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF2";
			informationAdditionalInfo2.CSI_Description = "INFDescription2";

			var referenceAdditionalInfo1 = bill.AdditionalInfos.AddNew();
			referenceAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo1.CSI_Code = "CD1";
			referenceAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo2 = bill.AdditionalInfos.AddNew();
			referenceAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo2.CSI_Code = "CD1";
			referenceAdditionalInfo2.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo3 = bill.AdditionalInfos.AddNew();
			referenceAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo3.CSI_Code = "CD2";
			referenceAdditionalInfo3.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo4 = bill.AdditionalInfos.AddNew();
			referenceAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo4.CSI_Code = "CD2";
			referenceAdditionalInfo4.CSI_ReferenceNumber = "REF2";

			var cusSupplyChainActorReferences1 = bill.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences1.CFR_Code = "ABC";
			cusSupplyChainActorReferences1.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences2 = bill.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences2.CFR_Code = "ABC";
			cusSupplyChainActorReferences2.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences3 = bill.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences3.CFR_Code = "ABC";
			cusSupplyChainActorReferences3.CFR_Reference = "SCREF2";
			var cusSupplyChainActorReferences4 = bill.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences4.CFR_Code = "DEF";
			cusSupplyChainActorReferences4.CFR_Reference = "SCREF2";

			var supportingDocument1 = bill.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "N380";
			supportingDocument1.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument1.CSI_ReferenceNumber = "SUP1";

			var supportingDocument3 = bill.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "N380";
			supportingDocument3.CSI_DateOfExpiry = new ZDateTime(2023, 01, 02);
			supportingDocument3.CSI_ReferenceNumber = "SUP1";

			var supportingDocument4 = bill.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "N380";
			supportingDocument4.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument4.CSI_ReferenceNumber = "SUP2";

			var supportingDocument5 = bill.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "N270";
			supportingDocument5.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument5.CSI_ReferenceNumber = "SUP1";

			return bill;
		}

		TemporaryStorageBill bill;
	}
}
