using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3Q01;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3Q01MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3Q01MessageProcessor, Ie3Q01Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 Do Not Load Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Bill Number</th><th>Description</th></tr></thead><tr><td>EFGH</td><td>Do Not Load</td></tr></table>",
		};

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to DNL", EUICS2CustomsStatusList.Codes.DNL, manifestHeader.AMA_CustomsStatus);
		}

		protected override TestEdiMessage GetIncomingMessage(string primaryReferenceNumber)
		{
			return GetIncomingMessageForTest(primaryReferenceNumber);
		}

		TestEdiMessage GetIncomingMessageForTest(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.Q01;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3Q01 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<documentIssueDate>
		<DateTime>2022-11-09T11:22:33+04:05</DateTime>
	</documentIssueDate>
	<MRN>{0}</MRN>
	<responsibleMemberState>
		<country>FR</country>
	</responsibleMemberState>
	<representative>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</representative>
	<transportDocument>
		<documentNumber>ABCD</documentNumber>
		<type>C665</type>
	</transportDocument>
	<transportDocumentHouse>
		<documentNumber>EFGH</documentNumber>
		<type>C666</type>
	</transportDocumentHouse>
	<carrier>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</carrier>
	<declarant>
		<identificationNumber>BE08EORI1000003</identificationNumber>
	</declarant>
	<doNotLoadDetails>
		<receptacle>
			<receptacleIdentificationNumber>TestReceptacle</receptacleIdentificationNumber>
		</receptacle>
		<transportDocument>
			<documentNumber>IJLM</documentNumber>
			<type>C667</type>
		</transportDocument>
		<transportEquipment>
			<containerIdentificationNumber>TestContainer</containerIdentificationNumber>
		</transportEquipment>
	</doNotLoadDetails>
</IE3Q01>", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3Q01MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3Q01MessageProcessor(logger);
		}
	}
}
