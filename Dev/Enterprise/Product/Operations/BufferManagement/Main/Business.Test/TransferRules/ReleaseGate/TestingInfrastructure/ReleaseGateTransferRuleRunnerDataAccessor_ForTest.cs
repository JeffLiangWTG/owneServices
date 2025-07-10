using System.Linq;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class ReleaseGateTransferRuleRunnerDataAccessor_ForTest : ReleaseGateTransferRuleRunnerDataAccessor
	{
		public ReleaseGateTransferRuleRunnerDataAccessor_ForTest(ITransferRuleRunnerLogger logger)
			: base(logger)
		{
		}

		protected override void OnBatchLoaded_ForTest(ref ITransferrableProcessHeader[] workflowBatch, ref ITransferrableProcessHeader lastProcessHeaderRead)
		{
			base.OnBatchLoaded_ForTest(ref workflowBatch, ref lastProcessHeaderRead);

			lastProcessHeaderRead = workflowBatch.LastOrDefault();
			workflowBatch = workflowBatch.OrderByDescending(w => w?.VoteUpDownAmount).ToArray();
		}
	}
}
