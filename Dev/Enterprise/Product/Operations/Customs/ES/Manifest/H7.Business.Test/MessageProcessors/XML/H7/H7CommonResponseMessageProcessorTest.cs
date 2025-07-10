using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public abstract class H7CommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponse, AsycudaBill, TResponseProvider>
		where TResponse : H7CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode, IH7CommonErrors
		where TPrettyMessage : IMessagePrettyFormatter
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

		public void TestProcessRejectedMessage_OneErrorTypeF()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithOneErrorTypeF(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>101</td><td>301</td><td>/ExportOperation/item1</td><td>Error 1</td><td>201</td><td>20ES0099</td></tr>" +
				"</table>";

			AssertRejectedResults(message, expectedBillMessageStatus: LogicalStatusList.Codes.Invalid, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessRejectedMessage_OneErrorTypeN()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithOneErrorTypeN(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>101</td><td>301</td><td>/ExportOperation/item1</td><td>Error 1</td><td>201</td><td>20ES0099</td></tr>" +
				"</table>";

			AssertRejectedResults(message, expectedBillMessageStatus: LogicalStatusList.Codes.Error, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessRejectedMessage_MultipleErrorsTypeF()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithMultipleErrorsTypeF(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>101</td><td>301</td><td>/ExportOperation/item1</td><td>Error 1</td><td>201</td><td>20ES0099</td></tr>" +
				"<tr><td>102</td><td>302</td><td>/ExportOperation/item2</td><td>Error 2</td><td>202</td><td>20ES0088</td></tr>" +
				"</table>";

			AssertRejectedResults(message, expectedBillMessageStatus: LogicalStatusList.Codes.Invalid, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessRejectedMessage_MultipleErrors()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetRejectedMessageWithMultipleErrors(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Pointer</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Item</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>101</td><td>301</td><td>/ExportOperation/item1</td><td>Error 1</td><td>201</td><td>20ES0099</td></tr>" +
				"<tr><td>102</td><td>302</td><td>/ExportOperation/item2</td><td>Error 2</td><td>202</td><td>20ES0088</td></tr>" +
				"</table>";

			AssertRejectedResults(message, expectedBillMessageStatus: LogicalStatusList.Codes.Error, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestFindRelevantBusinessObject()
		{
			var trackingId1 = ZGuid.NewZGuid();
			var billWithNumber = Factory.NewWithValidTestData<AsycudaBill>();
			billWithNumber.ABL_BillNumber = "TestBillNumber";

			SetSentInterchange(billWithNumber, trackingId1);
			var incomingMessageWillAttachToBillWithBillNumber = CreateNewEDIMessage("", "", trackingId1, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(incomingMessageWillAttachToBillWithBillNumber);
			AssertEquals("Find relevant business object when bill number is not empty", billWithNumber, incomingMessageWillAttachToBillWithBillNumber.EM_LinkedObject);

			var trackingId2 = ZGuid.NewZGuid();
			var billWithoutNumber = Factory.NewWithValidTestData<AsycudaBill>();
			SetSentInterchange(billWithoutNumber, trackingId2);
			var incomingMessageWillAttachToBillWithoutBillNumber = CreateNewEDIMessage("", "", trackingId2, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(incomingMessageWillAttachToBillWithoutBillNumber);
			AssertEquals("Find relevant business object when bill number is empty", billWithoutNumber, incomingMessageWillAttachToBillWithoutBillNumber.EM_LinkedObject);
		}

		void AssertRejectedResults(TestEdiMessage message, string expectedBillMessageStatus, string expectedMessageInterpretation)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Bill Message Status", expectedBillMessageStatus, bill.ABL_MessageStatus);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Received, message.EM_Status);
				AssertEquals("Message Sub Type", Messaging.DeclarationMessageSubTypeList.Codes.RejectedResponse, message.EM_MessageSubType);
				AssertEquals("Message Interpretation", expectedMessageInterpretation, message.EM_MessageInterpretation);
			});
		}

		protected ABLEntryNum GetCusEntryNumber(AsycudaBill bill, string entryType, string entryNum, ZDateTime issueDate, string entryStatus = null, string entryLineReference = H7EntryLineReference)
		{
			return bill.CustomsEntryNumbers.OfType<ABLEntryNum>().FirstOrDefault(x =>
				x.CE_EntryType == entryType
				&& (entryNum == null || x.CE_EntryNum == entryNum)
				&& (entryLineReference == null || x.CE_EntryLineReference == H7EntryLineReference)
				&& (entryStatus == null || x.CE_EntryStatus == entryStatus)
				&& (issueDate.IsEmpty || x.CE_IssueDate == issueDate));
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

		protected void SetupOutgoingMessageWithCertificate(EDIMessage incomingMessage)
		{
			var sentMessages = MessageProcessorHelper.GetRelatedSentInterchange(incomingMessage).ContainedMessages;
			sentMessages[0].EM_ApplicationReference = "certName";
		}

		protected virtual void SetupTestData()
		{
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_BillNumber = ApplicationReference;
			bill.ABL_MessageStatus = string.Empty;

			var staffWithCertificateHelper = new StaffWithCertificateTestHelper(Factory);
			bill.Header.AMA_GS_NKCustomsAgent = staffWithCertificateHelper.Staff.GS_Code;

			SetSentInterchange(bill, InterchangeID);
		}

		protected AsycudaBill bill;

		protected const string DocumentationRequiredS = "S";
		protected const string DocumentationRequiredN = "N";
		protected const string H7EntryLineReference = "H7";
		protected const string G3EntryLineReference = "G3";

		protected abstract string GetRejectedMessageWithOneErrorTypeN();
		protected abstract string GetRejectedMessageWithOneErrorTypeF();
		protected abstract string GetRejectedMessageWithMultipleErrorsTypeF();
		protected abstract string GetRejectedMessageWithMultipleErrors();
	}
}
