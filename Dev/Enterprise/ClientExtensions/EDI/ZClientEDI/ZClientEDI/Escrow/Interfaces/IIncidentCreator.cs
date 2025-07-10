using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow
{
	interface IIncidentCreator
	{
		void Create(IExportResult exportResult, ILogger logger);
	}
}
