using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IRepositoryProcessor
	{
		void PrepareAndCopy(IWorkingDirectory workingDirectory, IWorkingDirectory outputDirectory, ILogger logger);
	}
}
