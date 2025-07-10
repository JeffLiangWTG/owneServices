
using System.Threading;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class FRNctsResponseServiceTask : INctsResponseDownloader
	{
		public void ExecuteDownload(ILogger serviceLogger, GlbBranch branch, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();

			using (var processor = new FRNctsResponseMessageProcessor(serviceLogger))
			{
				processor.ExecuteBatch(token);
			}
		}
	}
}
