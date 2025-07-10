using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IE.ServiceTasks
{
	public abstract class NonBranchSpecificServiceTask : ServiceProviderImpl
	{
		protected NonBranchSpecificServiceTask()
		{ }

		public sealed override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				try
				{
					RunTaskCore(youMustReactToThisToken);
				}
				catch (EmailSendFailedException ex)
				{
					ServiceLogger.Log(LogType.Error, ex.Message);
				}
			}
		}

		protected void ExecuteBatch(BatchProcess processor, CancellationToken youMustReactToThisToken)
		{
			try
			{
				processor.Logger.OnLogInfoAdded += Logger_OnLogInfoAdded;
				processor.ExecuteBatch(youMustReactToThisToken);
			}
			finally
			{
				processor.Logger.OnLogInfoAdded -= Logger_OnLogInfoAdded;
			}
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}

		protected abstract void RunTaskCore(CancellationToken youMustReactToThisToken);
	}
}
