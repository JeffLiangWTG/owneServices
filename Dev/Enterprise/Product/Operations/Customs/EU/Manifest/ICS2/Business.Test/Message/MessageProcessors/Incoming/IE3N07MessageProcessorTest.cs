using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N07;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N07MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N07MessageProcessor, Ie3N07Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override string ExpectedEmailSubject => "ICS2 ENS In Incorrect State MAN0009999 Response";

		protected override string[] ExpectedEmailBody => new string[] {
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"><th>Code</th><th>Description</th></tr></thead><tr><td>code1</td><td>description1</td></tr><tr><td>code2</td><td>description2</td></tr></table>" };

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N07;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
													<IE3N07 xmlns=""urn:wco:datamodel:eu:ics2:2"">
													  <MRN>{0}</MRN>
													  <error>
														<description>description1</description>
														<validationCode>code1</validationCode>
														<pointer>
														  <messageElementPath>messageElementPath1</messageElementPath>
														</pointer>
													  </error>
													  <error>
														<description>description2</description>
														<validationCode>code2</validationCode>
														<pointer>
														  <messageElementPath>messageElementPath2</messageElementPath>
														</pointer>
													  </error>
													  <notifyParty>
														<identificationNumber>identificationNu1</identificationNumber>
													  </notifyParty>
													</IE3N07>", registrationNumber);

			return incomingMessage;
		}

		protected override IE3N07MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N07MessageProcessor(logger);
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to INS", EUICS2CustomsStatusList.Codes.INS, manifestHeader.AMA_CustomsStatus);
		}
	}
}
