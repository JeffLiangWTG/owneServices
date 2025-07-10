using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader50ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader50Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader50Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader50Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertNull(Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F50, Provider.SpecificCircumstanceIndicator);
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

		public void TestPlaceOfAcceptance()
		{
			manifestHeader.AMA_RL_NKOrigin = "FR222";
			AssertEquals("Place of acceptance", "FR222", Provider.PlaceOfAcceptance.Unlocode);
		}

		public void TestSupportingDocuments()
		{
			manifestHeader.SupportingDocuments.AddNew();
			AssertEquals("SupportingDocuments Count", 1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			manifestHeader.CusSupplyChainActorReferences.AddNew();
			AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActors.Count);
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

		public void TestTransportChargesMethodOfPayment()
		{
			manifestHeader.AMA_PaymentMethod = "A";
			AssertEquals("Payment of Method", "A", Provider.TransportChargesMethodOfPayment);
		}

		public void TestPlaceOfDelivery()
		{
			manifestHeader.AMA_RL_NKFinalDestination = "AUSYD";
			AssertEquals("Place of delivery", "AUSYD", Provider.PlaceOfDelivery.Unlocode);
		}

		public void TestConsignments()
		{
			manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend50ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.Consignments.Single());
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

		public void TestTransportDocumentMasterLevel()
		{
			manifestHeader.AMA_MasterBill = "bill123";
			manifestHeader.MasterBill.TransportDocumentType = "N722";
			AssertEquals("TransportDocument DocumentNumber", "bill123", Provider.TransportDocumentMasterLevel.Identifier);
			AssertEquals("TransportDocument Type", "N722", Provider.TransportDocumentMasterLevel.Type);
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

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
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
