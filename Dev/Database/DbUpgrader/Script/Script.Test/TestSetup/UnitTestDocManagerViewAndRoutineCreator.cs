using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script.Test.TestSetup
{
	class UnitTestDocManagerViewAndRoutineCreator : DocManagerViewAndRoutineCreator
	{
		public UnitTestDocManagerViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb)
		{
		}

		protected override DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			DbRoutineScriptCollection result = new DbRoutineScriptCollection();
			result.Add(new DbScript("VW_Test_View_DocManager_01", "CREATE VIEW VW_Test_View_DocManager_01 AS SELECT 0 Col1", "VIEW"));

			return result;
		}
	}
}
