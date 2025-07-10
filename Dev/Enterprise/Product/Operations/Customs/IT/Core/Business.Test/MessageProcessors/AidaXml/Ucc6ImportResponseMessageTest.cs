using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ImportResponseMessageTest : TestCase
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
		AssertSame("Data not processed twice", data, responseMessage.Data);
	}

	public void TestGetResponseBody()
	{
		var responseBody = responseMessage.ResponseBody;
		AssertNotNull("ResponseBody", responseBody);
		AssertSame("ResponseBody not processed twice", responseBody, responseMessage.ResponseBody);
	}

	public void TestGetResponseMessageContents()
	{
		var responseMessageContents = ((IResponseMessageWithWrapper)responseMessage).GetResponseMessageContents();
		AssertNotNull("Response Message Content", responseMessageContents);
	}

	[ExpectNoExceptions]
	public void TestGetResponseWrapper_ResponseSanitizedBeforeDeserialization()
	{
		var responseInvalidDateTimeTags = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ResponseInvalidDateTimeTags.xml");
		var responseMessage = new Ucc6ImportResponseMessage(responseInvalidDateTimeTags);
		_ = ((IResponseMessageWithWrapper)responseMessage).GetResponseMessageContents();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_PositiveResponseWithClearance.xml");
		responseMessage = new Ucc6ImportResponseMessage(soapMessage);
	}

	Ucc6ImportResponseMessage responseMessage;
}
