using CargoWise.ServiceManager.Next.Shared.Services;

namespace CargoWise.ServiceManager.Next.Launcher;

public interface INextProcessRunnerPool
{
	Task<TResponse> RunAsync<TRequest, TResponse>(string runnerCode, TokenHandlerDelegate<TRequest, TResponse> handler, TRequest request, CancellationToken cancellationToken);
}
