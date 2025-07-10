using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N03;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N03MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N03MessageProcessor, Ie3N03Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => $"ICS2 - Risk Assessment Complete {CommonManifestJobReference}/{CommonReferenceNumber}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"Risk Assessment Completed - 2020-09-25T18:00:00Z"
		};

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N03;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3N03 xmlns=""urn:wco:datamodel:eu:ics2:2"">
	<MRN>{0}</MRN>
	<completionDate>
		<DateTime>2020-09-25T18:00:00Z</DateTime>
	</completionDate>
</IE3N03>", registrationNumber);
			return incomingMessage;
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to ASC", EUICS2CustomsStatusList.Codes.ASC, manifestHeader.AMA_CustomsStatus);
		}

		protected override IE3N03MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N03MessageProcessor(logger);
		}
	}
}
