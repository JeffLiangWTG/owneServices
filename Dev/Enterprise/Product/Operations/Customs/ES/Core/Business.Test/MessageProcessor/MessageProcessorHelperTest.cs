using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PresentaMercanciasV1Sal;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.Testing
{
	class MessageProcessorHelperTest : TestCaseWithFactory
	{
		public void TestGetRelevantBusinessObjectFromMRNCode()
		{
			var xsdSchemaNamePresentaMercanciasV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.PresentaMercanciasV1Sal.xsd";

			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter("20ES00999930006184", ZDateTime.Today);

			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_LinkedObject = entryHeader;
			sentInterchange.ContainedMessages.Add(sentMessage);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			var message = Factory.New<TestEdiMessage>();
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new CargoWise.EntityFramework.BusinessObject[] { entryHeader };

			CombineAssertions(() =>
			{
				AssertExceptionThrown<InvalidOperationException>("EM_MessageText is empty", () => MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<PresentaMercanciasV1Sal>(message, xsdSchemaNamePresentaMercanciasV1Sal, sentBusinessObjects));

				message.EM_MessageText = GetTestFileResponse();

				var foundBO = MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<PresentaMercanciasV1Sal>(message, xsdSchemaNamePresentaMercanciasV1Sal, sentBusinessObjects);
				AssertEquals("found entryheader is the same as the declared", entryHeader, foundBO);

				message.EM_MessageText = GetTestFileResponseNoMRN();
				var xsdSchemaNameDocumentosSimplifiV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.DocumentosSimplifiV1Sal.xsd";

				foundBO = MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<DocumentosSimplifiV1Sal>(message, xsdSchemaNameDocumentosSimplifiV1Sal, sentBusinessObjects);
				AssertEquals("Return null if no MRN in the Response", null, foundBO);
			});
		}

		public void TestGetRelevantBusinessObjectForEmailResponseForCusEntryHeader()
		{
			var mrnCode = "20ES00999930006184";
			var mrnCodeT2C = "20ES00999930006185";

			var entryHeader1 = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader1.MovementReferenceNumberSetter(mrnCode, ZDateTime.Today);

			var entryHeader2 = Factory.NewWithValidTestData<CusEntryHeader>();
			var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader2, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = mrnCodeT2C;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			var message = Factory.New<TestEdiMessage>();

			Factory.Save();

			CombineAssertions(() =>
			{
				var foundBO = MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				AssertEquals("no entryheader found when EM_ApplicationReference is empty", null, foundBO);

				message.EM_ApplicationReference = mrnCode;

				foundBO = MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				AssertEquals("found entryheader1 is the same as the declared", entryHeader1, foundBO);

				foundBO = MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber);
				AssertEquals("no entryheader found when when EM_ApplicationReference is not the one asked for", null, foundBO);

				message.EM_ApplicationReference = mrnCodeT2C;

				foundBO = MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber);
				AssertEquals("found entryheader2 is the same as the declared", entryHeader2, foundBO);
			});
		}

		public void TestGetRelatedSentInterchange()
		{
			var interchangeID = new ZGuid("87FAA3F5-423A-4C15-B8B4-3170AFA894BB");
			var message = Factory.NewWithValidTestData<ESEDIMessage>();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<InvalidOperationException>("Exception when no interchange associated to the message", "Unable to locate the related sent interchange", () => MessageProcessorHelper.GetRelatedSentInterchange(message));

				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				message.EM_EI = interchange.PK;
				AssertExceptionThrown<InvalidOperationException>("Exception when no sessionguid in the interchange associated to the message", "Unable to locate the related sent interchange", () => MessageProcessorHelper.GetRelatedSentInterchange(message));

				interchange.EI_SessionGUID = interchangeID;
				AssertExceptionThrown<InvalidOperationException>("Exception when no interchange with same sessionguid as the interchange associated to the message", "Unable to locate the related sent interchange", () => MessageProcessorHelper.GetRelatedSentInterchange(message));

				var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				sentInterchange.EI_SessionGUID = interchangeID;
				AssertExceptionThrown<InvalidOperationException>("Exception when no transmit interchange with same sessionguid as the interchange associated to the message", "Unable to locate the related sent interchange", () => MessageProcessorHelper.GetRelatedSentInterchange(message));

				sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;

				var sentInterchange2 = Factory.NewWithValidTestData<EDIInterchange>();
				sentInterchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				sentInterchange2.EI_SessionGUID = ZGuid.NewZGuid();
				AssertEquals("Returns the sentInterchange with the same sessionguid as the message's interchange", sentInterchange, MessageProcessorHelper.GetRelatedSentInterchange(message));
			});
		}

		public void TestGetOutgoingMessage()
		{
			var interchangeID = new ZGuid("87FAA3F5-423A-4C15-B8B4-3170AFA894BB");
			var message = Factory.NewWithValidTestData<ESEDIMessage>();

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			message.EM_EI = interchange.PK;
			interchange.EI_SessionGUID = interchangeID;

			var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			CombineAssertions(() =>
			{
				AssertExceptionThrown<InvalidOperationException>("Exception when no message associated to the sent interchange", "Sent interchange must have one and only one message associated", () => MessageProcessorHelper.GetOutgoingMessage(message));

				var sentMessage = sentInterchange.ContainedMessages.AddNew();
				AssertEquals("Returns the sentInterchange with the same sessionguid as the message's interchange", sentMessage, MessageProcessorHelper.GetOutgoingMessage(message));

				sentInterchange.ContainedMessages.AddNew();
				AssertExceptionThrown<InvalidOperationException>("Exception when multiple messages associated to the sent interchange", "Sent interchange must have one and only one message associated", () => MessageProcessorHelper.GetOutgoingMessage(message));
			});
		}

		public void TestGetInterchangeCertificateName()
		{
			var certificateName = "CertName";
			var certificateName2 = "CertName2";

			var interchangeHeader = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>{0}</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>", certificateName);

			var logger = new LoggingInformation();

			CombineAssertions(() =>
			{
				var message = Factory.NewWithValidTestData<ESEDIMessage>();
				AssertEquals("CertificateName is empty when received message doesn't have an interchange", ZString.Empty, MessageProcessorHelper.GetInterchangeCertificateName(message, logger));
				AssertContains("Logger has error when received message doesn't have an interchange", "Error reading xml", logger.UserLogStrings[0]);

				logger.UserLogStrings.Clear();
				var interchange = Factory.NewWithValidTestData<EDIInterchange>();
				message.EM_EI = interchange.PK;
				AssertEquals("CertificateName is empty when received interchange doesn't have header text", ZString.Empty, MessageProcessorHelper.GetInterchangeCertificateName(message, logger));
				AssertContains("Logger has error when received interchange doesn't have header text", "Error reading xml", logger.UserLogStrings[0]);

				logger.UserLogStrings.Clear();
				interchange.EI_HeaderText = interchangeHeader;
				Factory.Save();
				AssertEquals("CertificateName is correct when received interchange has a correct header text", certificateName, MessageProcessorHelper.GetInterchangeCertificateName(message, logger));
				AssertEquals("UserLogs empty when received interchange has a correct header text", 0, logger.UserLogStrings.Count);

				var interchangeID = new ZGuid("87FAA3F5-423A-4C15-B8B4-3170AFA894BB");
				logger.UserLogStrings.Clear();
				interchange.EI_HeaderText = ZString.Empty;
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				interchange.EI_SessionGUID = interchangeID;

				var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				sentInterchange.EI_SessionGUID = interchangeID;
				sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				var sentMessage = Factory.NewWithValidTestData<ESEDIMessage>();
				sentMessage.EM_EI = sentInterchange.PK;
				sentMessage.EM_ApplicationReference = certificateName2;
				Factory.Save();
				AssertEquals("CertificateName is correct when received interchange is direct xt and sent message was created correctly", certificateName2, MessageProcessorHelper.GetInterchangeCertificateName(message, logger));
				AssertEquals("UserLogs empty when received interchange has a correct header text", 0, logger.UserLogStrings.Count);
			});
		}

		string GetTestFileResponse() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessage.txt");

		string GetTestFileResponseNoMRN() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "DJP-reject-withoutMRN.txt");
	}
}
