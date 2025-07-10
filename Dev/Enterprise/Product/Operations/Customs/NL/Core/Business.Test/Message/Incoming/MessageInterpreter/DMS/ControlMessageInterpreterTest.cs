using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ControlMessageInterpreter))]
sealed class ControlMessageInterpreterTest : MessageInterpreterTestCase<ControlMessageInterpreter, IControlIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{ 
		get
		{ 
			var sb = new StringBuilder();
			sb.Append("<h1>Error Information</h1><br/>");
			sb.Append("<b>Functional Reference ID:</b> LRN123<br/><br/>");
			sb.Append("<table><tr><th>Error Text</th><th>Original Value</th><th>Position</th></tr>");
			sb.Append("<tr><td>Error message: Element 'CommunicationsAgreementID' is not valid for content model: '(ApplicationReferenceId, CommunicatonsAgreementId?,PreparationDateTime,Recipient,Sender)'</td><td></td><td>Seg 13 (LOC) 27</td></tr>");
			sb.Append("<tr><td>Error message: Datatype error: Type:InvalidDatatypeValueException, Message:Value '1118 AB 1' does not match regular expression facet '[A-Z]{2}' .</td><td>1118 AB 1</td><td>Seg 56 (LOC) 49</td></tr></table>");
			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		var documentMetaData = DMSResponseMessageTestHelper.MockDocumentMetaData("3.50", "NL", "DOUANE", "v1.1", "DMS", new DateTime(2025, 04, 24, 10, 53, 5));
		var response = DMSResponseMessageTestHelper.MockResponse("LRN123");
		var error1 = DMSResponseMessageTestHelper.MockError("Error message: Element 'CommunicationsAgreementID' is not valid for content model: '(ApplicationReferenceId, CommunicatonsAgreementId?,PreparationDateTime,Recipient,Sender)'", "Line-number: 13 ### Column-number: 27").Object;
		var error2 = DMSResponseMessageTestHelper.MockError("Error message: Datatype error: Type:InvalidDatatypeValueException, Message:Value '1118 AB 1' does not match regular expression facet '[A-Z]{2}' .", "Line-number: 56 ### Column-number: 49").Object;
		response.Setup(x => x.Errors).Returns(new IControlError[] { error1, error2 });
		DataProviderMock.Setup(x => x.DocumentMetaData).Returns(documentMetaData.Object);
		DataProviderMock.Setup(x => x.Response).Returns(response.Object);
	}
}
