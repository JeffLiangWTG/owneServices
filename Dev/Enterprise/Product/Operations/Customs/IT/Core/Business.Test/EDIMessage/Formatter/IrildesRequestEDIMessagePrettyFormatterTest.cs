using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IrildesRequestEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestFormatMessage()
	{
		const string inputRawData = @"<soapenv:Envelope xmlns:tran=""http://transitoservice.domest.sogei.it"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tran:Input>
      <tran:serviceId>richiestaIrildesNonFirmata</tran:serviceId>
      <tran:data>
        <tran:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxtcm4geG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOm5vTmFtZXNwYWNlU2NoZW1hTG9jYXRpb249IklucHV0LnhzZCI+MjRJVFEwQjhURUsxNzk1MUo0PC9tcm4+</tran:xml>
        <tran:dichiarante>13149600150</tran:dichiarante>
      </tran:data>
    </tran:Input>
  </soapenv:Body>
</soapenv:Envelope>";

		const string expectedPrettyPrint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<mrn xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""Input.xsd"">24ITQ0B8TEK17951J4</mrn>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", expectedPrettyPrint, message.EM_MessageInterpretation);
	}

	public void TestFormatMalformedMessage()
	{
		const string inputRawData = @"<soapenv:Envelope xmlns:tran=""http://transitoservice.domest.sogei.it"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tran:Input>
      <tran:serviceId>richiestaIrildesNonFirmata</tran:serviceId>
    </tran:Input>
  </soapenv:Body>
</soapenv:Envelope>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", inputRawData, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.New<ITEDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.IrildesRequest;
	}

	ITEDIMessage message;
}
