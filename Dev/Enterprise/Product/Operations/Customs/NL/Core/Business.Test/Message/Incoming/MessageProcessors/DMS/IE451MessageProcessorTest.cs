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

sealed class IE451MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC451A;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'><tr><td><b>MRN:</b></td><td><i>MRN123</i></td></tr><tr><td><b>Functional Reference ID:</b></td><td><i>TestReferenceABC</i></td></tr><tr><td><b>Statement Type:</b></td><td><i>Unexpected stops information</i></td></tr><tr><td><b>Control Remarks:</b></td><td><i>Niet conform</i></td></tr><tr><td><b>Control Remarks:</b></td><td><i>Statement Description</i></td></tr></table></font>";

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.NoRelease);

		var responseDeclarationMock = new Mock<IDMSDeclaration>();
		responseDeclarationMock.Setup(h => h.Id).Returns("TestReferenceABC");
		dataProviderMock.Setup(x => x.Declaration).Returns(responseDeclarationMock.Object);

		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(statementTypeCode: "ACR", statementDescription: "Statement Description").Object;
		dataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });

		var status = DMSResponseMessageTestHelper.MockResponseStatus(effectiveDateTime: new DateTime(2022, 02, 14, 15, 19, 20)).Object;
		dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });

		var controlResultControlMock = new Mock<IDMSControl>();
		controlResultControlMock.Setup(c => c.TypeCode).Returns("A2");
		controlResultControlMock.Setup(c => c.ControlResultDescription).Returns("Niet conform");
		dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { controlResultControlMock.Object });

		dataProviderMock.Setup(x => x.ControlResults).Returns(Array.Empty<IDMSControlResult>());

		return dataProviderMock;
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE451MessageProcessor>();

	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
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
		CombineAssertions("", () =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, testMessage.EM_Status);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, testMessage.EM_MessageInterpretation);

			AssertEquals("EntryHeader - Entry Status", NLConstants.EntryStatus.NoRelease_NRE, entryHeader.CH_EntryStatus);
			AssertEquals("EntryHeader - Status", NLConstants.Status.NoRelease_NRE, entryHeader.CH_Status);
			AssertEquals("EntryHeader - Release Date", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			AssertEquals("EntryHeader - Expiry Date", ZDateTime.Empty, mrnEntryNumber.CE_ExpiryDate);
		});
	}
}
