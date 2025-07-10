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

[TestedType(typeof(IE928MessageProcessor))]
sealed class IE928MessageProcessorTest : MessageProcessorTestCase<IE928MessageProcessor, IIE928DataProvider>
{
	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.IE928 }, processor.MessageTypesToInclude);
	}

	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.IE928;

	protected override Type ExpectedMessageInterpreterType => typeof(IE928MessageInterpreter);

	protected override IE928MessageProcessor Processor => processor;

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
			AssertEquals("Customs Status should be ACC", StatusCodes.DeclarationAcknowledged, entry.CH_EntryStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<IIE928DataProvider>();
		mockProvider.CallBase = true;
		mockProvider.Setup(m => m.CorrelationId).Returns("CIDNumberTest");
		var mockProcessor = new Mock<IE928MessageProcessor>(new BatchProcessor.LoggingInformation());
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

	Mock<IIE928DataProvider> mockProvider;
	EDIMessage incomingMessage;
	IE928MessageProcessor processor;
	CusEntryHeader entry;
}
