using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ReleaseProspectusResponseMessageTest : TestCase
{
	public void TestGetResponseBody()
	{
		var responseMessage = LoadReleaseProspectusResponseMessage(ReleaseProspectusPositive200ResourceKey);
		var responseBody = responseMessage.ResponseBody;
		var data = responseBody.Data;

		CombineAssertions(() =>
		{
			AssertNotNull("Response Body", responseBody);
			AssertNotNull("Response Data", data);
			AssertEquals("200", responseMessage.ResponseStatus);
		});
	}

	public void TestGetData()
	{
		var responseMessage = LoadReleaseProspectusResponseMessage(ReleaseProspectusPositive200ResourceKey);

		CombineAssertions(() =>
		{
			AssertEquals("File name", "SVI_24ITQYH4TAA11834R0.pdf", responseMessage.FileName);
			AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
			AssertEquals("MRN", "24ITQYH4TAA11834R0", responseMessage.MRN);
			AssertEquals("Document Type", "CLR", responseMessage.DocumentType);
		});
	}

	ReleaseProspectusResponseMessage LoadReleaseProspectusResponseMessage(string resourceKey)
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
		return new ReleaseProspectusResponseMessage(soapMessage);
	}

	const string ReleaseProspectusPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusPositiveResponseResultCode200.xml";
}

