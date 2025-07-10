using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ProspectusDownloadMessageTest : TestCase
{
	public void TestGetResponseBody()
	{
		var responseMessage = LoadProspectusDownloadMessage(AccountingSummaryDownloadPositive200ResourceKey, "PRD");
		var responseBody = responseMessage.ResponseBody;
		var data = responseBody.Data;

		AssertNotNull("Response Body", responseBody);
		AssertNotNull("Response Data", data);
		AssertEquals("200", responseMessage.ResponseStatus);
	}

	public void TestGetDataForPRD()
	{
		var responseMessage = LoadProspectusDownloadMessage(AccountingSummaryDownloadPositive200ResourceKey, "PRD");

		AssertEquals("File name", $"PRD_24ITQYH4TAA11834R0.pdf", responseMessage.FileName);
		AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
		AssertEquals("MRN", $"24ITQYH4TAA11834R0", responseMessage.MRN);
		AssertEquals("Document Type", "MCD", responseMessage.DocumentType);
	}

	public void TestGetDataForSPD()
	{
		var responseMessage = LoadProspectusDownloadMessage(SummaryProspectusDownloadPositive200ResourceKey, "SPD");

		AssertEquals("File name", $"SPD_25ITQYH7TAA00605R6.pdf", responseMessage.FileName);
		AssertEquals("File content", true, responseMessage.ContentData.Length > 0);
		AssertEquals("MRN", $"25ITQYH7TAA00605R6", responseMessage.MRN);
		AssertEquals("Document Type", "MCD", responseMessage.DocumentType);
	}

	ProspectusDownloadMessage LoadProspectusDownloadMessage(string resourceKey, string messageType)
	{
		var soapMessage = ManifestResourceHelper.ReadManifestResourceContent(resourceKey);
		return new ProspectusDownloadMessage(soapMessage, messageType);
	}

	const string AccountingSummaryDownloadPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AccountingSummaryDownloadPositiveResponseResultCode200.xml";

	const string SummaryProspectusDownloadPositive200ResourceKey = "Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadPositiveResponseResultCode200.xml";
}
