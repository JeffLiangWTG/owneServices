using System.Diagnostics;

namespace CargoWise.Setup.Services;

internal class ProcessWrapper(Process? inner) : IProcess
{
	public void WaitForExit() => inner?.WaitForExit();
	public int ExitCode => inner?.ExitCode ?? -1;
	public bool Present => inner != null;
}