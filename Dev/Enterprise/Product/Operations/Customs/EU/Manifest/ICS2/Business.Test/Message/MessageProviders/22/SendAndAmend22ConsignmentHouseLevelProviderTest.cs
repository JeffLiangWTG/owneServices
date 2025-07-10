using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend22ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend22ConsignmentHouseLevelProvider>
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
			bill.ABL_GrossWeight = 1m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 0.001m, Provider.TotalGrossMass);
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

		public void TestConsignor()
		{
			bill.ABL_ShipperName = "TestConsignor";
			AssertType<BillPartyProvider>("Consignor", Provider.Consignor);
			AssertEquals("Consignor", "TestConsignor", Provider.Consignor.Name);
		}

		public void TestTransportDocumentMasterLevel()
		{
			bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
			AssertEquals("TransportDocumentMasterLevel", "TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		public void TestPlaceOfAcceptance()
		{
			bill.ABL_RL_NKOrigin = "FR222";
			AssertEquals("Place of acceptance Unlocode", "FR222", Provider.PlaceOfAcceptance.Unlocode);
			AssertEquals("Place of acceptance Country", string.Empty, Provider.PlaceOfAcceptance.Country);
		}

		public void TestPlaceOfDelivery()
		{
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Place of delivery", "AUSYD", Provider.PlaceOfDelivery.Unlocode);
		}

		public void TestSupportingDocumentsHouseLevel()
		{
			bill.SupportingDocuments.AddNew();
			AssertEquals("SupportingDocuments Count", 1, Provider.SupportingDocumentsHouseLevel.Count);
		}

		public void TestPaymentMethod()
		{
			AssertEquals("PaymentMethod", string.Empty, Provider.PaymentMethod);

			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;
			AssertEquals("PaymentMethod", EUICS2PaymentMethodList.Codes.D, Provider.PaymentMethod);
		}

		public void TestNotifyParty()
		{
			bill.ABL_NotifyPartyName = "TestNotifyParty";
			AssertType<BillPartyProvider>("NotifyParty", Provider.NotifyParty);
			AssertEquals("Notify Party Name", "TestNotifyParty", Provider.NotifyParty.Name);
		}

		public void TestUCRNumber()
		{
			bill.ABL_UCRNumber = "1234";
			AssertEquals("UCRNumber", "1234", Provider.UCRNumber);
		}

		public void TestSupplementaryDeclarants()
		{
			bill.SupplementaryDeclarants.AddNew();
			AssertEquals("SupplementaryDeclarants Count", 1, Provider.SupplementaryDeclarants.Count);
		}

		public void TestItineraries()
		{
			var itinerary1 = bill.Header.Itinerary.AddNew();
			itinerary1.CY_Code = "AUSYD";

			var itinerary2 = bill.Header.Itinerary.AddNew();
			itinerary2.CY_Code = "IEADA";

			AssertContainsExactElementsInAnyOrder("Itinerary.Country", new[] { "AU", "IE" }, Provider.Itineraries.Select(c => c.Country));
		}

		SendAndAmend22ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend22ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend22ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
