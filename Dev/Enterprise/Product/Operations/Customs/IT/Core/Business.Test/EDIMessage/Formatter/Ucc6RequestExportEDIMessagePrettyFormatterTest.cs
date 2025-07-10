using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6RequestExportEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettyMessage_WithValidEncodedMessage_EXP_NEW() => AssertPrettyFormatedMessageText(OUTPUT_DATA_REQUEST_MESSAGE_NEW_EXP, SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_NEW_EXP);

	public void TestPrettyMessage_WithValidEncodedMessage_EXP_CAN() => AssertPrettyFormatedMessageText(OUTPUT_DATA_REQUEST_MESSAGE_CAN_EXP, SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_CAN_EXP);

	public void TestPrettyMessage_WithNoDataElement_EXP_NEW() => AssertPrettyFormatedMessageText(SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_EXP, SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_EXP);

	public void TestPrettyMessage_WithNoDataElement_EXP_CAN() => AssertPrettyFormatedMessageText(SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_EXP, SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_EXP);

	public void TestPrettyMessage_WithAnEmptyDataElement_EXP_NEW() => AssertPrettyFormatedMessageText(SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_EXP, SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_EXP);

	public void TestPrettyMessage_WithAnEmptyDataElement_EXP_CAN() => AssertPrettyFormatedMessageText(SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_EXP, SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_EXP);

	#region Implementation

	void AssertPrettyFormatedMessageText(string expectedMessage, string toBeFormatedMessageWithEnvolope)
	{
		var message = Factory.New<ITEDIMessage>();
		message.EM_ApplicationReference = "EXP";
		message.EM_MessageType = EDIMessageTypeList.Codes.NewDeclaration;

		message.EM_MessageText = toBeFormatedMessageWithEnvolope;
		var parsedFormattedMessage = XDocument.Parse(message.EM_MessageInterpretation).ToString();

		var parsedExpectedMessage = XDocument.Parse(expectedMessage).ToString();

		AssertEquals("Prettyfied Text", parsedExpectedMessage, parsedFormattedMessage);
	}

	const string SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_NEW_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>invioDichiarazione</exp:serviceId>
			<exp:data>
				<exp:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPFNvbWVUYWc+U2FtcGxlIHhtbCB0ZXh0IHRvIGJlIGRlY29kZWQgZm9yIE5FVywgYXMgRVhQIG5vdCB5ZXQgcmVhZHk8L1NvbWVUYWc+</exp:xml>
				<exp:dichiarante>13149600150</exp:dichiarante>
			</exp:data>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_BASE64_VALID_ENCODED_DATA_REQUEST_MESSAGE_CAN_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>annullaDichiarazione</exp:serviceId>
			<exp:data>
				<exp:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4KPFNvbWVUYWc+U2FtcGxlIHhtbCB0ZXh0IHRvIGJlIGRlY29kZWQgZm9yIENBTiwgYXMgRVhQIG5vdCB5ZXQgcmVhZHk8L1NvbWVUYWc+</exp:xml>
				<exp:dichiarante>13149600150</exp:dichiarante>
			</exp:data>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string OUTPUT_DATA_REQUEST_MESSAGE_NEW_EXP = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SomeTag>Sample xml text to be decoded for NEW, as EXP not yet ready</SomeTag>";

	const string OUTPUT_DATA_REQUEST_MESSAGE_CAN_EXP = @"<?xml version=""1.0"" encoding=""utf-8""?>
<SomeTag>Sample xml text to be decoded for CAN, as EXP not yet ready</SomeTag>";

	const string SOAP_WITH_NO_DATA_REQUEST_MESSAGE_NEW_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>invioDichiarazione</exp:serviceId>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_NO_DATA_REQUEST_MESSAGE_CAN_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>annullaDichiarazione</exp:serviceId>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_NEW_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>invioDichiarazione</exp:serviceId>
			<exp:data>
			</exp:data>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	const string SOAP_WITH_EMPTY_DATA_REQUEST_MESSAGE_CAN_EXP = @"<soapenv:Envelope
	xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""
	xmlns:exp=""http://exportservice.domest.sogei.it"">
	<soapenv:Header/>
	<soapenv:Body>
		<exp:Input>
			<exp:serviceId>annullaDichiarazione</exp:serviceId>
			<exp:data>
			</exp:data>
		</exp:Input>
	</soapenv:Body>
</soapenv:Envelope>";

	#endregion
}
