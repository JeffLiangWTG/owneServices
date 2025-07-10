using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public class TransactionAllocationAndPostProcessorCreator : ITransactionAllocationAndPostProcessorCreator
	{
		public IProcessor Create(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog)
		{
			var tempUser = queuedLog.Factory.Load<GlbStaff>(AccountingConfigurationRegistry.Instance.SecurityRightsForTransactionAllocateAndPostTrigger.Value);
			return new TriggerActionWithOptionalFactorySaveProcessor(triggerAction, parent, queuedLog, tempUser);
		}
	}
}
