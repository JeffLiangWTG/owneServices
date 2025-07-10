using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IrildesResponseEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestFormatPositiveMessage()
	{
		const string inputRawData = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
	<soapenv:Body>
		<ns2:Output xmlns:ns2=""http://ws.sogei.it/output/"" xmlns=""http://transitoservice.domest.sogei.it"">
			<ns2:IUT>20231114D17014171042</ns2:IUT>
			<ns2:esito>
				<ns2:codice>200</ns2:codice>
				<ns2:messaggio>Elaborazione OK: con esito</ns2:messaggio>
			</ns2:esito>
			<ns2:data>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+DQo8bjE6Q0MwNDVDIHhtbG5zOm4xPSJodHRwOi8vbmN0cy5kZ3RheHVkLmVjIj4NCgk8bWVzc2FnZVNlbmRlcj5OVEEuSVQ8L21lc3NhZ2VTZW5kZXI+DQoJPG1lc3NhZ2VSZWNpcGllbnQ+YWNjb2dsaWVuemE8L21lc3NhZ2VSZWNpcGllbnQ+DQoJPHByZXBhcmF0aW9uRGF0ZUFuZFRpbWU+MjAyNC0wMy0yOFQxNjo0NjowMDwvcHJlcGFyYXRpb25EYXRlQW5kVGltZT4NCgk8bWVzc2FnZUlkZW50aWZpY2F0aW9uPjE3MTE2NDA3NjAzNDgxPC9tZXNzYWdlSWRlbnRpZmljYXRpb24+DQoJPG1lc3NhZ2VUeXBlPkNDMDQ1QzwvbWVzc2FnZVR5cGU+DQoJPFRyYW5zaXRPcGVyYXRpb24+DQoJCTxNUk4+MjRJVFFZRzhUSks5NTkwMEw5PC9NUk4+DQoJCTx3cml0ZU9mZkRhdGU+MjAyNC0wMy0yODwvd3JpdGVPZmZEYXRlPg0KCTwvVHJhbnNpdE9wZXJhdGlvbj4NCgk8Q3VzdG9tc09mZmljZU9mRGVwYXJ0dXJlPg0KCQk8cmVmZXJlbmNlTnVtYmVyPklUMjc4MTAwPC9yZWZlcmVuY2VOdW1iZXI+DQoJPC9DdXN0b21zT2ZmaWNlT2ZEZXBhcnR1cmU+DQoJPEhvbGRlck9mVGhlVHJhbnNpdFByb2NlZHVyZT4NCgkJPGlkZW50aWZpY2F0aW9uTnVtYmVyPklUMTA3MjYxNzAxNTE8L2lkZW50aWZpY2F0aW9uTnVtYmVyPg0KCTwvSG9sZGVyT2ZUaGVUcmFuc2l0UHJvY2VkdXJlPg0KPC9uMTpDQzA0NUM+</ns2:data>
			<ns2:dataRegistrazione>2023-11-14+01:00</ns2:dataRegistrazione>
		</ns2:Output>
	</soapenv:Body>
</soapenv:Envelope>";

		const string expectedPrettyPrint = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<n1:CC045C xmlns:n1=""http://ncts.dgtaxud.ec"">
	<messageSender>NTA.IT</messageSender>
	<messageRecipient>accoglienza</messageRecipient>
	<preparationDateAndTime>2024-03-28T16:46:00</preparationDateAndTime>
	<messageIdentification>17116407603481</messageIdentification>
	<messageType>CC045C</messageType>
	<TransitOperation>
		<MRN>24ITQYG8TJK95900L9</MRN>
		<writeOffDate>2024-03-28</writeOffDate>
	</TransitOperation>
	<CustomsOfficeOfDeparture>
		<referenceNumber>IT278100</referenceNumber>
	</CustomsOfficeOfDeparture>
	<HolderOfTheTransitProcedure>
		<identificationNumber>IT10726170151</identificationNumber>
	</HolderOfTheTransitProcedure>
</n1:CC045C>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", expectedPrettyPrint, message.EM_MessageInterpretation);
	}

	public void TestFormatNegativeMessage()
	{
		const string inputRawData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soapenv:Body>
		<Output xmlns=""http://ws.sogei.it/output/"">
			<IUT>20230821D16005825779</IUT>
			<esito>
				<codice>16</codice>
				<messaggio>Certificato autenticazione non valido</messaggio>
			</esito>
		</Output>
	</soapenv:Body>
</soapenv:Envelope>";

		message.EM_MessageText = inputRawData;
		AssertEquals("Formatted Text", inputRawData, message.EM_MessageInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();

		message = Factory.New<ITEDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.IrildesResponse;
	}

	ITEDIMessage message;
}
