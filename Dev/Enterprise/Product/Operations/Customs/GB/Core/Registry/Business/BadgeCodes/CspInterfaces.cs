using System.Net;
namespace Enterprise.Customs.GB.Registry
{
	public interface ICspPrintsMailBoxProvider : IUrlProvider
	{
		ICredentials Credentials { get; set; }
		ICspResultOfAcknowledgement acknowledgeEdifactPrints(string companyCode, string printer, string batchId);
		ICspDownloadResult getAvailableEdifactPrints(string company, string printer);
		ICspDownloadResult checkCdsCredentials(string company, string printer);
	}

	public interface IUrlProvider
	{
		string Url { get; set; }
		string UserAgent { get; set; }
	}

	public interface ICspDownloadResult
	{
		string errorText { get; }
		string[] MessagesArray { get; }
		int batchId { get; set; }
	}

	public interface ICspResultOfAcknowledgement
	{
		int messageCode { get; set; }
		string messageText { get; set; }
	}
}
