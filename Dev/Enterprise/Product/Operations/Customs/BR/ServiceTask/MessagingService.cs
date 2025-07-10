using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.ServiceTasks
{
	public abstract class MessagingService : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "BRC";

		protected sealed override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Brazil))
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
}
