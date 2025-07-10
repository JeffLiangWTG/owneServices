using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Eur1ResponseMessageTest : TestCase
{
	public void TestGetResponseBody()
	{
		var responseMessage = LoadEur1ResponseMessage(Eur1positive200ResourceKey);
		var responseBody = responseMessage.ResponseBody;
		var data = responseBody.Data;

		AssertNotNull("Response Body", responseBody);
		AssertNotNull("Response Data", data);
		AssertEquals("200", responseMessage.ResponseStatus);
	}

	public void TestGetData()
	{
		var responseMessage = LoadEur1ResponseMessage(Eur1positive200ResourceKey);

		AssertEquals("File name", $"EUR1_24ITQ0B01AA28984A9.pdf", responseMessage.FileName);
		AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
		AssertEquals("MRN", $"24ITQ0B01AA28984A9", responseMessage.MRN);
		AssertEquals("Document Type", "COO", responseMessage.DocumentType);
	}

	Eur1ResponseMessage LoadEur1ResponseMessage(string resourceKey)
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
		return new Eur1ResponseMessage(soapMessage);
	}

	const string Eur1positive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1PositiveResponseResultCode200_sample.xml";
}
