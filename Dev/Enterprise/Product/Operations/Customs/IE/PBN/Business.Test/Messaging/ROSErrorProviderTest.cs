using System.Linq;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions.PBN;

namespace Enterprise.Customs.IE.PBN.Messaging.Testing;

class ROSErrorProviderTest : Customs.Business.Testing.DataProviderTestCase<ROSErrorProvider>
{
	protected override ROSErrorProvider GetProvider() => new ROSErrorProvider(rosErrorDefinition);

	public void TestTryConvert_Success()
	{
		var success = ROSErrorProvider.TryConvert(testText, out var rosError);
		CombineAssertions(() =>
		{
			Assert("Converting a valid error definition should succeed", success);
			AssertType<ROSErrorDefinition>($"Output object is of type {nameof(ROSErrorDefinition)}", rosError);
		});
	}

	public void TestTryConvert_Failure()
	{
		var success = ROSErrorProvider.TryConvert(null, out var rosError);
		CombineAssertions(() =>
		{
			Assert("Converting an invalid error definition should not succeed", !success);
			AssertEquals($"Output {nameof(ROSErrorDefinition)} should be null", null, rosError);
		});
	}

	public void TestValidationErrors()
	{
		var rosError = Provider.ValidationErrors.Single();
		AssertEquals("code", "111007", rosError.code);
		AssertEquals("description", "Message was not digitally signed", rosError.description);
	}

	protected override void SetUp()
	{
		base.SetUp();
		rosErrorDefinition = (ROSErrorDefinition)JsonSerializer.Deserialize(testText, typeof(ROSErrorDefinition));
	}

	ROSErrorDefinition rosErrorDefinition;
	readonly string testText = @"
{
	""validationErrors"": [
			{
					""code"": ""111007"",
					""description"": ""Message was not digitally signed""
			}
	]
}";
}
