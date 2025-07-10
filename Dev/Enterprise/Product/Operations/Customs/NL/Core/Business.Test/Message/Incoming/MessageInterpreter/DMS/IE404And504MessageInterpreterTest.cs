using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE404And504MessageInterpreter))]
sealed class IE404And504MessageInterpreterTest : MessageInterpreterTestCase<IE404And504MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Amendment date::</b></td><td><i>20220112</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		DataProviderMock.Setup(x => x.Declaration.IssueDateTime).Returns(new DateTime(2022, 01, 12, 14, 34, 32));
	}
}
