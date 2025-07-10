using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;

namespace Enterprise.Customs.IE.PBN.Messaging.Testing;

class LPCPBNProviderTest : Customs.Business.Testing.DataProviderTestCase<LPCPBNProvider>
{
	protected override LPCPBNProvider GetProvider() => new LPCPBNProvider(jsonObject);

	public void TestPbnID()
	{
		AssertEquals("PBN ID of the message", "AA11GH99", Provider.PbnID);
	}

	public void TestChannel()
	{
		AssertEquals("Message Channel", "CUSTOMS", Provider.Channel);
	}

	public void TestAction()
	{
		AssertEquals("Message Action", "Go to T11", Provider.Action);
	}

	public void TestPairedTransport()
	{
		CombineAssertions("Paired Transport", () =>
		{
			AssertNotNull(Provider.PairedTransport);
			AssertEquals("Customs Office", "IEDUB100", Provider.PairedTransport.CustomsOffice);
			AssertEquals("Ship ID", "9332559", Provider.PairedTransport.ShipId);
			AssertEquals("Scheduled time of arrival", "202105041215", Provider.PairedTransport.ScheduledTimeofArrival);
			AssertEquals("Registration Number", "98D12345", Provider.PairedTransport.RegistrationNumber);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jsonObject = (LPCDefinition)JsonSerializer.Deserialize("{\r\n    \"pbnId\": \"AA11GH99\",\r\n    \"channel\": \"CUSTOMS\",\r\n    \"action\": \"Go to T11\",\r\n    \"pairedTransport\": {\r\n        \"customsOffice\": \"IEDUB100\",\r\n        \"shipId\": \"9332559\",\r\n        \"sta\": \"202105041215\",\r\n        \"regNum\": \"98D12345\"\r\n    }\r\n}", typeof(LPCDefinition));
	}

	LPCDefinition jsonObject;
}
