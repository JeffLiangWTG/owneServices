using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend25ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend25ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(bill));
			});
		}

		public void TestAdditionalInformationCollection()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestTransportDocumentMasterLevel()
		{
			bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
			AssertEquals("TransportDocumentMasterLevel", "TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestCarrierIdentificationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			var eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI123", Core.Constants.CountryCodes.Greece);
			bill.Header.AMA_OA_Carrier = orgAddress.PK;
			AssertEquals("Carrier Identification Number", "GREORI123", Provider.CarrierIdentificationNumber);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		SendAndAmend25ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend25ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend25ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
