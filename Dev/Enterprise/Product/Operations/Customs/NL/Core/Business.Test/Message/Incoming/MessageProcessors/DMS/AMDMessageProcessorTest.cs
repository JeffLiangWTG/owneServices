using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AMDMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2025, 04, 11);

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", "ACC", entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", "AMD", entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", "515", entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCAMDA;

	protected override string ExpectedMessageInterpretation => ZString.Empty;

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<AMDMessageProcessor>();

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.IncomingAmendment);
		var control = DMSResponseMessageTestHelper.MockResponseControl(limitDate: new DateTime(2021, 10, 15)).Object;
		return dataProviderMock;
	}
}
