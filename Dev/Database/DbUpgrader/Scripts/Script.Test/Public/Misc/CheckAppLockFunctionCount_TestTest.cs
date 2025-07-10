using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(CheckAppLockFunctionCount_Test))]
	class CheckAppLockFunctionCount_TestTest : DbCreateScriptTest
	{
		#region TestCheckAppLockFunctionCount_Test

		public void TestCheckAppLockFunctionCount_Test()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				AssertNull("No locks taken, result should be null.", GetAppLockCount(connection, "TG_PreventOverCommitOfStockViaPickLine"));

				using (connection.BeginTransactionWithManager())
				{
					connection.ExecuteNonQuery("EXEC dbo.SuspendTrigger 'TG_PreventOverCommitOfStockViaPickLine'");
					AssertEquals("Trigger suspended once, should return a Lock Count of 1.", 1, GetAppLockCount(connection, "TG_PreventOverCommitOfStockViaPickLine"));

					connection.ExecuteNonQuery("EXEC dbo.SuspendTrigger 'TG_PreventOverCommitOfStockViaPickLine'");
					AssertEquals("Trigger suspended twice, should return a Lock Count of 2.", 2, GetAppLockCount(connection, "TG_PreventOverCommitOfStockViaPickLine"));

					connection.ExecuteNonQuery("EXEC dbo.ResumeTrigger 'TG_PreventOverCommitOfStockViaPickLine'");
					AssertEquals("Trigger suspended twice, then released once, should return a Lock Count of 1.", 1, GetAppLockCount(connection, "TG_PreventOverCommitOfStockViaPickLine"));
				}

				AssertNull("Transaction is rolled back, no locks taken, result should be null.", GetAppLockCount(connection, "TG_PreventOverCommitOfStockViaPickLine"));
			}
		}

		static int? GetAppLockCount(DbConnection connection, string triggerName)
		{
			return (short?)connection.ExecuteScalar($"SELECT RequestedCount FROM dbo.CheckAppLockFunctionCount_Test('{triggerName}')");
		}
		#endregion
	}
}

