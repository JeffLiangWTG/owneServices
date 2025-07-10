using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE504MessageProcessorTest : IE404MessageProcessorTest
{
	public void TestProcessMessage_Phase513()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReferenceABC";
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		entryHeader.CH_EntryStatus = EntryStatusNew.Received;
		entryHeader.CH_Status = StatusNew.Accepted;
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2021, 10, 02);

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", StatusNew.Accepted, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", EntryStatusNew.Received, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", CustomsEntryPhaseStatusList.Codes._515, entryHeader.CH_PhaseStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		var declaration = DMSResponseMessageTestHelper.MockResponseDeclaration(new DateTime(2024, 09, 27, 10, 00, 11)).Object;
		dataProviderMock.Setup(x => x.Declaration).Returns(declaration);
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.ExportAmendmentAccepted);
		return dataProviderMock;
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC504C;
}
