using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateTransferRuleRunnerDataAccessor : TransferRuleRunnerDataAccessor
	{
		public ReleaseGateTransferRuleRunnerDataAccessor(ITransferRuleRunnerLogger logger)
			: base(logger)
		{
		}

		protected override int GetWorkflowBatchSizeCore() => BMSRegistry.Instance.ReleaseGateBatchSize.Value;

		protected override IEnumerable<IComponentLink> GetComponentLinksCore(IPAVESystem system) => base.GetComponentLinksCore(system).Where(l => l.IsReleaseGateRuleApplied);

		protected override void AddFetchHintsForLinkAssociatedWorkflowProcessingCore(IReadOnlyCollection<ITransferrableProcessHeader> workflowBatch, IComponentLink link)
		{
			base.AddFetchHintsForLinkAssociatedWorkflowProcessingCore(workflowBatch, link);

			var batch = workflowBatch.Cast<ProcessHeader>().ToArray();
			var factory = batch.FirstOrDefault()?.Factory;

			if (factory != null)
			{
				ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(factory, batch.Select(x => x.PK));
				ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(factory, batch.Select(x => x.FH_FH_ParentHeader));

				if (BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					factory.Load<BMReleaseSequenceItem>(new ZQuery(BMReleaseSequenceItemSchema.BMI_FH_ProcessHeader, batch.Select(x => x.PK)));
					factory.Load<BMReleaseSequenceItem>(new ZQuery(BMReleaseSequenceItemSchema.BMI_FH_ProcessHeader, batch.Select(x => x.FH_FH_ParentHeader)));
				}
			}
			AddFetchHintsForSavingWorkflows(batch, link);
		}
	}
}
