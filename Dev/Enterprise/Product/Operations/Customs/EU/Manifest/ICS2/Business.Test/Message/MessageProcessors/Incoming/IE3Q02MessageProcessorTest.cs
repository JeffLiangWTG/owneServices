using System.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3Q02;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(IE3Q02MessageProcessor))]
	sealed class IE3Q02MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3Q02MessageProcessor, Ie3Q02Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2RequestsTo;

		protected override string ExpectedEmailSubject => $"ICS2 Additional Information Request for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"Referral Request",
			$"Issue Date: 2022-09-30T00:00:00Z",
			$"MRN: {CommonReferenceNumber}",
			$"Country: DE",
			$"Declarant: DE08EORI1000003",
			"Information",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Reference</th><th>Type</th><th>Code</th><th>Info Type</th><th>Text</th></tr></thead><tr><td>AAAA</td><td>RFI</td><td>D10</td><td>R1</td><td>&nbsp;</td></tr><tr><td>BBBB</td><td>RFI</td><td>C10</td><td>R1</td><td>Please confirm the weight of the package as per the invoice</td></tr><tr><td>CCCC</td><td>AMD</td><td>A70</td><td>R1</td><td>Amend the total gross mass</td></tr></table>",
			"Supporting Documents",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Reference</th><th>Type</th><th>Document Type</th><th>Reference Number</th></tr></thead><tr><td>AAAA</td><td>RFI</td><td>N380</td><td>Document 56 ID</td></tr></table>"
		};

		protected override TestEdiMessage GetIncomingMessage(string masterReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.Q02;
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3Q02 xmlns=""urn:wco:datamodel:eu:ics2:2"">
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
    <pointer>
      <messageElementPath>IE4Q02/Related PLACI/Consignment (Master level)/Consignment (House level)/Total gross mass</messageElementPath>
    </pointer>
  </referralRequestDetails>
  <referralRequestDetails>
    <referralRequestReference>CCCC</referralRequestReference>
    <requestType>AMD</requestType>
    <additionalInformation>
      <code>A70</code>
      <text>Amend the total gross mass</text>
      <type>R1</type>
    </additionalInformation>
    <pointer>
      <messageElementPath>IE4Q02/Consignment (Master level)/Consignment (House level)/Total gross mass</messageElementPath>
    </pointer>
  </referralRequestDetails>
</IE3Q02>
", masterReferenceNumber);
			return incomingMessage;
		}

		protected override IE3Q02MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3Q02MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			void AssertRequestHeader(string identifier, string requestType, string messageElement)
			{
				var requestHeader = manifestHeader.RequestHeaders.Cast<RequestHeader>().FirstOrDefault(c => c.EUS_Identifier == identifier);

				AssertNotNull(identifier, requestHeader);
				AssertEquals($"{identifier} - Request Type", requestType, requestHeader.EUS_Type);
				AssertEquals($"{identifier} - Message Element", messageElement, requestHeader.EUS_MessageElement);
				AssertEquals($"{identifier} - Responsible Member State", "DE", requestHeader.EUS_MemberState);
			}

			CombineAssertions("Communication records should be filled correctly", () =>
			{
				AssertRequestHeader("AAAA", "RFI", string.Empty);
				AssertRequestHeader("BBBB", "RFI", "IE4Q02/Related PLACI/Consignment (Master level)/Consignment (House level)/Total gross mass");
				AssertRequestHeader("CCCC", "AMD", "IE4Q02/Consignment (Master level)/Consignment (House level)/Total gross mass");

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

				AssertEquals("Customs status on manifest header should be updated to RIR", EUICS2CustomsStatusList.Codes.RIR, manifestHeader.AMA_CustomsStatus);
				AssertEquals("Message status on manifest header should be updated to SNT", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			});
		}

		public void TestSupportingDocumentSection_ShouldBeHiddenWhenThereIsNoSuchData()
		{
			var incomingMessageWithoutSupportingDoc = Factory.New<TestEdiMessage>();
			incomingMessageWithoutSupportingDoc.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessageWithoutSupportingDoc.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessageWithoutSupportingDoc.EM_MessageType = MessageTypes.Codes.Q02;
			incomingMessageWithoutSupportingDoc.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3Q02 xmlns=""urn:wco:datamodel:eu:ics2:2"">
  <documentIssueDate>
    <DateTime>2022-09-30T00:00:00Z</DateTime>
  </documentIssueDate>
  <MRN>{CommonReferenceNumber}</MRN>
  <responsibleMemberState>
    <country>DE</country>
  </responsibleMemberState>
  <declarant>
    <identificationNumber>DE08EORI1000003</identificationNumber>
  </declarant>
  <referralRequestDetails>
    <referralRequestReference>AAAA</referralRequestReference>
    <requestType>RFI</requestType>
    <additionalInformation>
      <code>D10</code>
      <type>R1</type>
    </additionalInformation>
  </referralRequestDetails>
</IE3Q02>
";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "D1";
			staff.GS_FullName = "Default Staff 1";
			staff.GS_LoginName = "Default Staff 1";
			staff.GS_EmailAddress = "Test@cw.com";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = CommonManifestJobReference;
			manifestHeader.AMA_MasterBill = CommonMasterBill;
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, CommonReferenceNumber);

			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_MessageText = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><IE3F10 xmlns=\"urn:wco:datamodel:eu:ics2:2\"><MRN>{CommonReferenceNumber}</MRN></IE3F10>";
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Processor.PreProcessMessage(incomingMessageWithoutSupportingDoc);
				Processor.ProcessMessage(incomingMessageWithoutSupportingDoc);

				var outgoingEmails = Env.OutgoingCustomsMailManager.EmailsCreated;
				Assert("No Emails Created", outgoingEmails.Count > 0);
				var outgoingEmail = outgoingEmails.Single(x => x.Subject.StartsWith(ExpectedEmailSubject));

				AssertNotContains("Supporting Document section should be hidden.", "Supporting Documents", outgoingEmail.Body);
			}
		}
	}
}
