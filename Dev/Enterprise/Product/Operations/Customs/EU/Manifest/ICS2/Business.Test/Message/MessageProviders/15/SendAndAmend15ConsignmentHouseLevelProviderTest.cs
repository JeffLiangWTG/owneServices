using System;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend15ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend15ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(bill));
			});
		}

		public void TestContainerIndicator()
		{
			AssertEquals("0", Provider.ContainerIndicator);
		}

		public void TestTotalGrossMass()
		{
			bill.ABL_GrossWeight = 12312m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 12.312m, Provider.TotalGrossMass);
		}

		public void TestPlaceOfAcceptance()
		{
			bill.ABL_RL_NKOrigin = "FR222";
			AssertEquals("Place of acceptance", "FR222", Provider.PlaceOfAcceptance.Unlocode);
		}

		public void TestSupportingDocuments()
		{
			bill.SupportingDocuments.AddNew();
			AssertEquals("SupportingDocuments Count", 1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalInformation()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformation.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			bill.CusSupplyChainActorReferences.AddNew();
			AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActor.Count);
		}

		public void TestTransportDocumentMasterLevel()
		{
			bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
			AssertEquals("TransportDocumentMasterLevel", "TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestCarrierIdentificationNumber()
		{
			AssertNotNull("CarrierIdentificationNumber", Provider.CarrierIdentificationNumber);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "TestConsignee";
			AssertType<BillPartyProvider>("Consignee", Provider.Consignee);
			AssertEquals("Consignee", "TestConsignee", Provider.Consignee.Name);
		}

		public void TestGoodsItem()
		{
			bill.Packs.AddNew();
			AssertEquals("GoodsItem", 1, Provider.GoodsItem.Count);
		}

		public void TestConsignor()
		{
			bill.ABL_ShipperName = "TestConsignor";
			AssertType<BillPartyProvider>("Consignor", Provider.Consignor);
			AssertEquals("Consignor", "TestConsignor", Provider.Consignor.Name);
		}

		public void TestPaymentMethod()
		{
			AssertEquals("PaymentMethod", string.Empty, Provider.PaymentMethod);

			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;
			AssertEquals("PaymentMethod", EUICS2PaymentMethodList.Codes.D, Provider.PaymentMethod);
		}

		public void TestPlaceOfDelivery()
		{
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Place of delivery", "AUSYD", Provider.PlaceOfDelivery.Unlocode);
		}

		public void TestGoodsShipmentBuyer()
		{
			var buyerOrg = Factory.New<OrgAddress>().PK;
			bill.ABL_OA_Buyer = buyerOrg;
			bill.ABL_BuyerName = "TestBuyer";
			AssertType<BillPartyProvider>("Buyer", Provider.GoodsShipmentBuyer);
			AssertEquals("Buyer", "TestBuyer", Provider.GoodsShipmentBuyer.Name);
		}

		public void TestGoodsShipmentSeller()
		{
			var sellerOrg = Factory.New<OrgAddress>().PK;
			bill.ABL_OA_Seller = sellerOrg;
			bill.ABL_SellerName = "TestSeller";
			AssertType<BillPartyProvider>("Seller", Provider.GoodsShipmentSeller);
			AssertEquals("Seller", "TestSeller", Provider.GoodsShipmentSeller.Name);
		}

		public void TestItineraries()
		{
			var itinerary1 = bill.Header.Itinerary.AddNew();
			itinerary1.CY_Code = "AUSYD";

			var itinerary2 = bill.Header.Itinerary.AddNew();
			itinerary2.CY_Code = "IEADA";

			AssertContainsExactElementsInAnyOrder("Itinerary.Country", new[] { "AU", "IE" }, Provider.Itineraries.Select(c => c.Country));
		}

		public void TestNotifyParty()
		{
			bill.ABL_NotifyPartyName = "TestNotifyParty";
			AssertType<BillPartyProvider>("NotifyParty", Provider.NotifyParty);
			AssertEquals("Notify Party Name", "TestNotifyParty", Provider.NotifyParty.Name);
		}

		public void TestPassiveBorderTransportMeans()
		{
			AssertEquals(Array.Empty<IPassiveBorderTransportMeans>(), Provider.PassiveBorderTransportMeans);
		}

		public void TestSupplementaryDeclarant()
		{
			bill.SupplementaryDeclarants.AddNew();
			AssertEquals("SupplementaryDeclarants Count", 1, Provider.SupplementaryDeclarant.Count);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		public void TestTransportEquipment()
		{
			var container1 = bill.Header.Containers.AddNew();
			var container2 = bill.Header.Containers.AddNew();
			var container3 = bill.Header.Containers.AddNew();

			bill.Packs.AddNew().ContainerPK = container1.PK;
			bill.Packs.AddNew().ContainerPK = container2.PK;

			var transportEquipment = Provider.TransportEquipment;
			AssertEquals(2, transportEquipment.Count);
			AssertType<TransportEquipmentProvider>(transportEquipment.First());
		}

		public void TestUCRNumber()
		{
			bill.ABL_UCRNumber = "1234";
			AssertEquals("UCRNumber", "1234", Provider.UCRNumber);
		}

		SendAndAmend15ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend15ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend15ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
