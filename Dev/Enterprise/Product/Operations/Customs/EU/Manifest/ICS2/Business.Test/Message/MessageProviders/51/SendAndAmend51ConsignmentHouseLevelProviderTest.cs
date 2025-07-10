using System;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend51ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend51ConsignmentHouseLevelProvider>
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
			bill.ABL_GrossWeight = 1231m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 1.231m, Provider.TotalGrossMass);
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

			AssertType<SendAndAmend51GoodsItemProvider>(Provider.GoodsItems.First());
		}

		public void TestTransportChargesMethodOfPayment()
		{
			AssertEquals("PaymentMethod", string.Empty, Provider.TransportChargesMethodOfPayment);

			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;
			AssertEquals("PaymentMethod", EUICS2PaymentMethodList.Codes.D, Provider.TransportChargesMethodOfPayment);
		}

		public void TestCarrierIdentificationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI123", Core.Constants.CountryCodes.Greece);
			bill.Header.AMA_OA_Carrier = orgAddress.PK;
			AssertEquals("Carrier Identification Number", "GREORI123", Provider.CarrierIdentificationNumber);
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

		public void TestPassiveBorderTransportMeansFromPackExist()
		{
			_ = bill.AsycudaTransportMeans.AddNew();
			var pack = bill.Packs.AddNew();
			_ = pack.AsycudaTransportMeans.AddNew();

			AssertEquals(Array.Empty<IPassiveBorderTransportMeans>(), Provider.PassiveBorderTransportMeans);
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

		SendAndAmend51ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend51ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected override SendAndAmend51ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
