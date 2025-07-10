using System.Diagnostics;

namespace CargoWise.Setup.Services;

public interface IProcessRunner
{
	IProcess Start(ProcessStartInfo startInfo);
}