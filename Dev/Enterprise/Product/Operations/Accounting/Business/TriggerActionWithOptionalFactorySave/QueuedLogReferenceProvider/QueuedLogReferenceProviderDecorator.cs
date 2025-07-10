using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public abstract class QueuedLogReferenceProviderDecorator : IQueuedLogReferenceProvider
	{
		public QueuedLogReferenceProviderDecorator(IQueuedLogReferenceProvider provider)
		{
			this.provider = provider;
		}

		public virtual IDictionary<string, string> CreateReferenceMap(ProcessTaskNotification triggerAction, IQueuedLog queuedLog, BusinessObject parent, GlbStaff userContext)
			=> provider.CreateReferenceMap(triggerAction, queuedLog, parent, userContext);

		public virtual QueuedLogParameters GetParametersFromReferenceMap(IDictionary<string, string> referenceMap, IQueuedLog queuedLog)
			=> provider.GetParametersFromReferenceMap(referenceMap, queuedLog);

		protected readonly IQueuedLogReferenceProvider provider;

#if DEBUG
		public IQueuedLogReferenceProvider GetProvider_ForTestOnly() => provider;
#endif
	}
}
