using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N04;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(IE3N04MessageProcessor))]
	sealed class IE3N04MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N04MessageProcessor, Ie3N04Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;

		protected override string ExpectedEmailSubject => $"ICS2 Additional Information Request Notification for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"ICS2 Additional Information Request Notification",
			$"Issue Date: 2022-09-30T00:00:00Z",
			$"MRN: {CommonReferenceNumber}",
			$"Country: DE",
			$"Carrier: car123",
			$"Referral Request Details",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Reference</th><th>Type</th><th>Code</th><th>Info Type</th><th>Text</th></tr></thead><tr><td>AAAA</td><td>RFI</td><td>D10</td><td>R1</td><td>&nbsp;</td></tr><tr><td>BBBB</td><td>RFI</td><td>C10</td><td>R1</td><td>Please confirm the weight of the package as per the invoice</td></tr><tr><td>CCCC</td><td>AMD</td><td>A70</td><td>R1</td><td>Amend the total gross mass</td></tr></table><br />\r\n<br />Supporting Documents<br />\r\n<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Reference</th><th>Type</th><th>Document Type</th><th>Reference Number</th></tr></thead><tr><td>AAAA</td><td>RFI</td><td>N380</td><td>Document 56 ID</td></tr></table>",
		};

		protected override TestEdiMessage GetIncomingMessage(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N04;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N04 xmlns=""urn:wco:datamodel:eu:ics2:2"">
  <documentIssueDate>
    <DateTime>2022-09-30T00:00:00Z</DateTime>
  </documentIssueDate>
  <MRN>{0}</MRN>
  <responsibleMemberState>
    <country>DE</country>
  </responsibleMemberState>
  <carrier>
	<identificationNumber>car123</identificationNumber>
  </carrier>
  <referralRequestDetails>
	<transportDocumentMaster>
	  <documentNumber>Document 56 ID</documentNumber>
      <type>N380</type>
	</transportDocumentMaster>
	<transportDocumentHouse>
	  <documentNumber>Document 56 ID</documentNumber>
      <type>N380</type>
	</transportDocumentHouse>
    <referralRequestReference>AAAA</referralRequestReference>
    <requestType>RFI</requestType>
    <supportingDocuments>
      <referenceNumber>Document 56 ID</referenceNumber>
      <type>N380</type>
    </supportingDocuments>
    <additionalInformation>
      <code>D10</code>
      <type>R1</type>
    </additionalInformation>
  </referralRequestDetails>
  <referralRequestDetails>
    <referralRequestReference>BBBB</referralRequestReference>
    <requestType>RFI</requestType>
    <additionalInformation>
      <code>C10</code>
      <text>Please confirm the weight of the package as per the invoice</text>
      <type>R1</type>
    </additionalInformation>
  </referralRequestDetails>
  <referralRequestDetails>
    <referralRequestReference>CCCC</referralRequestReference>
    <requestType>AMD</requestType>
    <additionalInformation>
      <code>A70</code>
      <text>Amend the total gross mass</text>
      <type>R1</type>
    </additionalInformation>
  </referralRequestDetails>
</IE3N04>
", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3N04MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N04MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			base.TestProcessMessageCore_AdditionalAssertion(manifestHeader);

			void AssertRequestHeader(string identifier, string requestType)
			{
				var requestHeader = manifestHeader.RequestHeaders.Cast<RequestHeader>().FirstOrDefault(c => c.EUS_Identifier == identifier);

				AssertNotNull(identifier, requestHeader);
				AssertEquals($"{identifier} - Request Type", requestType, requestHeader.EUS_Type);
				AssertEquals($"{identifier} - Responsible Member State", "DE", requestHeader.EUS_MemberState);
			}

			CombineAssertions("Communication records should be filled correctly", () =>
			{
				AssertRequestHeader("AAAA", "RFI");
				AssertRequestHeader("BBBB", "RFI");
				AssertRequestHeader("CCCC", "AMD");

				AssertContainsExactElementsInAnyOrder("Check detailed text.",
					new[]
					{
					"",
					"Please confirm the weight of the package as per the invoice",
					"Amend the total gross mass"
					},
					MessageProcessorTestHelper.GetAllRequestInformationText(manifestHeader));

				AssertContainsExactElementsInAnyOrder("Check supporting document.",
					new[] { ("N380", "Document 56 ID") },
					MessageProcessorTestHelper.GetAllSupportingDocuments(manifestHeader));
			});
		}
	}
}
