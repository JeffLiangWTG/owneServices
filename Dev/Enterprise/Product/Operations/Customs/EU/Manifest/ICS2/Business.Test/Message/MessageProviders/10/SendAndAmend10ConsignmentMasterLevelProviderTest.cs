using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend10ConsignmentMasterLevelProviderTest : DataProviderTestCase<SendAndAmend10ConsignmentMasterLevelProvider>
	{
		public void TestNewOrNull()
		{
			CombineAssertions(() =>
			{
				AssertNull(SendAndAmend10ConsignmentMasterLevelProvider.NewOrNull(null));
				AssertNotNull(SendAndAmend10ConsignmentMasterLevelProvider.NewOrNull(manifestHeader));
			});
		}

		public void TestCarrier()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "CarrierName";
			var orgAddress = orgHeader.Addresses.AddNew();
			var eoriNumber = orgAddress.CustomsCodes.AddNew();
			eoriNumber.OK_RN_NKCodeCountry = "IE";
			eoriNumber.OK_CodeType = "EOR";
			eoriNumber.OK_CustomsRegNo = "1234567";

			manifestHeader.AMA_OA_Carrier = orgAddress.PK;

			CombineAssertions(() =>
			{
				AssertType<PartyProvider>(Provider.Carrier);
				AssertEquals("CarrierName", Provider.Carrier.Name);
				AssertEquals("IE1234567", Provider.Carrier.IdentificationNumber);
			});
		}

		public void TestConsignmentHouseLevel()
		{
			manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend10ConsignmentHouseLevelProvider>(Provider.ConsignmentHouseLevel);
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
			manifestHeader.AMA_MasterBill = "5475687";
			CombineAssertions(() =>
			{
				AssertType<TransportDocumentProvider>(Provider.TransportDocumentMasterLevel);
				AssertEquals("5475687", Provider.TransportDocumentMasterLevel.Identifier);
			});
		}

		public void TestPlaceOfUnloading()
		{
			manifestHeader.AMA_RL_NKPortOfDischarge = "OTT2";
			CombineAssertions(() =>
			{
				AssertType<UNLOCOProvider>(Provider.PlaceOfUnloading);
				AssertEquals("OTT2", Provider.PlaceOfUnloading.Unlocode);
			});
		}

		protected override SendAndAmend10ConsignmentMasterLevelProvider GetProvider() => SendAndAmend10ConsignmentMasterLevelProvider.NewOrNull(manifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader manifestHeader;
	}
}
