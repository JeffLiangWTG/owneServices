using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class DummyTransferRuleRunner : TestTransferRuleRunner
	{
		public DummyTransferRuleRunner(BMSystem system, ILogger logger, int batchSize = 0, bool shouldSortByCompletionStatement = false)
			: this(system, new TransferRuleRunnerLogger(logger, system), batchSize, shouldSortByCompletionStatement)
		{
		}

		DummyTransferRuleRunner(BMSystem system, ITransferRuleRunnerLogger logger, int batchSize, bool shouldSortByCompletionStatement)
			: base(system, logger, dataAccessor: new DummyTransferRuleRunnerDataAccessor(logger, batchSize, shouldSortByCompletionStatement))
		{
		}

		protected override void OnTransferWorkflows()
		{
			BeforeOnBatchProcessedAction?.Invoke();

			base.OnTransferWorkflows();

			using (DataAccessor.GetTemporaryEnvironmentForServiceTaskBranch())
			{
				AfterOnBatchProcessedAction?.Invoke();
			}
		}

		public Action BeforeOnBatchProcessedAction { get; set; }
		public Action AfterOnBatchProcessedAction { get; set; }

		protected override void SavePendingWorkflows()
		{
			using (DataAccessor.GetTemporaryEnvironmentForServiceTaskBranch())
			{
				OnBeforeSaveTransferredWorkflowsAction?.Invoke();
			}

			base.SavePendingWorkflows();
		}

		public Action OnBeforeSaveTransferredWorkflowsAction { get; set; }

		class DummyTransferRuleRunnerDataAccessor : TransferRuleRunnerDataAccessor
		{
			public DummyTransferRuleRunnerDataAccessor(ITransferRuleRunnerLogger logger, int batchSize = 0, bool shouldSortByCompletionStatement = false)
				: base(logger)
			{
				this.batchSize = batchSize;
				this.shouldSortByCompletionStatement = shouldSortByCompletionStatement;
			}

			readonly int batchSize;
			readonly bool shouldSortByCompletionStatement;

			protected override int GetWorkflowBatchSizeCore() => batchSize != 0 ? batchSize : base.GetWorkflowBatchSizeCore();

			protected override ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
			{
				var query = base.GetLinkAssociatedWorkflowQueryCore(link);

				if (shouldSortByCompletionStatement)
				{
					query.OrderBy = ProcessHeaderSchema.Constants.FH_CompletionStatement;
				}

				return query;
			}
		}
	}
}
