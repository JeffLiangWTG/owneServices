namespace CargoWise.ServiceManager.Next.Launcher;

public interface INextProcessRunner : IDisposable
{
	public int ProcessId { get; }
	public bool HasExited { get; }
	string RunnerCode { get; }
	Task<int> StartAsync(CancellationToken cancellationToken);
}
