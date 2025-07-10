using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

class IE428MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = DeclarationMessageType;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", ExpectedEntryHeaderStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", ExpectedEntryHeaderEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Issue date", new ZDateTime(2022, 01, 12), mrnEntryNumber.CE_IssueDate);
			AssertEquals("Entry Header - Mrn", "22NL13215444", mrnEntryNumber.CE_EntryNum);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
		});
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC428A;

	protected override string BGMReference => "TestReference428";

	protected virtual string DeclarationMessageType => Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

	protected virtual string WCOTypeCode => WCoTypeCodes.Acceptance;

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE428And528MessageProcessor>();

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCOTypeCode);

		var responseDeclarationMock = new Mock<IDMSDeclaration>();
		responseDeclarationMock.Setup(h => h.FunctionalReference).Returns("TestReference428");
		responseDeclarationMock.Setup(h => h.Id).Returns("22NL13215444");
		responseDeclarationMock.Setup(h => h.AcceptanceDate).Returns(new DateTime(2022, 01, 12));
		dataProviderMock.Setup(x => x.Declaration).Returns(responseDeclarationMock.Object);

		return dataProviderMock;
	}

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'><tr><td><b>Event Type:</b></td><td><i>Acceptance</i></td></tr><tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr><tr><td><b>Statement Description Type:</b></td><td><i>Acceptance</i></td></tr><tr><td><b>Customs Remark:</b></td><td><i>Acceptance</i></td></tr></table></font>";

	protected virtual string ExpectedEntryHeaderStatus => Status.MRN;

	protected virtual string ExpectedEntryHeaderEntryStatus => EntryStatus.Accepted;
}
