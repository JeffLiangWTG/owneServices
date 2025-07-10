using System.Threading;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IExporter
	{
		IExportResult Export(IWorkingDirectory tempDirectoryMock, ILogger logger, CancellationToken cancellationToken);
	}
}
