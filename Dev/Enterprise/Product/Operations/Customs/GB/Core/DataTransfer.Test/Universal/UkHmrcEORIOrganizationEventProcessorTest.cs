using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	sealed class UkHmrcEORIOrganizationEventProcessorTest : OrganizationEventProcessorTestBase
	{
		protected override string DataProviderString => "UkHmrcEORI";

		protected override string NegativeResponseJsonPayload => @"{
  ""eori"": ""GB8392848394939"",
  ""valid"": false,
  ""processingDate"": ""2021-01-05T09:54:08+00:00""
}
";
		protected override string PositiveResponseJsonPayload => @"{
    ""eori"": ""GB123456729136"",
    ""valid"": true,
    ""processingDate"": ""2025-03-19T18:20:21+00:00""
  }
";

		protected override OrgCusCode GetCodeForTest(OrgHeader org) => org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "42424242");
	}
}
