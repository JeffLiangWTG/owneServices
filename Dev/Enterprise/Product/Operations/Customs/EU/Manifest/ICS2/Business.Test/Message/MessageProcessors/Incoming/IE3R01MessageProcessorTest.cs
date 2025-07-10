using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R01;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3R01MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3R01MessageProcessor, Ie3R01Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - ENS Registration Response for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"ENS Registration Response",
			"LRN: EmailTestReferenceNumber",
			$"MRN: {CommonReferenceNumber}",
			"Declarant: TEST002",
			"Carrier: 0550045",
			"Registration Date: 2022-09-30T00:00:00Z",
			"Addressed Member State: DE",
			"Representative: TEST001",
			"Transport Document Reference Number: Document 33 ID",
			"Transport Document Type: N113",
			"Customs Office of First Entry: TEST003"
		};

		protected override string EntryNumberTypeToCreate => CusEntryNumberTypes.Standard.LocalReferenceNumber;

		public void TestMessageProcessCore()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "TC1";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0006789";
			manifestHeader.AMA_MasterBill = "TestMasterBill";
			EUICS2MessageTestHelper.SetManifestHeaderEntryNumberForTesting(manifestHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, "TestReferenceNumber");

			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_MessageText = "<?xml version=\"1.0\" encoding=\"utf-8\"?><IE3F10 xmlns=\"urn:wco:datamodel:eu:ics2:2\"><LRN>TestReferenceNumber</LRN></IE3F10>";

			var incomingMessage = GetIncomingMessage("TestReferenceNumber");

			Factory.Save();

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			CombineAssertions("Manifest header should be update with the MRN and registration datetime.", () =>
			{
				AssertEquals("MRN", CommonReferenceNumber, manifestHeader.RegistrationNumber);
				AssertEquals("Registration date time", new DateTime(2022, 09, 30, 0, 0, 0, DateTimeKind.Utc), manifestHeader.RegistrationDate.UtcDateTime());
			});

			AssertEquals("Manifest header message status should be updated to 'Sent'.", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			AssertEquals("Customs status on manifest header should be updated to REG", EUICS2CustomsStatusList.Codes.REG, manifestHeader.AMA_CustomsStatus);
			AssertEquals("Incoming message should be updated to 'PRS'.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
		}

		protected override TestEdiMessage GetIncomingMessage(string localReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.R01;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(
@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3R01 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<LRN>{0}</LRN>
	<MRN>{1}</MRN>
	<registrationDate>
		<DateTime>2022-09-30T00:00:00Z</DateTime>
	</registrationDate>
	<addressedMemberState>
		<country>DE</country>
	</addressedMemberState>
	<representative>
		<identificationNumber>TEST001</identificationNumber>
	</representative>
	<transportDocument>
		<documentNumber>Document 33 ID</documentNumber>
		<type>N113</type>
	</transportDocument>
	<carrier>
		<identificationNumber>0550045</identificationNumber>
	</carrier>
	<declarant>
		<identificationNumber>TEST002</identificationNumber>
	</declarant>
	<customsOfficeOfFirstEntry>
		<referenceNumber>TEST003</referenceNumber>
	</customsOfficeOfFirstEntry>
</IE3R01>", localReferenceNumber, CommonReferenceNumber);

			return incomingMessage;
		}

		protected override IE3R01MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3R01MessageProcessor(logger);
		}
	}
}
