using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend13ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend13ConsignmentHouseLevelProvider>
	{
		public void TestNewOrNull()
		{
			CombineAssertions(() =>
			{
				AssertNull(SendAndAmend13ConsignmentHouseLevelProvider.NewOrNull(null));
				AssertNotNull(SendAndAmend13ConsignmentHouseLevelProvider.NewOrNull(bill));
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
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = "1234";
			supportingDocument.CSI_Code = "A";

			AssertEquals(1, Provider.SupportingDocuments.Count);
			AssertEquals("1234", Provider.SupportingDocuments.First().Identifier);
			AssertEquals("A", Provider.SupportingDocuments.First().Type);
		}

		public void TestAdditionalInformationCollection()
		{
			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "A";
			additionalInfo.CSI_Description = "1234";

			AssertEquals(1, Provider.AdditionalInformationCollection.Count);
			AssertEquals("A", Provider.AdditionalInformationCollection.First().Code);
			AssertEquals("1234", Provider.AdditionalInformationCollection.First().Text);
		}

		public void TestAdditionalSupplyChainActors()
		{
			_ = bill.CusSupplyChainActorReferences.AddNew();

			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "Consignee";
			bill.ConsigneePersonType = "A";
			bill.ABL_ConsigneeRegNo = "ConsigneeRegNo";
			bill.ABL_ConsigneePhone = "ConsigneePhone";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Consignee);
				AssertEquals("Consignee", Provider.Consignee.Name);
				AssertEquals("A", Provider.Consignee.TypeOfPerson);
				AssertEquals("ConsigneeRegNo", Provider.Consignee.IdentificationNumber);
				AssertEquals("ConsigneePhone", Provider.Consignee.Communications.First().Identifier);
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

		public void TestSupplementaryDeclarants()
		{
			AssertNull(Provider.SupplementaryDeclarants);

			var supplementaryDeclarant = bill.SupplementaryDeclarants.AddNew();
			var newProvider = GetProvider();

			AssertType<SupplementaryDeclarantProvider>(newProvider.SupplementaryDeclarants);
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

		SendAndAmend13ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => SendAndAmend13ConsignmentHouseLevelProvider.NewOrNull(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected override SendAndAmend13ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
