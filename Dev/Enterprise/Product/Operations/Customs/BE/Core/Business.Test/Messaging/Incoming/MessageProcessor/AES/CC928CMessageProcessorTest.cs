using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CC928CMessageProcessor))]
sealed class CC928CMessageProcessorTest : MessageProcessorTestCase<CC928CMessageProcessor, ICC928CDataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CC928C }, processor.MessageTypesToInclude);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CC928C;

	protected override Type ExpectedMessageInterpreterType => typeof(CC928CMessageInterpreter);

	protected override CC928CMessageProcessor Processor => processor;

	public void TestProcessMessage()
	{
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		var cidEntryNumber = CusEntryNumber.Load(entry, CusEntryNumberTypes.EU.CorrelationIdentifier, Core.Constants.CountryCodes.Belgium);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message status", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CorrelationID", "CIDNumberTest", cidEntryNumber.CE_EntryNum);
			AssertEquals("CorrelationID in CE_EntryLineReference", "CIDNumberTest", cidEntryNumber.CE_EntryLineReference);
			AssertEquals("Customs Message status", StatusCodes.DeclarationAcknowledged, entry.CH_Status);
		});
	}

	public void TestProcessedMessageEntryStatusIsNotD()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entry.CH_CEI_Instruction = entryInstruction.PK;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals("Entry status should be ACK for SubStyle not equal to \"D\"", EDIMessageStatusList.Codes.Acknowledged, entry.CH_EntryStatus);
	}

	public void TestProcessedMessageEntryStatusIsD()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		entry.CH_CEI_Instruction = entryInstruction.PK;
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryStandardDeclarationUnderCodeA;
		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		AssertEquals("Entry status should be PRE for SubStyle equal to \"D\"", StatusCodes.PreLodged, entry.CH_EntryStatus);
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<ICC928CDataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.CorrelationId).Returns("CIDNumberTest");
		var mockProcessor = new Mock<CC928CMessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045991480600000001";

		var sessionGUID = ZGuid.BrettsGuid;
		var ediMessageBadStatus = Factory.New<BEMessage>();
		ediMessageBadStatus.EM_Status = "FAL";
		var ediMessageOld = Factory.New<BEMessage>();
		ediMessageOld.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

		var interchangeOutgoing = Factory.New<BECInterchange>();
		interchangeOutgoing.EI_SessionGUID = sessionGUID;
		interchangeOutgoing.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		interchangeOutgoing.EI_Status = LogicalStatusList.Codes.Sent;
		interchangeOutgoing.EI_From = "DEJOS";
		interchangeOutgoing.EI_To = "DEFRANS";
		var interchangeIncoming = Factory.New<BECInterchange>();
		interchangeIncoming.EI_SessionGUID = sessionGUID;
		interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		interchangeIncoming.EI_From = "DEFRANS";
		interchangeIncoming.EI_To = "DEJOS";

		incomingMessage = CreateIncomingMessage(Factory);
		incomingMessage.EM_LinkTable = entry.TableName;
		incomingMessage.EM_LinkUniqueID = entry.PK;
		incomingMessage.EM_EI = interchangeIncoming.PK;

		Factory.Save();
	}

	Mock<ICC928CDataProvider> mockProvider;
	EDIMessage incomingMessage;
	CC928CMessageProcessor processor;
	CusEntryHeader entry;
}
