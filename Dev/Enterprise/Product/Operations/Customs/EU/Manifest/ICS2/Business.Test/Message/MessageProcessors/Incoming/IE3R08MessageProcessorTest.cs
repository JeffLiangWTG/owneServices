using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R08;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3R08MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3R08MessageProcessor, Ie3R08Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override string ExpectedEmailSubject => "ICS2 – ENS Consultation Results Response";

		protected override string[] ExpectedEmailBody => new string[]
		{
			"ICS2 – ENS Consultation Results Response",
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Type</th><th>Identification</th><th>State</th></tr></thead><tr><td>type1 - </td><td>identification1</td><td>state1 - </td></tr><tr><td>type2 - </td><td>identification2</td><td>state2 - </td></tr></table>"
		};

		protected override TestEdiMessage GetIncomingMessage(string primaryReferenceNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.R08;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
															<IE3R08 xmlns=""urn:wco:datamodel:eu:ics2:2"">
																<functionalReference>{0}</functionalReference>
																<ENSEntity>
																	<type>type1</type>
																	<identification>identification1</identification>
																	<state>state1</state>
																</ENSEntity>
																<ENSEntity>
																	<type>type2</type>
																	<identification>identification2</identification>
																	<state>state2</state>
																</ENSEntity>
															</IE3R08>"
				, primaryReferenceNumber);
			return incomingMessage;
		}
		
		protected override IE3R08MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3R08MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_MessageProcessedStatus(TestEdiMessage incomingMessage)
		{
			AssertEquals("The message status should be 'PRS'", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
		}

		protected override void CreateOutgoingMessage(AsycudaManifestHeader manifestHeader)
		{
			var outgoingMessage = Factory.New<TestEdiMessage>();
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			outgoingMessage.EM_LinkUniqueID = manifestHeader.PK;
			outgoingMessage.EM_MessageText = "Test Message";
			outgoingMessage.EM_SystemCreateUser = staffCode;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = "TST";
			outgoingMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			outgoingMessage.EM_MessageType = MessageTypes.Codes.Q05;
			outgoingMessage.EM_ExternalReferenceNumber = CommonReferenceNumber;
		}
	}
}
