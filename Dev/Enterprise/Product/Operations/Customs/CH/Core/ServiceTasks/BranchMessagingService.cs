using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.ServiceTasks;

public abstract class BranchMessagingService : Customs.ServiceTasks.CustomsServiceTask
{
	public const string MessageServiceTaskCategory = "CHC";

	protected sealed override void RunTaskCore(CancellationToken token)
	{
		foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Switzerland))
		{
			token.ThrowIfCancellationRequested();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				RunTaskForEachBranch(token);
			}
		}
	}

	protected abstract void RunTaskForEachBranch(CancellationToken token);
}
