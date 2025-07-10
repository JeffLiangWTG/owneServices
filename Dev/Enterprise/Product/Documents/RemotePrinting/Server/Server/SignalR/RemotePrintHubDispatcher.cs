using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Enterprise.RemotePrinting.Server
{
	public class RemotePrintHubDispatcher : HubDispatcher
	{
		public RemotePrintHubDispatcher(HubConfiguration configuration) : base(configuration)
		{
		}

		protected override bool AuthorizeRequest(IRequest request)
		{
			try
			{
				return base.AuthorizeRequest(request);
			}
			catch (JsonSerializationException)
			{
				return false;
			}
		}
	}
}
