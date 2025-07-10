using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE438MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC438A;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Reminder</i></td></tr>");
			sb.Append("<tr><td><b>Expiry Date:</b></td><td><i>20200612</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>Information request</i></td></tr>");
			sb.Append("<tr><td><b>Customs Remark:</b></td><td><i>Outstanding request for information (RFI)</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.ReminderForInformation);
		var responseDeclarationMock = new Mock<IDMSDeclaration>();
		responseDeclarationMock.Setup(h => h.ExpirationDate).Returns(new DateTime(2020, 06, 12));
		dataProviderMock.Setup(x => x.Declaration).Returns(responseDeclarationMock.Object);
		return dataProviderMock;
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE438MessageProcessor>();
}
