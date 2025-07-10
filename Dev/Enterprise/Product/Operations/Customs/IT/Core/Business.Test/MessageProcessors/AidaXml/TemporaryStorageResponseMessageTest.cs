using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class TemporaryStorageResponseMessageTest : TestCase
{
	public void TestResponseBodyStatus()
	{
		var status = responseMessage.ResponseStatus;
		AssertEquals("Status", "200", status);
	}

	public void TestGetData()
	{
		var data = responseMessage.Data;
		AssertNotNull("Data", data);
		AssertSame("Data, Cached", data, responseMessage.Data);
	}

	public void TestGetResponseBody()
	{
		var responseBody = responseMessage.ResponseBody;
		AssertNotNull("ResponseBody", responseBody);
		AssertSame("ResponseBody not processed twice", responseBody, responseMessage.ResponseBody);
	}

	public void TestGetResponseMessageContents()
	{
		var responseMessageContents = responseMessage.GetResponseMessageContents();
		AssertNotNull("Response Message Content", responseMessageContents);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.TemporaryStorage_G4_PositiveResponse_AllOutcomesPositive_ResultCode200.xml");
		responseMessage = new TemporaryStorageResponseMessage(soapMessage);
	}

	TemporaryStorageResponseMessage responseMessage;
}
