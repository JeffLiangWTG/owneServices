using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalInformationResponseProviderTest : DataProviderTestCase<AdditionalInformationResponseProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("RequestHeader missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(requestHeader));
			});
		}

		public void TestReferralResponseReference()
		{
			AssertEquals("ReferralResponseReference", "TestReference", Provider.ReferralResponseReference);
		}

		public void TestAdditionalInformations()
		{
			AssertEquals("AdditionalInformations", 1, Provider.AdditionalInformations.Count);
		}

		public void TestBinaryAttachments()
		{
			AssertEquals("BinaryAttachments", 1, Provider.BinaryAttachments.Count);
		}

		public void TestTransportDocumentHouseLevel()
		{
			AssertEquals("TransportDocumentHouseLevel", "TestBill", Provider.TransportDocumentHouseLevel.Identifier);
			AssertEquals("TransportDocumentHouseLevel Type", "N720", Provider.TransportDocumentHouseLevel.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "TestBill";
			bill.TransportDocumentType = "N720";

			var bill2 = manifestHeader.Bills.AddNew();
			bill2.ABL_BillNumber = "TestBill2";
			bill2.TransportDocumentType = "N722";

			requestHeader = manifestHeader.RequestHeaders.AddNew();

			requestHeader.EUS_Identifier = "TestReference";
			requestHeader.EUS_HouseBillNumber = bill.ABL_BillNumber;
			requestHeader.RequestResponses.AddNew();
			requestHeader.Attachments.AddNew();
		}
		RequestHeader requestHeader;

		AdditionalInformationResponseProvider GenerateProvider(RequestHeader requestHeader) => new AdditionalInformationResponseProvider(requestHeader);

		protected override AdditionalInformationResponseProvider GetProvider()
		{
			return GenerateProvider(requestHeader);
		}
	}
}
