using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class InboundMessageCreatorAbstractTest : TestCaseWithFactory
{
	public virtual void TestResponseDoNotContainContentType()
	{
		var interchange = Factory.New<EDIInterchange>();

		interchange.EI_BodyText = @"------=_Part_
			Test String";

		MessageCreator.CreateMessagesForInterchange(interchange);
		AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
		AssertEquals("EDI Message Status should be: ", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages[0].EM_Status);
	}

	public void TestEmptyInterchangeBody()
	{
		var interchange = Factory.New<EDIInterchange>();
		MessageCreator.CreateMessagesForInterchange(interchange);

		CombineAssertions(() =>
		{
			AssertEquals($"When body is empty, Interchange status should be: {EDIInterchange.Status.Error}", EDIInterchange.Status.Error, interchange.EI_Status);
			AssertEquals("Log Error", "NO CH CUSTOMS DATA", interchange.Logs.MostRecentLogByEventTime(Events.ErrorReport)?.SL_Reference);
		});
	}

	public virtual void TestUnrecognizedResponseMessageType()
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = UnrecognizedResponseMessage;
		MessageCreator.CreateMessagesForInterchange(interchange);

		AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message Status should be: ", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages[0].EM_Status);
			AssertContains("EDI Message Log should contain: ", ExpectedUnrecognizedResponseMessageTypeLogMessage, interchange.ContainedMessages[0].Logs.MostRecentLog?.ReferenceFreeText);
		});
	}

	protected virtual string UnrecognizedResponseMessage => @"Content-Type: type

			Unrecognized Content";

	protected virtual string ExpectedUnrecognizedResponseMessageTypeLogMessage => "Unrecognized response message type";

	public void TestFailingDeserialization()
	{
		var baseMessageCreator = MessageCreator as BaseInboundMessageCreator;
		if (baseMessageCreator?.RemoveSoapEnvelope ?? false)
		{
			foreach (var failingDeserialization in FailingDeserializations)
			{
				AssertFailingDeserialization(failingDeserialization.From, failingDeserialization.BodyText);
			}
		}
		else
		{
			Assert("Message is not deserialized during EDI Message creation", true);
		}
	}

	public void TestUnsupportedSchemaVersionException()
	{
		if (!string.IsNullOrWhiteSpace(UnsupportedSchemaVersion))
		{
			const string errorMessageContaining = "Not Supported XML Schema";
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_BodyText = UnsupportedSchemaVersion;
			MessageCreator.CreateMessagesForInterchange(interchange);

			CombineAssertions(() =>
			{
				AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
				AssertEquals("EDI Message Status should be: ", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages[0]?.EM_Status);
				ErrorReporter.Instance.Clear();
				AssertContains("ErrorReporter last message should contain: ", errorMessageContaining, ErrorReporter.LastMessageReported);
				Assert("Log Error", Logger.Logs.Any(log => log.Type == Integration.LogType.Error && log.Message.Contains(errorMessageContaining)));
			});
		}
		else
		{
			Assert("Message does not contain a schema version", true);
		}
	}

	protected abstract IInboundMessageCreator GetMessageCreator();

	protected abstract string ApplicationCode { get; }

	protected abstract string InterchangeType { get; }

	protected abstract string GetExpectedMessageText(string parsingResult);

	protected abstract (string From, string BodyText)[] FailingDeserializations { get; }

	protected abstract string UnsupportedSchemaVersion { get; }

	protected IInboundMessageCreator MessageCreator => messageCreator ?? (messageCreator = GetMessageCreator());
	IInboundMessageCreator messageCreator;

	protected LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
	LoggingInformation logger;

	protected void TestGenerateMessagesFromInterchange(string bodyText, string expectedMessageSubType, string expectedMessageText = null)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCode;
		interchange.EI_BodyText = bodyText;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_InterchangeType = InterchangeType;

		MessageCreator.CreateMessagesForInterchange(interchange);

		AssertEquals($"InboundMessageCreatorTest for {ApplicationCode} should create 1 EDI Messages", 1, interchange.ContainedMessages.Count);
		AssertEDIMessage(interchange, interchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault(), expectedMessageSubType, "0001", expectedMessageText);

		void AssertEDIMessage(EDIInterchange interchange, EDIMessage message, string expectedMessageSubType, string messageNum, string expectedMessageText)
		{
			CombineAssertions($"EDIMessage {messageNum}", () =>
			{
				AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", interchange.EI_InterchangeType, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", expectedMessageSubType, message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_MessageNum", messageNum, message.EM_MessageNum);
				AssertEquals("EM_EI", interchange.PK, message.EM_EI);
				AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
				AssertEquals("EM_MessageText", expectedMessageText ?? GetExpectedMessageText(MIMETypeXTParser.ParseTextHttp(interchange.EI_BodyText).First().BodyText), message.EM_MessageText);
			});
		}
	}

	protected void TestGenerateMessagesFromInterchange(string from, string interchangeType, string bodyText, int expectedNumberOfMessages)
	{
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_ApplicationCode = ApplicationCode;
		interchange.EI_BodyText = bodyText;
		interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		interchange.EI_Status = EDIInterchange.Status.Queued;
		interchange.EI_InterchangeType = interchangeType;
		interchange.EI_From = from;
		interchange.EI_To = "TEST";

		MessageCreator.CreateMessagesForInterchange(interchange);

		AssertEquals($"InboundMessageCreatorTest for {ApplicationCode} should create {expectedNumberOfMessages} EDI Messages", expectedNumberOfMessages, interchange.ContainedMessages.Count);

		var messageNumber = 1;
		foreach (EDIMessage message in interchange.ContainedMessages)
		{
			AssertEDIMessage(interchange, message, messageNumber++.ToString().PadLeft(4, '0'));
		}

		void AssertEDIMessage(EDIInterchange interchange, EDIMessage message, string messageNum)
		{
			CombineAssertions($"EDIMessage {messageNum}", () =>
			{
				AssertEquals("EM_ApplicationCode", interchange.EI_ApplicationCode, message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", interchange.EI_InterchangeType, message.EM_MessageType);
				AssertNotNullOrEmpty("EM_MessageSubType", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_MessageNum", messageNum, message.EM_MessageNum);
				AssertEquals("EM_EI", interchange.PK, message.EM_EI);
				AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
			});
		}
	}

	protected void AssertLog(string message, LogType logType) => Assert($@"Log {logType} expected: ""{message}""", Logger.Logs.Any(log => log.Type == logType && log.Message.Contains(message)));

	protected void AssertResponseParsingResult(ResponseParsingResult parsingResult, string type, string description, bool isXml, bool isPdf)
	{
		AssertEquals("Type", type, parsingResult.Type);
		AssertEquals("Description", description, parsingResult.Description);
		AssertEquals("IsXml", isXml, parsingResult.IsXml);
		AssertEquals("IsPdf", isPdf, parsingResult.IsPdf);
	}

	void AssertFailingDeserialization(string from, string bodyText)
	{
		var interchange = Factory.New<EDIInterchange>();

		interchange.EI_From = from;
		interchange.EI_BodyText = bodyText;

		MessageCreator.CreateMessagesForInterchange(interchange);
		AssertEquals("Interchange should contain 1 EDI Message", 1, interchange.ContainedMessages.Count);
		CombineAssertions(() =>
		{
			AssertEquals("EDI Message Status should be: ", EDIMessageStatusList.Codes.Discarded, interchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault().EM_Status);
			ErrorReporter.Instance.Clear();
			AssertContains("ErrorReporter last message should contain: ", "error reading", ErrorReporter.LastMessageReported);
			AssertLog("error reading", logType: LogType.Error);
		});
	}
}
