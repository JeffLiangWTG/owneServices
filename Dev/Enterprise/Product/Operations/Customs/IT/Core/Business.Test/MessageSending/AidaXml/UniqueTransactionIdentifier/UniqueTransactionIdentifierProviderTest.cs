using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier.Testing;

sealed class UniqueTransactionIdentifierProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When interchange is null", () => new UniqueTransactionIdentifierProvider(interchange: null));
	}

	public void TestGetUniqueTransactionID_WhenNoReceivedAcknowledgmentInterchangesAreFound()
	{
		var transactionIdProvider = new UniqueTransactionIdentifierProvider(interchange);
		var transactionId = transactionIdProvider.GetUniqueTransactionID();
		AssertEquals("Unique Transaction ID", "", transactionId);
	}

	public void TestGetUniqueTransactionID_WhenOneReceivedAcknowledgmentInterchangeIsFound()
	{
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange1.EI_BodyText = GetSoapEnvelope("<ns2:IUT>20220307D11000328189</ns2:IUT>");
		interchange1.EI_ReceiveTransmit = "RCV";
		interchange1.EI_InterchangeType = "ACK";

		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange2.EI_BodyText = GetSoapEnvelope("<ns2:IUT>20220307D11000328191</ns2:IUT>");
		interchange2.EI_ReceiveTransmit = "RCV";

		var transactionIdProvider = new UniqueTransactionIdentifierProvider(interchange);
		var transactionId = transactionIdProvider.GetUniqueTransactionID();
		AssertEquals("Unique Transaction ID", "20220307D11000328189", transactionId);
	}

	public void TestGetUniqueTransactionID_WhenOneReceivedAcknowledgmentInterchangeIsFoundButMalformed()
	{
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange1.EI_BodyText = GetSoapEnvelope(iutSegment: "");
		interchange1.EI_ReceiveTransmit = "RCV";
		interchange1.EI_InterchangeType = "ACK";

		var transactionIdProvider = new UniqueTransactionIdentifierProvider(interchange);
		var transactionId = transactionIdProvider.GetUniqueTransactionID();
		AssertEquals("Unique Transaction ID", "", transactionId);
	}

	public void TestGetUniqueTransactionID_WhenMoreThanOneReceivedAcknowledgmentInterchangesAreFound()
	{
		var utcNow = ZDateTime.UtcNow;
		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_SystemCreateTimeUtc = utcNow;
		interchange1.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange1.EI_BodyText = GetSoapEnvelope("<ns2:IUT>20220307D11000328189</ns2:IUT>");
		interchange1.EI_ReceiveTransmit = "RCV";
		interchange1.EI_InterchangeType = "ACK";

		var interchange2 = Factory.New<EDIInterchange>();
		interchange2.EI_SystemCreateTimeUtc = utcNow.AddDays(-1);
		interchange2.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange2.EI_BodyText = GetSoapEnvelope("<ns2:IUT>20220307D11000328190</ns2:IUT>");
		interchange2.EI_ReceiveTransmit = "RCV";
		interchange2.EI_InterchangeType = "ACK";

		var transactionIdProvider = new UniqueTransactionIdentifierProvider(interchange);
		var transactionId = transactionIdProvider.GetUniqueTransactionID();
		AssertEquals("Unique Transaction ID", "20220307D11000328190", transactionId);
	}

	ZString GetSoapEnvelope(string iutSegment)
	{
		return $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
	<soapenv:Body>
		<ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://importservice.domest.sogei.it"">
			{iutSegment}
			<ns2:esito>
				<ns2:codice>15</ns2:codice>
				<ns2:messaggio>Input data is not valid</ns2:messaggio>
			</ns2:esito>
			<ns2:dataRegistrazione>2022-03-07+01:00</ns2:dataRegistrazione>
		</ns2:Output>
	</soapenv:Body>
</soapenv:Envelope>";
	}

	protected override void SetUp()
	{
		base.SetUp();
		interchange = Factory.New<EDIInterchange>();
		interchange.EI_SessionGUID = ZGuid.BrettsGuid;
		interchange.EI_ReceiveTransmit = "TRX";
	}

	EDIInterchange interchange;
}
