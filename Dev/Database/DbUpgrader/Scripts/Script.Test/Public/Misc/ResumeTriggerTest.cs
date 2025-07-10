using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(ResumeTrigger))]
	class ResumeTriggerTest : DbCreateScriptTest
	{
		#region TestResumeTrigger

		public void TestResumeTrigger()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				foreach (var trigger in SuspendedTriggersTest.SuspendableTriggers)
				{
					var procedureSQL = $"EXEC dbo.ResumeTrigger '{trigger}'";
					AssertExceptionThrown(typeof(SqlException), "Do not call ResumeTrigger() outside a Transaction.", () => connection.ExecuteNonQuery(procedureSQL));

					using (connection.BeginTransactionWithManager())
					{
						connection.ExecuteNonQuery($"EXEC dbo.SuspendTrigger '{trigger}'");

						AssertEquals("Precondition: Trigger is suspended.", "Shared", connection.ExecuteScalar($"SELECT APPLOCK_MODE('public', '{trigger}', 'Transaction')"));

						connection.ExecuteNonQuery(procedureSQL);
						AssertEquals("Trigger should no longer be suspended.", "NoLock", connection.ExecuteScalar($"SELECT APPLOCK_MODE('public', '{trigger}', 'Transaction')"));
					}
				}
			}
		}
		#endregion
	}
}

