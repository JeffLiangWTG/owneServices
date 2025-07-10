using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(XtErrorMessageProcessor))]
sealed class XtErrorMessageProcessorTest : TestCaseWithFactory
{
	public void TestSetMessageStatus()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageType = Constants.MessageType.XtErrorResponse;
		var log = new BatchProcessor.LoggingInformation();
		var processor = new XtErrorMessageProcessor(incomingMessage, log);

		CombineAssertions(() =>
		{
			ErrorReporter.Clear();
			processor.Process();
			AssertEquals("LinkObject is null, ErrorReported", "LinkObject is not IMessageAttachee, type is null", ErrorReporter.LastMessageReported);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			incomingMessage.EM_LinkedObject = orgHeader;
			ErrorReporter.Clear();
			processor.Process();
			AssertEquals("LinkObject is OrgHeader, ErrorReported", "LinkObject is not IMessageAttachee, type is Enterprise.MasterFiles.Business.OrgHeader", ErrorReporter.LastMessageReported);

			incomingMessage.EM_LinkedObject = EntryHeader;
			ErrorReporter.Clear();
			processor.Process();
			AssertEquals("LinkObject is CusEntryHeader, Status", MessageStatusList.Codes.MessageDeliveryFailed, EntryHeader.CH_Status);
			AssertEquals("LinkObject is CusEntryHeader, ErrorReported", ZString.Empty, ErrorReporter.LastMessageReported);
		});
	}

	public void TestGetLinkedObject()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageType = Constants.MessageType.XtErrorResponse;
		var log = new BatchProcessor.LoggingInformation();
		var processor = new XtErrorMessageProcessor(incomingMessage, log);

		CombineAssertions(() =>
		{
			AssertNull("When outgoing/request message not linked", processor.GetLinkedObject());
			AssertEquals(" *** \tGet LinkedObject by XtErrorMessageProcessor", log.DebugLogStrings[0]);

			incomingMessage.EM_LinkTable = EntryHeader.TableName;
			incomingMessage.EM_LinkUniqueID = EntryHeader.PK;

			AssertSame("Messages are linked", EntryHeader, processor.GetLinkedObject());
		});
	}

	public void TestProcessMessage()
	{
		var incomingMessage = Factory.New<EDIMessage>();
		incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		incomingMessage.EM_MessageType = Constants.MessageType.XtErrorResponse;
		incomingMessage.EM_LinkedObject = EntryHeader;
		var log = new BatchProcessor.LoggingInformation();
		var processor = new XtErrorMessageProcessor(incomingMessage, log);
		CombineAssertions(() =>
		{
			Assert(processor.Process());
			AssertEquals(" *** \tMessage processing by XtErrorMessageProcessor", log.DebugLogStrings[0]);
		});
	}

	CusEntryHeader EntryHeader => entryHeader ??= Factory.New<CusEntryHeader>();
	CusEntryHeader entryHeader;
}
