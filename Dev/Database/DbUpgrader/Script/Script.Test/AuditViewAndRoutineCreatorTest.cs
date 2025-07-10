using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class AuditViewAndRoutineCreatorTest : BiViewAndRoutineCreatorTest
	{
		protected override string BiDatabaseName
		{
			get { return TestConnection.CurrentDatabase + Db.AuditDatabaseSuffix; }
		}

		protected override void RunViewAndRoutineCreator()
		{
			AuditViewAndRoutineCreator.New(new DummyUpgradeManager(), TestConnection, TestConnection.CurrentDatabase).Run();
		}
	}
}
