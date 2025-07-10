using System.Net;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	class SystemToSystemTrustApiResponse
	{
		public bool Success => StatusCode == HttpStatusCode.OK;
		public string Content { get; }
		public HttpStatusCode StatusCode { get; }

		public SystemToSystemTrustApiResponse(HttpStatusCode statusCode, string content)
		{
			StatusCode = statusCode;
			Content = content;
		}
	}
}
