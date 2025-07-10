using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.VisualBoards.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateDirector : ReleaseGateServiceTaskProcessor
	{
		public ReleaseGateDirector(BMSystem system, ILogger logger, ReleaseGateLogger releaseGateLogger)
			: base(logger, null)
		{
			this.system = system;
			this.releaseGateLogger = releaseGateLogger;
		}

		readonly BMSystem system;
		readonly ReleaseGateLogger releaseGateLogger;

		public bool EnableDataRefresh { get; set; }

#if DEBUG

		public void Process()
		{
			Process(CancellationToken.None);
		}

#endif

		public override void ProcessCore(CancellationToken token)
		{
			var buffers = system.Components.Where(c => c.FC_IsActive && c.FC_Type == BMComponentTypeList.Codes.Buffer).OrderBy(b => b.FC_DisplaySequence).ToArray();
			var workflowPKs = new ConcurrentBag<ZGuid>();

			try
			{
				AsyncStrategy.Default.ParallelForEach(buffers, buffer_unsafe =>
				{
					if (token.IsCancellationRequested)
					{
						return;
					}
					var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ":" + buffer_unsafe.FC_Name, RefreshEnabled = false };
					var buffer = factory.Load<BMComponent>(buffer_unsafe.PK);
					var releaseGateKeeper = GetReleaseGateKeeper(buffer, Logger, releaseGateLogger, EnableDataRefresh);

					releaseGateKeeper.Process(token);
					foreach (var pk in releaseGateKeeper.WorkflowPKs)
					{
						workflowPKs.Add(pk);
					}
				});
			}
			finally
			{
				if (buffers.Any())
				{
					var contextBuffer = buffers.First();
					var provider = contextBuffer as IBranchDepartmentProvider;
					var branchPK = provider.GetBranch(contextBuffer.Factory).PK.ToGuid();
					var departmentPK = provider.GetDepartment(contextBuffer.Factory).PK.ToGuid();

					using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, departmentPK))
					{
						releaseGateLogger.CommitAllLogs(ReleaseGateKeeper.GetFactoryForSavingLogs());
					}
				}
			}

			WorkflowPKs = workflowPKs;
		}

		protected virtual ReleaseGateKeeper GetReleaseGateKeeper(BMComponent buffer, ILogger logger, ReleaseGateLogger gateLogger, bool enableDataRefresh)
		{
			return new ReleaseGateKeeper(buffer, Logger, releaseGateLogger, EnableDataRefresh);
		}
	}
}
