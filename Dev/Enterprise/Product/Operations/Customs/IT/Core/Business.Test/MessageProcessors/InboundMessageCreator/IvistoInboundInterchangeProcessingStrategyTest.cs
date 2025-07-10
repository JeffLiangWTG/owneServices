using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IvistoInboundInterchangeProcessingStrategyTest : XTradeInboundMessageCreatorAbstractTest
{
	public void TestProcessIvistoInterchange_WithResponseNotAvailable_WithError198()
	{
		ivistoResponseInterchange.EI_BodyText = IvistoNotAvailableCode198Response;

		ProcessInterchange(ivistoResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=198, Ivisto Not Available", ivistoResponseInterchange, expectedCount: 0);
	}

	public void TestProcessIvistoInterchange_WithResponseNotAvailable_WithError197()
	{
		ivistoResponseInterchange.EI_BodyText = IvistoNotAvailableCode197Response;

		ProcessInterchange(ivistoResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=197, Ivisto Not Available", ivistoResponseInterchange, expectedCount: 0);
	}

	public void TestProcessIvistoInterchange_WithResponseAvailable_WithError198()
	{
		ivistoResponseInterchange.EI_BodyText = IvistoAvailableCode198Response;

		ProcessInterchange(ivistoResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=198, Ivisto Available", ivistoResponseInterchange, expectedCount: 1);
	}

	public void TestProcessIvistoInterchange_WithEmptyResponse_WithError198()
	{
		ivistoResponseInterchange.EI_BodyText = Code198EmptyDataResponse;

		ProcessInterchange(ivistoResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=198, Empty data", ivistoResponseInterchange, expectedCount: 1);
	}

	public void TestProcessIvistoInterchange_WhenResponseIsServiceNotAvailable()
	{
		ivistoResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(ivistoResponseInterchange);
		AssertContainedMessagesCount("When Interchange=IVR, Code=0, Service not available", ivistoResponseInterchange, expectedCount: 0);
	}

	[TestDate]
	public void TestProcessIvistoInterchange_WithResponseNotAvailable_CreateNewCusPollingTransaction()
	{
		ivistoResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(ivistoResponseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(ivistoRequestInterchange, "OPN", ZDateTime.UtcNow.AddDays(1), "IVI");
	}

	[TestDate]
	public void TestProcessIvistoInterchange_WithResponseNotAvailable_UpdateCusPollingTransaction()
	{
		helper.CreateIvistoPollingTransaction("PND", ZDateTime.UtcNow, ivistoRequestInterchange, ZDateTime.UtcNow);

		ivistoResponseInterchange.EI_BodyText = ServiceNotAvailableResponse;

		ProcessInterchange(ivistoResponseInterchange);

		CusPollingTransactionTestHelper.AssertUniquePollingTransactionForInterchange(ivistoRequestInterchange, "OPN", ZDateTime.UtcNow.AddDays(1), "IVI");
	}

	[TestDate]
	public void TestProcessIvistoInterchange_WithResponseAvailable_DeleteCusPollingTransaction()
	{
		helper.CreateIvistoPollingTransaction("PND", ZDateTime.UtcNow, ivistoRequestInterchange, ZDateTime.UtcNow);
		helper.CreateIvistoPollingTransaction("OPN", ZDateTime.UtcNow, ivistoRequestInterchange, ZDateTime.UtcNow);

		ivistoResponseInterchange.EI_BodyText = IvistoAvailableCode198Response;

		ProcessInterchange(ivistoResponseInterchange);

		CusPollingTransactionTestHelper.AssertNoPollingTransactionsForInterchange(ivistoRequestInterchange);
	}

	public void TestProcessIvistoInterchange_ThrowsCouldNotFindRelatedBusinessObjectException()
	{
		ivistoResponseInterchange.EI_SessionGUID = ZGuid.NewZGuid();

		AssertExceptionThrown<CouldNotFindRelatedBusinessObjectException>(() => ProcessInterchange(ivistoResponseInterchange));

		AssertContainedMessagesCount("When CouldNotFindRelatedBusinessObjectException is thrown", ivistoResponseInterchange, expectedCount: 0);
	}

	protected override void SetUp()
	{
		base.SetUp();

		helper = new CusPollingTransactionTestDataHelper(Factory);

		ivistoRequestInterchange = helper.CreateIvistoRequestInterchange();

		ivistoResponseInterchange = Factory.New<EDIInterchange>();
		ivistoResponseInterchange.EI_ApplicationCode = "ITH";
		ivistoResponseInterchange.EI_InterchangeType = "IVR";
		ivistoResponseInterchange.EI_SessionGUID = ivistoRequestInterchange.EI_SessionGUID;
	}

	CusPollingTransactionTestDataHelper helper;
	EDIInterchange ivistoRequestInterchange;
	EDIInterchange ivistoResponseInterchange;

	#region Ivisto Responses

	const string IvistoNotAvailableCode198Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://exportservice.domest.sogei.it"">
         <ns2:IUT>20230427D16001305375</ns2:IUT>
         <ns2:esito>
            <ns2:codice>198</ns2:codice>
            <ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
         </ns2:esito>
         <ns2:data>RmlsZSBJdmlzdG8gbm9uIGRpc3BvbmliaWxlIHBlciBtcm4gWzIzSVRRMEIwMUFBMDE4MjdBMF0=</ns2:data>
         <ns2:dataRegistrazione>2023-04-27+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string IvistoNotAvailableCode197Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://exportservice.domest.sogei.it"">
         <ns2:IUT>20230427D16001305375</ns2:IUT>
         <ns2:esito>
            <ns2:codice>197</ns2:codice>
            <ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
         </ns2:esito>
         <ns2:data>RmlsZSBJdmlzdG8gbm9uIGRpc3BvbmliaWxlIHBlciBtcm4gWzIzSVRRMEIwMUFBMDE4MjdBMF0=</ns2:data>
         <ns2:dataRegistrazione>2023-04-27+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string IvistoAvailableCode198Response = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://exportservice.domest.sogei.it"">
         <ns2:IUT>20230427D16001305375</ns2:IUT>
         <ns2:esito>
            <ns2:codice>198</ns2:codice>
            <ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
         </ns2:esito>
         <ns2:data>RmlsZSBJdmlzdG8gZGlzcG9uaWJpbGUgcGVyIG1ybiBbMjNJVFEwQjAxQUEwMTgyN0EwXQ==</ns2:data>
         <ns2:dataRegistrazione>2023-04-27+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string Code198EmptyDataResponse = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
   <soapenv:Body>
      <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://exportservice.domest.sogei.it"">
         <ns2:IUT>20230427D16001305375</ns2:IUT>
         <ns2:esito>
            <ns2:codice>198</ns2:codice>
            <ns2:messaggio>Elaborazione KO: con esito</ns2:messaggio>
         </ns2:esito>
         <ns2:data />
         <ns2:dataRegistrazione>2023-04-27+02:00</ns2:dataRegistrazione>
      </ns2:Output>
   </soapenv:Body>
</soapenv:Envelope>";

	const string ServiceNotAvailableResponse = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Body>
    <ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://exportservice.domest.sogei.it"">
      <ns2:IUT>20240304D16307349964</ns2:IUT>
      <ns2:esito>
        <ns2:codice>0</ns2:codice>
        <ns2:messaggio>Servizio non disponibile</ns2:messaggio>
      </ns2:esito>
      <ns2:dataRegistrazione>2024-03-04+01:00</ns2:dataRegistrazione>
    </ns2:Output>
  </soapenv:Body>
</soapenv:Envelope>";

	#endregion
}
