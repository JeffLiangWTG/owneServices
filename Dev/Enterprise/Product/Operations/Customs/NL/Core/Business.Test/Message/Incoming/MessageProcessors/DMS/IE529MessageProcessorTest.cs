using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE529MessageProcessorTest : IE429MessageProcessorTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC529C;

	protected override string DeclarationMessageType => Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.Release);
		var responseDeclarationMock = new Mock<IDMSDeclaration>();
		responseDeclarationMock.Setup(h => h.Id).Returns("TestReferenceABC");
		dataProviderMock.Setup(x => x.Declaration).Returns(responseDeclarationMock.Object);

		dataProviderMock.Setup(x => x.AdditionalInformations).Returns(Array.Empty<IDMSAdditionalInformation>());

		var status = DMSResponseMessageTestHelper.MockResponseStatus(effectiveDateTime: new DateTime(2022, 02, 14, 15, 19, 20), nameCode: statusNameCode, releaseDate: new DateTime(2022, 02, 14)).Object;
		dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });

		var controlResultControlMock = new Mock<IDMSControl>();
		controlResultControlMock.Setup(c => c.TypeCode).Returns("A2");
		controlResultControlMock.Setup(c => c.ControlResultDescription).Returns("Geacht conform te zijn");
		dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControlMock.Object });
		dataProviderMock.Setup(x => x.ControlResults).Returns(Array.Empty<IDMSControlResult>());

		return dataProviderMock;
	}

	string statusNameCode = "4";

	public new void TestProcessMessage()
	{
		TestMessage("4", NLConstants.EntryStatusNew.Released, NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
		TestMessage("109", NLConstants.EntryStatusNew.NoRelease, NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
		TestMessage("115", NLConstants.EntryStatusNew.ProvisionalRelease, NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
	}

	void TestMessage(string nameCode, string expectedEntryStatus, string expectedStatus, string expectedPhaseStatus)
	{
		statusNameCode = nameCode;
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = DeclarationMessageType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = NLConstants.EntryStatus.Accepted;
		entryHeader.CH_BGMReference = BGMReference;
		entryHeader.MovementReferenceNumberSetter("MRN123", new ZDateTime(2022, 02, 10, 15, 12, 29));
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_ExpiryDate = new ZDateTime(2022, 02, 28, 23, 59, 59);

		var testMessage = CreateNewTestMessage();
		testMessage.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		testMessage.EM_LinkedObject = entryHeader;

		Factory.Save();

		MessageProcessor.ProcessMessage(testMessage);
		CombineAssertions($"{expectedEntryStatus} - {expectedStatus}:", () =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, testMessage.EM_Status);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);

			AssertEquals("EntryHeader - Entry Status", expectedEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("EntryHeader - Status", expectedStatus, entryHeader.CH_Status);
			AssertEquals("EntryHeader - Phase Status", expectedPhaseStatus, entryHeader.CH_PhaseStatus);
			AssertEquals("EntryHeader - Release Date", new ZDateTime(2022, 02, 14), entryHeader.CH_EntryReleaseDate);
			AssertEquals("EntryHeader - Expiry Date", ZDateTime.Empty, mrnEntryNumber.CE_ExpiryDate);
		});
	}
}
