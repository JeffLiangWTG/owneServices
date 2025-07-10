using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader41ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader41Provider
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SendAndAmendHeader41Provider(null));
			AssertNoExceptionThrown(() => new SendAndAmendHeader41Provider(manifestHeader));
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F41, Provider.SpecificCircumstanceIndicator);
		}

		public void TestReentryIndicator()
		{
			manifestHeader.ReEntryIndicator = false;
			AssertEquals(0, Provider.ReentryIndicator);

			manifestHeader.ReEntryIndicator = true;
			AssertEquals(1, Provider.ReentryIndicator);
		}

		public void TestRepresentative()
		{
			manifestHeader.AMA_OA_ShippingAgent = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertNotNull("Representative", Provider.Representative);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertType<ActiveBorderTransportMeansProvider>(Provider.ActiveBorderTransportMeans);
		}

		public void TestContainerIndicator()
		{
			AssertEquals("Precondition", 0, manifestHeader.Containers.Count);
			AssertEquals(0, Provider.ContainerIndicator);

			var container = manifestHeader.Containers.AddNew();
			AssertEquals("Precondition", ZString.Empty, container.ACN_ContainerNumber);
			AssertEquals(0, Provider.ContainerIndicator);

			container.ACN_ContainerNumber = "123";
			AssertEquals(1, Provider.ContainerIndicator);
		}

		public void TestReceptacleIdentificationNumbers()
		{
			var newReceptacle = manifestHeader.Receptacles.AddNew();
			newReceptacle.CY_Data = "342516";

			var receptacleIdentificationNumber = Provider.ReceptacleIdentificationNumbers.Single();
			AssertEquals("342516", receptacleIdentificationNumber);
		}

		public void TestSupportingDocuments()
		{
			manifestHeader.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.SupportingDocuments.Count);
				AssertType<SupportingDocumentProvider>(Provider.SupportingDocuments.Single());
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			manifestHeader.CusSupplyChainActorReferences.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
				AssertType<AdditionalSupplyChainActorProvider>(Provider.AdditionalSupplyChainActors.Single());
			});
		}

		public void TestCarrier()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.MainAddress.OA_Phone_Formatted = "123456";
			manifestHeader.AMA_OA_Carrier = carrier.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertType<PartyProvider>("Carrier", Provider.Carrier);
				AssertEquals("Carrier", "123456", Provider.Carrier.Communications.First().Identifier);
			});
		}

		public void TestConsignee()
		{
			manifestHeader.Bills.AddNew().ABL_ConsigneeName = "TestConsignee";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>("Consignee", Provider.Consignee);
				AssertEquals("Consignee", "TestConsignee", Provider.Consignee.Name);
			});
		}

		public void TestConsignor()
		{
			manifestHeader.Bills.AddNew().ABL_ShipperName = "TestConsignor";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>("Consignor", Provider.Consignor);
				AssertEquals("Consignor", "TestConsignor", Provider.Consignor.Name);
			});
		}

		public void TestPlaceOfLoading()
		{
			manifestHeader.AMA_RL_NKPortOfLoading = "OTT1";

			CombineAssertions(() =>
			{
				AssertType<UNLOCOProvider>(Provider.PlaceOfLoading);
				AssertEquals("OTT1", Provider.PlaceOfLoading.Unlocode);
			});
		}
		public void TestPassiveBorderTransportMeans()
		{
			AsycudaBill bill = manifestHeader.Bills.AddNew();
			bill.AsycudaTransportMeans.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.PassiveBorderTransportMeans.Count);
				AssertType<PassiveBorderTransportMeansProvider>(Provider.PassiveBorderTransportMeans.First());
			});
		}

		public void TestTransportDocumentMasterLevel()
		{
			manifestHeader.AMA_MasterBill = "TransportDocumentMasterLevel";

			AssertEquals("TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestTransportEquipments()
		{
			manifestHeader.Containers.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.TransportEquipments.Count);
				AssertType<TransportEquipmentProvider>(Provider.TransportEquipments.Single());
			});
		}

		public void TestUCRNumber()
		{
			AssertNull(Provider.UCRNumber);
		}

		public void TestPlaceOfUnloading()
		{
			manifestHeader.AMA_RL_NKPortOfDischarge = "OTT1";

			CombineAssertions(() =>
			{
				AssertType<UNLOCOProvider>(Provider.PlaceOfUnloading);
				AssertEquals("OTT1", Provider.PlaceOfUnloading.Unlocode);
			});
		}

		public void TestEntryCustomsOfficeReferenceNumber()
		{
			manifestHeader.AMA_CustomsOffice = "CO1";
			AssertEquals("CO1", Provider.EntryCustomsOfficeReferenceNumber);

			manifestHeader.AMA_CustomsOffice = "CO2";
			AssertEquals("CO2", Provider.EntryCustomsOfficeReferenceNumber);
		}
	}
}
