using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Moq;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class ScriptUpgraderTest : BaseUpgraderTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			SetupTestResources();
		}

		protected override void TearDown()
		{
			DbCommitTracker.Ignore("CREATE TRIGGER TG_Test_Trigger_");
			TearDownTestResources();

			base.TearDown();
		}

		void SetupTestResources()
		{
			CreateTestDbs();
			testConnection = Db.NewAdminConnection(Db.ServerName, ScriptUpgraderForTesting.TestMainDb);
			manager = new UpgradeManagerForTestWithOutputBuffer();
			testUpgrader = new ScriptUpgraderForTesting(manager, testConnection, new Resource.Version.VersionLabel(10000,0));
			testUpgrader.DoTestUpgrade();
			((ICurrentDbControl)testConnection).UseDatabase(ScriptUpgraderForTesting.TestMainDb);
		}

		void CreateTestDbs()
		{
			using (var adminConn = Db.NewAdminConnection())
			{
				new ScriptUpgraderForTesting(Mock.Of<IUpgradeManager>(), adminConn)
					.CreateTestDbs();
			}
		}

		void TearDownTestResources()
		{
			testUpgrader.CleanTestResources();
			testUpgrader = null;
			testConnection.Dispose();
			testConnection = null;
		}

		UpgradeManagerForTestWithOutputBuffer manager;
		ScriptUpgraderForTesting testUpgrader;
		AdminConnection testConnection;

		#region Client Specific

		public void TestClientSpecificViewCreated()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "ClientView", "V", true);
		}

		public void TestUnmatchingClientSpecificFunctionsAreNotRemoved()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "Client_TestClientFunction", "IF", true);
		}

		#endregion

		#region Other Databases

		public void TestDocManagerObjectsSynchronised()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestDocManagerDb, "VW_Test_View_DocManager_01", "V", true);
			AssertObjectExists(ScriptUpgraderForTesting.TestDocManagerDb, "Test_Function_DocManager_01", "FN", false);
		}

		#endregion

		/// <summary>
		/// V  VIEW
		/// IF SQL_INLINE_TABLE_VALUED_FUNCTION
		/// FN SQL_SCALAR_FUNCTION
		/// TF SQL_TABLE_VALUED_FUNCTION
		/// </summary>
		void AssertObjectExists(string dbName, string objName, string objType, bool expected)
		{
			string sqlText = String.Format(
				"IF EXISTS(SELECT null FROM [{0}].sys.objects WHERE name = '{1}' {2}) SELECT 1 ELSE SELECT 0",
				dbName, objName, (expected) ? "AND [type] = '" + objType + "'" : "");
			bool actual = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(String.Format("Does object [{1}] of type [{2}] exist in database [{0}]?", dbName, objName, objType), expected, actual);
		}

		void AssertTriggerExists(string dbName, string triggerName, bool expected)
		{
			var sqlText = string.Format("SELECT CONVERT(bit, CASE WHEN EXISTS (SELECT NULL FROM [{0}].sys.triggers WHERE name = '{1}') THEN 1 ELSE 0 END);", dbName, triggerName);
			bool actual = Convert.ToBoolean(testConnection.ExecuteScalar(sqlText));
			AssertEquals(string.Format("Does trigger [{1}] exist in database [{0}]?", dbName, triggerName), expected, actual);
		}

		#region Views and Routines

		public void TestViewWithSpecialCharactersOnNameWasDropped()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "VW_&_Match_DefaultDiff", "V", false);
		}

		public void TestTableCodeNameMappingFunctionExists()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "TableCodeNameMapping", "IF", true);
			AssertObjectExists(ScriptUpgraderForTesting.TestDocManagerDb, "TableCodeNameMapping", "IF", false);
			AssertObjectExists(ScriptUpgraderForTesting.TestEdwDb, "TableCodeNameMapping", "IF", false);
		}

		public void TestFunctionsSynchronised()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "Test_Function_01", "IF", false);
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "Test_Function_02", "IF", true);
		}

		public void TestProceduresSynchronised()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "XT_Test_Proc_01", "P", true);

			string sqlText = String.Format("EXEC {0}..XT_Test_Proc_01", ScriptUpgraderForTesting.TestMainDb);
			string procResult = testConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Stored Procedure should return modified result", "something", procResult);
		}

		public void TestTriggersSynchronised()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "TG_Test_Trigger_01", "TR", true);
		}

		public void TestViewsSynchronised()
		{
			Assert(manager.OutputTextCollection.Contains("Creating temporary indexes and columns for offline indexed view synchronisation"));
			Assert(manager.OutputTextCollection.Contains("    (+) Creating temporary columns for view: [dbo].[ViewStmData]"));
			Assert(manager.OutputTextCollection.Contains("        (~) Creating index for view: [dbo].[ViewStmData]"));

			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "ViewStmData", "V", true);
			Assert(manager.OutputTextCollection.Contains("    (+) [dbo].[ViewStmData] (VIEW)"));
			Assert(manager.OutputTextCollection.Contains("        (~) Creating indexes for view: [dbo].[ViewStmData]"));
			Assert(manager.OutputTextCollection.Contains("        (-) Dropping temporary indexes and columns used to create view: [dbo].[ViewStmData]"));
		}

		public void TestTriggersIgnored()
		{
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "TG_Test_Trigger_02", "TR", false);

			var ex = AssertExceptionThrown<InvalidOperationException>(() => testUpgrader.CheckTriggersAreEnabledAndSync());
			AssertStartsWith("", "The following triggers are not synchronised:", ex.Message);
			AssertNotContains("[dbo].[TG_Test_Trigger_01] was not found", ex.Message);
			AssertContains("[dbo].[TG_Test_Trigger_02] was not found", ex.Message);
		}

		public void TestDDLTriggerExists_DoesNotThrowError()
		{
			AssertTriggerExists(ScriptUpgraderForTesting.TestMainDb, "TG_Test_DDL_Trigger", true);

			var ex = AssertExceptionThrown<InvalidOperationException>(() => testUpgrader.CheckTriggersAreEnabledAndSync());
			AssertStartsWith("", "The following triggers are not synchronised:", ex.Message);
			AssertNotContains("[TG_Test_DDL_Trigger] was not found", ex.Message);
			AssertContains("[dbo].[TG_Test_Trigger_02] was not found", ex.Message);
		}

		public void TestDDLTriggerDoesNotExist_ThrowsError()
		{
			using (((ICurrentDbControl)testConnection).UseDatabase(ScriptUpgraderForTesting.TestMainDb))
			{
				testConnection.ExecuteScalar("DROP TRIGGER [TG_Test_DDL_Trigger] ON DATABASE");
			}
			AssertTriggerExists(ScriptUpgraderForTesting.TestMainDb, "TG_Test_DDL_Trigger", false);

			var ex = AssertExceptionThrown<InvalidOperationException>(() => testUpgrader.CheckTriggersAreEnabledAndSync());
			AssertStartsWith("", "The following triggers are not synchronised:", ex.Message);
			AssertContains("[TG_Test_DDL_Trigger] was not found", ex.Message);
		}

		#endregion // Views and Routines

		#region Non DBO Schema Objects

		public void TestNonDboObjectsRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.sys.objects WHERE is_ms_shipped = 0 AND schema_id != 1",
				ScriptUpgraderForTesting.TestMainDb);
			int qtyRows = Convert.ToInt32(testConnection.ExecuteScalar(sqlText));
			AssertEquals("Non dbo object count", 0, qtyRows);
			AssertObjectExists(ScriptUpgraderForTesting.TestMainDb, "XT_Test_Proc_01", "P", true);
		}

		#endregion

		public void TestNewlyCreatedEDocDbsAreIncludedInUpgrade()
		{
			// Arrange
			const string testEDocDatabase = $"{ScriptUpgraderForTesting.TestMainDb}_SD002";

			using (var adminConnection = Db.NewAdminConnection())
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(adminConnection, testEDocDatabase)))
			{
				testUpgrader.CreateTestDbs();

				var cacheEdocDbList = testUpgrader.EstimatedNumberOfTasks;
				adminConnection.CreateDatabase(testEDocDatabase);

				// Act
				testUpgrader.DoTestUpgrade();

				// Assert
				Assert(manager.OutputTextCollection.Contains($"*** Database: {testEDocDatabase} ***"));
			}
		}

		#region Task Numbers

		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			return new ScriptUpgraderForTesting(dummyUpgradeManager, testConnection);
		}

		#endregion

	}
}
