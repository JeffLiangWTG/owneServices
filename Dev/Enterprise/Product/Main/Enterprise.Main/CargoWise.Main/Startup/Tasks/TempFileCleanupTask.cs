using CargoWise.Definitions;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public class TempFileCleanupTask : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.TempFileCleanupTaskError;

		public override void DoExecute()
		{
			CleanTempFolder();
			CleanTemplateCache();
		}

		public void CleanTempFolder()
		{
			new TempFileCleanup().CleanTempFolder();
		}

		void CleanTemplateCache()
		{
			TemplateCache.Instance.PurgeOldRecords();
		}
	}
}
