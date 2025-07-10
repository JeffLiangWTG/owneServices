using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IDirectoryAdapter
	{
		ITempDirectory CreateTempDirectory(ILogger logger);

		ITempDirectory CreateGitDirectory(ILogger logger);

		IWorkingDirectory CreateOutputDirectory(ILogger logger);
	}
}
