using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N02;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class IE3N02MessageProcessorTest : MessageProcessorWithEmailNotificationTest<IE3N02MessageProcessor, Ie3N02Type>
	{
		protected override IRegistryItem EmailGroupNotificationRegistryItem => ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo;

		protected override string ExpectedEmailSubject => $"ICS2 - ENS Not Complete for {CommonManifestJobReference}/{CommonMasterBill}";

		protected override string[] ExpectedEmailBody => new string[]
		{
			$"<br /><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>{CommonMasterBill}</td><td>ENS Not Complete</td></tr></table>"
		};

		protected override TestEdiMessage GetIncomingMessage(string registrationNumber)
		{
			var incomingMessage = Factory.New<TestEdiMessage>();
			incomingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingMessage.EM_MessageType = MessageTypes.Codes.N02;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
															<IE3N02 xmlns=""urn:wco:datamodel:eu:ics2:2"">
															  <MRN>{0}</MRN>
															  <notificationDate>
																<DateTime>1900-01-01T01:01:01+08:00</DateTime>
															  </notificationDate>
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
															  <supplementaryDeclarant>
																<identificationNumber>identificationNu1</identificationNumber>
															  </supplementaryDeclarant>
															  <supplementaryDeclarant>
																<identificationNumber>identificationNu2</identificationNumber>
															  </supplementaryDeclarant>
															  <supplementaryDeclarant>
																<identificationNumber>identificationNu3</identificationNumber>
															  </supplementaryDeclarant>
															</IE3N02>", registrationNumber);

			return incomingMessage;
		}

		protected override void TestProcessMessageCore_AdditionalAssertion(AsycudaManifestHeader manifestHeader)
		{
			AssertEquals("Customs status on manifest header should be updated to NCN", EUICS2CustomsStatusList.Codes.NCN, manifestHeader.AMA_CustomsStatus);
		}

		protected override IE3N02MessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger)
		{
			return new IE3N02MessageProcessor(logger);
		}
	}
}
