using System.Threading;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks
{
	public interface INctsResponseDownloader
	{
		void ExecuteDownload(ILogger serviceLogger, GlbBranch branch, CancellationToken token);
	}
}
