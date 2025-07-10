using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

class IE404MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC404A;

	protected override string ExpectedMessageInterpretation => $"<font size='2' face='Courier New' ><table style='margin-left: 10pt'><tr><td><b>Amendment date::</b></td><td><i>{DMSResponseMessageHelper.GetFormattedShortDateString(new DateTime(2024, 09, 27, 10, 00, 11, DateTimeKind.Utc).ToLocalTime())}</i></td></tr></table></font>";

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		var declaration = DMSResponseMessageTestHelper.MockResponseDeclaration(issueDateTime).Object;
		dataProviderMock.Setup(x => x.Declaration).Returns(declaration);
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.AmendmentAccepted);
		return dataProviderMock;
	}

	static readonly DateTime issueDateTime = new DateTime(2024, 09, 27, 10, 00, 11);

	protected override string BGMReference => "TestReference404";

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE404And504MessageProcessor>();

	protected override void AssertMessage(NLEDIMessage testMessage)
	{
		CombineAssertions(() =>
		{
			base.AssertMessage(testMessage);
			AssertEquals("EntryHeadder - Message Status", "AMD", entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", "730", entryHeader.CH_EntryStatus);
		});
	}
}
