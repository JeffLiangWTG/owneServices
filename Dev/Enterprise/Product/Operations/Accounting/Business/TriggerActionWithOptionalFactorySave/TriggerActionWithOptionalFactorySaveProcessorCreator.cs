using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public class TriggerActionWithOptionalFactorySaveProcessorCreator : IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator
	{
		IProcessor IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator.Create(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog)
		{
			return new TriggerActionWithOptionalFactorySaveProcessor(triggerAction, parent, queuedLog);
		}
	}
}
