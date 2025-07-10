using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EadTadResponseMessageTest : TestCase
{
	public void TestGetResponseBody()
	{
		var responseMessage = LoadEadTadResponseMessage(EadPositive200ResourceKey);
		var responseBody = responseMessage.ResponseBody;
		var data = responseBody.Data;

		CombineAssertions(() =>
		{
			AssertNotNull("Response Body", responseBody);
			AssertNotNull("Response Data", data);
			AssertEquals("200", responseMessage.ResponseStatus);
		});
	}

	public void TestGetData_EADResponseKey()
	{
		var responseMessage = LoadEadTadResponseMessage(EadPositive200ResourceKey);

		CombineAssertions(() =>
		{
			AssertEquals("File name", "EAD_24ITQ0B01AA28984A9.pdf", responseMessage.FileName);
			AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
			AssertEquals("MRN", "24ITQ0B01AA28984A9", responseMessage.MRN);
			AssertEquals("Document Type", "CLR", responseMessage.DocumentType);
		});
	}

	public void TestGetData_TADResponseKey()
	{
		var responseMessage = LoadEadTadResponseMessage(TadPositive200ResourceKey);

		CombineAssertions(() =>
		{
			AssertEquals("File name", "TAD_24ITQTU08AA28957J9.pdf", responseMessage.FileName);
			AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
			AssertEquals("MRN", "24ITQTU08AA28957J9", responseMessage.MRN);
			AssertEquals("Document Type", "CLR", responseMessage.DocumentType);
		});
	}

	EadTadResponseMessage LoadEadTadResponseMessage(string resourceKey)
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
		return new EadTadResponseMessage(soapMessage);
	}

	const string EadPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadPositiveResponseResultCode200.xml";
	const string TadPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Ncts_TadPositiveResponseResultCode200.xml";
}
