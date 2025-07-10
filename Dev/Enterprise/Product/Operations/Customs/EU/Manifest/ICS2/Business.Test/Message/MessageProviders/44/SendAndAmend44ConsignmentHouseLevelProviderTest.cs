using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend44ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend44ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(bill));
			});
		}

		public void TestReceptacleIdentificationNumber()
		{
			bill.ReceptacleId = "342516";

			AssertEquals("342516", Provider.ReceptacleIdentificationNumber);
		}

		public void TestAdditionalInformationCollection()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformationCollection", 1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		SendAndAmend44ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend44ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend44ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
