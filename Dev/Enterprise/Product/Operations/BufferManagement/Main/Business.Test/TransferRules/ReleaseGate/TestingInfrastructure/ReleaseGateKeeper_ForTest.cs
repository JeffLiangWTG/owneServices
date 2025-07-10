using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	class ReleaseGateKeeper_ForTest : ReleaseGateKeeper
	{
		public ReleaseGateKeeper_ForTest(BMComponent buffer, ILogger logger, ReleaseGateLogger releaseGateLogger, bool enableDataRefresh, ReleaseGateTestCoordinator coordinator)
			: base(buffer, logger, releaseGateLogger, enableDataRefresh)
		{
			this.coordinator = coordinator;
			this.saveCapacityCacheForAllStaff = coordinator.SaveCapacityCacheForAllStaff;
		}

		readonly bool saveCapacityCacheForAllStaff = true;
		protected override bool SaveCapacityCacheForAllStaff => saveCapacityCacheForAllStaff;

		readonly ReleaseGateTestCoordinator coordinator;

		protected override void RunPreWorkflowLoadActions(IEnumerable<ZGuid> workflowPKs)
		{
			base.RunPreWorkflowLoadActions(workflowPKs);
			coordinator.PreWorkflowReloadAction?.Invoke();
		}

		protected override bool IsWorkflowReleasable(ViewProcessHeader workflow, CapacityReservationReport report)
		{
			coordinator.AddProcessedWorkflow(workflow);

			if (coordinator.IsWorkflowReleasableFunc != null)
			{
				return coordinator.IsWorkflowReleasableFunc(workflow);
			}
			else
			{
				return base.IsWorkflowReleasable(workflow, report);
			}
		}

		protected override LinksProcessorWithDeactivation GetLinksProcessor(HashSet<ReleaseGateRequest> eligibleWorkflowsForReleaseGate, TransferRuleRunnerLogger logger, ReleaseGateTransferRuleRunnerDataAccessor dataAccessor)
		{
			if (!coordinator.SortBatchedWorkflows)
			{
				return base.GetLinksProcessor(eligibleWorkflowsForReleaseGate, logger, dataAccessor);
			}

			var dataAccessorForTest = new ReleaseGateTransferRuleRunnerDataAccessor_ForTest(new TransferRuleRunnerLogger(Logger, Buffer.System));

			return base.GetLinksProcessor(eligibleWorkflowsForReleaseGate, logger, dataAccessorForTest);
		}
	}
}
