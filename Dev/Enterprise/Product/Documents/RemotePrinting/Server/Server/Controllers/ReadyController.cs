using System;
using System.Net;
using System.Runtime.Caching;
using System.Web.Http;
using CargoWise.Common;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Controllers
{
	public class ReadyController : ApiController
	{
		public ReadyController() => this.cacheExpiry = TimeSpan.FromSeconds(30);

		[Route("wtg/ready")]
		[HttpGet]
		[HttpHead]
		public IHttpActionResult CheckReady()
		{
			if (MemoryCache.Default.Get(ReadyCacheKey) is HttpStatusCode cachedValue)
			{
				return StatusCode(cachedValue);
			}

			lock (cacheLock)
			{
				if (!(MemoryCache.Default.Get(ReadyCacheKey) is HttpStatusCode status))
				{
					status = HttpStatusCode.ServiceUnavailable;

					try
					{
						using var connection = DbHelper.NewConnection();
						status = HttpStatusCode.OK;
					}
					catch (Exception ex)
					{
						if (!(ex is RemotePrintingDbConnectionException connectionException && connectionException.ErrorCode >= 530 && connectionException.ErrorCode <= 537))
						{
							ErrorReporter.ReportOnce(ex.Message, ex);
						}
					}
					MemoryCache.Default.Set(ReadyCacheKey, status, DateTimeOffset.UtcNow.Add(cacheExpiry));
				}

				return StatusCode(status);
			}
		}

		public const string ReadyCacheKey = "dbCheckReady";
		readonly TimeSpan cacheExpiry;
		static readonly object cacheLock = new object();
	}
}
