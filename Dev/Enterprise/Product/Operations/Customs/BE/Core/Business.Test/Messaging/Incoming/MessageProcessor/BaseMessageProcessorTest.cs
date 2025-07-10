using System;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(BaseMessageProcessorForTest))]
sealed class BaseMessageProcessorTest : MessageProcessorTestCase<BaseMessageProcessorForTest, IInboundProvider>
{
	protected override string ExpectedMessageFriendlyName => "Message Friendly Name Overrode";

	protected override Type ExpectedMessageInterpreterType => null;

	protected override BaseMessageProcessorForTest Processor => processor;

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCodeList.Codes.BECustoms, Processor.ApplicationCode);
	}

	public void TestPreProcessMessage_InvalidXml()
	{
		Processor.TriggerXmlException = true;
		var ediMessage = CreateIncomingMessage(Factory);
		Processor.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Error, ediMessage.EM_Status);
			AssertNotNull("ExceptionLog", ediMessage.Notes.FindByDescription(Constants.MessageProcessingNotes.ExceptionLog));
		});
	}

	public void TestPreProcessMessage_Fail()
	{
		Processor.FindParentOfMessageSuccess = false;
		var ediMessage = CreateIncomingMessage(Factory);
		Processor.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Failed, ediMessage.EM_Status);
			AssertEquals("Error log", true, logger.Logs.Any(log => log.Type == LogType.Error && log.Message.StartsWith("Unable to find a linked business object for message")));
		});
	}

	public void TestPreProcessMessage_Success()
	{
		Processor.FindParentOfMessageSuccess = true;
		var ediMessage = CreateIncomingMessage(Factory);
		Processor.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.PreProcessedOK, ediMessage.EM_Status);
			AssertNotNull("EM_LinkedObject", ediMessage.EM_LinkedObject);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, ediMessage.EM_GB);
		});
	}

	public void TestPreProcessMessage_SkipNonQueueMessage()
	{
		Processor.FindParentOfMessageSuccess = true;
		var ediMessage = CreateIncomingMessage(Factory);
		ediMessage.EM_Status = EDIMessage.Status.Error;
		Processor.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Error, ediMessage.EM_Status);
			AssertNull("EM_LinkedObject", ediMessage.EM_LinkedObject);
		});
	}

	[TestDate(2022, 3, 12, 4, 0, 0)]
	public void TestPreProcessMessage_WrongMessageSequence_RetryCountLessThan5()
	{
		var processorForWrongMessageSequenceTest = new BaseMessageProcessorForWrongMessageSequenceTest(logger);
		processorForWrongMessageSequenceTest.FindParentOfMessageSuccess = true;
		var ediMessage = CreateIncomingMessage(Factory);
		ediMessage.EM_RetryCount = 1;
		var expectedSeconds = ediMessage.EM_RetryCount * 60 + 5;

		processorForWrongMessageSequenceTest.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("MessageSequenceIsValid", false, CheckMessageSequenceIsValid(ediMessage, logger));
			AssertEquals("Error log does not contain DISCARDED", false, logger.Logs.Any(log => log.Type == LogType.Error && log.Message.IndexOf("DISCARDED", StringComparison.InvariantCultureIgnoreCase) >= 0));
			AssertEquals("EM_RetryCount", (ZByte)2, ediMessage.EM_RetryCount);
			AssertEquals("EM_HeldUntilDate", new ZDateTime(2022, 3, 12, 4, 0, 0).AddSeconds(expectedSeconds), ediMessage.EM_HeldUntilDate);
		});
	}

	[TestDate(2022, 3, 12, 4, 0, 0)]
	public void TestPreProcessMessage_WrongMessageSequence_RetryCountMoreOrEqual5()
	{
		var processorForWrongMessageSequenceTest = new BaseMessageProcessorForWrongMessageSequenceTest(logger);
		processorForWrongMessageSequenceTest.FindParentOfMessageSuccess = true;
		var ediMessage = CreateIncomingMessage(Factory);
		ediMessage.EM_RetryCount = 5;
		ediMessage.EM_HeldUntilDate = new ZDateTime(2022, 3, 12, 4, 0, 0);

		processorForWrongMessageSequenceTest.PreProcessMessage(ediMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, ediMessage.EM_Status);
			AssertEquals("Log added", true, logger.Logs.Any(log => log.Type == LogType.Error && log.Message.StartsWith("The message was discarded") && log.Message.Contains("MessageSequenceInvalidMessage test")));
			AssertEquals("Message interpretation set", true, ediMessage.EM_MessageInterpretation.Contains("MessageSequenceInvalidMessage test"));
		});
	}

	public void TestPreProcessMessage_InterpretMessageNoNullPointerException()
	{
		var processor = new BaseMessageProcessorForNoNullPointerExceptionWhenInterpretTest(logger);
		processor.FindParentOfMessageSuccess = true;
		var ediMessage = CreateIncomingMessage(Factory);
		processor.PreProcessMessage(ediMessage);
		Assert("Although can't find Note whose description is Processing log, no NullPointerException is thrown", ediMessage.EM_MessageInterpretation.IsEmpty);
	}

	public void TestMessageSequenceInvalidMessage()
	{
		var messageSequenceInvalidMessageProperty = typeof(BaseMessageProcessorForTest).GetProperty("MessageSequenceInvalidMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		AssertNull(messageSequenceInvalidMessageProperty.GetValue(Processor));
	}

	public void TestProcessMessage_OnlyAfterPreProcessedOK()
	{
		CombineAssertions(() =>
		{
			var ediMessage = CreateIncomingMessage(Factory);
			Processor.ProcessMessage(ediMessage);
			AssertEquals("Process won't happen if not PreProcessedOK", false, Processor.Processed);
			ediMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
			Processor.ProcessMessage(ediMessage);
			AssertEquals("Process only after PreProcessedOK", true, Processor.Processed);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		logger = new LoggingInformation();
		processor = new BaseMessageProcessorForTest(logger);
	}

	LoggingInformation logger;
	BaseMessageProcessorForTest processor;
}

class BaseMessageProcessorForTest : BaseMessageProcessor<IInboundProvider>
{
	public BaseMessageProcessorForTest(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => "Message Friendly Name Overrode";

	public bool FindParentOfMessageSuccess { get; set; }

	public bool TriggerXmlException { get; set; }

	public bool Processed { get; private set; }

	protected internal override IInboundProvider GetMessageDataProvider(BEMessage message) => TriggerXmlException ? throw new Exception("An error occured", new XmlSchemaValidationException("you forgot the namespace")) : new Mock<IInboundProvider>().Object;

	protected override BusinessObject FindParentOfMessage(BEMessage message, IInboundProvider messageDataProvider) => FindParentOfMessageSuccess ? message.Factory.New<DummyBusinessObject>() : null;

	protected override void ProcessMessageCore(BEMessage message, IInboundProvider messageDataProvider) => Processed = true;

	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => GlbBranch.CurrentBranch.PK;
}

class BaseMessageProcessorForWrongMessageSequenceTest : BaseMessageProcessorForTest
{
	public BaseMessageProcessorForWrongMessageSequenceTest(LoggingInformation logger) : base(logger) { }

	protected override bool CheckMessageSequenceIsValidCore(BEMessage message) => false;

	protected override string MessageSequenceInvalidMessage => "MessageSequenceInvalidMessage test";

	protected override Type MessageInterpreterType => typeof(BaseMessageInterpreterForWrongMessageSequenceTest);
}

class BaseMessageProcessorForNoNullPointerExceptionWhenInterpretTest : BaseMessageProcessorForTest
{
	public BaseMessageProcessorForNoNullPointerExceptionWhenInterpretTest(LoggingInformation logger) : base(logger) { }

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, IInboundProvider messageDataProvider)
	{
		message.EM_Status = EDIMessage.Status.Discarded;
	}

	protected override Type MessageInterpreterType => typeof(BaseMessageInterpreterForWrongMessageSequenceTest);
}

class BaseMessageInterpreterForWrongMessageSequenceTest : BaseMessageInterpreter<IInboundProvider>
{
	public override string Interpret(IInboundProvider dataProvider, EDIMessage ediMessage)
	{
		return null;
	}
}
