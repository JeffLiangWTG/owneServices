using CargoWise.ServiceManager.Next.Shared;

namespace CargoWise.ServiceManager.Next.Launcher;

public interface INextProcessRunnerFactory
{
	INextProcessRunner Create(string runnerCode, INextRunnerOptions nextRunnerOptions);
}
