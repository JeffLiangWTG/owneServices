using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Script.Test.TestSetup;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Script.Test
{
	public sealed class ScriptUpgraderForTesting : ScriptUpgrader
	{
		public ScriptUpgraderForTesting(AdminConnection connection)
			: this(new DummyUpgradeManager(), connection)
		{ }

		public ScriptUpgraderForTesting(IUpgradeManager upgradeManager, AdminConnection connection, VersionLabel version = null)
			: base(upgradeManager, connection, connection, connection, version ?? new VersionLabel(0, 0))
		{
			hasCustomConnection = true;
		}

		internal bool hasCustomConnection;
		public const string TestMainDb = UpgUtils.UpgraderPrefix + "TestMainDB";
		public const string TestDocManagerDb = TestMainDb + "_SD001";
		public const string TestEdwDb = TestMainDb + Db.EdwDatabaseSuffix;

		public void DoTestUpgrade()
		{
			using (((ICurrentDbControl)upgConnection).UseDatabase(TestMainDb))
			{
				CreateTemporaryScriptIndexes();
				RecreateMainDbViewsAndRoutines();
				RecreateDocManagerViewsAndRoutines();
				RecreateBiViewsAndRoutines();
			}
		}

		public void CleanTestResources()
		{
			DropTestDbs();
		}

		public void CreateTestDbs()
		{
			TestDbCreator.CreateDropExisting();

			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbDropExisting(connection, TestDocManagerDb, Db.DatabaseName);
				AdoTestUtils.CreateDbDropExisting(connection, TestEdwDb, Db.DatabaseName);
				CreateEdwSchema(connection, TestEdwDb);
			}
		}

		void DropTestDbs()
		{
			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, TestDocManagerDb, Db.DatabaseName);
				AdoTestUtils.DropDbIfExists(connection, TestEdwDb, Db.DatabaseName);
			}

			TestDbCreator.Drop();
		}

		void CreateEdwSchema(DbConnection connection, string dbName)
		{
			string schemaFileContents = new Resource.ScriptManager().BiEdwDbSchemaScript;
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteNonQuery(schemaFileContents);
			}
		}

		internal DbConnection UpgConnectionExposed
		{
			get
			{
				return upgConnection;
			}
		}

		protected override string MainDbBeingUpgraded
		{
			get { return TestMainDb; }
		}

		protected override IEnumerable<string> DocManagerDBsToUpgrade
		{
			get { return base.DocManagerDBsToUpgrade; }
		}

		internal override ViewAndRoutineCreator GetNewMainDbViewAndRoutineCreator(string upgradingDb)
		{
			return new UnitTestViewAndRoutineCreator(Manager, upgConnection, upgradingDb);
		}

		internal override DocManagerViewAndRoutineCreator GetNewViewAndRoutineCreator_DocManagerDbs(string upgradingDb)
		{
			return new UnitTestDocManagerViewAndRoutineCreator(Manager, upgConnection, upgradingDb);
		}

		internal override EdwViewAndRoutineCreator GetNewViewAndRoutineCreator_EDW(string upgradingDb)
		{
			return UnitTestEDWViewAndRoutineCreator.Test(Manager, upgConnection, upgradingDb);
		}

		readonly IAuxiliaryDbCreator TestDbCreator = new DbCreatorWithViewsAndRoutinesForTesting(TestMainDb);
	}
}
