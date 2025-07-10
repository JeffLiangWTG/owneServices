using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE410MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC410A;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'><tr><td><b>MRN:</b></td><td><i>MRN-Number</i></td></tr><tr><td><b>Functional Reference ID:</b></td><td><i>TestReferenceABC</i></td></tr><tr><td><b>Control Remarks:</b></td><td><i>TestStatementDescriptionValue</i></td></tr><tr><td><b></b></td><td><i>&nbsp;</i></td></tr></table></font>";

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.InvalidationConfirmation);
		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(statementDescription: "TestStatementDescriptionValue").Object;
		dataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });
		return dataProviderMock;
	}

	protected override void AssertMessage(NLEDIMessage testMessage)
	{
		CombineAssertions(() =>
		{
			base.AssertMessage(testMessage);
			AssertEquals("Entry Header - Message Status", "CAN", entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", "630", entryHeader.CH_EntryStatus);
		});
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE410MessageProcessor>();
}
