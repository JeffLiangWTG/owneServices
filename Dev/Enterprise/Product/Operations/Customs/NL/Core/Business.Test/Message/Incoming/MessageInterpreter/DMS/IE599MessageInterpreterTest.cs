using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE599MessageInterpreter))]
sealed class IE599MessageInterpreterTest : MessageInterpreterTestCase<IE599MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Event Type:</b></td><td><i>Exit information</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Exit confirmed</i></td></tr>");
			sb.Append("<tr><td><b>Exit Date:</b></td><td><i>20240820</i></td></tr>");
			sb.Append("<tr><td><b>Control Results:</b></td><td><i>sb666</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var control = DMSResponseMessageTestHelper.MockResponseControl(controlResultID: "sb666", controlResultExitTime: new DateTime(2024, 08, 20, 14, 34, 32)).Object;
		DataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });
	}
}
