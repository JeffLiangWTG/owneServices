using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	public class CreateTriggerTransformationTest : TransactionedTestCase
	{
		public void TestUserDescription()
		{
			var transform = new TransformForTesting();
			transform.Initialise(version: null, manager: new DummyUpgradeManager());
			AssertEquals("UserDescription", "Creating trigger TriggerForTesting", transform.UserDescription);
		}

		public void TestTransform() => TestTransformCore(new DummyUpgradeManager());
		public void TestTransform_MinorVersionSetToNegativeOne() => TestTransformCore(new UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(new VersionLabel(9001, -1)));

		void TestTransformCore(IUpgradeManager upgradeManager)
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID(N'dbo.TableForTrigger', N'U') is NOT NULL) DROP TABLE dbo.TableForTrigger;");
			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.TableForTrigger (id int);");

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TableForTrigger"));
			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));

			var transform = new TransformForTesting();
			transform.Initialise(null, upgradeManager);
			transform.Run();

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));
			var expected = "CREATE TRIGGER TriggerForTesting ON [TableForTrigger] AFTER DELETE AS ROLLBACK";
			var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.TriggerForTesting', N'TR'))";
			AssertEquals(expected, Db.Connection.ExecuteScalar<string>(sql));
		}

		public void TestTransform_RunTwice()
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID(N'dbo.TableForTrigger', N'U') is NOT NULL) DROP TABLE dbo.TableForTrigger;");
			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.TableForTrigger (id int);");

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TableForTrigger"));
			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));

			var transform = new TransformForTesting();
			transform.Initialise(null, new DummyUpgradeManager());
			transform.Run();

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));
			var expected = "CREATE TRIGGER TriggerForTesting ON [TableForTrigger] AFTER DELETE AS ROLLBACK";
			var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.TriggerForTesting', N'TR'))";
			AssertEquals(expected, Db.Connection.ExecuteScalar<string>(sql));

			var upgradeManager = new UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(new VersionLabel(9001, 1));
			var transform2 = new TransformForTesting();
			transform2.Initialise(null, upgradeManager);

			AssertExceptionThrown<SqlException>("Should have thrown an exception.", "There is already an object named 'TriggerForTesting' in the database.", transform2.Run);
		}

		public void TestTransform_RunTwice_MinorVersionSetToNegativeOne()
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID(N'dbo.TableForTrigger', N'U') is NOT NULL) DROP TABLE dbo.TableForTrigger;");
			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.TableForTrigger (id int);");

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TableForTrigger"));
			AssertEquals(false, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));

			var transform = new TransformForTesting();
			transform.Initialise(null, new DummyUpgradeManager());
			transform.Run();

			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TriggerForTesting"));
			var expected = "CREATE TRIGGER TriggerForTesting ON [TableForTrigger] AFTER DELETE AS ROLLBACK";
			var sql = "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.TriggerForTesting', N'TR'))";
			AssertEquals(expected, Db.Connection.ExecuteScalar<string>(sql));

			var upgradeManager = new UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(new VersionLabel(9001, -1));
			var transform2 = new TransformForTesting();
			transform2.Initialise(null, upgradeManager);
			AssertNoExceptionThrown(transform2.Run);
			AssertEquals(expected, Db.Connection.ExecuteScalar<string>(sql));
		}

		public void TestTransform_RunTwice_MinorVersionSetToNegativeOne_TriggerScriptMismatch()
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID(N'dbo.TableForTrigger', N'U') is NOT NULL) DROP TABLE dbo.TableForTrigger;");
			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.TableForTrigger (id int);");
			Db.Connection.ExecuteNonQuery("CREATE TRIGGER TriggerForTesting ON [TableForTrigger] INSTEAD OF DELETE AS ROLLBACK;");

			var upgradeManager = new UpgradeManagerForTestWithMockSchemaVersionBeforeUpgrade(new VersionLabel(9001, -1));
			var transform = new TransformForTesting();
			transform.Initialise(null, upgradeManager);
			AssertExceptionThrown<SqlException>("Should have thrown an exception.", "There is already an object named 'TriggerForTesting' in the database.", transform.Run);
		}

		public void TestTriggerName()
		{
			var transform = new TransformForTesting();
			transform.Initialise(null, new DummyUpgradeManager());
			AssertEquals("TriggerName", "TriggerForTesting", transform.TriggerName);
		}
	}
}
