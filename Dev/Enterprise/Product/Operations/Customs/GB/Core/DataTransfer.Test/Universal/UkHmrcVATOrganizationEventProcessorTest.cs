using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	sealed class UkHmrcVATOrganizationEventProcessorTest : OrganizationEventProcessorTestBase
	{
		protected override void AssertNegativeEventResponseValidity(OrgCusCodeValidity validity)
		{
			AssertEquals(ZDateTime.Empty, validity.LastVerifiedTime);
			AssertEquals(ZString.Empty, validity.VerificationAuthority);
		}

		protected override OrgCusCode GetCodeForTest(OrgHeader org) => org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345678");

		protected override string DataProviderString => "UkHmrcVAT";

		protected override string NegativeResponseJsonPayload => @"{
  ""code"": ""NOT_FOUND"",
  ""message"": ""targetVrn does not match a registered company""
}
";
		protected override string PositiveResponseJsonPayload => @"{
  ""target"": {
    ""name"": ""Credite Sberger Donal Inc."",
    ""vatNumber"": ""553557881"",
    ""address"": {
      ""line1"": ""131B Barton Hamlet"",
      ""postcode"": ""SW97 5CK"",
      ""countryCode"": ""GB""
    }
  },
  ""processingDate"": ""2025-03-19T18:20:21+00:00""
}
";

		protected override string BuildNegativeResponse(ZGuid interchangePK) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
<Event>
	<DataContext>
		<DataTargetCollection>
			<DataTarget>
				<Key>UKDEMOLHR</Key>
				<Type>Organization</Type>
			</DataTarget>
		</DataTargetCollection>
		<DataSource>
			<Key>UkHmrcVAT</Key>
			<Type>DataProvider</Type>
		</DataSource>
	</DataContext>
	<EventTime>2023-11-10T10:41:20</EventTime>
	<EventType>MRJ</EventType>
	<ContextCollection>
		<Context>
			<Type>eHubTrackingID</Type>
			<Value>{interchangePK}</Value>
		</Context>
		<Context>
			<Type>FailedAction</Type>
			<Value>Not found</Value>
		</Context>
		<Context>
			<Type>Message</Type>
			<Value>targetVrn does not match a registered company</Value>
		</Context>
		<Context>
			<Type>ResponseText</Type>
			<Value>ew0KICAiY29kZSI6ICJOT1RfRk9VTkQiLA0KICAibWVzc2FnZSI6ICJ0YXJnZXRWcm4gZG9lcyBub3QgbWF0Y2ggYSByZWdpc3RlcmVkIGNvbXBhbnkiDQp9DQo=</Value>
		</Context>
	</ContextCollection>
</Event>
</UniversalEvent>
";
	}
}
