using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public abstract class NCTS5CommonInboxNotificationResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESNCTSResponseMessageProcessorTest<TResponse, TResponseProvider>
		where TResponse : NCTS5CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode
		where TPrettyMessage : IMessagePrettyFormatter
	{
		public void TestProcessMessageAcceptedWithSegmentIdTooLong()
		{
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No exception expected since SementId is cut to 35 chars", () => ProcessMessageForTest(message));

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			});
		}

		protected void AssertNCTSDeclaration(TestEdiMessage message, string messageSubType, string expectedMessageInterpretation = "", string commonCustomsStatus = "", string emStatus = EDIMessageStatusList.Codes.Received, string messageStatus = "", CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberClearance = null, string mrnEntrynum = "", string mrnEntryStatus = "", string clearanceReferenceNumber = "", ZDateTime? clearanceIssueDate = null, ZDateTime? mrnIssueDate = null, ZDateTime? clearanceExpiryDate = null)
		{
			GenericCommonAssertProcessEntryDataNCTS(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageNum: MessageNum, commonCustomsStatus: commonCustomsStatus, emStatus: emStatus, messageStatus: messageStatus, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, mrnEntrynum: mrnEntrynum, mrnEntryStatus: mrnEntryStatus, clearanceReferenceNumber: clearanceReferenceNumber, clearanceIssueDate: clearanceIssueDate, mrnIssueDate: mrnIssueDate, clearanceExpiryDate: clearanceExpiryDate, movementReferenceNumber: mrnEntrynum, clearanceDate: clearanceIssueDate, arrivalLimit: clearanceExpiryDate);
		}

		protected virtual ZString GetAcceptanceTestFile() => string.Empty;
		protected abstract string GetAcceptanceTestFileWithLongSegmentId();

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GetMrncode(), ES.Business.Declaration.MessageFunctionCodeList.Codes.GreenCircuitText);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(nctsHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);

			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(nctsHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFile();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected virtual void AssertLoggerMessagesWhenProcessMessageWrongXML()
		{
			AssertContains("logger", "Unable to read message text from message", GetAllConcatenatedUserLogStrings());
		}

		protected virtual void AssertLoggerMessagesWhenMessageProcessingError()
		{
			AssertContains("Log has error", "Unable to find business object for message", GetAllConcatenatedUserLogStrings());
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GetMrncode(), ZString.Empty);

			SetSentInterchange(nctsHeader, InterchangeID);
		}

		protected virtual ZString GetMrncode() => MRNCodeClearedNotPending;

		const string MRNCodeClearedNotPending = "22ES000101500647AA";

		protected const string MessageNum = "20221202112032086006";
	}
}
