using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IrildesResponseMessageTest : TestCase
{
	public void TestResponseBodyStatus()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesPositive200ResourceKey);
		var status = responseMessage.ResponseStatus;
		AssertEquals("Status", "200", status);
	}

	public void TestGetResponseBody()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesPositive200ResourceKey);
		var responseBody = responseMessage.ResponseBody;
		AssertNotNull("ResponseBody", responseBody);
		AssertSame("ResponseBody not processed twice", responseBody, responseMessage.ResponseBody);
	}

	public void TestGetData()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesPositive200ResourceKey);
		var data = responseMessage.Data;
		AssertNotNull("Data", data);
		AssertSame("Data not processed twice", data, responseMessage.Data);
	}

	public void TestHasIvistoNotAvailableTag()
	{
		CombineAssertions(() =>
		{
			var responseMessage = LoadIrildesResponseMessage(IrildesNegative198ResourceKey);
			AssertEquals("When Irildes Not Available", true, responseMessage.HasIrildesNotAvailableTag);

			responseMessage = LoadIrildesResponseMessage(IrildesPositive200ResourceKey);
			AssertEquals("When Irildes Available", false, responseMessage.HasIrildesNotAvailableTag);
		});
	}

	public void TestIsElaborationKO_WhenStatusIs197()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesNegative197ResourceKey);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "197", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), true, responseMessage.IsElaborationKO);
		});
	}

	public void TestIsElaborationKO_WhenStatusIs198()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesNegative198ResourceKey);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "198", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), true, responseMessage.IsElaborationKO);
		});
	}

	public void TestIsElaborationKO_WhenStatusIs200()
	{
		var responseMessage = LoadIrildesResponseMessage(IrildesPositive200ResourceKey);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "200", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), false, responseMessage.IsElaborationKO);
		});
	}

	IrildesResponseMessage LoadIrildesResponseMessage(string resourceKey)
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
		return new IrildesResponseMessage(soapMessage);
	}

	const string IrildesNegative197ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_IrildesNegativeResponseError197.xml";
	const string IrildesNegative198ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_IrildesNegativeResponseError198.xml";
	const string IrildesPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_IrildesPositiveResponseResultCode200.xml";
}
