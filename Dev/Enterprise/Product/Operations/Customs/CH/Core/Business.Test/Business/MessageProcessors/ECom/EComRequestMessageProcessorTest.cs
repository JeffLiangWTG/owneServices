using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EComRequestMessageProcessor))]
class EComRequestMessageProcessorTest : BaseInboundMessageProcessorTest
{
	protected override ZString ExpectedMessageFriendlyName => "Customs ECom Message Request Processor";

	protected override ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override (string MessageType, Event ExpectedEvent)[] MessageTypes => new[] { ( MessageTypeCodeList.Codes.ECM, Events.EComStatusChange) };

	protected override ZString MessageSubType => MessageSubTypeCodeList.Codes.Request;

	protected override ZString ReceivedMessage => TestingData.GetEComRequest();

	protected override ZString BGMReference => ZString.Empty;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EComRequestMessageProcessor(Logger);

	public override (CusEntryHeader entryHeader, EDIMessage ediMessage) PrepareDataForLinkToParent_Success(BusinessObjectFactory factory, ZString messageType) => MessageProcessorTestHelper.CreateHeaderAndMessageWithCusEntryNum(factory, MessageTypeCodeList.Codes.ECM, MessageSubTypeCodeList.Codes.Request, TestingData.GetEComRequest(customsDeclarationNumber: "24CH202410090001J0"), "24CH202410090001J0");

	public void TestMessageLinkedByGDRN_AnyVersion() => CombineAssertions(() =>
	{
		const string baseMRN1 = "24CH202410090001J0";
		const string baseMRN2 = "24CH202410090002J0";
		const string baseMRN3 = "24CH202410090003J9";
		const string baseMRN4 = "24CH202410090004J8";

		AssertLinked(baseMRN1, baseMRN1);
		AssertLinked(baseMRN2, baseMRN2 + ".1");
		AssertLinked(baseMRN3 + ".1", baseMRN3);
		AssertLinked(baseMRN4 + ".1", baseMRN4 + ".2");

		void AssertLinked(string messageCustomsDeclarationNumber, string entryHeaderMRN)
		{
			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessageWithCusEntryNum(Factory, MessageTypeCodeList.Codes.ECM, MessageSubTypeCodeList.Codes.Request, TestingData.GetEComRequest(customsDeclarationNumber: messageCustomsDeclarationNumber), entryHeaderMRN);
			Factory.Save();

			ProcessMessage(ediMessage);

			AssertSame($"Message {messageCustomsDeclarationNumber} linked to EntryHeader {entryHeaderMRN}", entryHeader, ediMessage.EM_LinkedObject);
		}
	});

	public void TestMessageProcessed() => CombineAssertions(() =>
	{
		const string MRN = "24CH202410090001J0";

		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessageWithCusEntryNum(Factory, MessageTypeCodeList.Codes.ECM, MessageSubTypeCodeList.Codes.Request, TestingData.GetEComRequest(customsDeclarationNumber: MRN), MRN);
		entryHeader.CH_LastEComplaintStatus = "XXX";
		Factory.Save();

		ProcessMessage(ediMessage);

		AssertEquals("CH_LastEComplaintStatus", EComplaintStatusList.Codes.Received, entryHeader.CH_LastEComplaintStatus);
		EventsTestHelper.AssertEventAdded(entryHeader, Events.EComStatusChange, $"|NEW={EComplaintStatusList.Codes.Received}|OLD=XXX");
	});

	[TestDate()]
	public void TestEvent_NotWrittenIfNoStatusChange() => CombineAssertions(() =>
	{
		const string MRN = "24CH202410090001J0";

		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessageWithCusEntryNum(Factory, MessageTypeCodeList.Codes.ECM, MessageSubTypeCodeList.Codes.Request, TestingData.GetEComRequest(customsDeclarationNumber: MRN), MRN);
		entryHeader.CH_LastEComplaintStatus = "XXX";
		Factory.Save();

		ProcessMessage(ediMessage);
		ediMessage.EM_Status = EDIMessage.Status.Queued;
		Factory.Save();

		TestDateAttribute.AddMinutes(1);
		var processStartTime = ZDateTime.UtcNow;

		ProcessMessage(ediMessage);
		AssertEquals("2nd message has been processed", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
		EventsTestHelper.AssertEventNotAdded(entryHeader, Events.EComStatusChange, sinceUtc: processStartTime);
	});
}
