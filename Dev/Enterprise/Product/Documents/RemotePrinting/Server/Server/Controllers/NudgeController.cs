using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Controllers
{
	public class NudgeController : ApiController
	{
		[Route("api/nudge")]
		[HttpPost]
		// POST api/Nudge?serverName=abc&printQueueName=xyz&printJobPK=12345678-1234-1234-1234-1234567890AB
		public HttpResponseMessage Post(string serverName, string printQueueName, Guid printJobPK = default, bool isForwarded = false)
		{
			using (var connection = DbHelper.NewConnection())
			{
				if (RemoteHub.Controller.Nudge(serverName, printQueueName, printJobPK, connection, isForwarded: isForwarded))
				{
					return new HttpResponseMessage(HttpStatusCode.Accepted);
				}
				else
				{
					return new HttpResponseMessage(HttpStatusCode.NoContent);
				}
			}
		}
	}
}
