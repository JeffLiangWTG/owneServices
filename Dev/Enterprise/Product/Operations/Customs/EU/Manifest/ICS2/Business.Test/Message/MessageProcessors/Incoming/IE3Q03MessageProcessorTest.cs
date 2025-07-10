using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3Q03;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(IE3Q03MessageProcessor))]
	sealed class IE3Q03MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3Q03MessageProcessor, Ie3Q03Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - High Risk Cargo & Mail Screening Request for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"High Risk Cargo & Mail Screening Request",
			$"Issue Date: 2022-09-30T00:00:00Z",
			$"MRN: {CommonReferenceNumber}",
			$"Country: DE",
			$"Declarant: DE08EORI1000003",
			"Representative:",
			"Transport Document Reference Number:",
			"Transport Document Type:",
			"Referral Request Details:",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Referral Request Reference</th><th>Request Type</th><th>HRCM Screening Method</th><th>Transport Document Reference Number</th><th>Transport Document Type</th></tr></thead><tr><td>AAAA</td><td>RFI</td><td>123,321</td><td>Document 11 ID</td><td>N111</td></tr><tr><td>BBBB</td><td>RFS</td><td>123,322</td><td>Document 12 ID</td><td>N112</td></tr><tr><td>CCCC</td><td>QQQ</td><td>111,222,333,444,555,666,777,888,999</td><td>Document 33 ID</td><td>N113</td></tr>",
		};

		protected override TestEdiMessage GetIncomingMessage(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.Q03;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3Q03 xmlns=""urn:wco:datamodel:eu:ics2:2"">
  <documentIssueDate>
    <DateTime>2022-09-30T00:00:00Z</DateTime>
  </documentIssueDate>
  <MRN>{0}</MRN>
  <responsibleMemberState>
    <country>DE</country>
  </responsibleMemberState>
  <declarant>
    <identificationNumber>DE08EORI1000003</identificationNumber>
  </declarant>
  <referralRequestDetails>
    <referralRequestReference>AAAA</referralRequestReference>
    <requestType>RFI</requestType>
	<recommendedHRCMScreeningMethod>
		<method>123</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>321</method>
	</recommendedHRCMScreeningMethod>
    <transportDocumentHouse>
        <documentNumber>Document 11 ID</documentNumber>
        <type>N111</type>
    </transportDocumentHouse>
  </referralRequestDetails>
  <referralRequestDetails>
    <referralRequestReference>BBBB</referralRequestReference>
    <requestType>RFS</requestType>
	<recommendedHRCMScreeningMethod>
		<method>123</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>322</method>
	</recommendedHRCMScreeningMethod>
    <transportDocumentHouse>
        <documentNumber>Document 12 ID</documentNumber>
        <type>N112</type>
    </transportDocumentHouse>
  </referralRequestDetails>
  <referralRequestDetails>
    <referralRequestReference>CCCC</referralRequestReference>
    <requestType>QQQ</requestType>
	<recommendedHRCMScreeningMethod>
		<method>111</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>222</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>333</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>444</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>555</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>666</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>777</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>888</method>
	</recommendedHRCMScreeningMethod>
	<recommendedHRCMScreeningMethod>
		<method>999</method>
	</recommendedHRCMScreeningMethod>
    <transportDocumentHouse>
        <documentNumber>Document 33 ID</documentNumber>
        <type>N113</type>
    </transportDocumentHouse>
  </referralRequestDetails>
</IE3Q03>
", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3Q03MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3Q03MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			void AssertRequestHeader(string identifier, string requestType, string documentNum, string documentType, bool includeHRCMDetails, string screeningMethod)
			{
				var requestHeader = manifestHeader.RequestHeaders.Cast<RequestHeader>().FirstOrDefault(c => c.EUS_Identifier == identifier);

				AssertNotNull(identifier, requestHeader);
				AssertEquals($"{identifier} - Request Type", requestType, requestHeader.EUS_Type);
				AssertEquals($"{identifier} - Document Number", documentNum, requestHeader.EUS_HouseBillNumber);
				AssertEquals($"{identifier} - Document Type", documentType, requestHeader.EUS_TransportDocumentType);
				AssertEquals($"{identifier} - Responsible Member State", "DE", requestHeader.EUS_MemberState);
				AssertEquals($"{identifier} - Include HRCM Details", includeHRCMDetails, requestHeader.EUS_IncludeScreeningDetails);
				AssertEquals($"{identifier} - HRCM Screening Method", screeningMethod, requestHeader.EUS_ScreeningMethod);
			}

			CombineAssertions("Communication records should be filled correctly", () =>
			{
				AssertRequestHeader("AAAA", "RFI", "Document 11 ID", "N111", false, "123,321");
				AssertRequestHeader("BBBB", "RFS", "Document 12 ID", "N112", true, "123,322");
				AssertRequestHeader("CCCC", "QQQ", "Document 33 ID", "N113", false, "111,222,333,444,555,666,777,888,999");
			});

			AssertEquals("Customs status on manifest header should be updated to HRC", EUICS2CustomsStatusList.Codes.HRC, manifestHeader.AMA_CustomsStatus);
			AssertEquals("Message status on manifest header should be updated to SNT", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
		}
	}
}
