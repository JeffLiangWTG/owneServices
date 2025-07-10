using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE456And556MessageInterpreter))]
sealed class IE456And556MessageInterpreterTest : MessageInterpreterTestCase<IE456And556MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><H1>Error Information</H1><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>22NL13215444</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReference456</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Status details</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Additional Info Statement Description</i></td></tr>");
			sb.Append("</table><BR><H1>Errors reported by customs</H1><BR/>");
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>Validation Code</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>Description</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>Original Attribute Value</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location1</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location2</i></td></tr></table><BR/>");
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>Validation Code2</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>Description2</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>Original Attribute Value2</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location3</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location4</i></td></tr></table>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		DataProviderMock.Setup(x => x.Declaration.Id).Returns("22NL13215444");
		DataProviderMock.Setup(x => x.BOReference).Returns("TestReference456");

		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(ResponseStatementTypes.Codes.StatusDetails, "Statement Description").Object;
		DataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });

		var control = DMSResponseMessageTestHelper.MockResponseControl(additionalInfoStatementDescription: "Additional Info Statement Description").Object;
		DataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control });

		var error = DMSResponseMessageTestHelper.MockResponseError("Validation Code", "Description", "Original Attribute Value", new string[] { "Location1", "Location2" }).Object;
		var error2 = DMSResponseMessageTestHelper.MockResponseError("Validation Code2", "Description2", "Original Attribute Value2", new string[] { "Location3", "Location4" }).Object;
		DataProviderMock.Setup(x => x.Errors).Returns(new IDMSError[] { error, error2 });
	}
}
