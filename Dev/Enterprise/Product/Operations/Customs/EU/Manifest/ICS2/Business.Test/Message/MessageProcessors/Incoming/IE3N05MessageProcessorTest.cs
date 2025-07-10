using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N05;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N05MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N05MessageProcessor, Ie3N05Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - High Risk Cargo & Mail Screening Request Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"High Risk Cargo & Mail Screening Request Notification",
			$"Issue Date: 2022-09-30T00:00:00Z",
			$"MRN: {CommonReferenceNumber}",
			$"Country: DE",
			$"Carrier: 1234",
			$"Transport document (Master level) Reference: DocumentID",
			$"Transport document (Master level) Type: N380",
			$"Referral request details",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Referral Request Reference</th><th>Request Type</th><th>Transport document (House level) Reference</th><th>Transport document (House level) Type</th></tr></thead><tr><td>RHSKEXVOY4</td><td>RFI</td><td>Document 11 ID</td><td>N111</td></tr><tr><td>V2U94FODGU</td><td>RFI</td><td>Document 22 ID</td><td>N222</td></tr><tr><td>V2U94FODGU</td><td>RFI</td><td>Document 33 ID</td><td>N333</td></tr></table>",
			$"Recommended HRCM Screening Method",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Referral Request Reference</th><th>Method</th></tr></thead><tr><td>V2U94FODGU</td><td>M1</td></tr><tr><td>V2U94FODGU</td><td>M3</td></tr><tr><td>V2U94FODGU</td><td>M2</td></tr>",
		};

		protected override TestEdiMessage GetIncomingMessage(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N05;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N05 xmlns=""urn:wco:datamodel:eu:ics2:2"">
    <documentIssueDate>
        <DateTime>2022-09-30T00:00:00Z</DateTime>
    </documentIssueDate>
    <MRN>{0}</MRN>
    <responsibleMemberState>
        <country>DE</country>
    </responsibleMemberState>
    <transportDocument>
        <documentNumber>DocumentID</documentNumber>
        <type>N380</type>
    </transportDocument>
    <carrier>
        <identificationNumber>1234</identificationNumber>
    </carrier>
    <referralRequestDetails>
        <referralRequestReference>RHSKEXVOY4</referralRequestReference>
        <requestType>RFI</requestType>
        <transportDocumentHouse>
            <documentNumber>Document 11 ID</documentNumber>
            <type>N111</type>
        </transportDocumentHouse>
    </referralRequestDetails>
    <referralRequestDetails>
        <referralRequestReference>V2U94FODGU</referralRequestReference>
        <requestType>RFI</requestType>
        <recommendedHRCMScreeningMethod>
            <method>M1</method>
        </recommendedHRCMScreeningMethod>
        <transportDocumentHouse>
            <documentNumber>Document 22 ID</documentNumber>
            <type>N222</type>
        </transportDocumentHouse>
    </referralRequestDetails>
    <referralRequestDetails>
        <referralRequestReference>V2U94FODGU</referralRequestReference>
        <requestType>RFI</requestType>
        <recommendedHRCMScreeningMethod>
            <method>M3</method>
        </recommendedHRCMScreeningMethod>
        <recommendedHRCMScreeningMethod>
            <method>M2</method>
        </recommendedHRCMScreeningMethod>
        <transportDocumentHouse>
            <documentNumber>Document 33 ID</documentNumber>
            <type>N333</type>
        </transportDocumentHouse>
    </referralRequestDetails>
</IE3N05>
", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3N05MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N05MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			base.TestProcessMessageCore_AdditionalAssertion(manifestHeader);
			AssertEquals("Customs status on manifest header should be updated to HRC", "HRC", manifestHeader.RegistrationStatus);
		}
	}
}
