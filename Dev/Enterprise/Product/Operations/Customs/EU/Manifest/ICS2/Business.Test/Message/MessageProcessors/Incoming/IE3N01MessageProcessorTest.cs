using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N01;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N01MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N01MessageProcessor, Ie3N01Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override string ExpectedEmailSubject => $"ICS2 Validation Error Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Code</th><th>Description</th></tr></thead><tr><td>ABCD1234</td><td>TestErrorDescription</td></tr><tr><td>EFGH5678</td><td>TestErrorDescription2</td></tr></table>",
		};

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to VAL", EUICS2CustomsStatusList.Codes.VAL, manifestHeader.AMA_CustomsStatus);
			AssertEquals("Message status on manifest header should be updated to ERR", MessageStatusCodeList.Codes.Error, manifestHeader.AMA_MessageStatus);
		}

		protected override TestEdiMessage GetIncomingMessage(string primaryReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N01;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N01 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<LRN>ignoreLRN</LRN>
	<MRN>{0}</MRN>
	<notificationDate>
		<DateTime>2022-11-15T11:22:33+04:05</DateTime>
	</notificationDate>
	<addressedMemberState>
		<country>DE</country>
	</addressedMemberState>
	<representative>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</representative>
	<transportDocument>
		<documentNumber>ABCD</documentNumber>
		<type>C665</type>
	</transportDocument>
	<declarant>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</declarant>
	<customsOfficeOfFirstEntry>
		<referenceNumber>BE08EORI1000003</referenceNumber>
	</customsOfficeOfFirstEntry>
	<error>
		<description>TestErrorDescription</description>
		<validationCode>ABCD1234</validationCode>
		<pointer>
			<messageElementPath>SomeRandomPath</messageElementPath>
		</pointer>
	</error>
	<error>
		<description>TestErrorDescription2</description>
		<validationCode>EFGH5678</validationCode>
		<pointer>
			<messageElementPath>SomeRandomPath</messageElementPath>
		</pointer>
	</error>
</IE3N01>", primaryReferenceNumber);
			return incomingMessage;
		}

		protected override IE3N01MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N01MessageProcessor(logger);
		}
	}
}
