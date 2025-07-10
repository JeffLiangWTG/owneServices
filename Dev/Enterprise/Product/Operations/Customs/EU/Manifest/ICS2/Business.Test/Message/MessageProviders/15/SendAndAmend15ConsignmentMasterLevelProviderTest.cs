using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend15ConsignmentMasterLevelProviderTest : DataProviderTestCase<SendAndAmend15ConsignmentMasterLevelProvider>
	{
		public void TestAdditionalInformationCollection()
		{
			_ = manifestHeader.AdditionalInfos.AddNew();

			AssertEquals(1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestConsignmentHouseLevelCollection()
		{
			_ = manifestHeader.Bills.AddNew();

			AssertEquals(1, Provider.ConsignmentHouseLevelCollection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		}

		SendAndAmend15ConsignmentMasterLevelProvider GenerateProvider(AsycudaManifestHeader mh) => SendAndAmend15ConsignmentMasterLevelProvider.NewOrNull(mh);

		AsycudaManifestHeader manifestHeader;

		protected override SendAndAmend15ConsignmentMasterLevelProvider GetProvider()
		{
			return GenerateProvider(manifestHeader);
		}
	}
}
