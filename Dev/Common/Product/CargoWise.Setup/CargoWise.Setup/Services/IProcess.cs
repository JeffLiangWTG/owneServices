namespace CargoWise.Setup.Services;

public interface IProcess
{
	void WaitForExit();
	int ExitCode { get; }
	bool Present { get; }
}