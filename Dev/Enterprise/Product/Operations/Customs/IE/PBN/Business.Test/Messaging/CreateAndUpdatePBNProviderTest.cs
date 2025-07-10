using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;

namespace Enterprise.Customs.IE.PBN.Messaging.Testing;

class CreateAndUpdatePBNProviderTest : Customs.Business.Testing.DataProviderTestCase<CreateAndUpdatePBNProvider>
{
	protected override CreateAndUpdatePBNProvider GetProvider() => new CreateAndUpdatePBNProvider(jsonObject);

	public void TestPbnID()
	{
		AssertEquals("PBN ID is empty for rejected", string.Empty, Provider.PbnID);
	}

	public void TestStatus()
	{
		AssertEquals("Status for rejected", "REJECTED", Provider.Status);
	}

	public void TestIssue()
	{
		AssertNullOrEmpty(Provider.Issue);
	}

	public void TestValidationErrors()
	{
		CombineAssertions("Validation errors provider", () =>
		{
			AssertNotNull(Provider.listValidationErrorProviders);
			AssertEquals("Number of validation errors", 3, Provider.listValidationErrorProviders.Count);
			AssertEquals("Error code", "00001", Provider.listValidationErrorProviders[0].code);
			AssertEquals("Path", "declarations[1].declarationId", Provider.listValidationErrorProviders[1].path);
			AssertEquals("description", "Vehicle declared as empty. Declarations not permitted.", Provider.listValidationErrorProviders[2].description);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		jsonObject = (CreateAndUpdatePBNMessageDefinition)JsonSerializer.Deserialize(@"{
    ""status"": ""REJECTED"",
    ""validationErrors"": [
        {
            ""code"": ""00001"",
            ""path"": ""declarations[0].declarationId"",
            ""description"": ""Invalid MRN Format.""
        },
        {
            ""code"": ""00001"",
            ""path"": ""declarations[1].declarationId"",
            ""description"": ""MRN at invalid status.""
        },
        {
            ""code"": ""00001"",
            ""path"": ""declarations"",
            ""description"": ""Vehicle declared as empty. Declarations not permitted.""
        }
    ]
}", typeof(CreateAndUpdatePBNMessageDefinition));
	}

	CreateAndUpdatePBNMessageDefinition jsonObject;
}
