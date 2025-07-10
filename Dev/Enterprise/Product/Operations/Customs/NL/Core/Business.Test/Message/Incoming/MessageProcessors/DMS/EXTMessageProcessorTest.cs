using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Moq;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class EXTMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCEXTA;

	protected override string ExpectedMessageInterpretation => "<font size='2' face='Courier New' ><table style='margin-left: 10pt'>" +
			"<tr><td><b>Event Type:</b></td><td><i>External Message</i></td></tr>" +
			"<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>" +
			"<tr><td><b>Statement Description Type:</b></td><td><i>External Message</i></td></tr>" +
			"<tr><td><b>Customs Remark:</b></td><td><i>External Message</i></td></tr>" +
			$"<tr><td><b>Registration date:</b></td><td><i>{DMSResponseMessageHelper.GetFormattedLocalLongTimeString(effectiveDateTime)}</i></td></tr>" +
			"</table></font>";

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		var status = DMSResponseMessageTestHelper.MockResponseStatus(effectiveDateTime).Object;
		dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });
		return dataProviderMock;
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<EXTMessageProcessor>();

	static readonly DateTime effectiveDateTime = new DateTime(2024, 07, 31, 16, 53, 11);
}
