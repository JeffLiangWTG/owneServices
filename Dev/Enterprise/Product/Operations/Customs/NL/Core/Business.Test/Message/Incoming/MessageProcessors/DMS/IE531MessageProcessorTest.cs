using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE531MessageProcessorTest : IE431MessageProcessorTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC531C;

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE531MessageProcessor>();

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC531C</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>00000002</ID></Sender></CommunicationMetaData><Response><Control><LimitDateTime>20211015</LimitDateTime><AdditionalInformation><StatementDescription>StatementDescription</StatementDescription></AdditionalInformation></Control></Response></MetaData>";

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.ExportSupplementReminder);
		var control = DMSResponseMessageTestHelper.MockResponseControl(limitDate: new DateTime(2021, 10, 15)).Object;
		dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });
		return dataProviderMock;
	}

	public void TestProcessMessageForSubStyleB()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.B, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessageForSubStyleC()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.C, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessageForSubStyleE()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.E, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessageForSubStyleF()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.F, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessageForSubStyleX()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.X, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessageForSubStyleY()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.Y, string.Empty, StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515);
	}

	public void TestProcessMessage_PhaseStatus513()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.E, CustomsEntryPhaseStatusList.Codes._513, string.Empty, string.Empty, CustomsEntryPhaseStatusList.Codes._513);
	}

	public void TestProcessMessage_PhaseStatusSUP()
	{
		TestProcessMessageForSubStyle(DeclarationSubTypeList.Codes.E, CustomsEntryPhaseStatusList.Codes.SUP, string.Empty, string.Empty, CustomsEntryPhaseStatusList.Codes.SUP);
	}

	void TestProcessMessageForSubStyle(string subStyle, string phaseStatus, string expectedMessageStatus, string expectedEntryStatus, string expectedPhaseStatus)
	{
		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.CH_PhaseStatus = phaseStatus;
		var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		entryInstruction.CEI_SubStyle = subStyle;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN-Number";
		mrnEntryNumber.CE_IssueDate = new ZDateTime(2021, 10, 02);

		var testMessage = CreateNewTestMessage();
		testMessage.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;
		testMessage.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(testMessage);

		CombineAssertions(() =>
		{
			AssertEquals("Entry Header - Message Status", expectedMessageStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Phase Status", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
		});
	}
}
