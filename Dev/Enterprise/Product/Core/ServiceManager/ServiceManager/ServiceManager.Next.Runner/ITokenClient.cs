using Microsoft.AspNetCore.SignalR.Client;

namespace CargoWise.ServiceManager.Next.Runner;

public interface ITokenClient
{
	void RegisterTokenClient(HubConnection hubConnection);
}
