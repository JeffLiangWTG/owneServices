namespace CargoWise.ServiceManager.Next.Shared.Services;

public interface INextHubClient
{
	Task<int> ProcessId(CancellationToken cancellationToken);
	Task Close(CancellationToken cancellationToken);
}
