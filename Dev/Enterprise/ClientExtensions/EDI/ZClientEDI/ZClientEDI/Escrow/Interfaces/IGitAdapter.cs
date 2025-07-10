using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	interface IGitAdapter
	{
		void Clone(string repository, string folder, ILogger logger, string extraArgs);

		IWorkingDirectory GitDirectory { get; set; }
	}
}
