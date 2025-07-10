using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IRepositoryRetriever
	{
		IReadOnlyCollection<IRepository> DownloadRepositories(IWorkingDirectory workingDirectory, ILogger logger, IWorkingDirectory gitDirectory);
	}
}
