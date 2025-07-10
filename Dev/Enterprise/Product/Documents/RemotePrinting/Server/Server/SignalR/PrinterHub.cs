using System;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Types;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Enterprise.RemotePrinting.Server
{
	[HubName("RemoteHub")]
	public class RemoteHub : Hub<IRemoteClient>, IRemoteServer
	{
		static readonly Lazy<RemotePrintController> controller = new Lazy<RemotePrintController>();
		public static RemotePrintController Controller => controller.Value;

		public void Initialise(string name, string version, string[] printers) // Version number will be used to push new versions to clients in future versions, but is currently ignored
			=> Controller.RegisterClient(Context, name, printers, version);

		// RequestPrint is used from RTUS, it also may be used for direct printing to 'label' printers in future.
		public bool RequestPrint(string server, SerialisablePrintJob job)
			=> Controller.RequestPrint(server, job);

		public bool Nudge(string server, string printQueue, Guid printJobPk)
			=> Controller.Nudge(server, printQueue, printJobPk, Context);

		public override Task OnDisconnected(bool stopCalled)
		{
			Controller.UnregisterClientAndGetVersionNumber(Context);
			return base.OnDisconnected(stopCalled);
		}

		public override Task OnReconnected()
		{
			Controller.RegisterClientForReconnecting(Context.ConnectionId);
			return base.OnReconnected();
		}

		public void SetPrintStatus(Guid jobPk, ProcessedStatus status, string failureInfo)
			=> Controller.SetPrintStatus(jobPk, status, failureInfo);

		public void UpdatePrinters(string name, string[] printers)
		{
			var versionNum = Controller.UnregisterClientAndGetVersionNumber(Context);
			Controller.RegisterClient(Context, name, printers, versionNum);
		}

		public void RequestRefreshCNSWClientSetting(string server)
			=> Controller.RequestRefreshCNSWClientSetting(server);
	}
}
