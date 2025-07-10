using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class RepresentativeProviderTest : PartyProviderTest
	{
		public new void TestConstructor()
		{
			AssertNull(GenerateProvider(null));
			AssertNotNull(GenerateProvider(manifestHeader));
		}

		protected override void AssertStatus()
		{
			var provider = RepresentativeProvider.NewOrNull(manifestHeader);
			AssertEquals("Should be set to 3 - Indirect (CL094) when AMA_AgentType is not DRT", "3", provider.Status);

			manifestHeader.AMA_AgentType = "DRT";
			provider = RepresentativeProvider.NewOrNull(manifestHeader);
			AssertEquals("Should be set to 2 - Direct (CL094) when AMA_AgentType is DRT", "2", provider.Status);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Name";

			orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Email = "1234@test.org";
			orgAddress.OA_RN_NKCountryCode = "IE";

			eoriNumber = orgAddress.CustomsCodes.AddNew();
			eoriNumber.OK_RN_NKCodeCountry = "IE";
			eoriNumber.OK_CodeType = "EOR";
			eoriNumber.OK_CustomsRegNo = "IdentificationNumber";

			manifestHeader.AMA_OA_ShippingAgent = orgAddress.PK;
		}
		AsycudaManifestHeader manifestHeader;

		PartyProvider GenerateProvider(AsycudaManifestHeader manifestHeader) => RepresentativeProvider.NewOrNull(manifestHeader);

		protected sealed override PartyProvider GetProvider()
		{
			return GenerateProvider(manifestHeader);
		}
	}
}
