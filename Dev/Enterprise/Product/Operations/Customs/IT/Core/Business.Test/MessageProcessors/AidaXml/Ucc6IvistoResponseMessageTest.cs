using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6IvistoResponseMessageTest : TestCase
{
	public void TestResponseBodyStatus()
	{
		var status = responseMessage.ResponseStatus;
		AssertEquals("Status", "200", status);
	}

	public void TestGetResponseBody()
	{
		var responseBody = responseMessage.ResponseBody;
		AssertNotNull("ResponseBody", responseBody);
		AssertSame("ResponseBody not processed twice", responseBody, responseMessage.ResponseBody);
	}

	public void TestGetData()
	{
		var data = responseMessage.Data;
		AssertNotNull("Data", data);
		AssertSame("Data not processed twice", data, responseMessage.Data);
	}

	public void TestHasIvistoNotAvailableTag()
	{
		CombineAssertions(() =>
		{
			var ivistoNotAvailableMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoNegativeResponseError197.xml");
			responseMessage = new Ucc6IvistoResponseMessage(ivistoNotAvailableMessage);
			AssertEquals("When Ivisto Not Available", true, responseMessage.HasIvistoNotAvailableTag());

			var ivistoAvailableMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
			responseMessage = new Ucc6IvistoResponseMessage(ivistoAvailableMessage);
			AssertEquals("When Ivisto Available", false, responseMessage.HasIvistoNotAvailableTag());
		});
	}

	public void TestIsElaborationKO_WhenStatusIs197()
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoNegativeResponseError197.xml");
		responseMessage = new Ucc6IvistoResponseMessage(soapMessage);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "197", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), true, responseMessage.IsElaborationKO);
		});
	}

	public void TestIsElaborationKO_WhenStatusIs198()
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoNegativeResponse.xml");
		responseMessage = new Ucc6IvistoResponseMessage(soapMessage);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "198", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), true, responseMessage.IsElaborationKO);
		});
	}

	public void TestIsElaborationKO_WhenStatusIs200()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(responseMessage.ResponseStatus), "200", responseMessage.ResponseStatus);
			AssertEquals(nameof(responseMessage.IsElaborationKO), false, responseMessage.IsElaborationKO);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
		responseMessage = new Ucc6IvistoResponseMessage(soapMessage);
	}

	Ucc6IvistoResponseMessage responseMessage;
}
