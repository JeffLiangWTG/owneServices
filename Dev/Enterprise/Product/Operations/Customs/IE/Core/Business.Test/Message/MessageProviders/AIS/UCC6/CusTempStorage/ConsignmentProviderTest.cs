using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class ConsignmentProviderTest : DataProviderTestCase<ConsignmentProvider>
	{
		public void TestIConsignment()
		{
			Assert("Should implement IConsignment", Provider is IConsignment);
		}

		public void TestNew()
		{
			AssertNull(ConsignmentProvider.New(null));
		}

		public void TestHouseConsignments()
		{
			Header.Bills.AddNew();
			Header.Bills.AddNew();
			Assert("Should be IReadOnlyCollection<HouseConsignmentProvider>", Provider.HouseConsignments is IReadOnlyCollection<HouseConsignmentProvider>);
			AssertEquals("One HouseConsignment for each house bills", 2, Provider.HouseConsignments.Count);
		}

		public void TestConsignmentItems()
		{
			Assert("Should be IReadOnlyCollection<ConsignmentItemProvider>", Provider.ConsignmentItems is IReadOnlyCollection<ConsignmentItemProvider>);
		}

		public void TestEstimatedDateAndTimeOfArrivalAtThePortOfUnloading()
		{
			Header.MasterBill.ABL_A_ARV = new ZDateTime(2023, 11, 7);
			AssertEquals("EstimatedDateAndTimeOfArrivalAtThePortOfUnloading returns DateTime(2023, 11, 7)", new DateTime(2023, 11, 7), Provider.EstimatedDateAndTimeOfArrivalAtThePortOfUnloading);
		}

		public void TestGrossMass()
		{
			Header.MasterBill.ABL_GrossWeight = 1000m;
			AssertEquals("GrossMass returns 1000", 1000m, Provider.GrossMass);
		}

		public void TestReceptacleIdentificationNumber()
		{
			AssertNull("ReceptacleIdentificationNumber null", Provider.ReceptacleIdentificationNumber);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var actor = Bill.SupplyChainActors.AddNew();
			actor.CFR_Reference = "IdentificationNo";
			actor.CFR_Code = "CS";
			Assert("Should be IReadOnlyCollection<AdditionalSupplyChainActorProvider>", Provider.AdditionalSupplyChainActors is IReadOnlyCollection<AdditionalSupplyChainActorProvider>);
			AssertEquals("AdditionalSupplyChainActor number", 1, Provider.AdditionalSupplyChainActors.Count);
			AssertEquals("AdditionalSupplyChainActor ID", "IdentificationNo", Provider.AdditionalSupplyChainActors.FirstOrDefault().ID);
			AssertEquals("AdditionalSupplyChainActor Role", "CS", Provider.AdditionalSupplyChainActors.FirstOrDefault().Role);
		}

		public void TestArrivalTransportMeans()
		{
			AssertType<ArrivalTransportMeansProvider>("ArrivalTransportMeans", Provider.ArrivalTransportMeans);
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
			using (Bill.GetValidationSuspender())
			{
				Bill.ABL_OA_Consignee = orgAddress.PK;
				Bill.ABL_ConsigneeRegNo = "REG0192";
				Bill.ABL_ConsigneeRegNoType = "2";
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
			using (Bill.GetValidationSuspender())
			{
				Bill.ABL_OA_Shipper = orgAddress.PK;
				Bill.ABL_ShipperRegNo = "JB4443";
				Bill.ABL_ShipperRegNoType = "2";
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

		public void TestLocationOfGoods()
		{
			AssertType<LocationOfGoodsProvider>("LocationOfGoods", Provider.LocationOfGoods);
		}

		public void TestLoadingLocation()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AAA";
			unloco.RL_RN_NKCountryCode = "FR";

			AssertNull("Port of Loading is not set", Provider.LoadingLocation);

			Bill.ABL_RL_NKPortOfLoading = "AAA";
			var loadingLocation = GetProvider().LoadingLocation;
			CombineAssertions(() =>
			{
				AssertNull("LoadingLocation.Location", loadingLocation.Location);
				AssertEquals("LoadingLocation.UNLOCODE", "AAA", loadingLocation.UNLOCODE);
				AssertEquals("LoadingLocation.Country", "FR", loadingLocation.Country);
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

			Bill.ABL_OA_NotifyParty = orgAddress.PK;
			Bill.ABL_NotifyPartyRegNo = "KLS232";
			Bill.ABL_NotifyPartyRegNoType = "2";
			Bill.ABL_NotifyPartyPhone = "+55439832";

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
				AssertEquals("Communication Count", 1, Provider.NotifyParty.Communication.Count);
				AssertEquals("Communication Id", "+55439832", Provider.NotifyParty.Communication.First().Id);
				AssertEquals("Communication Type", "TE", Provider.NotifyParty.Communication.First().Type);
			});
		}

		public void TestUCR()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.MasterBill.ABL_UCRNumber = "12345";
			var provider = ConsignmentProvider.New(header);
			AssertEquals("UCR ReferenceNumber", "12345", provider.UCR);
		}

		public void TestPlaceOfUnloading()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "BBB";
			unloco.RL_RN_NKCountryCode = "IE";

			AssertNull("Port of Discharge is not set", Provider.PlaceOfUnloading);

			Bill.ABL_RL_NKPortOfDischarge = "BBB";
			var placeOfUnloading = GetProvider().PlaceOfUnloading;
			CombineAssertions(() =>
			{
				AssertNull("PlaceOfUnloading.Location", placeOfUnloading.Location);
				AssertEquals("PlaceOfUnloading.UNLOCODE", "BBB", placeOfUnloading.UNLOCODE);
				AssertEquals("PlaceOfUnloading.Country", "IE", placeOfUnloading.Country);
			});
		}

		public void TestWarehouse()
		{
			Header.AuthorizationUsageOrNew.AGC_Number = "AGC12345";
			AssertEquals("Warehouse.Id", "AGC12345", Provider.Warehouse.Id);
			AssertEquals("Warehouse.Type", "V", Provider.Warehouse.Type);
		}

		public void TestPreviousDocuments()
		{
			Bill.PreviousDocuments.AddNew();
			Bill.PreviousDocuments.AddNew();
			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("Count", 2, previousDocuments.Count);
			Assert("Should be IReadOnlyCollection<PreviousDocumentProvider>", previousDocuments is IReadOnlyCollection<PreviousDocumentProvider>);
		}

		public void TestSupportingDocuments()
		{
			Bill.SupportingDocuments.AddNew();
			Bill.SupportingDocuments.AddNew();
			var supportingDocuments = Provider.SupportingDocuments;
			AssertEquals("Count", 2, supportingDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", supportingDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestTransportContractDocuments()
		{
			Bill.AdditionalInfos.AddNew().CSI_SubType = "TRA";
			Bill.AdditionalInfos.AddNew().CSI_SubType = "TRA";
			var transportContractDocuments = Provider.TransportContractDocuments;
			AssertEquals("Count", 2, transportContractDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", transportContractDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestTransportEquipments()
		{
			Header.Containers.AddNew();
			AssertType<TransportEquipmentProvider>("One TransportEquipment06Provider for one Container", Provider.TransportEquipments.Single());
		}

		public void TestAdditionalInformations()
		{
			Bill.AdditionalInfos.AddNew().CSI_SubType = "INF";
			Bill.AdditionalInfos.AddNew().CSI_SubType = "INF";
			var additionalInformations = Provider.AdditionalInformations;
			AssertEquals("Count", 2, additionalInformations.Count);
			Assert("Should be IReadOnlyCollection<AdditionalInformationProvider>", additionalInformations is IReadOnlyCollection<AdditionalInformationProvider>);
		}

		public void TestInlandModeOfTransport()
		{
			AssertNull("InlandModeOfTransport null", Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			AssertNull("ModeOfTransportAtTheBorder null", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestReferenceNumberUCR()
		{
			Header.MasterBill.ABL_UCRNumber = "12345";
			AssertEquals("ReferenceNumberUCR", "12345", Provider.ReferenceNumberUCR);
		}

		public void TestTotalPackageNumber()
		{
			AssertEquals("TotalPackageNumber", 0, Provider.TotalPackageNumber);
		}

		public void TestTransportAndInsuranceCostsToTheDestination()
		{
			AssertNull("TransportAndInsuranceCostsToTheDestination null", Provider.TransportAndInsuranceCostsToTheDestination);
		}

		public void TestTransportDocument()
		{
			AssertNull("TransportDocument null", Provider.TransportDocument);
		}

		protected override ConsignmentProvider GetProvider() => ConsignmentProvider.New(Header);

		TemporaryStorageBill Bill => bill ??= Header.MasterBill;
		TemporaryStorageBill bill;

		TemporaryStorageHeader Header => header ??= Factory.New<TemporaryStorageHeader>();
		TemporaryStorageHeader header;
	}
}
