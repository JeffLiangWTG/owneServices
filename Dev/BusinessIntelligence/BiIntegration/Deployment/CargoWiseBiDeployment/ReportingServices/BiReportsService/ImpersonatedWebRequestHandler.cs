using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Bi.Registration.Common;

namespace CargoWise.Bi.Deployment.ReportingServices
{
#if NETFRAMEWORK
	public class ImpersonatedWebRequestHandler : WebRequestHandler
#else
	public class ImpersonatedWebRequestHandler : HttpClientHandler
#endif
	{
		public ImpersonatedWebRequestHandler(BiReportUser reportUser) : base()
		{
			ReportUser = reportUser;
			UseDefaultCredentials = true;
			Credentials = ReportUser.NetworkCredential;
		}
		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return SendAsync(request, CancellationToken.None);
		}
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			using (ReportUser.Impersonate())
			{
				return base.SendAsync(request, cancellationToken);
			}
		}

		public BiReportUser ReportUser { get; }
	}
}
