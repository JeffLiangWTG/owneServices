using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IrildesInboundInterchangeProcessingStrategyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestProcessIrildesInterchange_WithResponseNotAvailable_WithError198()
	{
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_BodyText = IrildesNotAvailableCode198Response;

		ProcessInterchange(irildesResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IRR, Code=198, Irildes Not Available", irildesResponseInterchange, expectedCount: 0);
	}

	public void TestProcessIrildesInterchange_WithResponseNotAvailable_WithError197()
	{
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_BodyText = IrildesNotAvailableCode197Response;

		ProcessInterchange(irildesResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=197, Irildes Not Available", irildesResponseInterchange, expectedCount: 0);
	}

	public void TestProcessIrildesInterchange_WithResponseAvailable_WithError198()
	{
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_BodyText = IrildesAvailableCode198Response;

		ProcessInterchange(irildesResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IRR, Code=198, Irildes Available", irildesResponseInterchange, expectedCount: 1);
	}

	public void TestProcessIrildesInterchange_WithEmptyResponse_WithError198()
	{
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_BodyText = Code198EmptyDataResponse;

		ProcessInterchange(irildesResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IRR, Code=198, Empty data", irildesResponseInterchange, expectedCount: 1);
	}

	public void TestProcessIrildesInterchange_WithServiceNotAvailableResponse()
	{
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(irildesResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IRR, Code=0, Service not available", irildesResponseInterchange, expectedCount: 0);
	}

	[TestDate]
	public void TestProcessIrildesInterchange_WithResponseNotAvailable_CreateNewCusPollingTransaction()
	{
		irildesResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(irildesResponseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(irildesRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");
	}

	[TestDate]
	public void TestProcessIrildesInterchange_WithResponseNotAvailable_UpdateCusPollingTransaction()
	{
		helper.CreateIrildesPollingTransaction("PND", ZDateTime.Now, irildesRequestInterchange, ZDateTime.Now);

		irildesResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(irildesResponseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(irildesRequestInterchange, "OPN", ZDateTime.Now.AddDays(1), "IRI");
	}

	[TestDate]
	public void TestProcessIrildesInterchange_WithResponseAvailable_DeleteCusPollingTransaction()
	{
		helper.CreateIrildesPollingTransaction("PND", ZDateTime.Now, irildesRequestInterchange, ZDateTime.Now);
		helper.CreateIrildesPollingTransaction("OPN", ZDateTime.Now, irildesRequestInterchange, ZDateTime.Now);

		irildesResponseInterchange.EI_BodyText = IrildesAvailableCode198Response;

		ProcessInterchange(irildesResponseInterchange);

		CusPollingTransactionTestHelper.AssertNoPollingTransactionsForInterchange(irildesRequestInterchange);
	}

	public void TestProcessIrildesInterchange_ThrowsCouldNotFindRelatedBusinessObjectException()
	{
		irildesResponseInterchange.EI_SessionGUID = ZGuid.NewZGuid();

		AssertExceptionThrown<CouldNotFindRelatedBusinessObjectException>(() => ProcessInterchange(irildesResponseInterchange));

		AssertContainedMessagesCount("When CouldNotFindRelatedBusinessObjectException is thrown", irildesResponseInterchange, expectedCount: 0);
	}

	protected override void SetUp()
	{
		base.SetUp();
		helper = new CusPollingTransactionTestDataHelper(Factory);

		irildesRequestInterchange = helper.CreateIrildesRequestInterchange();

		irildesResponseInterchange = Factory.New<EDIInterchange>();
		irildesResponseInterchange.EI_ApplicationCode = "ITH";
		irildesResponseInterchange.EI_InterchangeType = "IRR";
		irildesResponseInterchange.EI_SessionGUID = irildesRequestInterchange.EI_SessionGUID;
	}

	CusPollingTransactionTestDataHelper helper;
	EDIInterchange irildesRequestInterchange;
	EDIInterchange irildesResponseInterchange;

	const string IrildesNotAvailableCode198Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
		 <ns2:IUT>20240329D17032938522</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>198</ns2:codice>
			<ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
		 </ns2:esito>
		 <ns2:data>RWxhYm9yYXppb25lIEtPIGNvbiBlc2l0bzogRmlsZSBpcmlsZGVzIG5vbiBkaXNwb25pYmlsZQ==</ns2:data>
		 <ns2:dataRegistrazione>2024-03-29+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string IrildesNotAvailableCode197Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
		 <ns2:IUT>20240329D17032938522</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>197</ns2:codice>
			<ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
		 </ns2:esito>
		 <ns2:data>RWxhYm9yYXppb25lIEtPIGNvbiBlc2l0bzogRmlsZSBpcmlsZGVzIG5vbiBkaXNwb25pYmlsZQ==</ns2:data>
		 <ns2:dataRegistrazione>2024-03-29+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string IrildesAvailableCode198Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
		 <ns2:IUT>20240329D17032938522</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>198</ns2:codice>
			<ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
		 </ns2:esito>
		 <ns2:data>RmlsZSBJcmlsZGVzIGRpc3BvbmliaWxlIHBlciBtcm4gWzIzSVRRMEIwMUFBMDE4MjdBMF0</ns2:data>
		 <ns2:dataRegistrazione>2024-03-29+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string Code198EmptyDataResponse = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
	  <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
		 <ns2:IUT>20240329D17032938522</ns2:IUT>
		 <ns2:esito>
			<ns2:codice>198</ns2:codice>
			<ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
		 </ns2:esito>
		 <ns2:data />
		 <ns2:dataRegistrazione>2024-03-29+01:00</ns2:dataRegistrazione>
	  </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string ServiceNotAvailableResponse = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Body>
    <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
      <ns2:IUT>20240304D16307349964</ns2:IUT>
      <ns2:esito>
        <ns2:codice>0</ns2:codice>
        <ns2:messaggio>Servizio non disponibile</ns2:messaggio>
      </ns2:esito>
      <ns2:dataRegistrazione>2024-03-04+01:00</ns2:dataRegistrazione>
    </ns2:Output>
  </soapenv:Body>
</soapenv:Envelope>";
}
