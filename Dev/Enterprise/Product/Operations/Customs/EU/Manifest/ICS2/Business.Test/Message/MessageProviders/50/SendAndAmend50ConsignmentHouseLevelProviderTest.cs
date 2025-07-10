using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend50ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend50ConsignmentHouseLevelProvider>
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
			bill.ABL_GrossWeight = 0.2235m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 0.000224m, Provider.TotalGrossMass);
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

		public void TestAdditionalInformationCollection()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			bill.CusSupplyChainActorReferences.AddNew();
			AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestGoodsItems()
		{
			bill.Packs.AddNew();
			AssertEquals("GoodsItem", 1, Provider.GoodsItems.Count);

			AssertType<SendAndAmend50GoodsItemProvider>(Provider.GoodsItems.First());
		}

		public void TestTransportChargesMethodOfPayment()
		{
			AssertEquals("PaymentMethod", string.Empty, Provider.TransportChargesMethodOfPayment);

			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;
			AssertEquals("PaymentMethod", EUICS2PaymentMethodList.Codes.D, Provider.TransportChargesMethodOfPayment);
		}

		public void TestPlaceOfDelivery()
		{
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Place of delivery", "AUSYD", Provider.PlaceOfDelivery.Unlocode);
		}
		public void TestGoodsShipmentBuyer()
		{
			bill.ABL_BuyerName = "TestBuyer";
			AssertType<BillPartyProvider>("NotifyBuyer", Provider.GoodsShipmentBuyer);
			AssertEquals("Buyer Name", "TestBuyer", Provider.GoodsShipmentBuyer.Name);
		}

		public void TestGoodsShipmentSeller()
		{
			bill.ABL_SellerName = "TestSeller";
			AssertType<BillPartyProvider>("NotifySeller", Provider.GoodsShipmentSeller);
			AssertEquals("Seller Name", "TestSeller", Provider.GoodsShipmentSeller.Name);
		}

		public void TestItineraries()
		{
			var itinerary1 = bill.Header.Itinerary.AddNew();
			itinerary1.CY_Code = "AUSYD";

			var itinerary2 = bill.Header.Itinerary.AddNew();
			itinerary2.CY_Code = "IEADA";

			AssertContainsExactElementsInAnyOrder("Itinerary.Country", new[] { "AU", "IE" }, Provider.Itineraries.Select(c => c.Country));
		}

		public void TestPassiveBorderTransportMeans()
		{
			_ = bill.AsycudaTransportMeans.AddNew();

			AssertEquals(1, Provider.PassiveBorderTransportMeans.Count);
			AssertType<PassiveBorderTransportMeansProvider>(Provider.PassiveBorderTransportMeans.First());
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

		SendAndAmend50ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend50ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected override SendAndAmend50ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
