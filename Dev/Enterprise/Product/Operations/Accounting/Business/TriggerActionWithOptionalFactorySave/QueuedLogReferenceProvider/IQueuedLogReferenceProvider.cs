using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public interface IQueuedLogReferenceProvider
	{
		IDictionary<string, string> CreateReferenceMap(ProcessTaskNotification triggerAction, IQueuedLog queuedLog, BusinessObject parent, GlbStaff userContext);

		QueuedLogParameters GetParametersFromReferenceMap(IDictionary<string, string> referenceMap, IQueuedLog queuedLog);
	}
}
