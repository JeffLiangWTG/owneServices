using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ITriggerProvider
	{
		ICollection<(IBaseTrigger trigger, IBusiness parent)> LoadAllMilestonesAndLineTriggersForEvent(IBusiness logParent, IStmALog log, string lineTriggerType);
		ICollection<(IBaseTrigger trigger, IBusiness parent)> LoadAllMilestonesAndTriggersForEvent(IBusiness logParent, IStmALog log, ZDateTimeOffset eventTime);
		bool TryGetQueryForAllTriggersIncludingThoseOnParentObjects(IBusiness logParent, string eventCode, out ZQuery query);
		void AddFetchHintsForTriggeringEvent(BusinessObjectFactory factory, IBusiness logsParent, string eventCode);
	}
}
