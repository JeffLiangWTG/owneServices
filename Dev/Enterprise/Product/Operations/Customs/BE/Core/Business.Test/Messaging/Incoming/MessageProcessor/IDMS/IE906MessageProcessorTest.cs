using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(IE906MessageProcessor))]
sealed class IE906MessageProcessorTest : MessageProcessorTestCase<IE906MessageProcessor, IIE906DataProvider>
{
	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.IE906;

	protected override Type ExpectedMessageInterpreterType => typeof(IE906MessageInterpreter);

	protected override IE906MessageProcessor Processor => processor;

	public void TestMessagesTypeToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.IE906 }, processor.MessageTypesToInclude);
	}

	public void TestProcessIE906MessageDeclaration_ByMRN()
	{
		entry.CH_EntryStatus = StatusCodes.RejectedAmendment;
		var mrn = CusEntryNumber.LoadOrCreate(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, entry.CountryCode);
		mrn.CE_EntryNum = "22045281480600000002";
		mockProvider.Setup(x => x.LRN).Returns("FAKE");
		mockProvider.Setup(x => x.MRN).Returns("22045281480600000002");
		mockProvider.Setup(x => x.FunctionalErrors).Returns(new Collection<IDMSFunctionalError>());
		Factory.Save();

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EDI status should be PRS", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status should not be updated when the former value is not empty", StatusCodes.RejectedAmendment, entry.CH_EntryStatus);
		});
	}

	public void TestProcessIE906MessageDeclaration_ByLRN()
	{
		mockProvider.Setup(x => x.LRN).Returns("22045281480600000001");
		mockProvider.Setup(x => x.MRN).Returns("FAKE");
		mockProvider.Setup(x => x.FunctionalErrors).Returns(new Collection<IDMSFunctionalError>());

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EDI status should be PRS", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status should be REJ", StatusCodes.Rejected, entry.CH_EntryStatus);
		});
	}

	public void TestProcessIE906MessageDeclaration_BySessionID()
	{
		mockProvider.Setup(x => x.FunctionalErrors).Returns(new Collection<IDMSFunctionalError>());
		MessageProcessorTestHelper.SetupEDIInterchangeWithLink(entry, incomingMessage);

		processor.PreProcessMessage(incomingMessage);
		processor.ProcessMessage(incomingMessage);

		CombineAssertions(() =>
		{
			AssertEquals("EDI status should be PRS", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			AssertEquals("CusEntryHeader status should be REJ", StatusCodes.Rejected, entry.CH_EntryStatus);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		mockProvider = new Mock<IIE906DataProvider>();
		mockProvider.CallBase = true;
		var mockProcessor = new Mock<IE906MessageProcessor>(new BatchProcessor.LoggingInformation());
		mockProcessor.CallBase = true;
		mockProcessor.Setup(m => m.GetMessageDataProvider(It.IsAny<BEMessage>())).Returns(mockProvider.Object);
		processor = mockProcessor.Object;
		incomingMessage = CreateIncomingMessage(Factory);
		entry = Factory.NewWithValidTestData<CusEntryHeader>();
		entry.CH_BGMReference = "22045281480600000001";
		Factory.Save();
	}
	Mock<IIE906DataProvider> mockProvider;
	EDIMessage incomingMessage;
	IE906MessageProcessor processor;
	CusEntryHeader entry;
}
