using System.Diagnostics;

namespace CargoWise.Setup.Services;

internal class ProcessRunnerProxy : IProcessRunner
{
	public IProcess Start(ProcessStartInfo startInfo)
	{
#pragma warning disable CW1078
		return new ProcessWrapper(Process.Start(startInfo));
#pragma warning restore CW1078
	}
}
