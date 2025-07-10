using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class EdwViewAndRoutineCreatorTest : BiViewAndRoutineCreatorTest
	{
		protected override string BiDatabaseName
		{
			get { return TestConnection.CurrentDatabase + Db.EdwDatabaseSuffix; }
		}

		protected override void RunViewAndRoutineCreator()
		{
			EdwViewAndRoutineCreator.New(new DummyUpgradeManager(), TestConnection, TestConnection.CurrentDatabase).Run();
		}
	}
}
