using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend10ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend10ConsignmentHouseLevelProvider>
	{
		public void TestNewOrNull()
		{
			CombineAssertions(() =>
			{
				AssertNull(SendAndAmend10ConsignmentHouseLevelProvider.NewOrNull(null));
				AssertNotNull(SendAndAmend10ConsignmentHouseLevelProvider.NewOrNull(bill));
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
			bill.ABL_GrossWeight = 1m;
			bill.ABL_GrossWeightUQ = "KG";
			AssertEquals(1m, Provider.TotalGrossMass);
		}

		public void TestPlaceOfAcceptance()
		{
			bill.ABL_RL_NKOrigin = "FR222";

			AssertEquals("FR222", Provider.PlaceOfAcceptance.Unlocode);
			AssertEquals(string.Empty, Provider.PlaceOfAcceptance.Country);
		}

		public void TestSupportingDocuments()
		{
			_ = bill.SupportingDocuments.AddNew();

			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalInformation()
		{
			_ = bill.AdditionalInfos.AddNew();

			AssertEquals(1, Provider.AdditionalInformation.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			_ = bill.CusSupplyChainActorReferences.AddNew();

			AssertEquals(1, Provider.AdditionalSupplyChainActor.Count);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "Consignee";
			bill.ConsigneePersonType = "A";
			bill.ABL_ConsigneeRegNo = "ConsigneeRegNo";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Consignee);
				AssertEquals("Consignee", Provider.Consignee.Name);
				AssertEquals("A", Provider.Consignee.TypeOfPerson);
				AssertEquals("ConsigneeRegNo", Provider.Consignee.IdentificationNumber);
			});
		}

		public void TestGoodsItem()
		{
			_ = bill.Packs.AddNew();

			AssertEquals(1, Provider.GoodsItem.Count);
			AssertType<GoodsItemProvider>(Provider.GoodsItem.First());
		}

		public void TestConsignor()
		{
			bill.ABL_ShipperName = "Consignor";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Consignor);
				AssertEquals("Consignor", Provider.Consignor.Name);
			});
		}

		public void TestPaymentMethod()
		{
			bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.D;
			AssertEquals(EUICS2PaymentMethodList.Codes.D, Provider.PaymentMethod);
		}

		public void TestPlaceOfDelivery()
		{
			bill.ABL_RL_NKFinalDestination = "AUSYD";
			AssertEquals("AUSYD", Provider.PlaceOfDelivery.Unlocode);
		}

		public void TestGoodsShipmentBuyer()
		{
			TestHelper.PrepareCL010(Factory);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "BuyerName";
			var orgAddress = orgHeader.Addresses.AddNew();
			var eoriNumber = orgAddress.CustomsCodes.AddNew();
			eoriNumber.OK_RN_NKCodeCountry = "DE";
			eoriNumber.OK_CodeType = "EOR";
			eoriNumber.OK_CustomsRegNo = "9876543";

			bill.ABL_OA_Buyer = orgAddress.PK;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.GoodsShipmentBuyer);
				AssertEquals("BuyerName", Provider.GoodsShipmentBuyer.Name);
				AssertEquals("DE9876543", Provider.GoodsShipmentBuyer.IdentificationNumber);
			});
		}

		public void TestGoodsShipmentSeller()
		{
			TestHelper.PrepareCL010(Factory);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "SellerName";
			var orgAddress = orgHeader.Addresses.AddNew();
			var eoriNumber = orgAddress.CustomsCodes.AddNew();
			eoriNumber.OK_RN_NKCodeCountry = "DE";
			eoriNumber.OK_CodeType = "EOR";
			eoriNumber.OK_CustomsRegNo = "5432100";

			bill.ABL_OA_Buyer = orgAddress.PK;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.GoodsShipmentBuyer);
				AssertEquals("SellerName", Provider.GoodsShipmentBuyer.Name);
				AssertEquals("DE5432100", Provider.GoodsShipmentBuyer.IdentificationNumber);
			});
		}

		public void TestNotifyParty()
		{
			bill.ABL_NotifyPartyName = "NameP";
			bill.NotifyPartyPersonType = "N";
			bill.ABL_NotifyPartyRegNo = "N123490";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.NotifyParty);
				AssertEquals("NameP", Provider.NotifyParty.Name);
				AssertEquals("N", Provider.NotifyParty.TypeOfPerson);
				AssertEquals("N123490", Provider.NotifyParty.IdentificationNumber);
			});
		}

		public void TestPassiveBorderTransportMeans()
		{
			_ = bill.AsycudaTransportMeans.AddNew();
			_ = bill.AsycudaTransportMeans.AddNew();

			AssertEquals(2, Provider.PassiveBorderTransportMeans.Count);
			AssertType<PassiveBorderTransportMeansProvider>(Provider.PassiveBorderTransportMeans.First());
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
			const string expected = "1234";
			bill.ABL_UCRNumber = expected;

			AssertEquals(expected, Provider.UCRNumber);
		}

		protected override SendAndAmend10ConsignmentHouseLevelProvider GetProvider() => SendAndAmend10ConsignmentHouseLevelProvider.NewOrNull(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;
	}
}
