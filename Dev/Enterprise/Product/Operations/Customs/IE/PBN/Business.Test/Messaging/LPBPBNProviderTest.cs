using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;

namespace Enterprise.Customs.IE.PBN.Messaging.Testing;

class LPBPBNProviderTest : Customs.Business.Testing.DataProviderTestCase<LPBPBNProvider>
{
	protected override LPBPBNProvider GetProvider() => new LPBPBNProvider(jsonObject);

	public void TestPbnID()
	{
		AssertEquals("PBN ID of the message", "AA11GH99", Provider.PbnID);
	}

	public void TestStatus()
	{
		AssertEquals("Status of the message", "INCOMPLETE", Provider.Status);
	}

	public void TestIssue()
	{
		AssertEquals("Issue of the message", "Import declaration MRNs are missing", Provider.Issue);
	}

	public void TestDirection()
	{
		AssertEquals("Direction of the message", "IN_IRELAND", Provider.Direction);
	}

	public void TestEmptyVehicle()
	{
		AssertEquals("Is vehicle empty", false, Provider.EmptyVehicle);
	}

	public void TestContactDetails()
	{
		CombineAssertions("Contact details in message", () =>
		{
			AssertNotNull(Provider.ContactDetails);
			AssertEquals("Email", "tesX@XXX.XXX", Provider.ContactDetails.Email);
			AssertEquals("mobileNum1", "+35XXXXXXXXXX", Provider.ContactDetails.MobileNum1);
			AssertEquals("mobileNum2", "+35YYYYYYYYYY", Provider.ContactDetails.MobileNum2);
		});
	}

	public void TestDeclarations()
	{
		CombineAssertions("Declaration list in message", () =>
		{
			AssertNotNull(Provider.Declarations);
			AssertEquals(1, Provider.Declarations.Count);
			AssertEquals("Declaration ID", "21IE99900000000001", Provider.Declarations[0].DeclarationId);
			AssertEquals("Declaration Type", "", Provider.Declarations[0].DeclarationType);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		jsonObject = (LPBDefinition)JsonSerializer.Deserialize("{\r\n    \"pbnId\": \"AA11GH99\",\r\n    \"status\": \"INCOMPLETE\",\r\n    \"issue\": \"Import declaration MRNs are missing\",\r\n    \"direction\": \"IN_IRELAND\",\r\n    \"emptyVehicle\": false,\r\n    \"declarations\": [\r\n        {\r\n            \"declarationId\": \"21IE99900000000001\"\r\n        }\r\n    ],\r\n    \"contactDetails\": {\r\n        \"email\": \"tesX@XXX.XXX\",\r\n        \"mobileNum1\": \"+35XXXXXXXXXX\",\r\n        \"mobileNum2\": \"+35YYYYYYYYYY\"\r\n    }\r\n}", typeof(LPBDefinition));
	}

	LPBDefinition jsonObject;
}
