using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IvistoRequestEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestFormatMessage()
	{
		const string inputRawData = @"<soapenv:Envelope xmlns:type=""http://exportservice.domest.sogei.it"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>richiestaIvistoNonFirmata</type:serviceId>
      <type:data>
        <type:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxtcm4geG1sbnM6eHNpPSJodHRwOi8vd3d3LnczLm9yZy8yMDAxL1hNTFNjaGVtYS1pbnN0YW5jZSIgeHNpOm5vTmFtZXNwYWNlU2NoZW1hTG9jYXRpb249IklucHV0LnhzZCI+MjNJVFEwMTFUMDAwMDA2M0U2PC9tcm4+</type:xml>
        <type:dichiarante>123454555</type:dichiarante>
      </type:data>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

		const string expectedPrettyPrint = @"<?xml version=""1.0"" encoding=""utf-8""?>
<mrn xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:noNamespaceSchemaLocation=""Input.xsd"">23ITQ011T0000063E6</mrn>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", expectedPrettyPrint, message.EM_MessageInterpretation);
	}

	public void TestFormatMalformedMessage()
	{
		const string inputRawData = @"<soapenv:Envelope xmlns:type=""http://exportservice.domest.sogei.it"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <type:Input>
      <type:serviceId>richiestaIvistoNonFirmata</type:serviceId>
    </type:Input>
  </soapenv:Body>
</soapenv:Envelope>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", inputRawData, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.New<ITEDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.IvistoRequest;
	}

	ITEDIMessage message;
}
