using System.Threading;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	interface IImportService
	{
		void Process(CancellationToken token);
	}
}