using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	public class EnterpriseWebApiExceptionHandler : ExceptionHandler
	{
		public override void Handle(ExceptionHandlerContext context)
		{
			if (context.Exception.FlattenInnerExceptions().Any(e => e is DatabaseUpgradeException))
			{
				WebUpgradeManager.NotifyUpgradeRequired();
				context.Result = new DatabaseUpgradeResponse();
			}
			else
			{
				ErrorReporter.ReportOnce("EnterpriseWebApiExceptionHandler", context.Exception);
			}

			base.Handle(context);
		}

		public class DatabaseUpgradeResponse : IHttpActionResult
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API response message")]
			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
				response.Content = new StringContent("Upgrade in progress");
				return Task.FromResult(response);
			}
		}
	}
}
