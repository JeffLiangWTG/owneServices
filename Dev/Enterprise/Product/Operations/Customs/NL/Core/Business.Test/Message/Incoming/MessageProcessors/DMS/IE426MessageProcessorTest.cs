using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Moq;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class IE426MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC426A;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Declaration Acceptance</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>CUS</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>Declaration Acceptance</i></td></tr>");
			sb.Append("<tr><td><b>Customs Remark:</b></td><td><i>Declaration Acceptance</i></td></tr>");
			sb.Append("<tr><td><b>Registration date:</b></td><td><i>" + RegistrationDateTimeZoneDependent + "</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	ZDateTime RegistrationDateTimeZoneDependent
	{
		get
		{
			ZDateTime registrationDate;
			ZDateTime.TryParseExact("20200723143432Z", out registrationDate, "yyyyMMddHHmmssZ");
			return registrationDate;
		}
	}

	protected override Mock<IDMSIncomingDataProvider> GetDataProviderMock()
	{
		var dataProviderMock = base.GetDataProviderMock();
		dataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.DeclarationAcceptance);
		var responseDeclarationMock = new Mock<IDMSDeclaration>();
		responseDeclarationMock.Setup(h => h.Id).Returns("22NL13215444");
		dataProviderMock.Setup(x => x.Declaration).Returns(responseDeclarationMock.Object);
		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(statementDescription: "Declaration Acceptance", limitDate: new DateTime(2020, 06, 12)).Object;
		dataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });
		var status = DMSResponseMessageTestHelper.MockResponseStatus(effectiveDateTime: new DateTime(2020, 07, 23, 14, 34, 32)).Object;
		dataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });
		return dataProviderMock;
	}

	protected override DMSResponseMessageProcessor MessageProcessor => GetMessageProcessor<IE426MessageProcessor>();

	protected override void AssertMessage(NLEDIMessage testMessage)
	{
		CombineAssertions(() =>
		{
			base.AssertMessage(testMessage);
			AssertEquals("EntryHeadder - Message Status", "RCV", entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", "200", entryHeader.CH_EntryStatus);
			AssertEquals("Entry Header - Issue date", ZDateTime.Empty, mrnEntryNumber.CE_IssueDate);
			AssertEquals("Entry Header - Mrn", "22NL13215444", mrnEntryNumber.CE_EntryNum);
			AssertEquals("Entry Header - Expiry Date", new ZDateTime(2020, 06, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}
}
