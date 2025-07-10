using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ExportResponseMessageTest : TestCase
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

	protected override void SetUp()
	{
		base.SetUp();
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_PositiveResponse.xml");
		responseMessage = new Ucc6ExportResponseMessage(soapMessage);
	}

	Ucc6ExportResponseMessage responseMessage;
}
