using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.ZArchitecture.Modules;
using Microsoft.AspNet.SignalR;

namespace Enterprise.BlazorWinFormsInterop
{
	public class BlazorWinFormsInteropHub : Hub<IBlazorClient>, IBlazorHub
	{
		public static bool Connected { get => connected; }
		static volatile bool connected;

		public override Task OnDisconnected(bool stopCalled)
		{
			connected = false;
			return base.OnDisconnected(stopCalled);
		}

		public void BackchannelConnected()
		{
			connected = true;
		}

		public void Echo(string value)
		{
			Clients.All.EchoReply(value);
		}

		public void RunEnterpriseUrl(string url)
		{
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
		}

		public void DisableHybridMode()
		{
			ObjectFactory.Get<IWinFormsListener>().Disable();
		}
	}
}
