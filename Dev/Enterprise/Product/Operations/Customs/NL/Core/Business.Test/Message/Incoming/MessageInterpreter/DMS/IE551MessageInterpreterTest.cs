using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE551MessageInterpreter))]
sealed class IE551MessageInterpreterTest : MessageInterpreterTestCase<IE551MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			ZDateTime.TryParseExact("20240422091010Z", out var cancellationDateTimeZoneDependent, "yyyyMMddHHmmssZ");

			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Customs Decision</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Canceled by Customs</i></td></tr>");
			sb.Append("<tr><td><b>Cancellation Date:</b></td><td><i>" + cancellationDateTimeZoneDependent + "</i></td></tr>");
			sb.Append("<tr><td><b>Remark:</b></td><td><i>Canceled due to not following up outstanding requests</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}
}
