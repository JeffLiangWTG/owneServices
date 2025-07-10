using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.NL.Business.Testing;

abstract class DMSMessageProcessorAbstractTest : TestCaseWithFactory
{
	public void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2021, 10, 02);

		var testMessage = CreateNewTestMessage();
		testMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;
		testMessage.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(testMessage);
		AssertMessage(testMessage);
	}

	protected virtual void AssertMessage(NLEDIMessage testMessage)
	{
		AssertEquals("EDI Message - Message Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, testMessage.EM_Status);
		AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);
	}

	protected CusEntryHeader entryHeader;
	protected CusEntryNumber mrnEntryNumber;

	protected NLEDIMessage CreateNewTestMessage()
	{
		var testMessage = Factory.New<NLEDIMessage>();
		testMessage.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		testMessage.EM_MessageSubType = MessageSubType;
		testMessage.EM_MessageNum = "1";
		testMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;

		return testMessage;
	}

	protected abstract DMSResponseMessageProcessor MessageProcessor { get; }

	protected abstract string ExpectedMessageInterpretation { get; }

	protected abstract string MessageSubType { get; }

	protected virtual string TestMessageText { get; }

	protected virtual string BGMReference => "TestReferenceABC";

	protected virtual Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = DMSResponseMessageTestHelper.MockValidAndEmptyDMSIncomingDataProvider();
		dataProviderMock.Setup(x => x.BOReference).Returns(BGMReference);
		return dataProviderMock;
	}

	// Will replace MessageProcessor when all tests use mock data instead of TestMessageText
	protected T GetMessageProcessor<T>() where T : DMSResponseMessageProcessor
	{
		var processorMock = new Mock<T>(logger) { CallBase = true };
		processorMock
			.Protected()
			.Setup<IIncomingDataProvider>("GetMessageDataProvider", ItExpr.IsAny<EDIMessage>())
			.Returns(GetDataProviderMock().Object);
		return processorMock.Object;
	}

	protected LoggingInformation logger = new LoggingInformation();
}
