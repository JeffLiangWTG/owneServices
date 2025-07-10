using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N10;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N10MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N10MessageProcessor, Ie3N10Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 Amendment Acknowledged {CommonManifestJobReference}/{CommonMasterBill}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"ICS2 Amendment Acknowledged {CommonManifestJobReference}/{CommonMasterBill}"
		};

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N10;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N10 xmlns=""urn:wco:datamodel:eu:ics2:2"">
  <MRN>{0}</MRN>
  <completionDate>
    <DateTime>1900-01-01T01:01:01+08:00</DateTime>
  </completionDate>
  <addressedMemberState>
    <country>T1</country>
  </addressedMemberState>
  <representative>
    <identificationNumber>identificationNu1</identificationNumber>
  </representative>
  <transportDocument>
    <documentNumber>documentNumber1</documentNumber>
    <type>Tok1</type>
  </transportDocument>
  <declarant>
    <identificationNumber>identificationNu1</identificationNumber>
  </declarant>
  <customsOfficeOfFirstEntry>
    <referenceNumber>referen1</referenceNumber>
  </customsOfficeOfFirstEntry>
</IE3N10>", registrationNumber);
			return incomingMessage;
		}

		protected override IE3N10MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N10MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Message status on manifest header should be updated to SNT", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			AssertEquals("Customs status on manifest header should be updated to ACP", EUICS2CustomsStatusList.Codes.ACP, manifestHeader.RegistrationStatus);
		}
	}
}
