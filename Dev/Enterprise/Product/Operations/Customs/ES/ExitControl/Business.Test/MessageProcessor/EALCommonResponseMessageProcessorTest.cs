using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public abstract class EALCommonResponseMessageProcessorTest<TResponse, TPrettyMessage, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponse, CusExitReport, TResponseProvider>
		where TResponse : EALCommonResponseMessageProcessor<TResponseProvider, TPrettyMessage>
		where TResponseProvider : class, ICommonServiceSegment, IResponseCode, IMRNField
		where TPrettyMessage : IMessagePrettyFormatter
	{
		public void TestProcessMessageWrongXML()
		{
			var responseMessage = CreateNewEDIMessage(GetMRNCode(), WrongXMLTestFile, InterchangeID);

			ProcessMessageForTest(responseMessage);

			AssertEALCommonDeclaration(responseMessage, statusCode: OriginalEntryStatus, MessageStatusForWrongXML, messageStatus: EDIMessage.Status.Failed, messageSubType: "AAA", loggerDesc: GetLoggerMessagesWhenProcessMessageWrongXML());
		}

		protected virtual ZString MessageStatusForWrongXML => LogicalStatusList.Codes.Failed;

		public void TestMessageProcessingError()
		{
			var responseMessage = CreateNewEDIMessage("AAAAAAAA", ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			GenericCommonAssertProcessResponseOthers(responseMessage, emStatus: EDIMessage.Status.Failed, loggerDesc: GetLoggerMessagesWhenProcessMessageError());
		}

		public void TestMessageProcessingErrorNoReference()
		{
			var responseMessage = CreateNewEDIMessage(ZString.Empty, ZString.Empty, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			GenericCommonAssertProcessResponseOthers(responseMessage, emStatus: EDIMessage.Status.Failed, loggerDesc: "No Application Reference found for message");
		}

		public void TestProcessMessageAcceptedWithSegmentIdTooLong()
		{
			var message = CreateNewEDIMessage(GetMRNCode(), GetAcceptanceTestFileWithLongSegmentId(), InterchangeID);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("No exception expected since SementId is cut to 35 chars", () => ProcessMessageForTest(message));

				AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			});
		}

		public void TestGetRelevantBusinessObjectFindQuery_EHub()
		{
			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationReference = GetMRNCode();
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { report };

			var messageProcessor = new EALCommonResponseMessageProcessorForTest<TResponseProvider, TPrettyMessage>(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, false);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct report", report, foudBusinessObject);
		}

		public void TestGetRelevantBusinessObjectFindQuery_DirectxT()
		{
			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(report, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			var message = Factory.New<EDIMessage>();
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { report };

			var messageProcessor = new EALCommonResponseMessageProcessorForTest<TResponseProvider, TPrettyMessage>(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, true);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct report", report, foudBusinessObject);
		}

		protected void AssertEALCommonDeclaration(TestEdiMessage message, string statusCode, string cerMessageStatus = LogicalStatusList.Codes.Accepted, string messageStatus = EDIMessage.Status.Received, string messageSubType = "ACC", string messageNum = "", string loggerDesc = "", string expectedMessageInterpretation = "")
		{
			AssertEquals("CER_Status", statusCode, report.CER_Status);
			AssertEquals("CER_MessageStatus", cerMessageStatus, report.CER_MessageStatus);
			GenericCommonAssertProcessResponseOthers(message, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: messageStatus, messageNum: messageNum, messageSubType: messageSubType, loggerDesc: loggerDesc);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;

			report.CER_Status = OriginalEntryStatus;
			consignment.CXC_MovementReference = GetMRNCode();

			SetSentInterchange(report, InterchangeID);
		}
		protected CusExitHeader header;
		protected CusExitReport report;

		protected const string MRNCode = "22ES000101100171B8";
		protected const string MessageNum = "20220627163230766004";

		protected virtual ZString GetMRNCode() => MRNCode;

		protected abstract string GetAcceptanceTestFileWithLongSegmentId();

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var header = Factory.NewWithValidTestData<CusExitHeader>();
				var consignment = header.CusExitConsignments.AddNew();
				var report = header.CusExitReports.AddNew();
				report.CER_CXC_Consignment = consignment.PK;
				consignment.CXC_MovementReference = "TEST";

				var interchangeID = ZGuid.NewZGuid();

				SetSentInterchange(report, interchangeID);

				var message = Factory.New<TestEdiMessage>();
				message.EM_ApplicationReference = "TEST";
				var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
				responseInterchange.ContainedMessages.Add(message);
				return message;
			}
		}

		protected virtual ZString GetLoggerMessagesWhenProcessMessageWrongXML() => "Unable to read message text from message";
		protected virtual ZString GetLoggerMessagesWhenProcessMessageError() => "Unable to find business object for message";

		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			if (businessObject is CusExitReport exitReport)
			{
				AssertEquals("CER_MessageStatus", "FAL", exitReport.CER_MessageStatus);
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

		class EALCommonResponseMessageProcessorForTest<TResponseProviderForTest, TPrettyMessageForTest> : EALCommonResponseMessageProcessor<TResponseProviderForTest, TPrettyMessageForTest>
			where TResponseProviderForTest : class, ICommonServiceSegment, IResponseCode, IMRNField
			where TPrettyMessageForTest : IMessagePrettyFormatter
		{
			public EALCommonResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			protected override string MessageFriendlyNameCore => ZString.Empty;
			protected override ZString XsdSchemaEmbeddedResourceName => ZString.Empty;
			protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { ZString.Empty };

			protected override TResponseProviderForTest GetMessageProviderCore(EDIMessage message) => null;

			public CusExitReport FindRelevantBusinessObjectCoreExposed(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage) => FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);

			protected override TPrettyMessageForTest GetNewMessagePrettyFormatter(TResponseProviderForTest response) => default(TPrettyMessageForTest);

			protected override void ProcessAcceptedDeclaration(TResponseProviderForTest response, EDIMessage message, CusExitReport report) { }
		}
	}
}
