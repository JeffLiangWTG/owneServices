using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(SuspendTrigger))]
	class SuspendTriggerTest : DbCreateScriptTest
	{
		#region TestSuspendTrigger

		public void TestSuspendTrigger()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				foreach (var trigger in SuspendedTriggersTest.SuspendableTriggers)
				{
					var procedureSQL = $"EXEC dbo.SuspendTrigger '{trigger}'";
					AssertExceptionThrown(typeof(SqlException), "Do not call SuspendTrigger() outside a Transaction.", () => connection.ExecuteNonQuery(procedureSQL));

					using (connection.BeginTransactionWithManager())
					{
						AssertEquals("Precondition: Trigger is not suspended.", "NoLock", connection.ExecuteScalar($"SELECT APPLOCK_MODE('public', '{trigger}', 'Transaction')"));

						connection.ExecuteNonQuery(procedureSQL);
						AssertEquals("Trigger should be suspended.", "Shared", connection.ExecuteScalar($"SELECT APPLOCK_MODE('public', '{trigger}', 'Transaction')"));
					}

					using (connection.BeginTransactionWithManager())
					using (var connectionToStealLock = Db.NewExtraConnectionToMainDb())
					{
						connectionToStealLock.ExecuteNonQuery($"EXEC sp_getapplock @Resource = '{trigger}', @LockMode = 'Exclusive', @LockOwner = 'Session', @DbPrincipal = 'public'");

						AssertExceptionThrown(typeof(SqlException), $"Received Error Code -1 when attempting to suspend Trigger: {trigger}.", () => connection.ExecuteNonQuery(procedureSQL));
					}
				}
			}
		}
		#endregion
	}
}

