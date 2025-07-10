using System.Collections.Generic;
using CargoWise.Data;

namespace CargoWise.EntityFramework
{
	public interface IDeferredTriggerRunner
	{
		IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> DeferAndReturnTriggers(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjectsInLastSaveOrder);
		void RunDeferredTriggers(IReadOnlyDictionary<string, IReadOnlyCollection<TriggerMetaData>> deferredTriggers, IDbConnected factory);
	}
}
