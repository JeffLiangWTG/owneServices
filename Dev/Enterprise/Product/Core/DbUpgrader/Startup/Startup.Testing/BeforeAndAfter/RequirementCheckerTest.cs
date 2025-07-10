using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class RequirementCheckerTest : TransactionedTestCase
	{
		[ExpectNoExceptions()]
		public void TestCheckServicePackVersions()
		{
			var testChecker = new MainServerRequirementCheckerForTest();
			testChecker.CheckServicePackVersions_Exposed(TestConnection, new DummyLoggerForTest());
		}

		public void TestCheckSqlServerEdition()
		{
			var checker = new MainServerRequirementCheckerForTest();

			// Enterprise edition
			checker.ServerEditionTestOverride = DbConnection.SqlServerEdition.EnterpriseDeveloper;
			AssertNoExceptionThrown(
				"No exception should be thrown because it is Enterprise edition.",
				() => checker.CheckSqlServerEdition_Exposed(TestConnection));

			// Express edition
			checker.ServerEditionTestOverride = DbConnection.SqlServerEdition.Express;
			AssertExceptionThrown(
				"An exception should be thrown because the SQL Server edition is not supported.", typeof(ServerRequirementsNotMetException),
				() => checker.CheckSqlServerEdition_Exposed(TestConnection));

			// Standard edition
			checker.ServerEditionTestOverride = DbConnection.SqlServerEdition.StandardWorkgroup;
			AssertExceptionThrown(
				"An exception should be thrown because the SQL Server edition is not supported.", typeof(ServerRequirementsNotMetException),
				() => checker.CheckSqlServerEdition_Exposed(TestConnection));

			//
			// Turn off the block
			Enterprise.Environment.Env.Registry.OnlySupportsSqlServerEnterpriseEdition = false;

			// Standard edition - Block off
			AssertNoExceptionThrown(
				"No exception should be thrown because both the edition block is off.",
				() => checker.CheckSqlServerEdition_Exposed(TestConnection));

			// SQL 2012 - Express edition - Block off
			checker.ServerEditionTestOverride = DbConnection.SqlServerEdition.Express;
			AssertExceptionThrown(
				"An exception should be thrown because the edition block always apply to unsupported SQL EXPRESS edition.", typeof(ServerRequirementsNotMetException),
				() => checker.CheckSqlServerEdition_Exposed(TestConnection));
		}

		[ExpectNoExceptions()]
		public void TestCheckCollation()
		{
			var testChecker = new MainServerRequirementCheckerForTest();
			testChecker.CheckCollation(TestConnection);
		}

		/// <summary>
		/// If the Enterprise DB collation is not SQL_Latin1_General_CP1_CI_AS anymore, changes are required on:
		///  - Schema Upgrade synchronisation scripts (case-sensitive version SQL_Latin1_General_CP1_CS_AS)
		///  - Many Views/Procedures/Triggers/Functions on Enterprise\Product\Core\Database
		///  - EdiLoad & EdiDeploy
		/// </summary>
		public void TestCollationAdoptedByEnterprise()
		{
			AssertEquals("Adopted collation has changed", "SQL_Latin1_General_CP1_CI_AS", Db.DatabaseCollation);
		}

		public void TestDatabaseCollation()
		{
			var sqlText = string.Format("SELECT DatabasePropertyEx('{0}', 'Collation')", Db.DatabaseName);
			var dbCollation = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("DB Collation should have been fixed.", Db.DatabaseCollation, dbCollation.Trim());

			// Running this should not do anything as all databases already have the correct settings.
			// IF not, attempting to run ALTER DATABASE to fix it would fail in a transaction (TransactionedTestCase).
			new MainServerRequirementCheckerForTest().FixDbCollations_Exposed(TestConnection);

			dbCollation = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("DB Collation should have been fixed.", Db.DatabaseCollation, dbCollation.Trim());
		}

		public void TestMainDbCollation()
		{
			const string selectCollationCommand = "SELECT DatabasePropertyEx('{0}', 'Collation')";

			var sqlText = string.Format(selectCollationCommand, Db.DatabaseName);
			var mainDbCollation = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Main DB Collation", Db.DatabaseCollation.ToUpper(), mainDbCollation.ToUpper());
		}

		/// <summary>
		/// Ensures model and tempdb collations are different from the main DB one.
		/// </summary>
		public void TestModelAndTempdbCollationIsDifferentFromMainDbAsWeCanTestCollationConflictIssues()
		{
			const string selectCollationCommand = "SELECT DatabasePropertyEx('{0}', 'Collation')";
			const string assertMessage = "[{0}] collation should be different from the main database.";

			var dbName = "model";
			var sqlText = string.Format(selectCollationCommand, dbName);
			var collation = TestConnection.ExecuteScalar(sqlText).ToString();
			Assert(string.Format(assertMessage, dbName), collation.ToUpper() != Db.DatabaseCollation.ToUpper());

			dbName = "tempdb";
			sqlText = string.Format(selectCollationCommand, dbName);
			collation = TestConnection.ExecuteScalar(sqlText).ToString();
			Assert(string.Format(assertMessage, dbName), collation.ToUpper() != Db.DatabaseCollation.ToUpper());
		}
	}
}
