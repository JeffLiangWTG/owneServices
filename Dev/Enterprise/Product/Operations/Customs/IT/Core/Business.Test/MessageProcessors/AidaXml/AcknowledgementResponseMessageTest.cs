using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AcknowledgementResponseMessageTest : TestCase
{
	public void TestResponseBodyStatus()
	{
		var status = responseMessage.ResponseStatus;
		AssertEquals("Status", "20", status);
	}

	public void TestGetResponseBody()
	{
		var responseBody = responseMessage.ResponseBody;
		AssertNotNull("ResponseBody", responseBody);
		AssertSame("ResponseBody not processed twice", responseBody, responseMessage.ResponseBody);
	}

	protected override void SetUp()
	{
		base.SetUp();
		responseMessage = new AcknowledgementResponseMessage(soapMessage);
	}

	AcknowledgementResponseMessage responseMessage;

	const string soapMessage = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://importservice.domest.sogei.it"">
		 <ns2:IUT>20220307D11000328189</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>20</ns2:codice>
			<ns2:messaggio>Acquisito a sistema</ns2:messaggio>
		 </ns2:esito>
		 <ns2:dataRegistrazione>2022-03-07+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";
}
