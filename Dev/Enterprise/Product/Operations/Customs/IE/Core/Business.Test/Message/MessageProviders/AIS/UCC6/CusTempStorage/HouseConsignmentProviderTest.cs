using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class HouseConsignmentProviderTest : DataProviderTestCase<HouseConsignmentProvider>
	{
		public void TestIHouseConsignment()
		{
			Assert("Should implement IHouseConsignment", Provider is IHouseConsignment);
		}

		public void TestConsignmentItems()
		{
			HouseBill.PackedItems.AddNew();
			AssertType<ConsignmentItemProvider>("One and only one ConsignmentItemProvider for ConsignmentItem", Provider.ConsignmentItems.Single());
		}

		public void TestTotalGrossMass()
		{
			HouseBill.ABL_GrossWeight = 1000;
			HouseBill.ABL_GrossWeightUQ = "KG";
			AssertEquals("GoodsMeasure GrossMass", 1000m, Provider.TotalGrossMass);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var actor = HouseBill.SupplyChainActors.AddNew();
			actor.CFR_Reference = "IdentificationNo";
			actor.CFR_Code = "CS";
			Assert("Should be IReadOnlyCollection<AdditionalSupplyChainActorProvider>", Provider.AdditionalSupplyChainActors is IReadOnlyCollection<AdditionalSupplyChainActorProvider>);
			AssertEquals("AdditionalSupplyChainActor number", 1, Provider.AdditionalSupplyChainActors.Count);
			AssertEquals("AdditionalSupplyChainActor ID", "IdentificationNo", Provider.AdditionalSupplyChainActors.FirstOrDefault().ID);
			AssertEquals("AdditionalSupplyChainActor Role", "CS", Provider.AdditionalSupplyChainActors.FirstOrDefault().Role);
		}

		public void TestConsignee()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Phone = "+35312345678";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Bob the Builder";
			orgAddress.OA_Address1 = "No. 14";
			orgAddress.OA_Address2 = "Main Road";
			orgAddress.OA_City = "Borris";
			orgAddress.OA_PostCode = "X23 Y65";
			orgAddress.OA_RN_NKCountryCode = "IE";
			using (HouseBill.GetValidationSuspender())
			{
				HouseBill.ABL_OA_Consignee = orgAddress.PK;
				HouseBill.ABL_ConsigneeRegNo = "REG0192";
				HouseBill.ABL_ConsigneeRegNoType = "2";
			}
			AssertType<PersonProvider>(Provider.Consignee);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Bob the Builder", Provider.Consignee.Name);
				AssertEquals("Id", "REG0192", Provider.Consignee.Id);
				AssertEquals("TypeOfPerson", "2", Provider.Consignee.TypeOfPerson);
				AssertEquals("StreetAndNumber", "No. 14", Provider.Consignee.Address.StreetAndNumber);
				AssertEquals("StreetAdditionalLine", "Main Road", Provider.Consignee.Address.StreetAdditionalLine);
				AssertEquals("City", "Borris", Provider.Consignee.Address.City);
				AssertEquals("Country", "IE", Provider.Consignee.Address.Country);
				AssertEquals("Postcode", "X23 Y65", Provider.Consignee.Address.Postcode);
				AssertEquals("Communication Id", "+35312345678", Provider.Consignee.Communication.First().Id);
				AssertEquals("Communication Type", "TE", Provider.Consignee.Communication.First().Type);
			});
		}

		public void TestConsignor()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Email = "joe@bloggs.ie";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Joe Bloggs";
			orgAddress.OA_Address1 = "10 Main Street";
			orgAddress.OA_City = "Berlin";
			orgAddress.OA_PostCode = "DE00012";
			orgAddress.OA_RN_NKCountryCode = "DE";
			using (HouseBill.GetValidationSuspender())
			{
				HouseBill.ABL_OA_Shipper = orgAddress.PK;
				HouseBill.ABL_ShipperRegNo = "JB4443";
				HouseBill.ABL_ShipperRegNoType = "2";
			}
			AssertType<PersonProvider>(Provider.Consignor);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Joe Bloggs", Provider.Consignor.Name);
				AssertEquals("Id", "JB4443", Provider.Consignor.Id);
				AssertEquals("TypeOfPerson", "2", Provider.Consignor.TypeOfPerson);
				AssertEquals("StreetAndNumber", "10 Main Street", Provider.Consignor.Address.StreetAndNumber);
				AssertEquals("StreetAdditionalLine", "", Provider.Consignor.Address.StreetAdditionalLine);
				AssertEquals("City", "Berlin", Provider.Consignor.Address.City);
				AssertEquals("Country", "DE", Provider.Consignor.Address.Country);
				AssertEquals("Postcode", "DE00012", Provider.Consignor.Address.Postcode);
				AssertEquals("Communication Id", "joe@bloggs.ie", Provider.Consignor.Communication.First().Id);
				AssertEquals("Communication Type", "EM", Provider.Consignor.Communication.First().Type);
			});
		}

		public void TestNotifyParty()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var customsContact = orgHeader.Contacts.AddNew();
			customsContact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			customsContact.OC_Email = "jim@bloggs.ie";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.CompanyName = "Jim Bloggs";
			orgAddress.OA_Address1 = "10 Main Street";
			orgAddress.OA_City = "Berlin";
			orgAddress.OA_PostCode = "DE00012";
			orgAddress.OA_RN_NKCountryCode = "DE";

			HouseBill.ABL_OA_NotifyParty = orgAddress.PK;
			HouseBill.ABL_NotifyPartyRegNo = "KLS232";
			HouseBill.ABL_NotifyPartyRegNoType = "2";

			AssertType<PersonProvider>(Provider.NotifyParty);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "Jim Bloggs", Provider.NotifyParty.Name);
				AssertEquals("Id", "KLS232", Provider.NotifyParty.Id);
				AssertEquals("TypeOfPerson", "2", Provider.NotifyParty.TypeOfPerson);
				AssertEquals("StreetAndNumber", "10 Main Street", Provider.NotifyParty.Address.StreetAndNumber);
				AssertEquals("StreetAdditionalLine", "", Provider.NotifyParty.Address.StreetAdditionalLine);
				AssertEquals("City", "Berlin", Provider.NotifyParty.Address.City);
				AssertEquals("Country", "DE", Provider.NotifyParty.Address.Country);
				AssertEquals("Postcode", "DE00012", Provider.NotifyParty.Address.Postcode);
				AssertEquals("Communication Id", "jim@bloggs.ie", Provider.NotifyParty.Communication.First().Id);
				AssertEquals("Communication Type", "EM", Provider.NotifyParty.Communication.First().Type);
			});
		}

		public void TestUCR()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_UCRNumber = "12345";
			var provider = new HouseConsignmentProvider(bill);
			AssertEquals("UCR ReferenceNumber", "12345", provider.UCR);
		}

		public void TestPreviousDocuments()
		{
			HouseBill.PreviousDocuments.AddNew();
			HouseBill.PreviousDocuments.AddNew();
			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("Count", 2, previousDocuments.Count);
			Assert("Should be IReadOnlyCollection<PreviousDocumentProvider>", previousDocuments is IReadOnlyCollection<PreviousDocumentProvider>);
		}

		public void TestSupportingDocuments()
		{
			HouseBill.SupportingDocuments.AddNew();
			HouseBill.SupportingDocuments.AddNew();
			var supportingDocuments = Provider.SupportingDocuments;
			AssertEquals("Count", 2, supportingDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", supportingDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestTransportContractDocuments()
		{
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "TRA";
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "TRA";
			var transportContractDocuments = Provider.TransportContractDocuments;
			AssertEquals("Count", 2, transportContractDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", transportContractDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestTransportEquipments()
		{
			var container = Header.Containers.AddNew();
			var pack = HouseBill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			HouseBill.PackedItems.AddNew();
			AssertType<TransportEquipmentProvider>("One and only one TransportEquipment06Provider for TransportEquipment", Provider.TransportEquipments.Single());
		}

		public void TestAdditionalReferences()
		{
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "REF";
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "REF";
			var additionalReferences = Provider.AdditionalReferences;
			AssertEquals("Count", 2, additionalReferences.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", additionalReferences is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestAdditionalInformations()
		{
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "INF";
			HouseBill.AdditionalInfos.AddNew().CSI_SubType = "INF";
			var additionalInformations = Provider.AdditionalInformations;
			AssertEquals("Count", 2, additionalInformations.Count);
			Assert("Should be IReadOnlyCollection<AdditionalInformationProvider>", additionalInformations is IReadOnlyCollection<AdditionalInformationProvider>);
		}

		protected override HouseConsignmentProvider GetProvider() => HouseConsignmentProvider.New(HouseBill);

		TemporaryStorageHeader Header => header ?? (header = Factory.New<TemporaryStorageHeader>());
		TemporaryStorageHeader header;

		TemporaryStorageBill HouseBill => houseBill ?? (houseBill = Header.Bills.AddNew());
		TemporaryStorageBill houseBill;
	}
}
