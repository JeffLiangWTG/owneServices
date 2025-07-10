using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

class IE431MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC431A;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Reminder</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>Declaration awaiting a supplement</i></td></tr>");
			sb.Append("<tr><td><b>Customs Remark:</b></td><td><i>If no supplement is sent before Expiry, the declaration may be amended by Customs and (if applicable) preference will be removed</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}
	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.SupplementReminder);
		var control = DMSResponseMessageTestHelper.MockResponseControl(limitDate: new DateTime(2021, 10, 15)).Object;
		dataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });
		return dataProviderMock;
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE431And531MessageProcessor>();

	protected override void AssertMessage(NLEDIMessage testMessage)
	{
		CombineAssertions(() =>
		{
			base.AssertMessage(testMessage);
			AssertEquals("Entry Header - Message Status", "REM", entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", "510", entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - CusEntryNumber - Expiry Date", new ZDateTime(2021, 10, 15), mrnEntryNumber.CE_ExpiryDate);
		});
	}
}
