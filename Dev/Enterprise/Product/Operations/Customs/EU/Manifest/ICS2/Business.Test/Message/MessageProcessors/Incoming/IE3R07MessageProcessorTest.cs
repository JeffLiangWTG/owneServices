using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R07;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3R07MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3R07MessageProcessor, Ie3R07Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - Invalidation Acceptance Response for {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"ICS2 - Invalidation Acceptance Response for {CommonManifestJobReference}/{CommonReferenceNumber}"
		};

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.R07;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
															<IE3R07 xmlns=""urn:wco:datamodel:eu:ics2:2"">
																<MRN>{0}</MRN>
															</IE3R07>", registrationNumber);
			return incomingMessage;
		}

		protected override IE3R07MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3R07MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Message status on manifest header should be updated to SNT", MessageStatusCodeList.Codes.Sent, manifestHeader.AMA_MessageStatus);
			AssertEquals("Registration status on manifest header should be updated to CAN", EUICS2CustomsStatusList.Codes.CAN, manifestHeader.RegistrationStatus);
		}
	}
}
