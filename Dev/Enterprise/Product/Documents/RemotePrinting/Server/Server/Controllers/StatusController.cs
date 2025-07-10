using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Web.Http;
using CargoWise.Common;
using Enterprise.RemotePrinting.Server.RPSCore;
namespace Enterprise.RemotePrinting.Server.Controllers
{
	public class StatusController : ApiController
	{
		public StatusController() => cacheExpiry = TimeSpan.FromSeconds(30);

		[Route("wtg/status")]
		[HttpGet]
		[HttpHead]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public IHttpActionResult CheckStatus()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK);

			if (MemoryCache.Default.Get(StatusCacheKey) is StringBuilder cachedValue)
			{
				response.Content = new StringContent(cachedValue.ToString(), Encoding.UTF8, "text/plain");
				return ResponseMessage(response);
			}

			lock (cacheLock)
			{
				if (!(MemoryCache.Default.Get(StatusCacheKey) is StringBuilder content))
				{
					content = new StringBuilder();
					var status = "INFO(Database): OK";

					try
					{
						using var connection = DbHelper.NewConnection();
					}
					catch (Exception ex)
					{
						if (ex is RemotePrintingDbConnectionException connectionException && connectionException.ErrorCode >= 530 && connectionException.ErrorCode <= 537)
						{
							status = "INFO(Database): " + ex.Message;
						}
						else
						{
							ErrorReporter.ReportOnce(ex.Message, ex);
						}
					}

					content.AppendLine(status);
					var registeredClients = RemoteHub.Controller.GetRegisteredClients();
					content.AppendFormat("INFO(ConnectedClients): {0}", registeredClients.Length.ToString());

					MemoryCache.Default.Set(StatusCacheKey, content, DateTimeOffset.UtcNow.Add(cacheExpiry));
				}

				response.Content = new StringContent(content.ToString(), Encoding.UTF8, "text/plain");
				return ResponseMessage(response);
			}
		}

		public const string StatusCacheKey = "dbCheckStatus";
		readonly TimeSpan cacheExpiry;
		static readonly object cacheLock = new object();
	}
}
