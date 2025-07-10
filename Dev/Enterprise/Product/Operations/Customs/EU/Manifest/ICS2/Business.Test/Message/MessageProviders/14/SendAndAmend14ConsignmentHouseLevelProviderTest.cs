using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend14ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend14ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => _ = new SendAndAmend14ConsignmentHouseLevelProvider(null));
				AssertNoExceptionThrown(() => _ = new SendAndAmend14ConsignmentHouseLevelProvider(bill));
			});
		}

		public void TestContainerIndicator()
		{
			var container = bill.Header.Containers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("0", Provider.ContainerIndicator);

				container.ACN_ContainerNumber = "1234";
				AssertEquals("1", Provider.ContainerIndicator);
			});
		}

		public void TestTotalGrossMass()
		{
			bill.ABL_GrossWeight = 1231m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 1.231m, Provider.TotalGrossMass);
		}

		public void TestPlaceOfAcceptance()
		{
			const string expectedCode = "FR222";
			bill.ABL_RL_NKOrigin = expectedCode;

			AssertEquals(expectedCode, Provider.PlaceOfAcceptance.Unlocode);
			AssertEquals(string.Empty, Provider.PlaceOfAcceptance.Country);
		}

		public void TestSupportingDocuments()
		{
			_ = bill.SupportingDocuments.AddNew();

			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalInformationCollection()
		{
			_ = bill.AdditionalInfos.AddNew();

			AssertEquals(1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			_ = bill.CusSupplyChainActorReferences.AddNew();

			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestTransportDocumentMasterLevel()
		{
			const string expected = "TransportDocumentMasterLevel";
			bill.Header.AMA_MasterBill = expected;

			AssertEquals(expected, Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestCarrierIdentificationNumber()
		{
			var carrierOrg = Factory.New<OrgHeader>();
			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = "DE";
			orgCusCode.OK_CodeType = "EOR";
			orgCusCode.OK_CustomsRegNo = "123456";
			orgCusCode.OK_OH = carrierOrg.PK;
			var carrier = carrierOrg.MainAddress;
			bill.Header.AMA_OA_Carrier = carrier.PK;

			AssertEquals("DE123456", SendAndAmend24ConsignmentHouseLevelProvider.NewOrNull(bill).CarrierIdentificationNumber);
		}

		public void TestConsignee()
		{
			const string expectedName = "ConsigneeName";
			const string expectedPersonType = "A";
			const string expectedRegNo = "ConsigneeRegNo";
			bill.ABL_ConsigneeName = expectedName;
			bill.ConsigneePersonType = expectedPersonType;
			bill.ABL_ConsigneeRegNo = expectedRegNo;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Consignee);
				AssertEquals(expectedName, Provider.Consignee.Name);
				AssertEquals(expectedPersonType, Provider.Consignee.TypeOfPerson);
				AssertEquals(expectedRegNo, Provider.Consignee.IdentificationNumber);
			});
		}

		public void TestGoodsItems()
		{
			_ = bill.Packs.AddNew();

			AssertEquals(1, Provider.GoodsItems.Count);
			AssertType<GoodsItemProvider>(Provider.GoodsItems.First());
		}

		public void TestConsignor()
		{
			const string expected = "TestConsignor";
			bill.ABL_ShipperName = expected;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Consignor);
				AssertEquals(expected, Provider.Consignor.Name);
			});
		}

		public void TestPaymentMethod()
		{
			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;

			AssertEquals(EUICS2PaymentMethodList.Codes.D, Provider.PaymentMethod);
		}

		public void TestPlaceOfDelivery()
		{
			const string expected = "AUSYD";
			bill.ABL_RL_NKFinalDestination = expected;

			AssertEquals(expected, Provider.PlaceOfDelivery.Unlocode);
		}

		public void TestItineraries()
		{
			_ = bill.Header.Itinerary.AddNew("AUSYD");
			_ = bill.Header.Itinerary.AddNew("IEADA");

			AssertContainsExactElementsInAnyOrder(new[] { "AU", "IE" }, Provider.Itineraries.Select(c => c.Country));
		}

		public void TestNotifyParty()
		{
			const string expectedName = "Name";
			const string expectedPersonType = "A";
			const string expectedRegNo = "ConsigneeRegNo";
			bill.ABL_NotifyPartyName = expectedName;
			bill.NotifyPartyPersonType = expectedPersonType;
			bill.ABL_NotifyPartyRegNo = expectedRegNo;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.NotifyParty);
				AssertEquals(expectedName, Provider.NotifyParty.Name);
				AssertEquals(expectedPersonType, Provider.NotifyParty.TypeOfPerson);
				AssertEquals(expectedRegNo, Provider.NotifyParty.IdentificationNumber);
			});
		}

		public void TestSupplementaryDeclarants()
		{
			const string expectedCode = "123";
			const string expectedData = "data";
			_ = bill.SupplementaryDeclarants.AddNew(expectedCode, expectedData);

			AssertEquals(1, Provider.SupplementaryDeclarants.Count);
			AssertEquals(expectedCode, Provider.SupplementaryDeclarants.First().Type);
			AssertEquals(expectedData, Provider.SupplementaryDeclarants.First().Identifier);
		}

		public void TestTransportDocumentHouseLevel()
		{
			const string expectedIdentifier = "BillID";
			const string expectedType = "Type";
			bill.ABL_BillNumber = expectedIdentifier;
			bill.TransportDocumentType = expectedType;

			AssertEquals(expectedIdentifier, Provider.TransportDocumentHouseLevel.Identifier);
			AssertEquals(expectedType, Provider.TransportDocumentHouseLevel.Type);
		}

		public void TestTransportEquipmentCollection()
		{
			var container1 = bill.Header.Containers.AddNew();
			var container2 = bill.Header.Containers.AddNew();
			var container3 = bill.Header.Containers.AddNew();

			bill.Packs.AddNew().ContainerPK = container1.PK;
			bill.Packs.AddNew().ContainerPK = container2.PK;

			var transportEquipment = Provider.TransportEquipmentCollection;
			AssertEquals(2, transportEquipment.Count);
			AssertType<TransportEquipmentProvider>(transportEquipment.First());
		}

		public void TestUCRNumber()
		{
			const string expected = "1234";
			bill.ABL_UCRNumber = expected;

			AssertEquals(expected, Provider.UCRNumber);
		}

		public void TestPassiveBorderTransportMeans()
		{
			AssertEquals(0, Provider.PassiveBorderTransportMeans.Count);
		}

		SendAndAmend14ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend14ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected override SendAndAmend14ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
