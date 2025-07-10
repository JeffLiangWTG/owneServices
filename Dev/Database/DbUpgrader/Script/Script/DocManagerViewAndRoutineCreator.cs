using System.Collections.Generic;
using CargoWise.Common.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script
{
	class DocManagerViewAndRoutineCreator : ViewAndRoutineCreator
	{
		public DocManagerViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb)
		{
		}

		protected override DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			// There are currently no script objects in the DocManager databases
			// If one is needed, create a DocManagerScriptIndex class in the NoRegenScript project.
			return new DbRoutineScriptCollection();
		}

		protected override List<string> SkipObjectNameList
		{
			get
			{
				var result = base.SkipObjectNameList;
				result.Add(DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager);
				return result;
			}
		}
	}
}
