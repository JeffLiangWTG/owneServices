using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IBinaryProcessor
	{
		void FindAndCopy(IWorkingDirectory outputDirectory, IReadOnlyCollection<IRepository> repositories, ILogger logger);
	}
}
