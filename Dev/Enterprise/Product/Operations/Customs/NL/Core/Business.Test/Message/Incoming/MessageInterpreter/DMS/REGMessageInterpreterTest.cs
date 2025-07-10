using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(REGMessageInterpreter))]
sealed class REGMessageInterpreterTest : MessageInterpreterTestCase<REGMessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			ZDateTime.TryParseExact("20200723143432Z", out var registrationDateTimeZoneDependent, "yyyyMMddHHmmssZ");

			var sb = new StringBuilder();
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Registration date:</b></td><td><i>" + registrationDateTimeZoneDependent + "</i></td></tr></table></font>");
			sb.Append("<BR><H1>Errors reported by customs</H1><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>Validation Code</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>Description</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>Original Attribute Value</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location1</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location2</i></td></tr>");
			sb.Append("</table></font><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>Validation Code2</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>Description2</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>Original Attribute Value2</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location3</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location4</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var status = DMSResponseMessageTestHelper.MockResponseStatus(new DateTime(2020, 07, 23, 14, 34, 32)).Object;
		DataProviderMock.Setup(x => x.Statuses).Returns(new IDMSStatus[] { status });

		var error = DMSResponseMessageTestHelper.MockResponseError("Validation Code", "Description", "Original Attribute Value", new string[] { "Location1", "Location2" }).Object;
		var error2 = DMSResponseMessageTestHelper.MockResponseError("Validation Code2", "Description2", "Original Attribute Value2", new string[] { "Location3", "Location4" }).Object;
		DataProviderMock.Setup(x => x.Errors).Returns(new IDMSError[] { error, error2 });
	}
}
