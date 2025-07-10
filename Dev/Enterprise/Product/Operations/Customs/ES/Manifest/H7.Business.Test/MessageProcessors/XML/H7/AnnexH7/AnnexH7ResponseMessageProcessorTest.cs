using System;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestDate(2025, 5, 26, 11, 48, 00)]
	public class AnnexH7ResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<AnnexH7ResponseMessageProcessor, AsycudaBill, EnvioDeDocumentosV1Sal>
	{
		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			if (businessObject is AsycudaBill bill)
			{
				AssertEquals("ABL_MessageStatus", EDIMessageStatusList.Codes.Failed, bill.ABL_MessageStatus);
			}

			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "There is an error in XML document ", concatenatedUserLogStrings);
			AssertContains("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: There is an error in XML document ", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "Annex H7 Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.H7Annexes };

		protected override AnnexH7ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new AnnexH7ResponseMessageProcessor(logger);

		public void TestProcessAcceptedMessage_WithoutDocCsvIdClearanceRequest()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithoutDocCsvIdClearanceRequest(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>";

			AssertAcceptedResults(message, bill, true, expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_WithDocCsvId()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithDocCsvId(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>" +
				"<H4>Additional Information</H4>" +
				"<table border=\"0\"><tr><td>Document CSV ID:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>";

			AssertAcceptedResults(message, bill, true, expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_WithClearanceRequest()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithClearanceRequest(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>" +
				"<H4>Additional Information</H4>" +
				"<table border=\"0\"><tr><td>Clearance Request:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			AssertAcceptedResults(message, bill, true, expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_WithDocCsvIdClearanceRequest()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithDocCsvIdClearanceRequest(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>" +
				"<H4>Additional Information</H4>" +
				"<table border=\"0\"><tr><td>Document CSV ID:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>" +
				"<table border=\"0\"><tr><td>Clearance Request:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			AssertAcceptedResults(message, bill, true, expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_NoH7Mrn()
		{
			bill.H7MovementReferenceNumber = string.Empty;
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithDocCsvIdClearanceRequest(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>VRMNB3HSMAFTYUDR</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>0000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Operación Correcta</td></tr></table>" +
				"<H4>Additional Information</H4>" +
				"<table border=\"0\"><tr><td>Document CSV ID:</td><td>&nbsp;&nbsp;</td><td>SMPU4X96QKZZTZG9</td></tr></table>" +
				"<table border=\"0\"><tr><td>Clearance Request:</td><td>&nbsp;&nbsp;</td><td>S</td></tr></table>";

			AssertAcceptedResults(message, bill, false, expectedMessageInterpretationText);
		}

		public void TestProcessRejectedMessage()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessage(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>NNNN</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Description:</td><td>&nbsp;&nbsp;</td><td>Error</td></tr></table>";

			CombineAssertions(() =>
			{
				AssertEquals("Bill Message Status", LogicalStatusList.Codes.Error, bill.ABL_MessageStatus);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("Message Sub Type", DeclarationMessageSubTypeList.Codes.RejectedResponse, message.EM_MessageSubType);
				AssertEquals("Message Interpretation", expectedMessageInterpretationText, message.EM_MessageInterpretation);

				var sentMessage = bill.Messages.Where(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.H7Query).FirstOrDefault();
				AssertNull("Query H7 message should not be sent", sentMessage);
			});
		}

		void AssertAcceptedResults(TestEdiMessage responseMessage, AsycudaBill bill, bool expectedMessageSent = true, string expectedMessageInterpretation = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("Message Sub Type", DeclarationMessageSubTypeList.Codes.AcceptedResponse, responseMessage.EM_MessageSubType);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("Message Interpretation", expectedMessageInterpretation, responseMessage.EM_MessageInterpretation);

				var sentMessage = bill.Messages.Where(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.H7Query).FirstOrDefault();

				if (expectedMessageSent)
				{
					var delayTime = new DateTime(2025, 5, 26, 11, 50, 30);
					AssertNotNull("Query H7 message should be sent", sentMessage);
					Assert("Query H7 message should include MRN", sentMessage.EM_MessageText.Contains($"<MRN_H7>MRN0001</MRN_H7>"));
					AssertEquals("Query H7 message's EM_HeldUntilDate as EM_SystemCreateTimeUTC plus 150 seconds.", delayTime, sentMessage.EM_HeldUntilDate);
					AssertEquals("Message Status should be SNT", LogicalStatusList.Codes.Sent, bill.ABL_MessageStatus);
				}
				else
				{
					AssertNull("Query H7 message should not be sent", sentMessage);
					AssertEquals("Message Status should be ACC", LogicalStatusList.Codes.Accepted, bill.ABL_MessageStatus);
				}
			});
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var interchangeID = ZGuid.NewZGuid();

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_BillNumber = ApplicationReference;
			bill.ABL_MessageStatus = string.Empty;

			var staffWithCertificateHelper = new StaffWithCertificateTestHelper(Factory);
			bill.Header.AMA_GS_NKCustomsAgent = staffWithCertificateHelper.Staff.GS_Code;
			bill.H7MovementReferenceNumber = "MRN0001";

			SetSentInterchange(bill, InterchangeID);
		}

		protected AsycudaBill bill;

		string GetAcceptedMessageWithClearanceRequest() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.AnnexH7TestFilePath, "AcceptedMessageWithClearanceRequest.xml");
		string GetAcceptedMessageWithDocCsvId() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.AnnexH7TestFilePath, "AcceptedMessageWithDocCsvId.xml");
		string GetAcceptedMessageWithDocCsvIdClearanceRequest() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.AnnexH7TestFilePath, "AcceptedMessageWithDocCsvIdClearanceRequest.xml");
		string GetAcceptedMessageWithoutDocCsvIdClearanceRequest() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.AnnexH7TestFilePath, "AcceptedMessageWithoutDocCsvIdClearanceRequest.xml");
		string GetRejectedMessage() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.AnnexH7TestFilePath, "RejectedMessage.xml");
	}
}
