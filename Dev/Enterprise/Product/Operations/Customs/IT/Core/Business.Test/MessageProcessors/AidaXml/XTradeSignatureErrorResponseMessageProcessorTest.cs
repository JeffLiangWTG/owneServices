using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeSignatureErrorResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<XTradeSignatureErrorResponseMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "XSE" };

	public void TestProcessMessage_MessageTypeNEW_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		(_, var entryHeader, var originalMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XSE");
		entryHeader.CH_EntryStatus = "XYZ";

		originalMessage.EM_MessageType = "NEW";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("CusEntryHeader.CH_Status", "FFT", entryHeaderOnSeparateFactory.CH_Status.ToString());
	}

	public void TestProcessMessage_MessageTypeCAN_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		(_, var entryHeader, var originalMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XSE");
		entryHeader.CH_EntryStatus = "XYZ";

		originalMessage.EM_MessageType = "CAN";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("CusEntryHeader.CH_Status", "FFT", entryHeaderOnSeparateFactory.CH_Status);
	}

	public void TestProcessMessage_MessageTypeAMD_SetsStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		(_, var entryHeader, var originalMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XSE");
		entryHeader.CH_EntryStatus = "XYZ";

		originalMessage.EM_MessageType = "AMD";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("CusEntryHeader.CH_Status", "FFT", entryHeaderOnSeparateFactory.CH_Status.ToString());
	}

	public void TestProcessMessage_MessageTypeNOTNEW_CAN_AMD_DoesNotSetStatusToFailedForTransmission()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent(ReasonContainsMessageTransmissionFailureKey);
		(_, var entryHeader, var originalMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "XSE");
		entryHeader.CH_EntryStatus = "XYZ";

		originalMessage.EM_MessageType = "ITM";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("CusEntryHeader.CH_Status", "", entryHeaderOnSeparateFactory.CH_Status.ToString());
	}

	protected override XTradeSignatureErrorResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
		=> new XTradeSignatureErrorResponseMessageProcessor(logger);

	const string ReasonContainsMessageTransmissionFailureKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSginatureErrorFile_ReasonContainsMessageTransmissionFailure.xml";

	void AssertNumberOfResponseMessages(Business.Declaration.CusEntryHeader entryHeader, int expectedAcknowledgement)
	{
		var messages = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "XSE");
		AssertEquals("Number of Error Messages", expectedAcknowledgement, messages);
	}
}
