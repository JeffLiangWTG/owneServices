namespace Enterprise.Customs.IT.Business.Testing;

static class AcknowledgementResponseMessageProcessorTestHelper
{
	internal static string BuildAcknowledgement(string code, string message)
	{
		return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://importservice.domest.sogei.it"">
		 <ns2:IUT>20220307D11000328189</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>{code}</ns2:codice>
			<ns2:messaggio>{message}</ns2:messaggio>
		 </ns2:esito>
		 <ns2:dataRegistrazione>2022-03-07+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";
	}
}
