using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	sealed class UkHmrcNOPOrganizationEventProcessorTest : OrganizationEventProcessorTestBase
	{
		protected override string DataProviderString => "UkHmrcNOP";

		protected override string NegativeResponseJsonPayload => @"{
  ""date"": ""2025-03-19T18:20:21+01:00"",
  ""eoris"": [
    {
      ""eori"": ""GB123123123123"",
      ""authorised"": false
    }
  ]
}
";
		protected override string PositiveResponseJsonPayload => @"{
  ""date"": ""2025-03-19T18:20:21Z"",
  ""eoris"": [
    {
      ""eori"": ""GB123123123123"",
      ""authorised"": true
    }
  ]
}
";

		protected override OrgCusCode GetCodeForTest(OrgHeader org) => org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "42424242");
	}
}
