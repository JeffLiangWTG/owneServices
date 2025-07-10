using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE509MessageInterpreter))]
sealed class IE509MessageInterpreterTest : MessageInterpreterTestCase<IE509MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			ZDateTime.TryParseExact("20200723143432Z", out var invalidationDateTimeZoneDependent, "yyyyMMddHHmmssZ");

			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Statement</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Non-acceptance information</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description Type:</b></td><td><i>Statement Description</i></td></tr>");
			sb.Append("<tr><td><b>Invalidation Date:</b></td><td><i>" + invalidationDateTimeZoneDependent + "</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(ResponseStatementTypes.Codes.NonAcceptanceInformation, "Statement Description").Object;
		DataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });

		var status = DMSResponseMessageTestHelper.MockResponseStatus(new DateTime(2020, 07, 23, 14, 34, 32)).Object;
		DataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });
	}
}
