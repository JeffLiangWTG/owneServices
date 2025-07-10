using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	public abstract class BiViewAndRoutineCreatorTest : TransactionedTestCase
	{
		public void TestNoBiDatabaseViewProcFunctionTriggerOrSeviceQueueNamesStartsWithClient()
		{
			string sqlText = String.Format(@"
				DECLARE @ClientObjectNames varchar(8000) = '';

				SELECT @ClientObjectNames = @ClientObjectNames + name + char(13) + char(10)
					FROM [{0}].sys.objects
					WHERE type in ('V', 'P', 'FN','TF','IF', 'TR', 'SQ')
					AND name LIKE 'Client%';

				SELECT @ClientObjectNames",
				BiDatabaseName);

			string clientObjectNames = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Found some objects names containing the word [client]:\r\n", "", clientObjectNames);
		}

		[ExpectNoExceptions()]
		public void TestRoutinesAreInOrderOfDependency()
		{
			var sql = String.Format(
				"SELECT COUNT(*) FROM [{0}].sys.objects AS o WHERE o.is_ms_shipped = 0 AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF', 'SQ');",
				BiDatabaseName);
			int objCount = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals(true, objCount > 0);

			// Drop all objects
			var testCreator = new ViewAndRoutineCreatorForTesting(BiDatabaseName, TestConnection);
			testCreator.ExpectedList = new DbRoutineScriptCollection();
			testCreator.Run();
			objCount = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals(false, objCount > 0);

			// Create all objects
			RunViewAndRoutineCreator();
			objCount = (int)TestConnection.ExecuteScalar(sql);
			AssertEquals(true, objCount > 0);
		}

		protected override DbConnection TestConnection
		{
			get { return adminConnection ?? (adminConnection = Db.NewAdminConnection()); }
		}
		AdminConnection adminConnection;

		protected abstract string BiDatabaseName { get; }
		protected abstract void RunViewAndRoutineCreator();
	}
}
